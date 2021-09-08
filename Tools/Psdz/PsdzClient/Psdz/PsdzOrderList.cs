﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Psdz
{
    [KnownType(typeof(PsdzEcuVariantInstance))]
    [DataContract]
    public class PsdzOrderList : IPsdzOrderList
    {
        [DataMember]
        public int NumberOfUnits { get; set; }

        [DataMember]
        public IPsdzEcuVariantInstance[] BntnVariantInstances { get; set; }
    }
}
