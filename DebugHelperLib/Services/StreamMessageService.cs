using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Blish_HUD.DebugHelperLib.Models;
using ProtoBuf;

namespace Blish_HUD.DebugHelperLib.Services {

    public class StreamMessageService : IMessageService, IDisposable {

        private readonly ConcurrentDictionary<ulong, ManualResetEventSlim> waitingMessages   = new ConcurrentDictionary<ulong, ManualResetEventSlim>();
        private readonly ConcurrentDictionary<ulong, Message>              receivedMessages  = new ConcurrentDictionary<ulong, Message>();
        private readonly ConcurrentDictionary<Type, Action<Message>>       registedCallbacks = new ConcurrentDictionary<Type, Action<Message>>();
        private readonly Stream                                            inStream;
        private readonly Stream                                            outStream;
        private readonly object                                            outLock = new object();
        private          Thread?                                           thread;
        private          bool                                              stopRequested = false;
        private          long                                              lastMessageId = 0;

        public StreamMessageService(Stream inStream, Stream outStream) {
            this.inStream  = inStream;
            this.outStream = outStream;
        }

        public void Start() {
            if (thread != null) return;

            thread = new Thread(Loop);
            thread.Start();
        }

        public void Stop() {
            if ((thread == null) || stopRequested) return;

            stopRequested = true;
            thread.Join();

            stopRequested = false;
            thread        = null;
        }

        private void Loop() {
            Message message;

            while (!stopRequested && ((message = Serializer.DeserializeWithLengthPrefix<Message>(inStream, PrefixStyle.Base128, 1)) != null))
                if (waitingMessages.TryGetValue(message.Id, out var resetEvent)) {
                    receivedMessages.TryAdd(message.Id, message);
                    resetEvent.Set();
                } else if (registedCallbacks.TryGetValue(message.GetType(), out var callback)) callback(message);
        }

        public void Register<T>(Action<T> callback) where T : Message { registedCallbacks.AddOrUpdate(typeof(T), t => x => callback((T)x), (t, _) => x => callback((T)x)); }

        public void Unregister<T>() where T : Message { registedCallbacks.TryRemove(typeof(T), out _); }

        public void Send(Message message) {
            lock (outLock) {
                SetId(message);
                Serializer.SerializeWithLengthPrefix(outStream, message, PrefixStyle.Base128, 1);
                outStream.Flush();
            }
        }

        public T SendAndWait<T>(Message message) where T : Message => SendAndWait<T>(message, TimeSpan.FromMilliseconds(-1))!;

        public T? SendAndWait<T>(Message message, TimeSpan timeout) where T : Message {
            using var fResetEvent = new ManualResetEventSlim(false);

            lock (outLock) {
                SetId(message);
                if (!waitingMessages.TryAdd(message.Id, fResetEvent)) return null;

                Serializer.SerializeWithLengthPrefix(outStream, message, PrefixStyle.Base128, 1);
                outStream.Flush();
            }

            bool received = fResetEvent.Wait(timeout);
            waitingMessages.TryRemove(message.Id, out _);
            if (!received) return null;

            if (!receivedMessages.TryRemove(message.Id, out var response)) return null;

            return response as T;
        }

        private void SetId(Message message) {
            if (message.Id != 0) return;

            using var process = Process.GetCurrentProcess();

            ulong time      = (ulong)(DateTime.UtcNow - process.StartTime).TotalMilliseconds & 0x1FFFFFFFFFF;
            ulong processId = (ulong)Environment.ProcessId                                   & 0x3FF;
            ulong seq       = (ulong)Interlocked.Increment(ref lastMessageId)                & 0x1FFF;
            message.Id = (time << 23) | (processId << 13) | seq;
        }

        #region IDisposable Support

        private bool isDisposed = false; // To detect redundant calls

        protected virtual void Dispose(bool isDisposing) {
            if (isDisposed) return;

            if (isDisposing) Stop();
            isDisposed = true;
        }

        public void Dispose() { Dispose(true); }

        #endregion

    }

}
