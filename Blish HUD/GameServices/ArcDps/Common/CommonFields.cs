using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Blish_HUD.ArcDps.Common {

    public class CommonFields {
        private bool _enabled = false;

        /// <summary>
        ///     Delegate which will be invoked in <see cref="CommonFields.PlayerAdded" /> and
        ///     <see cref="CommonFields.PlayerRemoved" />
        /// </summary>
        public delegate void PresentPlayersChange(Player player);

        /// <summary>
        ///     Contains every player in the current group or squad.
        ///     Key: Character Name, Value: Account Name
        /// </summary>
        public IReadOnlyDictionary<string, Player> PlayersInSquad => GameService.ArcDpsV2.Common.PlayersInSquad
            .Select(x => new KeyValuePair<string, Player>(x.Key, new Player(x.Value.CharacterName, x.Value.AccountName, x.Value.Profession, x.Value.Elite, x.Value.Self)))
            .ToDictionary(x=> x.Key, x => x.Value);

        /// <summary>
        ///     Gets invoked whenever someone joins the squad or group.
        /// </summary>
        public event PresentPlayersChange PlayerAdded;

        /// <summary>
        ///     Gets invoked whenever someone leaves the squad or group.
        /// </summary>
        public event PresentPlayersChange PlayerRemoved;

        /// <summary>
        ///     Activates the <see cref="CommonFields" /> service.
        /// </summary>
        public void Activate() {
            GameService.ArcDpsV2.Common.PlayerAdded += player => PlayerAdded?.Invoke(new Player(player.CharacterName, player.AccountName, player.Profession, player.Elite, player.Self));
            GameService.ArcDpsV2.Common.PlayerRemoved += player => PlayerRemoved?.Invoke(new Player(player.CharacterName, player.AccountName, player.Profession, player.Elite, player.Self));

            if (_enabled) return;

            _enabled                          =  true;
        }

        public struct Player {

            public Player(string characterName, string accountName, uint profession, uint elite, bool self) {
                this.CharacterName = characterName;
                this.AccountName   = accountName;
                this.Profession    = profession;
                this.Elite         = elite;
                this.Self          = self;
            }

            /// <summary>
            /// The current character name.
            /// </summary>
            public string CharacterName { get; }

            /// <summary>
            /// The account name.
            /// </summary>
            public string AccountName { get; }

            /// <summary>
            /// The core profession.
            /// </summary>
            public uint Profession { get; }

            /// <summary>
            /// The elite if any is used.
            /// </summary>
            public uint Elite { get; }

            /// <summary>
            /// <see langword="True"/> if this player agent belongs to the account currently logged in on the local Guild Wars 2 instance. Otherwise <see langword="false"/>.
            /// </summary>
            public bool Self { get; }

        }

    }

}