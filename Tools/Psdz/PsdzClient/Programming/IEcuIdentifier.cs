﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PsdzClient.Programming
{
    //[AuthorAPI(SelectableTypeDeclaration = true)]
    public interface IEcuIdentifier
    {
        string BaseVariant { get; }

        int DiagAddrAsInt { get; }
    }
}
