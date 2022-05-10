﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BMW.Rheingold.CoreFramework.Contracts.Vehicle;
using PsdzClient.Core;
using PsdzClientLibrary;

namespace PsdzClient.Core
{
    public class VehicleLogistics
    {
        public class E46EcuCharacteristics : BaseEcuCharacteristics { }
        public class E36EcuCharacteristics : BaseEcuCharacteristics { }
        public class E39EcuCharacteristics : BaseEcuCharacteristics { }
        public class E38EcuCharacteristics : BaseEcuCharacteristics { }
        public class E52EcuCharacteristics : BaseEcuCharacteristics { }
        public class E53EcuCharacteristics : BaseEcuCharacteristics { }
        public class F01EcuCharacteristics : BaseEcuCharacteristics { }
        public class F25_1404EcuCharacteristics : BaseEcuCharacteristics { }
        public class F25EcuCharacteristics : BaseEcuCharacteristics { }
        public class R50EcuCharacteristics : BaseEcuCharacteristics { }
        public class RR6EcuCharacteristics : BaseEcuCharacteristics { }
        public class R55EcuCharacteristics : BaseEcuCharacteristics { }
        public class RR2EcuCharacteristics : BaseEcuCharacteristics { }
        public class RREcuCharacteristics : BaseEcuCharacteristics { }
        public class BNT_G11_G12_G3X_SP2015 : BaseEcuCharacteristics { }
        public class MRXEcuCharacteristics : BaseEcuCharacteristics { }
        public class MREcuCharacteristics : BaseEcuCharacteristics { }
        public class E70EcuCharacteristicsAMPT : BaseEcuCharacteristics { }
        public class E70EcuCharacteristicsAMPH : BaseEcuCharacteristics { }
        public class E70EcuCharacteristics : BaseEcuCharacteristics { }
        public class E60EcuCharacteristics : BaseEcuCharacteristics { }
        public class E83EcuCharacteristics : BaseEcuCharacteristics { }
        public class E85EcuCharacteristics : BaseEcuCharacteristics { }
        public class F15EcuCharacteristics : BaseEcuCharacteristics { }
        public class F01_1307EcuCharacteristics : BaseEcuCharacteristics { }
        public class BNT_G01_G02_G08_F97_F98_SP2015 : BaseEcuCharacteristics { }
        public class E89EcuCharacteristics : BaseEcuCharacteristics { }
        public class F56EcuCharacteristics : BaseEcuCharacteristics { }
        public class F20EcuCharacteristics : BaseEcuCharacteristics { }

        private static ConcurrentDictionary<object, BaseEcuCharacteristics> ecuCharacteristics;

        static VehicleLogistics()
        {
            ecuCharacteristics = new ConcurrentDictionary<object, BaseEcuCharacteristics>();
        }

		private static BaseEcuCharacteristics GetEcuCharacteristics(string storedXmlFileName, Vehicle vecInfo)
        {
            return GetEcuCharacteristics<GenericEcuCharacteristics>(storedXmlFileName, vecInfo);
        }

        private static BaseEcuCharacteristics GetEcuCharacteristics<T>(string storedXmlFileName, Vehicle vecInfo) where T : BaseEcuCharacteristics
		{
            PdszDatabase database = ClientContext.GetDatabase(vecInfo);
            if (database == null)
            {
                return null;
            }

            string xml = database.GetBordnetXmlFromDatabase(vecInfo);
            if (!string.IsNullOrWhiteSpace(xml))
            {
                BaseEcuCharacteristics baseEcuCharacteristics = CreateCharacteristicsInstance<GenericEcuCharacteristics>(vecInfo, xml, storedXmlFileName);
                if (baseEcuCharacteristics != null)
                {
                    return baseEcuCharacteristics;
                }
            }

            if (!string.IsNullOrEmpty(storedXmlFileName))
            {
                xml = database.GetEcuCharacteristicsXml(storedXmlFileName);
                if (!string.IsNullOrWhiteSpace(xml))
                {
                    BaseEcuCharacteristics baseEcuCharacteristics = CreateCharacteristicsInstance<GenericEcuCharacteristics>(vecInfo, xml, storedXmlFileName);
                    if (baseEcuCharacteristics != null)
                    {
                        return baseEcuCharacteristics;
                    }
                }
			}

			return null;
        }

        private static BaseEcuCharacteristics CreateCharacteristicsInstance<T>(Vehicle vehicle, string xml, string name) where T : BaseEcuCharacteristics
        {
            try
            {
                Type typeFromHandle = typeof(T);
                object[] args = new string[1] { xml };
                T val = (T)Activator.CreateInstance(typeFromHandle, args);
                if (val != null)
                {
                    ecuCharacteristics.TryAdd(vehicle.GetCustomHashCode(), val);
                    val.BordnetName = name;
                    //Log.Info(Log.CurrentMethod(), "Found characteristics with group sgdb: " + val.brSgbd);
                }
                return val;
            }
            catch (Exception)
            {
                //Log.ErrorException(Log.CurrentMethod(), exception);
                return null;
            }
        }

		// ToDo: Check on update
		public static BaseEcuCharacteristics GetCharacteristics(Vehicle vecInfo)
		{
			int customHashCode = vecInfo.GetCustomHashCode();
            if (ecuCharacteristics.ContainsKey(customHashCode))
            {
                return ecuCharacteristics[customHashCode];
            }

			if (!string.IsNullOrEmpty(vecInfo.Baureihenverbund))
			{
				switch (vecInfo.Baureihenverbund.ToUpper())
				{
					case "K001":
					case "KS01":
					case "KE01":
						return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
					case "K024":
					case "KH24":
						return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
					case "K01X":
						return GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
				}
			}
			if (!string.IsNullOrEmpty(vecInfo.Ereihe))
			{
				switch (vecInfo.Ereihe.ToUpper())
				{
					case "M12":
						return GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
					case "E39":
						return GetEcuCharacteristics<E39EcuCharacteristics>("BNT-XML-E39.xml", vecInfo);
					case "K17":
					case "K07":
						return GetEcuCharacteristics("MRKE01EcuCharacteristics.xml", vecInfo);
					case "E38":
						return GetEcuCharacteristics<E38EcuCharacteristics>("BNT-XML-E38.xml", vecInfo);
					case "E53":
						return GetEcuCharacteristics<E53EcuCharacteristics>("BNT-XML-E53.xml", vecInfo);
					case "E36":
						return GetEcuCharacteristics<E36EcuCharacteristics>("BNT-XML-E36.xml", vecInfo);
					case "E46":
						return GetEcuCharacteristics<E46EcuCharacteristics>("BNT-XML-E46.xml", vecInfo);
					case "E52":
						return GetEcuCharacteristics<E52EcuCharacteristics>("BNT-XML-E52.xml", vecInfo);
					case "I20":
						if (vecInfo.HasMrr30())
						{
							return GetEcuCharacteristics("BNT-XML-I20_FRS.xml", vecInfo);
						}
						if (vecInfo.HasFrr30v())
						{
							return GetEcuCharacteristics("BNT-XML-I20_FRSF.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_I20.xml", vecInfo);
					case "F01BN2K":
						return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
					case "I15":
					case "I12":
						return GetEcuCharacteristics("BNT-XML-I12_I15.xml", vecInfo);
					case "R50":
					case "R52":
					case "R53":
						return GetEcuCharacteristics<R50EcuCharacteristics>("BNT-XML-R50.xml", vecInfo);
					case "F25":
						if (vecInfo.C_DATETIME.HasValue && !(vecInfo.C_DATETIME < BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.DTimeF25Lci))
						{
							return GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
						}
						return GetEcuCharacteristics<F25EcuCharacteristics>("BNT-XML-F25.xml", vecInfo);
					case "F56":
						if (vecInfo.IsBev())
						{
							return GetEcuCharacteristics("BNT-XML-F56BEV.xml", vecInfo);
						}
						return GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
					case "F26":
						return GetEcuCharacteristics<F25_1404EcuCharacteristics>("BNT-XML-F25_1404.xml", vecInfo);
					case "RR5":
					case "RR4":
					case "RR6":
						return GetEcuCharacteristics<RR6EcuCharacteristics>("BNT-XML-RR6.xml", vecInfo);
					case "F44":
					case "F40":
						return GetEcuCharacteristics("BNT-XML-F40_F44.xml", vecInfo);
					case "U12":
					case "U06":
					case "U11":
					case "U10":
						if (vecInfo.HasMrr30())
						{
							if (!vecInfo.IsPhev())
							{
								return GetEcuCharacteristics("BNT-XML-U06_FRS.xml", vecInfo);
							}
							return GetEcuCharacteristics("BNT-XML-U06_PHEV_FRS.xml", vecInfo);
						}
						if (vecInfo.HasFrr30v())
						{
							if (!vecInfo.IsPhev())
							{
								return GetEcuCharacteristics("BNT-XML-U06_FRSF.xml", vecInfo);
							}
							return GetEcuCharacteristics("BNT-XML-U06_PHEV_FRSF.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_U06-Fallback.xml", vecInfo);
					case "RR1":
					case "RR3":
					case "RR2":
						if (vecInfo.C_DATETIME.HasValue && !(vecInfo.C_DATETIME < BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.DTimeRR_S2))
						{
							return GetEcuCharacteristics<RR2EcuCharacteristics>("BNT-XML-RR2.xml", vecInfo);
						}
						return GetEcuCharacteristics<RREcuCharacteristics>("BNT-XML-RR.xml", vecInfo);
					case "R55":
					case "R56":
					case "R57":
					case "R58":
					case "R59":
					case "R61":
					case "R60":
						return GetEcuCharacteristics<R55EcuCharacteristics>("BNT-XML-R55.xml", vecInfo);
					case "G08":
						if (vecInfo.IsBev())
						{
							return GetEcuCharacteristics("BNT-XML-G08BEV.xml", vecInfo);
						}
						if (vecInfo.HasEnavevo())
						{
							return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
						}
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo);
						}
						return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
					case "G83":
					case "G82":
					case "G81":
					case "G80":
					case "G87":
						if (!vecInfo.IsPhev() && vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
					case "J29":
						return GetEcuCharacteristics("BNT-XML-J29.xml", vecInfo);
					case "G38":
					case "G31":
					case "G32":
					case "G30":
						if (vecInfo.HasEnavevoOrNbtevo())
						{
							return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_NOMGU.xml", vecInfo);
						}
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
					case "K19":
					case "K18":
					case "K21":
						if (getBNType(vecInfo) == BNType.BN2000_MOTORBIKE)
						{
							return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
						}
						return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
					case "E61":
					case "E60":
					case "E63":
					case "E64":
						return GetEcuCharacteristics<E60EcuCharacteristics>("BNT-XML-E60.xml", vecInfo);
					case "E71":
					case "E70":
						if (vecInfo.HasAmpt70())
						{
							return GetEcuCharacteristics<E70EcuCharacteristicsAMPT>("BNT-XML-E70-AMPH70_AMPT70.xml", vecInfo);
						}
						if (vecInfo.HasAmph70())
						{
							return GetEcuCharacteristics<E70EcuCharacteristicsAMPH>("BNT-XML-E70-AMPH70_AMPT70.xml", vecInfo);
						}
						return GetEcuCharacteristics<E70EcuCharacteristics>("BNT-XML-E70_NOAMPT_NOAMPH.xml", vecInfo);
					case "K46":
						if (getBNType(vecInfo) == BNType.BN2020_MOTORBIKE)
						{
							return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
						}
						return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
					case "V98":
					case "MRK24":
					case "A67":
					case "K29":
					case "K28":
					case "K75":
					case "K73":
					case "K72":
					case "K71":
					case "K70":
					case "K42":
					case "K43":
					case "K40":
					case "K25":
					case "K26":
					case "K27":
					case "K44":
						return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
					case "E72":
						return GetEcuCharacteristics("BNT-XML-E72.xml", vecInfo);
					case "E66":
					case "E67":
					case "E65":
					case "E68":
						return GetEcuCharacteristics("BNT-XML-E65.xml", vecInfo);
					case "E83":
						return GetEcuCharacteristics<E83EcuCharacteristics>("BNT-XML-E83.xml", vecInfo);
					case "E85":
					case "E86":
						return GetEcuCharacteristics<E85EcuCharacteristics>("BNT-XML-E85.xml", vecInfo);
					case "E88":
					case "E89":
					case "E81":
					case "E82":
					case "E84":
					case "E93":
					case "E92":
					case "E91":
					case "E87":
					case "E90":
						_ = vecInfo.Baureihenverbund == "E89X";
						return GetEcuCharacteristics<E89EcuCharacteristics>("BNT-XML-E89.xml", vecInfo);
					case "H61":
					case "H91":
						return GetEcuCharacteristics("H61EcuCharacteristics.xml", vecInfo);
					case "I01":
						return GetEcuCharacteristics("BNT-XML-I01.xml", vecInfo);
					case "G11":
					case "G12":
					case "F90":
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
						}
						if (vecInfo.HasNbtevo())
						{
							return GetEcuCharacteristics<BNT_G11_G12_G3X_SP2015>("BNT-XML-G11_G12_G3X_SP2015.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
					case "K66":
					case "K67":
					case "K60":
					case "K61":
					case "K69":
					case "K63":
					case "K09":
					case "K08":
					case "K03":
					case "K02":
					case "V99":
					case "K84":
					case "K82":
					case "K83":
					case "K48":
					case "K80":
					case "K81":
					case "K49":
					case "K22":
					case "K33":
					case "K23":
					case "K32":
					case "K54":
					case "K47":
					case "K51":
					case "K35":
					case "K50":
					case "K34":
					case "K52":
					case "K53":
					case "X_K001":
						return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
					case "G14":
					case "G15":
					case "G16":
					case "F92":
					case "F91":
					case "F93":
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G1X_G3X_SP2018_MGU.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_G1X_G3X_SP2018.xml", vecInfo);
					case "G06":
					case "G07":
					case "G05":
					case "G18":
					case "F95":
					case "F96":
						return GetEcuCharacteristics("BNT-XML-G05_G06_G07.xml", vecInfo);
					case "G02":
					case "G01":
					case "F97":
					case "F98":
						if (vecInfo.HasEnavevoOrNbtevo())
						{
							return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
						}
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G01_G02_G08_F97_F98_SP2018.xml", vecInfo);
						}
						return GetEcuCharacteristics<BNT_G01_G02_G08_F97_F98_SP2015>("BNT-XML-G01_G02_G08_F97_F98_SP2015.xml", vecInfo);
					case "F86":
					case "F85":
					case "F15":
					case "F14":
					case "F16":
						return GetEcuCharacteristics<F15EcuCharacteristics>("BNT-XML-F15.xml", vecInfo);
					case "F10":
					case "F18":
					case "F12":
					case "F11":
					case "F03":
					case "F13":
					case "F02":
					case "F01":
					case "F06":
					case "F07":
					case "F04":
						if (vecInfo.C_DATETIME.HasValue && !(vecInfo.C_DATETIME < BMW.Rheingold.DiagnosticsBusinessData.DiagnosticsBusinessData.DTimeF01Lci))
						{
							if (vecInfo.ECU != null)
							{
								ECU eCU = vecInfo.getECU(16L);
								if (eCU != null && eCU.SubBUS != null && eCU.SubBUS.Contains(BusType.MOST))
								{
									return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
								}
							}
							return GetEcuCharacteristics<F01_1307EcuCharacteristics>("BNT-XML-F01_1307.xml", vecInfo);
						}
						return GetEcuCharacteristics<F01EcuCharacteristics>("BNT-XML-F01.xml", vecInfo);
					case "M13":
					case "F54":
					case "F55":
					case "F57":
					case "F47":
					case "F46":
					case "F45":
					case "F52":
					case "F49":
					case "F48":
					case "F39":
					case "F60":
						return GetEcuCharacteristics<F56EcuCharacteristics>("BNT-XML-F56.xml", vecInfo);
					case "F21":
					case "F23":
					case "F20":
					case "F22":
					case "F83":
					case "F82":
					case "F80":
					case "F81":
					case "F87":
					case "F36":
					case "F35":
					case "F34":
					case "F32":
					case "F33":
					case "F31":
					case "F30":
						return GetEcuCharacteristics<F20EcuCharacteristics>("BNT-XML-F20.xml", vecInfo);
					case "K14":
					case "K15":
					case "K16":
					case "259":
					case "247":
					case "K599":
					case "248":
					case "259R":
					case "259S":
					case "R21":
					case "259C":
					case "R22":
					case "259E":
					case "R28":
					case "K41":
					case "K30":
					case "K569":
					case "C01":
					case "K589":
					case "247E":
					case "E189":
					case "E169":
					case "R13":
						return GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
					case "RR11":
					case "RR12":
					case "RR31":
					case "RR21":
					case "RR22":
						return GetEcuCharacteristics("BNT-XML-RR1X_RR3X_RRNM.xml", vecInfo);
					case "G28":
					case "G26":
						if (vecInfo.IsBev())
						{
							return GetEcuCharacteristics("BNT-XML-G26_G28_BEV.xml", vecInfo);
						}
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
						}
						if (vecInfo.HasEnavevo())
						{
							return GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
					case "G29":
						return GetEcuCharacteristics("BNT-XML-G29.xml", vecInfo);
					case "G21":
					case "G20":
						if (vecInfo.IsPhev())
						{
							return GetEcuCharacteristics("BNT-XML-G20_G21_PHEV.xml", vecInfo);
						}
						if (vecInfo.HasHuMgu())
						{
							return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
						}
						if (vecInfo.HasEnavevo())
						{
							return GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
						}
						return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
					case "G42":
					case "G23":
					case "G22":
						if (!vecInfo.IsPhev())
						{
							if (vecInfo.HasHuMgu())
							{
								return GetEcuCharacteristics("BNT-XML-G20_G28_MGU.xml", vecInfo);
							}
							if (vecInfo.HasEnavevo())
							{
								return GetEcuCharacteristics("BNT-XML-G20_G28_NOMGU.xml", vecInfo);
							}
						}
						return GetEcuCharacteristics("BNT_G20_G28.xml", vecInfo);
				}
				//Log.Info("VehicleLogistics.GetCharacteristics()", "cannot retrieve bordnet configuration using ereihe");
			}
			switch (vecInfo.BNType)
			{
				case BNType.IBUS:
					return GetEcuCharacteristics("iBusEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_MOTORBIKE:
					return GetEcuCharacteristics<MREcuCharacteristics>("BNT-XML-BIKE-K024.xml", vecInfo);
				case BNType.BN2020_MOTORBIKE:
					return GetEcuCharacteristics<MRXEcuCharacteristics>("BNT-XML-BIKE-K001.xml", vecInfo);
				case BNType.BNK01X_MOTORBIKE:
					return GetEcuCharacteristics("BNT-XML-BIKE-K01X.xml", vecInfo);
				default:
                    return GetEcuCharacteristics(string.Empty, vecInfo);
				case BNType.BN2000_WIESMANN:
					return GetEcuCharacteristics("WiesmannEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_RODING:
					return GetEcuCharacteristics("RodingEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_PGO:
					return GetEcuCharacteristics("PGOEcuCharacteristics.xml", vecInfo);
				case BNType.BN2000_GIBBS:
					return GetEcuCharacteristics("GibbsEcuCharacteristics.xml", vecInfo);
				case BNType.BN2020_CAMPAGNA:
					return GetEcuCharacteristics("CampagnaEcuCharacteristics.xml", vecInfo);
			}
		}

		// ToDo: Check on update
		public static BNType getBNType(Vehicle vecInfo)
		{
			if (vecInfo == null)
			{
				//Log.Warning("VehicleLogistics.getBNType()", "vehicle was null");
				return BNType.UNKNOWN;
			}
			if (string.IsNullOrEmpty(vecInfo.Ereihe))
			{
				return BNType.UNKNOWN;
			}
			switch (vecInfo.Ereihe.ToUpper())
			{
				case "M12":
					return BNType.BEV2010;
				case "K19":
					if (!string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0C05") || vecInfo.VINType.Equals("0C15")))
					{
						return BNType.BN2020_MOTORBIKE;
					}
					return BNType.BN2000_MOTORBIKE;
				case "K18":
					if (!string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0C04") || vecInfo.VINType.Equals("0C14")))
					{
						return BNType.BN2020_MOTORBIKE;
					}
					return BNType.BN2000_MOTORBIKE;
				case "V99":
					return BNType.BN2020_CAMPAGNA;
				case "V98":
					return BNType.BN2000_MOTORBIKE;
				case "RODING_ROADSTER":
					return BNType.BN2000_RODING;
				case "N18":
					return BNType.BN2000_PGO;
				case "RR5":
				case "RR4":
				case "RR6":
					return BNType.BN2020;
				case "U06":
				case "U11":
					return BNType.BN2020;
				case "R55":
				case "R56":
				case "R57":
				case "R58":
				case "R59":
				case "R60":
				case "R61":
					return BNType.BN2000;
				case "RR1":
				case "RR3":
				case "RR2":
					return BNType.BN2000;
				case "K21":
					if (!string.IsNullOrEmpty(vecInfo.VINType) && (vecInfo.VINType.Equals("0A06") || vecInfo.VINType.Equals("0A16")))
					{
						return BNType.BN2000_MOTORBIKE;
					}
					return BNType.BN2020_MOTORBIKE;
				case "K46":
					if (!string.IsNullOrEmpty(vecInfo.VINType) && !"XXXX".Equals(vecInfo.VINType, StringComparison.OrdinalIgnoreCase) && (vecInfo.VINType.Equals("0D10") || vecInfo.VINType.Equals("0D21") || vecInfo.VINType.Equals("0D30") || vecInfo.VINType.Equals("0D40") || vecInfo.VINType.Equals("0D50") || vecInfo.VINType.Equals("0D60") || vecInfo.VINType.Equals("0D70") || vecInfo.VINType.Equals("0D80") || vecInfo.VINType.Equals("0D90")))
					{
						return BNType.BN2020_MOTORBIKE;
					}
					return BNType.BN2000_MOTORBIKE;
				case "K17":
				case "K66":
				case "K60":
				case "K67":
				case "K61":
				case "K63":
				case "K69":
				case "K08":
				case "K09":
				case "K02":
				case "K07":
				case "K03":
				case "K84":
				case "K82":
				case "K83":
				case "K48":
				case "K80":
				case "K49":
				case "K81":
				case "K22":
				case "K33":
				case "K23":
				case "K32":
				case "K54":
				case "K47":
				case "K51":
				case "K35":
				case "K53":
				case "K50":
				case "K34":
				case "K52":
					return BNType.BN2020_MOTORBIKE;
				case "E39":
				case "E38":
				case "E46":
				case "E53":
				case "E36":
				case "E52":
				case "R50":
				case "R52":
				case "R53":
				case "E83":
				case "E85":
				case "E86":
					return BNType.IBUS;
				case "E63":
				case "E62":
				case "E61":
				case "E60":
				case "E66":
				case "E67":
				case "E64":
				case "E88":
				case "E71":
				case "E65":
				case "E89":
				case "E70":
				case "E68":
				case "E72":
				case "E81":
				case "E82":
				case "E92":
				case "E84":
				case "E93":
				case "E91":
				case "E90":
				case "E87":
					return BNType.BN2000;
				case "AERO":
					return BNType.BN2000_MORGAN;
				case "K599":
				case "247":
				case "248":
				case "259":
				case "259R":
				case "259S":
				case "R21":
				case "259C":
				case "R22":
				case "259E":
				case "R28":
				case "K41":
				case "K30":
				case "K569":
				case "247E":
				case "K589":
				case "E169":
				case "E189":
					return BNType.BNK01X_MOTORBIKE;
				case "G14":
				case "G11":
				case "G38":
				case "G12":
				case "RR12":
				case "RR11":
				case "G31":
				case "G32":
				case "G30":
				case "RR31":
				case "RR21":
				case "RR22":
					return BNType.BN2020;
				case "K14":
				case "K15":
				case "K16":
				case "A67":
				case "K28":
				case "K75":
				case "K29":
				case "K73":
				case "K72":
				case "K71":
				case "K70":
				case "K42":
				case "K43":
				case "K40":
				case "K25":
				case "K26":
				case "K27":
				case "H61":
				case "K44":
				case "C01":
				case "H91":
				case "R13":
					return BNType.BN2000_MOTORBIKE;
				case "GT1":
					return BNType.BN2000_GIBBS;
				case "MF30":
				case "MF25":
				case "MF35":
				case "MF3":
				case "MF4":
				case "MF5":
				case "MF4-S":
				case "MF28":
					return BNType.BN2000_WIESMANN;
				case "M13":
				case "I20":
				case "I15":
				case "I12":
				case "F20":
				case "F21":
				case "F23":
				case "F54":
				case "F22":
				case "F25":
				case "F56":
				case "F55":
				case "F57":
				case "F26":
				case "F46":
				case "F52":
				case "F45":
				case "F44":
				case "F40":
				case "G02":
				case "F49":
				case "G15":
				case "G01":
				case "F48":
				case "G06":
				case "G16":
				case "G07":
				case "G08":
				case "G05":
				case "J29":
				case "I01":
				case "F83":
				case "F93":
				case "F95":
				case "F82":
				case "F80":
				case "F96":
				case "F81":
				case "F87":
				case "F97":
				case "F86":
				case "F98":
				case "F10":
				case "F85":
				case "F18":
				case "F12":
				case "F11":
				case "F03":
				case "F13":
				case "F15":
				case "F02":
				case "F14":
				case "F39":
				case "F01":
				case "F16":
				case "F34":
				case "F06":
				case "F07":
				case "F35":
				case "F05":
				case "F32":
				case "F04":
				case "F33":
				case "F60":
				case "F31":
				case "F30":
				case "G29":
				case "G28":
				case "G20":
				case "G21":
					return BNType.BN2020;
				default:
					if (!vecInfo.Ereihe.ToUpper().StartsWith("F", StringComparison.Ordinal) && !vecInfo.Ereihe.ToUpper().StartsWith("G", StringComparison.Ordinal) && !vecInfo.Ereihe.ToUpper().StartsWith("U", StringComparison.Ordinal))
					{
						return BNType.UNKNOWN;
					}
					return BNType.BN2020;
			}
		}
	}
}
