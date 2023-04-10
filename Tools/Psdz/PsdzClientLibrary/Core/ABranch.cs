﻿// BMW.Rheingold.Module.ISTA.ABranch
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core
{
    [Serializable]
    [XmlInclude(typeof(SingleChoice))]
    [XmlInclude(typeof(QuantityChoice))]
    [XmlInclude(typeof(MultipleChoice))]
    [DesignerCategory("code")]
    [XmlInclude(typeof(All))]
    [XmlInclude(typeof(Executable))]
    [XmlInclude(typeof(AChoice))]
    [DataContract(Name = "ABranch")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    [XmlInclude(typeof(Sequence))]
    public abstract class ABranch : ANode, INotifyPropertyChanged
    {
        private ObservableCollection<ANode> childrenField;

        [XmlArrayItem("Node", IsNullable = false)]
        [DataMember]
        public ObservableCollection<ANode> Children
        {
            get
            {
                return childrenField;
            }
            set
            {
                if (childrenField != null)
                {
                    if (!childrenField.Equals(value))
                    {
                        childrenField = value;
                        OnPropertyChanged("Children");
                    }
                }
                else
                {
                    childrenField = value;
                    OnPropertyChanged("Children");
                }
            }
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        public new virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
