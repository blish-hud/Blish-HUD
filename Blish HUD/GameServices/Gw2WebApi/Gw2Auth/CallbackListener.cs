using System;
using System.Net;

namespace Blish_HUD.Gw2WebApi.Gw2Auth {
    /// <summary>
    /// Encapsulates an HttpListener for handling start/stop listening with a given callback function.
    /// </summary>
    internal class CallbackListener : IDisposable {

        private readonly HttpListener _listener;

        /// <summary>
        /// Creates a new listener and adds all of the given addresses to the prefixes list.
        /// </summary>
        /// <param name="httpAddresses">Http addresses to listen on.</param>
        public CallbackListener(params string[] httpAddresses) {
            _listener = new HttpListener();
            this.AddPrefixes(httpAddresses);
        }

        public void AddPrefixes(string[] httpAddresses) {
            foreach (var address in httpAddresses) {
                _listener.Prefixes.Add(address);
            }
        }

        public void RemovePrefixes(string[] httpAddresses) {
            foreach (var address in httpAddresses) {
                _listener.Prefixes.Remove(address);
            }
        }

        /// <summary>
        /// Begins asynchronously retrieving an incoming request.
        /// </summary>
        /// <param name="callback">A function to execute on an incoming request.</param>
        public void Start(Action<HttpListenerContext> callback) {
            if (_listener.IsListening) {
                return;
            }
            _listener.Start();
            _listener.BeginGetContext(result => Receiver(result, callback), _listener);
        }

        private void Receiver(IAsyncResult result, Action<HttpListenerContext> callback) {
            var listener = (HttpListener)result.AsyncState;
            // Call EndGetContext to complete the asynchronous operation.
            callback(listener.EndGetContext(result));
        }

        /// <summary>
        /// Causes this instance to stop receiving new incoming requests and terminates processing of all ongoing requests.
        /// </summary>
        public void Stop() {
            if (_listener.IsListening) {
                _listener.Stop();
            }
        }

        /// <summary>
        /// Releases the resources held by the encapsulated HttpListener object.
        /// </summary>
        /// <remarks>
        ///     See also: <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener.system-idisposable-dispose?view=netframework-4.7.2"/>
        /// </remarks>
        public void Dispose() {
            _listener.Close();
        }
    }
}
