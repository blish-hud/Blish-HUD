using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.BHGw2Api {
    public class ApiCache {

        public ApiCache() {
            // TODO: Check for cache files to load
        }

        public T GetManyByExactEndpoint<T>(string endpoint) {
            return default(T);
        }

        public void CacheMany() {

        }

    }
}
