using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {

    /// <summary>
    /// The professions within Guild Wars 2.
    /// </summary>
    /// <remarks>Matches https://github.com/Ruhrpottpatriot/GW2.NET/blob/master/src/GW2NET.Core/Common/Profession.cs</remarks>
    public enum Gw2Profession {
        /// <summary>
        /// Indicates an unknown profession.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// The 'Guardian' profession.
        /// </summary>
        Guardian = 1 << 0,

        /// <summary>
        /// The 'Warrior' profession.
        /// </summary>
        Warrior = 1 << 1,

        /// <summary>
        /// The 'Engineer' profession.
        /// </summary>
        Engineer = 1 << 2,

        /// <summary>
        /// The 'Ranger' profession.
        /// </summary>
        Ranger = 1 << 3,

        /// <summary>
        /// The 'Thief' profession.
        /// </summary>
        Thief = 1 << 4,

        /// <summary>
        /// The 'Elementalist' profession.
        /// </summary>
        Elementalist = 1 << 5,

        /// <summary>
        /// The 'Mesmer' profession.
        /// </summary>
        Mesmer = 1 << 6,

        /// <summary>
        /// The 'Necromancer' profession.
        /// </summary>
        Necromancer = 1 << 7,

        /// <summary>
        /// The 'Revenant' profession.
        /// </summary>
        Revenant = 1 << 8
    }

}
