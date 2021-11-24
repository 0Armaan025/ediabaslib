﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Svb
{
    public interface IPsdzOrderPart : IPsdzLogisticPart
    {
        IPsdzLogisticPart[] Deliverables { get; }

        IPsdzLogisticPart[] Pattern { get; }
    }
}
