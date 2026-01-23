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
        TcpClient Client { get; }

        event EventHandler<SocketError> Error;

        void Disconnect();

        void Initialize(IPEndPoint endpoint, CancellationToken ct);
        
        bool IsMessageTypeAvailable(MessageType type);
        
        void RegisterMessageTypeListener<T>(IArcDpsMessageListener<T> listener) where T : struct;
    }
}
