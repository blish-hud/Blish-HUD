using System.Collections.Generic;

namespace Blish_HUD.ArcDps.Common
{
    public class CommonFields
    {
        public List<string> CharactersInSquad => new List<string>(_charactersInSquad);

        private List<string> _charactersInSquad = new List<string>();
    }
}
