﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    public enum PsdzTokenDetailedStatusEtoEnum
    {
        OK,
        ERROR,
        MALFORMED,
        LINKTOID_FORMAT,
        LINKTOID_MISSING,
        LINKTOID_MISMATCH,
        LINKTOID_DENIED,
        LINKTOIDTYPE_DENIED,
        VIN_MISSING,
        VIN_DENIED,
        FEATUREID_MISSING,
        FEATUREID_DENIED,
        FEATUREID_UNKNOWN,
        TOKENPACKAGEREFERENCE_DENIED,
        TOKENPACKAGEREFERENCE_UNKNOWN,
        TOKENPACKAGEREFERENCE_MISSMATCH,
        TOKENPACKAGE_REBUILD_ERROR,
        VIN_MALFORMED,
        NOT_A_DEVELOPMENT_VIN,
        NOT_A_CUSTOMER_VIN,
        ECU_UID_DENIED,
        NOT_A_DEVELOPMENT_ECU_UID,
        NOT_A_CUSTOMER_ECU_UID,
        ECU_UID_MALFORMED,
        ECU_UID_PREFIX_INVALID,
        FEATUREID_NOT_ALLOWED,
        FEATUREID_MALFORMED,
        NO_VALIDITY_CONDITION_ALLOWED,
        VALIDITY_CONDITION_TYPE_DENIED,
        VALIDITY_CONDITION_TYPE_UNKNOWN,
        VALIDITY_CONDITION_VALUE_NOT_ALLOWED,
        VALIDITY_CONDITION_VALUE_MALFORMED,
        NO_ENABLE_TYPE_ALLOWED,
        ENABLE_TYPE_NOT_ALLOWED,
        ENABLE_TYPE_UNKNOWN,
        NO_FEATURE_SPECIFIC_FIELD_ALLOWED,
        FEATURE_SPECIFIC_FIELD_TYPE_NOT_ALLOWED,
        FEATURE_SPECIFIC_FIELD_TYPE_UNKNOWN,
        FEATURE_SPECIFIC_FIELD_VALUE_NOT_ALLOWED,
        FEATURE_SPECIFIC_FIELD_VALUE_MALFORMED,
        FEATURE_SET_REFERENCE_DENIED,
        FEATURE_SET_REFERENCE_UNKNOWN,
        FEATURE_SET_REFERENCE_MISSMATCH,
        FEATURE_SET_REFERENCE_REBUILD_ERROR,
        REBUILD_ERROR,
        DIAGNOSTIC_ADDRESS_MALFORMED,
        E_VALIDITY_CONDITION_TAG_DUPLICATE,
        E_FEATURE_SPECIFIC_FIELD_TAG_DUPLICATE,
        E_NO_RIGHTS_ASSIGNED,
        E_ECU_UID_PREFIX_NOT_ALLOWED,
        UNDEFINED
    }
    [KnownType(typeof(PsdzFeatureIdCto))]
    [DataContract]
    [KnownType(typeof(PsdzDiagAddress))]
    public class PsdzDetailedStatusCto : IPsdzDetailedStatusCto
    {
        [DataMember]
        public IPsdzDiagAddress DiagAddressCto { get; set; }

        [DataMember]
        public IPsdzFeatureIdCto FeatureIdCto { get; set; }

        [DataMember]
        public PsdzTokenDetailedStatusEtoEnum TokenDetailedStatusEto { get; set; }
    }
}

