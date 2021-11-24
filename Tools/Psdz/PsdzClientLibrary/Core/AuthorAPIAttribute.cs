﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public class AuthorAPIAttribute : Attribute
    {
        public bool SelectableTypeDeclaration
        {
            get
            {
                return this.selectableTypeDeclaration;
            }
            set
            {
                this.selectableTypeDeclaration = value;
            }
        }

        private bool selectableTypeDeclaration = false;
    }
}
