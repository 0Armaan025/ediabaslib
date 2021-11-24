﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.Rheingold.Psdz.Model.Kds
{
    public interface IPsdzKdsPublicKeyResultCto
    {
        IPsdzKdsIdCto KdsId { get; }

        byte[] PublicKey { get; }
    }
}
