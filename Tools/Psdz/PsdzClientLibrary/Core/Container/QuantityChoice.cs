﻿// BMW.Rheingold.Module.ISTA.QuantityChoice
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [DataContract(Name = "QuantityChoice")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [DesignerCategory("code")]
    public class QuantityChoice : AChoice, INotifyPropertyChanged
    {
        private uint minimumField;

        private uint maximumField;

        [DataMember]
        [XmlAttribute]
        [DefaultValue(typeof(uint), "1")]
        public uint Minimum
        {
            get
            {
                return minimumField;
            }
            set
            {
                if (!minimumField.Equals(value))
                {
                    minimumField = value;
                    OnPropertyChanged("Minimum");
                }
            }
        }

        [DataMember]
        [XmlAttribute]
        public uint Maximum
        {
            get
            {
                return maximumField;
            }
            set
            {
                if (!maximumField.Equals(value))
                {
                    maximumField = value;
                    OnPropertyChanged("Maximum");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public QuantityChoice()
        {
            minimumField = 1u;
        }

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
