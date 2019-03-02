using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.Markers {

    public class AugTyrRoute {

        public List<AugTyrNode> Nodes { get; set; }
        public List<AugTyrNode> DetachedNodes { get; set; }

        public static AugTyrRoute FromFile(string filepath) {
            string rawRoute = System.IO.File.ReadAllText(filepath);
            return JsonConvert.DeserializeObject<AugTyrRoute>(rawRoute);
        }

    }

}
