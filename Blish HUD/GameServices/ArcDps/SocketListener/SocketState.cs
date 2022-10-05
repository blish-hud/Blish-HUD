namespace Blish_HUD.ArcDps {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    public class SocketState {
        // Size of receive buffer.
        public const int BUFFER_SIZE = 4096;
        // Client socket.
        public Socket Socket = null;
        // Receive buffer.
        public byte[] Buffer = new byte[BUFFER_SIZE];
        public AsyncUserToken Token;
    }
}
