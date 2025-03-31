using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.ArcDps.V2 {
    internal interface IArcDpsClient : IDisposable {
        public TcpClient Client { get; }

        public event EventHandler<SocketError> Error;

        public void Disconnect();

        public void Initialize(IPEndPoint endpoint, CancellationToken ct);

        public bool IsMessageTypeAvailable(MessageType type);

        public void RegisterMessageTypeListener<T>(int type, Func<T, CancellationToken, Task> listener) where T : struct;
    }
}
