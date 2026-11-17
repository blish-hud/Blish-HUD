using Blish_HUD.GameServices.ArcDps.V2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2.Processors {
    internal class ImGuiProcessor : MessageProcessor<ImGuiCallback> {
        internal override bool TryInternalProcess(byte[] message, out ImGuiCallback result) {

            try {
                result = new ImGuiCallback() { NotCharacterSelectOrLoading = message[0] };

                return true;
            } catch (Exception) {
                result = default;
                return false;
            }
        }
    }
}
