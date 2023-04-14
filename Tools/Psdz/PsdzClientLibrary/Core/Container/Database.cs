﻿// BMW.Rheingold.Module.ISTA.Database
using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace PsdzClient.Core.Container
{
    [Serializable]
    [DesignerCategory("code")]
    [DataContract(Name = "Database")]
    [GeneratedCode("Xsd2Code", "3.4.0.32990")]
    public class Database : INotifyPropertyChanged
    {
        private ObservableCollection<ANode> referencedNodesField;

        private string nameField;

        [XmlArrayItem("Node", IsNullable = false)]
        [DataMember]
        public ObservableCollection<ANode> ReferencedNodes
        {
            get
            {
                return referencedNodesField;
            }
            set
            {
                if (referencedNodesField != null)
                {
                    if (!referencedNodesField.Equals(value))
                    {
                        referencedNodesField = value;
                        OnPropertyChanged("ReferencedNodes");
                    }
                }
                else
                {
                    referencedNodesField = value;
                    OnPropertyChanged("ReferencedNodes");
                }
            }
        }

        [DataMember]
        [DefaultValue("Database")]
        [XmlAttribute]
        public string Name
        {
            get
            {
                return nameField;
            }
            set
            {
                if (nameField != null)
                {
                    if (!nameField.Equals(value))
                    {
                        nameField = value;
                        OnPropertyChanged("Name");
                    }
                }
                else
                {
                    nameField = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Database()
        {
            nameField = "Database";
        }

        public virtual void OnPropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
