﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BmwFileReader;
using EdiabasLib;
using log4net;
using PsdzClient.Core.Container;

namespace PsdzClient
{
    public class DetectVehicle : IDisposable
    {
        public enum DetectResult
        {
            Ok,
            NoResponse,
            Aborted,
            InvalidDatabase
        }

        private static readonly ILog log = LogManager.GetLogger(typeof(DetectVehicle));
        private readonly Regex _vinRegex = new Regex(@"^(?!0{7,})([a-zA-Z0-9]{7,})$");
        private static readonly Tuple<string, string, string, string>[] ReadVinJobsBmwFast =
        {
            new Tuple<string, string, string, string>("G_ZGW", "STATUS_VIN_LESEN", null, "STAT_VIN"),
            new Tuple<string, string, string, string>("ZGW_01", "STATUS_VIN_LESEN", null, "STAT_VIN"),
            new Tuple<string, string, string, string>("G_CAS", "STATUS_FAHRGESTELLNUMMER", null, "STAT_FGNR17_WERT"),
            new Tuple<string, string, string, string>("D_CAS", "STATUS_FAHRGESTELLNUMMER", null, "FGNUMMER"),
            new Tuple<string, string, string, string>("D_MRMOT", "STATUS_FAHRGESTELLNUMMER", null, "STAT_FGNUMMER"),
            new Tuple<string, string, string, string>("D_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT"),
            new Tuple<string, string, string, string>("G_MRMOT", "STATUS_LESEN", "ARG;FAHRGESTELLNUMMER_MR", "STAT_FAHRGESTELLNUMMER_TEXT"),
        };

        private static readonly Tuple<string, string, string>[] ReadIdentJobsBmwFast =
        {
            new Tuple<string, string, string>("G_ZGW", "STATUS_VCM_GET_FA", "STAT_BAUREIHE"),
            new Tuple<string, string, string>("ZGW_01", "STATUS_VCM_GET_FA", "STAT_BAUREIHE"),
            new Tuple<string, string, string>("G_CAS", "STATUS_FAHRZEUGAUFTRAG", "STAT_FAHRZEUGAUFTRAG_KOMPLETT_WERT"),
            new Tuple<string, string, string>("D_CAS", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_LM", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_KBM", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_MRKOMB", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_MRZFE", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
            new Tuple<string, string, string>("D_MRMOT", "C_FA_LESEN", "FAHRZEUGAUFTRAG"),
        };

        private static readonly Tuple<string, string>[] ReadILevelJobsBmwFast =
        {
            new Tuple<string, string>("G_ZGW", "STATUS_I_STUFE_LESEN_MIT_SIGNATUR"),
            new Tuple<string, string>("G_ZGW", "STATUS_VCM_I_STUFE_LESEN"),
            new Tuple<string, string>("G_FRM", "STATUS_VCM_I_STUFE_LESEN"),
        };

        private static readonly Tuple<string, string, string, string>[] ReadVoltageJobsBmwFast =
        {
            new Tuple<string, string, string, string>("G_MOTOR", "STATUS_LESEN", "ARG;MESSWERTE_IBS2015", "STAT_SPANNUNG_IBS2015_WERT"),
            new Tuple<string, string, string, string>("G_MOTOR", "STATUS_MESSWERTE_IBS", string.Empty, "STAT_U_BATT_WERT"),
        };

        public delegate bool AbortDelegate();

        private PdszDatabase _pdszDatabase;
        private bool _disposed;
        private EdiabasNet _ediabas;
        private bool _abortRequest;
        private AbortDelegate _abortFunc;

        public List<PdszDatabase.EcuInfo> EcuList { get; private set; }
        public string Vin { get; private set; }
        public string TypeKey { get; private set; }
        public string GroupSgdb { get; private set; }
        public string ModelSeries { get; private set; }
        public string Series { get; private set; }
        public string ConstructYear { get; private set; }
        public string ConstructMonth { get; private set; }
        public DateTime? ConstructDate { get; private set; }
        public string Paint { get; private set; }
        public string Upholstery { get; private set; }
        public string StandardFa { get; private set; }
        public List<string> Salapa { get; private set; }
        public List<string> HoWords { get; private set; }
        public List<string> EWords { get; private set; }
        public List<string> ZbWords { get; private set; }
        public string ILevelShip { get; private set; }
        public string ILevelCurrent { get; private set; }
        public string ILevelBackup { get; private set; }

        public DetectVehicle(PdszDatabase pdszDatabase, string ecuPath, EdInterfaceEnet.EnetConnection enetConnection = null, bool allowAllocate = true, int addTimeout = 0)
        {
            _pdszDatabase = pdszDatabase;
            EdInterfaceEnet edInterfaceEnet = new EdInterfaceEnet(false);
            _ediabas = new EdiabasNet
            {
                EdInterfaceClass = edInterfaceEnet,
                AbortJobFunc = AbortEdiabasJob
            };
            _ediabas.SetConfigProperty("EcuPath", ecuPath);

            bool icomAllocate = false;
            string hostAddress = "auto:all";
            if (enetConnection != null)
            {
                icomAllocate = allowAllocate && enetConnection.ConnectionType == EdInterfaceEnet.EnetConnection.InterfaceType.Icom;
                hostAddress = enetConnection.ToString();
            }
            edInterfaceEnet.RemoteHost = hostAddress;
            edInterfaceEnet.IcomAllocate = icomAllocate;
            edInterfaceEnet.AddRecTimeoutIcom += addTimeout;
            EcuList = new List<PdszDatabase.EcuInfo>();

            ResetValues();
        }

        public DetectResult DetectVehicleBmwFast(AbortDelegate abortFunc)
        {
            log.InfoFormat(CultureInfo.InvariantCulture, "DetectVehicleBmwFast Start");
            ResetValues();
            HashSet<string> invalidSgbdSet = new HashSet<string>();

            try
            {
                _abortFunc = abortFunc;
                if (!Connect())
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "DetectVehicleBmwFast Connect failed");
                    return DetectResult.NoResponse;
                }

                List<Dictionary<string, EdiabasNet.ResultData>> resultSets;
                string detectedVin = null;
                foreach (Tuple<string, string, string, string> job in ReadVinJobsBmwFast)
                {
                    if (_abortRequest)
                    {
                        return DetectResult.Aborted;
                    }

                    try
                    {
                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        if (!string.IsNullOrEmpty(job.Item3))
                        {
                            _ediabas.ArgString = job.Item3;
                        }

                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            if (detectedVin == null)
                            {
                                detectedVin = string.Empty;
                            }

                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item4, out EdiabasNet.ResultData resultData))
                            {
                                string vin = resultData.OpData as string;
                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!string.IsNullOrEmpty(vin) && _vinRegex.IsMatch(vin))
                                {
                                    detectedVin = vin;
                                    log.InfoFormat(CultureInfo.InvariantCulture, "Detected VIN: {0}", detectedVin);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        invalidSgbdSet.Add(job.Item1);
                        log.ErrorFormat(CultureInfo.InvariantCulture, "No VIN response");
                        // ignored
                    }
                }

                if (string.IsNullOrEmpty(detectedVin))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "No VIN detected");
                    return DetectResult.NoResponse;
                }

                Vin = detectedVin;
                foreach (Tuple<string, string, string> job in ReadIdentJobsBmwFast)
                {
                    if (_abortRequest)
                    {
                        return DetectResult.Aborted;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "Read BR job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "Job ignored: {0}", job.Item1);
                        continue;
                    }

                    try
                    {
                        bool statVcm = string.Compare(job.Item2, "STATUS_VCM_GET_FA", StringComparison.OrdinalIgnoreCase) == 0;

                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item3, out EdiabasNet.ResultData resultData))
                            {
                                if (!statVcm)
                                {
                                    string fa = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(fa))
                                    {
                                        _ediabas.ResolveSgbdFile("FA");

                                        _ediabas.ArgString = "1;" + fa;
                                        _ediabas.ArgBinaryStd = null;
                                        _ediabas.ResultsRequests = string.Empty;
                                        _ediabas.ExecuteJob("FA_STREAM2STRUCT");

                                        List<Dictionary<string, EdiabasNet.ResultData>> resultSetsFa = _ediabas.ResultSets;
                                        if (resultSetsFa != null && resultSetsFa.Count >= 2)
                                        {
                                            Dictionary<string, EdiabasNet.ResultData> resultDictFa = resultSetsFa[1];
                                            if (resultDictFa.TryGetValue("STANDARD_FA", out EdiabasNet.ResultData resultStdFa))
                                            {
                                                string stdFaStr = resultStdFa.OpData as string;
                                                if (!string.IsNullOrEmpty(stdFaStr))
                                                {
                                                    StandardFa = stdFaStr;
                                                    SetInfoFromStdFa(stdFaStr);
                                                }
                                            }

                                            if (resultDictFa.TryGetValue("BR", out EdiabasNet.ResultData resultDataBa))
                                            {
                                                string br = resultDataBa.OpData as string;
                                                if (!string.IsNullOrEmpty(br))
                                                {
                                                    log.InfoFormat(CultureInfo.InvariantCulture, "Detected BR: {0}", br);
                                                    string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                                                    if (!string.IsNullOrEmpty(vSeries))
                                                    {
                                                        log.InfoFormat(CultureInfo.InvariantCulture, "Detected vehicle series: {0}", vSeries);
                                                        ModelSeries = br;
                                                        Series = vSeries;
                                                    }
                                                }

                                                if (resultDictFa.TryGetValue("C_DATE", out EdiabasNet.ResultData resultDataCDate))
                                                {
                                                    string cDateStr = resultDataCDate.OpData as string;
                                                    DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                                    if (dateTime != null)
                                                    {
                                                        log.InfoFormat(CultureInfo.InvariantCulture, "Detected construction date: {0}",
                                                            dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                        SetConstructDate(dateTime);
                                                    }
                                                }

                                                if (resultDictFa.TryGetValue("LACK", out EdiabasNet.ResultData resultPaint))
                                                {
                                                    string paintStr = resultPaint.OpData as string;
                                                    if (!string.IsNullOrEmpty(paintStr))
                                                    {
                                                        Paint = paintStr;
                                                    }
                                                }

                                                if (resultDictFa.TryGetValue("POLSTER", out EdiabasNet.ResultData resultUpholstery))
                                                {
                                                    string upholsteryStr = resultUpholstery.OpData as string;
                                                    if (!string.IsNullOrEmpty(upholsteryStr))
                                                    {
                                                        Upholstery = upholsteryStr;
                                                    }
                                                }
                                            }

                                            if (Series != null)
                                            {
                                                SetFaSalpaInfo(resultDictFa);
                                                break;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    string br = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(br))
                                    {
                                        log.InfoFormat(CultureInfo.InvariantCulture, "Detected BR: {0}", br);
                                        string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                                        if (!string.IsNullOrEmpty(vSeries))
                                        {
                                            log.InfoFormat(CultureInfo.InvariantCulture, "Detected vehicle series: {0}", vSeries);
                                            ModelSeries = br;
                                            Series = vSeries;
                                        }

                                        if (resultDict.TryGetValue("STAT_ZEIT_KRITERIUM", out EdiabasNet.ResultData resultDataCDate))
                                        {
                                            string cDateStr = resultDataCDate.OpData as string;
                                            DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(cDateStr);
                                            if (dateTime != null)
                                            {
                                                log.InfoFormat(CultureInfo.InvariantCulture, "Detected construction date: {0}",
                                                    dateTime.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                                                SetConstructDate(dateTime);
                                            }
                                        }

                                        if (resultDict.TryGetValue("STAT_LACKCODE", out EdiabasNet.ResultData resultPaint))
                                        {
                                            string paintStr = resultPaint.OpData as string;
                                            if (!string.IsNullOrEmpty(paintStr))
                                            {
                                                Paint = paintStr;
                                            }
                                        }

                                        if (resultDict.TryGetValue("STAT_POLSTERCODE", out EdiabasNet.ResultData resultUpholstery))
                                        {
                                            string upholsteryStr = resultUpholstery.OpData as string;
                                            if (!string.IsNullOrEmpty(upholsteryStr))
                                            {
                                                Upholstery = upholsteryStr;
                                            }
                                        }

                                        if (resultDict.TryGetValue("STAT_TYP_SCHLUESSEL", out EdiabasNet.ResultData resultType))
                                        {
                                            string typeStr = resultType.OpData as string;
                                            if (!string.IsNullOrEmpty(typeStr))
                                            {
                                                TypeKey = typeStr;
                                            }
                                        }
                                    }

                                    if (Series != null)
                                    {
                                        SetStatVcmSalpaInfo(resultSets);
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "No BR response");
                        // ignored
                    }
                }

                VehicleStructsBmw.VersionInfo versionInfo = VehicleInfoBmw.GetVehicleSeriesInfoVersion();
                if (versionInfo == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Vehicle series no version info");
                    return DetectResult.InvalidDatabase;
                }

                PdszDatabase.DbInfo dbInfo = _pdszDatabase.GetDbInfo();
                if (dbInfo == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "DetectVehicleBmwFast no DbInfo");
                    return DetectResult.InvalidDatabase;
                }

                if (!versionInfo.IsMinVersion(dbInfo.Version, dbInfo.DateTime))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "DetectVehicleBmwFast Vehicles series too old");
                    return DetectResult.InvalidDatabase;
                }

                VehicleStructsBmw.VehicleSeriesInfo vehicleSeriesInfo = VehicleInfoBmw.GetVehicleSeriesInfo(Series, ConstructYear, ConstructMonth, _ediabas);
                if (vehicleSeriesInfo == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "Vehicle series info not found");
                    return DetectResult.InvalidDatabase;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Group SGBD: {0}", vehicleSeriesInfo.BrSgbd);
                GroupSgdb = vehicleSeriesInfo.BrSgbd;

                if (_abortRequest)
                {
                    return DetectResult.Aborted;
                }

                try
                {
                    _ediabas.ResolveSgbdFile(GroupSgdb);

                    _ediabas.ArgString = string.Empty;
                    _ediabas.ArgBinaryStd = null;
                    _ediabas.ResultsRequests = string.Empty;
                    _ediabas.ExecuteJob("IDENT_FUNKTIONAL");

                    EcuList.Clear();
                    resultSets = _ediabas.ResultSets;
                    if (resultSets != null && resultSets.Count >= 2)
                    {
                        int dictIndex = 0;
                        foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
                        {
                            if (dictIndex == 0)
                            {
                                dictIndex++;
                                continue;
                            }

                            string ecuName = string.Empty;
                            Int64 ecuAdr = -1;
                            string ecuDesc = string.Empty;
                            string ecuSgbd = string.Empty;
                            string ecuGroup = string.Empty;
                            // ReSharper disable once InlineOutVariableDeclaration
                            EdiabasNet.ResultData resultData;
                            if (resultDict.TryGetValue("ECU_GROBNAME", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    ecuName = (string)resultData.OpData;
                                }
                            }

                            if (resultDict.TryGetValue("ID_SG_ADR", out resultData))
                            {
                                if (resultData.OpData is Int64)
                                {
                                    ecuAdr = (Int64)resultData.OpData;
                                }
                            }

                            if (resultDict.TryGetValue("ECU_NAME", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    ecuDesc = (string)resultData.OpData;
                                }
                            }

                            if (resultDict.TryGetValue("ECU_SGBD", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    ecuSgbd = (string)resultData.OpData;
                                }
                            }

                            if (resultDict.TryGetValue("ECU_GRUPPE", out resultData))
                            {
                                if (resultData.OpData is string)
                                {
                                    ecuGroup = (string)resultData.OpData;
                                }
                            }

                            if (!string.IsNullOrEmpty(ecuName) && ecuAdr >= 0 && !string.IsNullOrEmpty(ecuSgbd))
                            {
                                PdszDatabase.EcuInfo ecuInfo = new PdszDatabase.EcuInfo(ecuName, ecuAdr, ecuDesc, ecuSgbd, ecuGroup);
                                EcuList.Add(ecuInfo);
                            }

                            dictIndex++;
                        }
                    }
                }
                catch (Exception)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "No ident response");
                    return DetectResult.NoResponse;
                }

                string iLevelShip = null;
                string iLevelCurrent = null;
                string iLevelBackup = null;
                foreach (Tuple<string, string> job in ReadILevelJobsBmwFast)
                {
                    if (_abortRequest)
                    {
                        return DetectResult.Aborted;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "Read ILevel job: {0},{1}", job.Item1, job.Item2);
                    if (invalidSgbdSet.Contains(job.Item1))
                    {
                        log.InfoFormat(CultureInfo.InvariantCulture, "Job ignored: {0}", job.Item1);
                        continue;
                    }

                    try
                    {
                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = string.Empty;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue("STAT_I_STUFE_WERK", out EdiabasNet.ResultData resultData))
                            {
                                string iLevel = resultData.OpData as string;
                                if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                    string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                {
                                    iLevelShip = iLevel;
                                    log.InfoFormat(CultureInfo.InvariantCulture, "Detected ILevel ship: {0}",
                                        iLevelShip);
                                }
                            }

                            if (!string.IsNullOrEmpty(iLevelShip))
                            {
                                if (resultDict.TryGetValue("STAT_I_STUFE_HO", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelCurrent = iLevel;
                                        log.InfoFormat(CultureInfo.InvariantCulture, "Detected ILevel current: {0}",
                                            iLevelCurrent);
                                    }
                                }

                                if (string.IsNullOrEmpty(iLevelCurrent))
                                {
                                    iLevelCurrent = iLevelShip;
                                }

                                if (resultDict.TryGetValue("STAT_I_STUFE_HO_BACKUP", out resultData))
                                {
                                    string iLevel = resultData.OpData as string;
                                    if (!string.IsNullOrEmpty(iLevel) && iLevel.Length >= 4 &&
                                        string.Compare(iLevel, VehicleInfoBmw.ResultUnknown, StringComparison.OrdinalIgnoreCase) != 0)
                                    {
                                        iLevelBackup = iLevel;
                                        log.InfoFormat(CultureInfo.InvariantCulture, "Detected ILevel backup: {0}",
                                            iLevelBackup);
                                    }
                                }

                                break;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "No ILevel response");
                        // ignored
                    }
                }

                if (string.IsNullOrEmpty(iLevelShip))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ILevel not found");
                    return DetectResult.NoResponse;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "ILevel: Ship={0}, Current={1}, Backup={2}", iLevelShip,
                    iLevelCurrent, iLevelBackup);

                ILevelShip = iLevelShip;
                ILevelCurrent = iLevelCurrent;
                ILevelBackup = iLevelBackup;

                if (_abortRequest)
                {
                    return DetectResult.Aborted;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "DetectVehicleBmwFast Finish");
                return DetectResult.Ok;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "DetectVehicleBmwFast Exception: {0}", ex.Message);
                return DetectResult.NoResponse;
            }
            finally
            {
                _abortFunc = null;
            }
        }

        public double ReadBatteryVoltage(AbortDelegate abortFunc)
        {
            double voltage = -1;

            try
            {
                _abortFunc = abortFunc;
                if (!Connect())
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ReadBatteryVoltage Connect failed");
                    return -1;
                }

                foreach (Tuple<string, string, string, string> job in ReadVoltageJobsBmwFast)
                {
                    if (_abortRequest)
                    {
                        return -1;
                    }

                    log.InfoFormat(CultureInfo.InvariantCulture, "Read voltage job: {0}, {1}, {2}", job.Item1,
                        job.Item2, job.Item3);

                    try
                    {
                        _ediabas.ResolveSgbdFile(job.Item1);

                        _ediabas.ArgString = job.Item3;
                        _ediabas.ArgBinaryStd = null;
                        _ediabas.ResultsRequests = string.Empty;
                        _ediabas.ExecuteJob(job.Item2);

                        List<Dictionary<string, EdiabasNet.ResultData>> resultSets = _ediabas.ResultSets;
                        if (resultSets != null && resultSets.Count >= 2)
                        {
                            Dictionary<string, EdiabasNet.ResultData> resultDict = resultSets[1];
                            if (resultDict.TryGetValue(job.Item4, out EdiabasNet.ResultData resultData))
                            {
                                if (resultData.OpData is Double)
                                {
                                    voltage = (double)resultData.OpData;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception)
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "No voltage response");
                        // ignored
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "ReadBatteryVoltage Exception: {0}", ex.Message);
                return -1;
            }
            finally
            {
                _abortFunc = null;
            }

            return voltage;
        }

        public string ExecuteContainerXml(AbortDelegate abortFunc, string configurationContainerXml, Dictionary<string,string> runOverrideDict = null)
        {
            string result = string.Empty;

            try
            {
                _abortFunc = abortFunc;
                if (!Connect())
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Connect failed");
                    return null;
                }

                ConfigurationContainer configurationContainer = ConfigurationContainer.Deserialize(configurationContainerXml);
                if (configurationContainer == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Deserialize failed");
                    return null;
                }

                if (runOverrideDict != null)
                {
                    foreach (KeyValuePair<string, string> runOverride in runOverrideDict)
                    {
                        configurationContainer.AddRunOverride(runOverride.Key, runOverride.Value);
                    }
                }

                EDIABASAdapter ediabasAdapter = new EDIABASAdapter(true, new ECUKom("DetectVehicle", _ediabas), configurationContainer);
                ediabasAdapter.DoParameterization();
                IDiagnosticDeviceResult diagnosticDeviceResult = ediabasAdapter.Execute(new ParameterContainer());
                if (diagnosticDeviceResult == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Execute failed");
                    return null;
                }

                if (diagnosticDeviceResult.Error != null && diagnosticDeviceResult.ECUJob != null && diagnosticDeviceResult.ECUJob.JobErrorCode != 0)
                {
                    string jobErrorText = diagnosticDeviceResult.ECUJob.JobErrorText;
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Job error: {0}", jobErrorText);
                    return jobErrorText;
                }

                bool jobOk = false;
                if (diagnosticDeviceResult.ECUJob != null && diagnosticDeviceResult.ECUJob.JobResultSets > 0)
                {
                    jobOk = diagnosticDeviceResult.ECUJob.IsOkay((ushort)diagnosticDeviceResult.ECUJob.JobResultSets);
                }

                if (!jobOk && diagnosticDeviceResult.ECUJob != null)
                {
                    string stringResult = diagnosticDeviceResult.ECUJob.getStringResult((ushort)diagnosticDeviceResult.ECUJob.JobResultSets, "JOB_STATUS");
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Job status: {0}", stringResult);
                    return stringResult;
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Job OK: {0}", jobOk);
                string jobStatus = diagnosticDeviceResult.getISTAResultAsType("/Result/Status/JOB_STATUS", typeof(string)) as string;
                if (jobStatus != null)
                {
                    result = jobStatus;
                    if (jobStatus != "OKAY")
                    {
                        log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Job status: {0}", jobStatus);
                        return jobStatus;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "ExecuteContainerXml Exception: {0}", ex.Message);
                return null;
            }
            finally
            {
                _abortFunc = null;
            }

            return result;
        }

        public static string ConvertContainerXml(string configurationContainerXml, Dictionary<string, string> runOverrideDict = null)
        {
            try
            {
                ConfigurationContainer configurationContainer = ConfigurationContainer.Deserialize(configurationContainerXml);
                if (configurationContainer == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ConvertContainerXml Deserialize failed");
                    return null;
                }

                if (runOverrideDict != null)
                {
                    foreach (KeyValuePair<string, string> runOverride in runOverrideDict)
                    {
                        configurationContainer.AddRunOverride(runOverride.Key, runOverride.Value);
                    }
                }

                EDIABASAdapter ediabasAdapter = new EDIABASAdapter(true, null, configurationContainer);
                ediabasAdapter.DoParameterization();
                if (string.IsNullOrEmpty(ediabasAdapter.EcuGroup))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ConvertContainerXml Empty EcuGroup");
                    return null;
                }

                if (string.IsNullOrEmpty(ediabasAdapter.EcuJob))
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ConvertContainerXml Empty EcuJob");
                    return null;
                }

                bool binMode = ediabasAdapter.IsBinModeRequired;
                if (binMode && ediabasAdapter.EcuData == null)
                {
                    log.ErrorFormat(CultureInfo.InvariantCulture, "ConvertContainerXml Binary data missing");
                    return null;
                }

                StringBuilder sb = new StringBuilder();
                sb.Append(ediabasAdapter.EcuGroup);
                sb.Append("#");
                sb.Append(ediabasAdapter.EcuJob);

                string paramString;
                if (binMode)
                {
                    paramString = "|" + BitConverter.ToString(ediabasAdapter.EcuData).Replace("-", "");
                }
                else
                {
                    paramString = ediabasAdapter.EcuParam;
                }

                string resultFilterString = ediabasAdapter.EcuResultFilter;
                if (!string.IsNullOrEmpty(paramString) || !string.IsNullOrEmpty(resultFilterString))
                {
                    sb.Append("#");
                    if (!string.IsNullOrEmpty(paramString))
                    {
                        sb.Append(paramString);
                    }

                    if (!string.IsNullOrEmpty(resultFilterString))
                    {
                        sb.Append("#");
                        sb.Append(resultFilterString);
                    }
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "ConvertContainerXml Exception: {0}", ex.Message);
                return null;
            }
        }

        public bool Connect()
        {
            try
            {
                return _ediabas.EdInterfaceClass.InterfaceConnect();
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                return _ediabas.EdInterfaceClass.InterfaceDisconnect();
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool AbortEdiabasJob()
        {
            if (_abortFunc != null)
            {
                if (_abortFunc.Invoke())
                {
                    _abortRequest = true;
                }
            }

            return _abortRequest;
        }

        public string GetFaInfo()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in Salapa)
            {
                sb.Append("$");
                sb.Append(item);
            }

            foreach (string item in EWords)
            {
                sb.Append("-");
                sb.Append(item);
            }

            foreach (string item in HoWords)
            {
                sb.Append("+");
                sb.Append(item);
            }

            foreach (string item in ZbWords)
            {
                sb.Append(";");
                sb.Append(item);
            }

            return sb.ToString();
        }

        private void ResetValues()
        {
            _abortRequest = false;
            _abortFunc = null;
            EcuList.Clear();
            Vin = null;
            TypeKey = null;
            GroupSgdb = null;
            ModelSeries = null;
            Series = null;
            ConstructDate = null;
            ConstructYear = null;
            ConstructMonth = null;
            Paint = null;
            Upholstery = null;
            StandardFa = null;
            Salapa = new List<string>();
            HoWords = new List<string>();
            EWords = new List<string>();
            ZbWords = new List<string>();
        }

        private void SetConstructDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return;
            }

            ConstructDate = dateTime.Value;
            ConstructYear = dateTime.Value.ToString("yyyy", CultureInfo.InvariantCulture);
            ConstructMonth = dateTime.Value.ToString("MM", CultureInfo.InvariantCulture);
        }

        private void SetStatVcmSalpaInfo(List<Dictionary<string, EdiabasNet.ResultData>> resultSets)
        {
            int dictIndex = 0;
            foreach (Dictionary<string, EdiabasNet.ResultData> resultDict in resultSets)
            {
                if (dictIndex == 0)
                {
                    dictIndex++;
                    continue;
                }

                if (resultDict.TryGetValue("STAT_SALAPA", out EdiabasNet.ResultData resultDataSa))
                {
                    string saStr = resultDataSa.OpData as string;
                    if (!string.IsNullOrEmpty(saStr))
                    {
                        if (!Salapa.Contains(saStr))
                        {
                            Salapa.Add(saStr);
                        }
                    }
                }

                if (resultDict.TryGetValue("STAT_HO_WORTE", out EdiabasNet.ResultData resultDataHo))
                {
                    string hoStr = resultDataHo.OpData as string;
                    if (!string.IsNullOrEmpty(hoStr))
                    {
                        if (!HoWords.Contains(hoStr))
                        {
                            HoWords.Add(hoStr);
                        }
                    }
                }

                if (resultDict.TryGetValue("STAT_E_WORTE", out EdiabasNet.ResultData resultDataEw))
                {
                    string ewStr = resultDataEw.OpData as string;
                    if (!string.IsNullOrEmpty(ewStr))
                    {
                        if (!EWords.Contains(ewStr))
                        {
                            EWords.Add(ewStr);
                        }
                    }
                }

                dictIndex++;
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "Detected FA: {0}", GetFaInfo());
        }

        private void SetFaSalpaInfo(Dictionary<string, EdiabasNet.ResultData> resultDict)
        {
            if (resultDict.TryGetValue("SA_ANZ", out EdiabasNet.ResultData resultDataSaCount))
            {
                Int64? saCount = resultDataSaCount.OpData as Int64?;
                if (saCount != null)
                {
                    for (int index = 0; index < saCount.Value; index++)
                    {
                        string saName = string.Format(CultureInfo.InvariantCulture, "SA_{0}", index + 1);
                        if (resultDict.TryGetValue(saName, out EdiabasNet.ResultData resultDataSa))
                        {
                            string saStr = resultDataSa.OpData as string;
                            if (!string.IsNullOrEmpty(saStr))
                            {
                                if (!Salapa.Contains(saStr))
                                {
                                    Salapa.Add(saStr);
                                }
                            }
                        }
                    }
                }
            }

            if (resultDict.TryGetValue("HO_WORT_ANZ", out EdiabasNet.ResultData resultDataHoCount))
            {
                Int64? haCount = resultDataHoCount.OpData as Int64?;
                if (haCount != null)
                {
                    for (int index = 0; index < haCount.Value; index++)
                    {
                        string hoName = string.Format(CultureInfo.InvariantCulture, "HO_WORT_{0}", index + 1);
                        if (resultDict.TryGetValue(hoName, out EdiabasNet.ResultData resultDataHo))
                        {
                            string hoStr = resultDataHo.OpData as string;
                            if (!string.IsNullOrEmpty(hoStr))
                            {
                                if (!HoWords.Contains(hoStr))
                                {
                                    HoWords.Add(hoStr);
                                }
                            }
                        }
                    }
                }
            }

            if (resultDict.TryGetValue("E_WORT_ANZ", out EdiabasNet.ResultData resultDataEwCount))
            {
                Int64? ewCount = resultDataEwCount.OpData as Int64?;
                if (ewCount != null)
                {
                    for (int index = 0; index < ewCount.Value; index++)
                    {
                        string ewName = string.Format(CultureInfo.InvariantCulture, "E_WORT_{0}", index + 1);
                        if (resultDict.TryGetValue(ewName, out EdiabasNet.ResultData resultDataEw))
                        {
                            string ewStr = resultDataEw.OpData as string;
                            if (!string.IsNullOrEmpty(ewStr))
                            {
                                if (!EWords.Contains(ewStr))
                                {
                                    EWords.Add(ewStr);
                                }
                            }
                        }
                    }
                }
            }

            if (resultDict.TryGetValue("ZUSBAU_ANZ", out EdiabasNet.ResultData resultDataZbCount))
            {
                Int64? zbCount = resultDataZbCount.OpData as Int64?;
                if (zbCount != null)
                {
                    for (int index = 0; index < zbCount.Value; index++)
                    {
                        string zbName = string.Format(CultureInfo.InvariantCulture, "ZUSBAU_{0}", index + 1);
                        if (resultDict.TryGetValue(zbName, out EdiabasNet.ResultData resultDataEw))
                        {
                            string zbStr = resultDataEw.OpData as string;
                            if (!string.IsNullOrEmpty(zbStr))
                            {
                                if (!ZbWords.Contains(zbStr))
                                {
                                    ZbWords.Add(zbStr);
                                }
                            }
                        }
                    }
                }
            }

            log.InfoFormat(CultureInfo.InvariantCulture, "Detected FA: {0}", GetFaInfo());
        }

        private bool SetInfoFromStdFa(string stdFaStr)
        {
            if (string.IsNullOrEmpty(stdFaStr))
            {
                return false;
            }

            try
            {
                string vehicleType = VehicleInfoBmw.GetVehicleTypeFromStdFa(stdFaStr, _ediabas);
                if (!string.IsNullOrEmpty(vehicleType))
                {
                    TypeKey = vehicleType;
                }

                Match matchBr = Regex.Match(stdFaStr, "^(?<BR>\\w+)[^\\w]");
                if (!matchBr.Success)
                {
                    matchBr = Regex.Match(stdFaStr, "(?<BR>\\w{4})[^\\w]");
                }

                if (matchBr.Success)
                {
                    string br = matchBr.Groups["BR"]?.Value;
                    if (!string.IsNullOrEmpty(br))
                    {
                        string vSeries = VehicleInfoBmw.GetVehicleSeriesFromBrName(br, _ediabas);
                        if (!string.IsNullOrEmpty(vSeries))
                        {
                            log.InfoFormat(CultureInfo.InvariantCulture, "Detected vehicle series: {0}", vSeries);
                            ModelSeries = br;
                            Series = vSeries;
                        }
                    }
                }

                Match matchDate = Regex.Match(stdFaStr, "#(?<C_DATE>\\d{4})");
                if (matchDate.Success)
                {
                    string dateStr = matchDate.Groups["C_DATE"]?.Value;
                    if (!string.IsNullOrEmpty(dateStr))
                    {
                        DateTime? dateTime = VehicleInfoBmw.ConvertConstructionDate(dateStr);
                        SetConstructDate(dateTime);
                    }
                }

                Match matchPaint = Regex.Match(stdFaStr, "%(?<LACK>\\w{4})");
                if (matchPaint.Success)
                {
                    string paintStr = matchPaint.Groups["LACK"]?.Value;
                    if (!string.IsNullOrEmpty(paintStr))
                    {
                        Paint = paintStr;
                    }
                }

                Match matchUpholstery = Regex.Match(stdFaStr, "&(?<POLSTER>\\w{4})");
                if (matchUpholstery.Success)
                {
                    string upholsteryStr = matchUpholstery.Groups["POLSTER"]?.Value;
                    if (!string.IsNullOrEmpty(upholsteryStr))
                    {
                        Upholstery = upholsteryStr;
                    }
                }

                foreach (Match match in Regex.Matches(stdFaStr, "\\$(?<SA>\\w{3})"))
                {
                    if (match.Success)
                    {
                        string saStr = match.Groups["SA"]?.Value;
                        if (!string.IsNullOrEmpty(saStr))
                        {
                            if (!Salapa.Contains(saStr))
                            {
                                Salapa.Add(saStr);
                            }
                        }
                    }
                }

                foreach (Match match in Regex.Matches(stdFaStr, "\\+(?<HOWORT>\\w{4})"))
                {
                    if (match.Success)
                    {
                        string hoStr = match.Groups["HOWORT"]?.Value;
                        if (!string.IsNullOrEmpty(hoStr))
                        {
                            if (!HoWords.Contains(hoStr))
                            {
                                HoWords.Add(hoStr);
                            }
                        }
                    }
                }

                foreach (Match match in Regex.Matches(stdFaStr, "\\-(?<EWORT>\\w{4})"))
                {
                    if (match.Success)
                    {
                        string ewStr = match.Groups["EWORT"]?.Value;
                        if (!string.IsNullOrEmpty(ewStr))
                        {
                            if (!EWords.Contains(ewStr))
                            {
                                EWords.Add(ewStr);
                            }
                        }
                    }
                }

                log.InfoFormat(CultureInfo.InvariantCulture, "Detected FA: {0}", GetFaInfo());
                return true;
            }
            catch (Exception ex)
            {
                log.ErrorFormat(CultureInfo.InvariantCulture, "SetInfoFromStdFa Exception: {0}", ex.Message);
                return false;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            // Check to see if Dispose has already been called.
            if (!_disposed)
            {
                if (_ediabas != null)
                {
                    _ediabas.Dispose();
                    _ediabas = null;
                }

                // If disposing equals true, dispose all managed
                // and unmanaged resources.
                if (disposing)
                {
                }

                // Note disposing has been done.
                _disposed = true;
            }
        }
    }
}
