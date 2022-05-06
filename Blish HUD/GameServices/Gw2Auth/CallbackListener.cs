using System;
using System.Net;

namespace Blish_HUD.GameServices.Gw2Auth {
    internal class CallbackListener {

        private readonly HttpListener _listener;

        public CallbackListener() {
            _listener = new HttpListener();
            _listener.Prefixes.Add("http://127.0.0.1:8080/");
        }

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
            this.Stop();
        }

        public void Stop() {
            if (_listener.IsListening) {
                _listener.Stop();
            }
        }
    }
}
