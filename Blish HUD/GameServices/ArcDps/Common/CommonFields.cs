using System.Collections.Generic;

namespace Blish_HUD.ArcDps.Common
{
    public class CommonFields
    {
        /// <summary>
        ///     Delegate which will be invoked in <see cref="CommonFields.PlayerAdded" /> and
        ///     <see cref="CommonFields.PlayerRemoved" />
        /// </summary>
        public delegate void PresentPlayersChange(string characterName, string accountName);

        private readonly Dictionary<string, string> _playersInSquad = new Dictionary<string, string>();

        private bool _enabled;

        /// <summary>
        ///     Contains every player in the current group or squad.
        ///     Key: Character Name, Value: Account Name
        /// </summary>
        public IReadOnlyDictionary<string, string> PlayersInSquad => _playersInSquad;

        /// <summary>
        ///     Gets invoked whenever someone joins the squad or group.
        /// </summary>
        public event PresentPlayersChange PlayerAdded;

        /// <summary>
        ///     Gets invoked whenever someone leaves the squad or group.
        /// </summary>
        public event PresentPlayersChange PlayerRemoved;

        /// <summary>
        ///     Activates the <see cref="CommonFields"/> service.
        /// </summary>
        public void Activate()
        {
            if (_enabled)
                return;

            _enabled = true;
            GameService.ArcDps.RawCombatEvent += CombatHandler;
        }

        private void CombatHandler(object sender, RawCombatEventArgs args)
        {
            if (args.CombatEvent.Ev != null) return;

            /* notify tracking change */
            if (args.CombatEvent.Src.Elite != 0) return;

            /* add */
            if (args.CombatEvent.Src.Profession != 0)
            {
                if (!_playersInSquad.ContainsKey(args.CombatEvent.Src.Name))
                {
                    _playersInSquad.Add(args.CombatEvent.Src.Name, args.CombatEvent.Dst.Name);
                    PlayerAdded?.Invoke(args.CombatEvent.Src.Name, args.CombatEvent.Dst.Name);
                }
            }
            /* remove */
            else
            {
                if (_playersInSquad.ContainsKey(args.CombatEvent.Src.Name))
                {
                    var accountName = _playersInSquad[args.CombatEvent.Src.Name];
                    _playersInSquad.Remove(args.CombatEvent.Src.Name);
                    PlayerRemoved?.Invoke(args.CombatEvent.Src.Name, accountName);
                }
            }
        }
    }
}