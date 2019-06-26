using System;
using System.Net.Sockets;

namespace Blish_HUD.ArcDps
{
    public sealed class MessageData
    {
        public byte[] Message;
        public AsyncUserToken Token;
    }

    public sealed class AsyncUserToken : IDisposable
    {
        public AsyncUserToken(Socket socket)
        {
            Socket = socket;
        }

        public Socket Socket { get; }
        public int? MessageSize { get; set; }
        public int DataStartOffset { get; set; }
        public int NextReceiveOffset { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
                Socket.Shutdown(SocketShutdown.Send);
            }
            catch (Exception)
            {
                // ignored
            }

            try
            {
                Socket.Close();
            }
            catch (Exception)
            {
                // ignored
            }
        }

        #endregion
    }
}