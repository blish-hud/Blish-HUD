using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    internal interface IArcDpsClient {
        TcpClient Client { get; }

        event EventHandler<SocketError> Error;

        void Disconnect();
        
        void Initialize(IPEndPoint endpoint, CancellationToken ct);

        void RegisterMessageTypeListener<T>(int type, Func<T, CancellationToken, Task> listener) where T : struct;
    }
}
