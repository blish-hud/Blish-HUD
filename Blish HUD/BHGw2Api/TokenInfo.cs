using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.BHGw2Api {

    public enum TokenPermissions {
        /// <summary>
        /// Grants access to the <c>/v2/account</c> endpoint (This permission is required for all API keys).
        /// </summary>
        Account,
        /// <summary>
        /// Grants access to view each character's equipped <a href="https://wiki.guildwars2.com/wiki/Specialization">specializations</a> and gear.
        /// </summary>
        Builds,
        /// <summary>
        /// Grants access to the <c>/v2/characters</c> endpoint.
        /// </summary>
        Characters,
        /// <summary>
        /// Grants access to guild info under the <c>/v2/guild/:id/</c> sub-endpoints.
        /// </summary>
        Guilds,
        /// <summary>
        /// Grants access to inventories in the <c>/v2/characters</c>, <c>/v2/account/bank</c>, and <c>/v2/account/materials</c> endpoints.
        /// </summary>
        Inventories,
        /// <summary>
        /// Grants access to achievements, dungeon unlock status, mastery point assignments, and general PvE progress.
        /// </summary>
        Progression,
        /// <summary>
        /// Grants access to the <c>/v2/pvp</c> sub-endpoints. (i.e. <c>/v2/pvp/games</c>, <c>/v2/pvp/stats</c>)
        /// </summary>
        PvP,
        /// <summary>
        /// Grants access to the <c>/v2/commerce/transactions</c> endpoint.
        /// </summary>
        TradingPost,
        /// <summary>
        /// Grants access to the <c>/v2/account/skins</c> and <c>/v2/account/dyes</c> endpoints.
        /// </summary>
        Unlocks,
        /// <summary>
        /// Grants access to the <c>/v2/account/wallet</c> endpoint.
        /// </summary>
        Wallet
    }

    public class TokenInfo : ApiItem {

        public string Id { get; set; }
        public string Name { get; set; }
        public TokenPermissions[] Permissions { get; set; }

        public override string CacheKey() => this.Id;
    }
}
