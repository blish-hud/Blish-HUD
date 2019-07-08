using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.BHGw2Api {
    public class CachedCall<T> where T : ApiItem {

        public static Dictionary<string, string> CachedCalls = new Dictionary<string, string>();

        public string CachedResults { get; private set; }

        public int CacheDuration { get; set; }
        public bool PersistInMemory { get; set; }

        public CachedCall(string requestedEndpoint, string callResults, int cacheDuration, bool persistInMemory = true) {
            this.CachedResults = callResults;

            this.PersistInMemory = persistInMemory;
            this.CacheDuration = cacheDuration;

            //CachedCalls.Add(requestedEndpoint, this);
        }



    }
}
