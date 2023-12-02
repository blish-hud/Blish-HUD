using Blish_HUD.GameServices.ArcDps.V2.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2 {

    public class CommonFields {

        /// <summary>
        ///     Delegate which will be invoked in <see cref="PlayerAdded" /> and
        ///     <see cref="PlayerRemoved" />
        /// </summary>
        public delegate void PresentPlayersChange(Player player);

        /// <summary>
        ///     Contains every player in the current group or squad.
        ///     Key: Character Name, Value: Account Name
        /// </summary>
        public IReadOnlyDictionary<string, Player> PlayersInSquad => _playersInSquad;

        private readonly ConcurrentDictionary<string, Player> _playersInSquad = new ConcurrentDictionary<string, Player>();

        private bool _enabled;

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
            if (_enabled) return;

            _enabled = true;
            GameService.ArcDpsV2.RegisterMessageType<CombatCallback>(0, CombatHandler);
        }

        private Task CombatHandler(CombatCallback combatEvent, CancellationToken ct) {
            //if (args.CombatEvent.Ev != null) return Task.CompletedTask;

            /* notify tracking change */
            if (combatEvent.Source.Elite != 0) return Task.CompletedTask;

            /* add */
            if (combatEvent.Source.Profession != 0) {
                if (_playersInSquad.ContainsKey(combatEvent.Source.Name)) return Task.CompletedTask;

                string accountName = combatEvent.Destination.Name.StartsWith(":")
                                         ? combatEvent.Destination.Name.Substring(1)
                                         : combatEvent.Destination.Name;

                var player = new Player(
                                        combatEvent.Source.Name, accountName,
                                        combatEvent.Destination.Profession, combatEvent.Destination.Elite, combatEvent.Destination.Self != 0
                                       );

                if (_playersInSquad.TryAdd(combatEvent.Source.Name, player)) this.PlayerAdded?.Invoke(player);
            }
            /* remove */
            else {
                if (_playersInSquad.TryRemove(combatEvent.Source.Name, out var player)) this.PlayerRemoved?.Invoke(player);
            }

            return Task.CompletedTask;
        }

        public struct Player {

            public Player(string characterName, string accountName, uint profession, uint elite, bool self) {
                this.CharacterName = characterName;
                this.AccountName = accountName;
                this.Profession = profession;
                this.Elite = elite;
                this.Self = self;
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