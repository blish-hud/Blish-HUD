using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Blish_HUD.ArcDps;
using Blish_HUD.ArcDps.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class ArcDpsService : GameService
    {
        public delegate void RawCombatEvent(CombatEvent data);

        private static readonly object WatchLock = new object();
        private readonly TimeSpan _leeway = TimeSpan.FromMilliseconds(1000);
        private bool _hudIsActive;
        public static long Counter = 0;

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

        /// <summary>
        ///     Holds unprocessed combat data
        /// </summary>
        public event RawCombatEvent OnRawCombatEvent;

        protected override void Initialize()
        {
            _stopwatch = new Stopwatch();
            _server = new SocketListener(4000, 500);
            _server.ReceivedMessage += MessageHandler;
            OnRawCombatEvent += data =>
            {
                Interlocked.Increment(ref Counter);
            };
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
            Console.WriteLine(@"counter: " + Counter);
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
                default:
                    break;
            }
        }

        private void ProcessCombat(byte[] data)
        {
            var message = CombatParser.ProcessCombat(data);
            OnRawCombatEvent?.Invoke(message);
        }

        private enum MessageType
        {
            ImGui = 1,
            Combat = 2
        }
    }
}