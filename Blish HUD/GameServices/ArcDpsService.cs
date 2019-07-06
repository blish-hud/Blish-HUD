using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Blish_HUD.ArcDps;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class ArcDpsService : GameService
    {
        private static readonly object WatchLock = new object();
#if DEBUG
        public static long Counter;
#endif
        private readonly TimeSpan _leeway = TimeSpan.FromMilliseconds(1000);
        private bool _hudIsActive;

        private SocketListener _server;

        private Stopwatch _stopwatch;

        /// <summary>
        ///     Indicates if arcdps updated <see cref="HudIsActive" /> in the last second (it should every in-game frame)
        /// </summary>
        public bool RenderPresent { get; private set; }

        /// <summary>
        ///     Indicates if arcdps currently draws its HUD (not in character select, cut scenes or loading screens)
        /// </summary>
        public bool HudIsActive
        {
            get
            {
                lock (WatchLock)
                {
                    return _hudIsActive;
                }
            }
            private set
            {
                lock (WatchLock)
                {
                    _stopwatch.Restart();
                    _hudIsActive = value;
                }
            }
        }

        /// <remarks>
        ///     Please note that you block the socket server with whatever
        ///     you are doing on this event. So please don't do anything
        ///     that requires heavy work. Make your own worker thread
        ///     if you need to.
        ///     Also note, that this is not the main thread, so operations
        ///     other parts of BHUD have to be thread safe.
        /// </remarks>
        /// <summary>
        ///     Holds unprocessed combat data
        /// </summary>
        public event EventHandler<RawCombatEventArgs> RawCombatEvent;

        protected override void Initialize()
        {
            _stopwatch = new Stopwatch();
            _server = new SocketListener(10, 200_000);
            _server.ReceivedMessage += MessageHandler;
#if DEBUG
            RawCombatEvent += (a, b) => { Interlocked.Increment(ref Counter); };
#endif
        }

        protected override void Load()
        {
            _server.Start(new IPEndPoint(IPAddress.Loopback, 8214));
            _stopwatch.Start();
        }

        protected override void Unload()
        {
            _stopwatch.Stop();
            _server.Stop();
            RenderPresent = false;
        }

        protected override void Update(GameTime gameTime)
        {
            TimeSpan elapsed;
            lock (WatchLock)
            {
                elapsed = _stopwatch.Elapsed;
            }

            RenderPresent = elapsed < _leeway;
        }

        private void MessageHandler(MessageData data)
        {
            switch (data.Message[0])
            {
                case (byte) MessageType.ImGui:
                    HudIsActive = data.Message[1] != 0;
                    break;
                case (byte) MessageType.Combat:
                    ProcessCombat(data.Message);
                    break;
            }
        }

        private void ProcessCombat(byte[] data)
        {
            var message = CombatParser.ProcessCombat(data);
            OnRawCombatEvent(new RawCombatEventArgs(message));
        }

        private void OnRawCombatEvent(RawCombatEventArgs e)
        {
            RawCombatEvent?.Invoke(this, e);
        }

        private enum MessageType
        {
            ImGui = 1,
            Combat = 2
        }
    }
}