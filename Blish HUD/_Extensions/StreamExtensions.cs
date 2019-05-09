using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD {
    public static class StreamExtensions {

        public static MemoryStream ToMemoryStream(this Stream stream) {
            var replacementMemoryStream = new MemoryStream();
            stream.CopyTo(replacementMemoryStream);
            replacementMemoryStream.Position = 0;

            return replacementMemoryStream;
        }

    }
}
