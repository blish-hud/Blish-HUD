using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {

    /// <summary>
    /// The races within Guild Wars 2.
    /// </summary>
    /// <remarks>Matches https://github.com/Ruhrpottpatriot/GW2.NET/blob/master/src/GW2NET.Core/Common/Race.cs</remarks>
    public enum Gw2Race {
        /// <summary>Indicates an unknown race.</summary>
        Unknown = 0,

        /// <summary>The 'Asura' race.</summary>
        Asura = 1 << 0,

        /// <summary>The 'Charr' race.</summary>
        Charr = 1 << 1,

        /// <summary>The 'Human' race.</summary>
        Human = 1 << 2,

        /// <summary>The 'Norn' race.</summary>
        Norn = 1 << 3,

        /// <summary>The 'Sylvari' race.</summary>
        Sylvari = 1 << 4
    }

}
