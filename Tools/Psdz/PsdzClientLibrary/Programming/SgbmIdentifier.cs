﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using PsdzClient.Programming;

namespace BMW.Rheingold.Programming.API
{
    [DataContract]
    public class SgbmIdentifier : ISgbmId, IComparable<ISgbmId>, IEquatable<ISgbmId>
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public int MainVersion { get; set; }

        [DataMember]
        public int PatchVersion { get; set; }

        [DataMember]
        public string ProcessClass { get; set; }

        [DataMember]
        public int SubVersion { get; set; }

        public string HexString
        {
            get
            {
                return string.Format(CultureInfo.InvariantCulture, "{0}-{1:X8}-{2:X2}.{3:X2}.{4:X2}", new object[]
                {
                    this.ProcessClass,
                    this.Id,
                    this.MainVersion,
                    this.SubVersion,
                    this.PatchVersion
                });
            }
        }

        public int CompareTo(ISgbmId other)
        {
            if (other == null)
            {
                return -1;
            }
            int num = string.Compare(this.ProcessClass, other.ProcessClass, StringComparison.OrdinalIgnoreCase);
            if (num != 0)
            {
                return num;
            }
            int num2 = this.Id.CompareTo(other.Id);
            if (num2 != 0)
            {
                return num2;
            }
            int num3 = this.MainVersion.CompareTo(other.MainVersion);
            if (num3 != 0)
            {
                return num3;
            }
            int num4 = this.SubVersion.CompareTo(other.SubVersion);
            if (num4 != 0)
            {
                return num4;
            }
            return this.PatchVersion.CompareTo(other.PatchVersion);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0}-{1:X8}-{2:000}.{3:000}.{4:000}", new object[]
            {
                this.ProcessClass,
                this.Id,
                this.MainVersion,
                this.SubVersion,
                this.PatchVersion
            });
        }

        public bool Equals(ISgbmId other)
        {
            return string.Equals(this.ProcessClass, other.ProcessClass, StringComparison.OrdinalIgnoreCase) & this.Id.Equals(other.Id) & this.MainVersion.Equals(other.MainVersion) & this.SubVersion.Equals(other.SubVersion) & this.PatchVersion.Equals(other.PatchVersion);
        }
    }
}
