using System.Collections.Generic;

namespace Blish_HUD.ArcDps.Common
{
    public class CommonFields
    {
        /// <summary>
        /// Contains every player in the current group or squad.
        /// Key: Character Name, Value: Account Name
        /// </summary>
        public IReadOnlyDictionary<string, string> PlayersInSquad => _playersInSquad;

        private Dictionary<string, string> _playersInSquad = new Dictionary<string, string>();

        private bool _enabled = false;

        public void Activate()
        {
            if (_enabled)
                return;

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
                }
            }

            /* remove */
            else
            {
                if (_playersInSquad.ContainsKey(args.CombatEvent.Src.Name))
                    _playersInSquad.Remove(args.CombatEvent.Src.Name);
            }
        }
    }
}
