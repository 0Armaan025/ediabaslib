﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient
{
	public interface IPsdzObjectBuilder
	{
		IPsdzDiagAddress BuildDiagAddress(int diagAddress);

		IPsdzSgbmId BuildPsdzSgbmId(string processClass, long id, int mainVersion, int subVersion, int patchVersion);

		IPsdzEcu BuildEcu(IEcuObj ecu);

		IPsdzEcuIdentifier BuildEcuIdentifier(IEcuIdentifier ecuIdentifier);

		IPsdzEcuIdentifier BuildEcuIdentifier(int diagAddrAsInt, string baseVariant);

		IPsdzFa BuildEmptyFa();

		IPsdzFa BuildFa(IFa fa, string vin17);

		IPsdzFa BuildFa(IPsdzStandardFa fa, string vin17);

		IPsdzFa BuildFaFromXml(string xml);

		IPsdzStandardFa BuildFaActualFromVehicleContext(IVehicle vehicleContext);

		IPsdzFp BuildFp(IVehicleProfile vehicleProfile);

		IPsdzIstufenTriple BuildIStufenTripleActualFromVehicleContext(IVehicle vehicleContext);

		IPsdzIstufe BuildIstufe(string istufe);

		IPsdzIstufenTriple BuildIstufenTriple(string shipment, string last, string current);

		IPsdzStandardSvt BuildStandardSvtActualFromVehicleContext(IVehicle vehicleContext, IEnumerable<IPsdzEcuIdentifier> ecuListFromPsdz = null);

		IPsdzSvt BuildSvt(IPsdzStandardSvt svtInput, string vin17);

		IPsdzSvt BuildSvt(ISvt svt, string vin17);

		IPsdzSwtAction BuildSwtAction(ISwt swt);

		IPsdzSwtApplication BuildSwtApplication(IFSCProvided fscProvided);

		IPsdzSwtApplication BuildSwtApplication(int appNo, int upgradeIdx, byte[] fsc, byte[] fscCertificate, SwtActionType? swtActionType);

		IPsdzSwtApplicationId BuildSwtApplicationId(ISwtApplicationId swtApplicationId);

		IPsdzSwtApplicationId BuildSwtApplicationId(int appNo, int upgradeIdx);

		IPsdzTalFilter BuildTalFilter();

		IPsdzTal BuildTalFromXml(string xml);

		IPsdzTal BuildEmptyTal();

		IPsdzVin BuildVin(string vin17);

		IPsdzTalFilter DefineFilterForAllEcus(TaCategories[] taCategories, TalFilterOptions talFilterOptions, IPsdzTalFilter inputTalFilter);

		IPsdzTalFilter DefineFilterForSelectedEcus(TaCategories[] taCategories, int[] diagAddress, TalFilterOptions talFilterOptions, IPsdzTalFilter inputTalFilter);

		PsdzFetchEcuCertCheckingResult BuildFetchEcuCertCheckingResult(IFetchEcuCertCheckingResult ecuCertCheckingResult);

		IList<IPsdzFeatureSpecificFieldCto> BuildFeatureSpecificFieldsCto(IList<IFeatureSpecificField> featureSpecificFields);

		IList<IPsdzValidityConditionCto> BuildValidityConditionsCto(IList<IValidityCondition> validityConditions);

		PsdzConditionTypeEtoEnum BuildConditionTypeEnum(ConditionTypeEnum conditionType);

		IPsdzAsamJobInputDictionary BuildAsamJobInputDictionary(IAsamJobInputDictionary inputDictionary);
	}
}
