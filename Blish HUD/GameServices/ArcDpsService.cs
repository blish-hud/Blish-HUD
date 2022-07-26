using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Blish_HUD.ArcDps;
using Blish_HUD.ArcDps.Common;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class ArcDpsService : GameService {
        private static readonly Logger Logger = Logger.GetLogger<ArcDpsService>();

        private static readonly object WatchLock = new object();
        #if DEBUG
        public static long Counter;
#endif

        /// <summary>
        ///     Triggered upon error of the underlaying socket listener.
        /// </summary>
        public event EventHandler<SocketError> Error;

        /// <summary>
        ///     Provides common fields that multiple modules might want to track
        /// </summary>
        public CommonFields Common { get; private set; }

        /// <summary>
        ///     Indicates if arcdps updated <see cref="HudIsActive" /> in the last second (it should every in-game frame)
        /// </summary>
        public bool RenderPresent { get; private set; }

        /// <summary>
        ///     Indicates if the socket listener for the arcdps service is running and arcdps send an update in the last second.
        /// </summary>
        public bool Running => this._server?.Running ?? false && this.RenderPresent;

        /// <summary>
        ///     Indicates if arcdps currently draws its HUD (not in character select, cut scenes or loading screens)
        /// </summary>
        public bool HudIsActive {
            get {
                lock (WatchLock) {
                    return _hudIsActive;
                }
            }
            private set {
                lock (WatchLock) {
                    _stopwatch.Restart();
                    _hudIsActive = value;
                }
            }
        }
        private readonly TimeSpan _leeway = TimeSpan.FromMilliseconds(1000);

        private readonly ConcurrentDictionary<uint, ConcurrentBag<Action<object, RawCombatEventArgs>>> _subscriptions =
            new ConcurrentDictionary<uint, ConcurrentBag<Action<object, RawCombatEventArgs>>>();

        private bool _hudIsActive;

        private SocketListener _server;

        private Stopwatch _stopwatch;

        private bool _subscribed;

        public void SubscribeToCombatEventId(Action<object, RawCombatEventArgs> func, params uint[] skillIds) {
            if (!_subscribed) {
                this.RawCombatEvent += DispatchSkillSubscriptions;
                _subscribed         =  true;
            }

            foreach (uint skillId in skillIds) {
                if (!_subscriptions.ContainsKey(skillId)) _subscriptions.TryAdd(skillId, new ConcurrentBag<Action<object, RawCombatEventArgs>>());

                _subscriptions[skillId].Add(func);
            }
        }

        private void DispatchSkillSubscriptions(object sender, RawCombatEventArgs eventHandler) {
            if (eventHandler.CombatEvent.Ev == null) return;

            uint skillId = eventHandler.CombatEvent.Ev.SkillId;
            if (!_subscriptions.ContainsKey(skillId)) return;

            foreach (Action<object, RawCombatEventArgs> action in _subscriptions[skillId]) action(sender, eventHandler);
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

        protected override void Initialize() {
            this.Common             =  new CommonFields();
            _stopwatch              =  new Stopwatch();
            _server                 =  new SocketListener(200_000);
            _server.ReceivedMessage += MessageHandler;
            _server.OnSocketError += SocketErrorHandler;
            #if DEBUG
            this.RawCombatEvent += (a, b) => { Interlocked.Increment(ref Counter); };
            #endif
        }

        protected override void Load() {
            Gw2Mumble.Info.ProcessIdChanged += Start;
            _stopwatch.Start();
        }

        /// <summary>
        /// Starts the socket listener for the arc dps bridge.
        /// </summary>
        private void Start(object sender, ValueEventArgs<uint> value) {
            this.Start(value.Value);
        }

        /// <summary>
        /// Starts the socket listener for the arc dps bridge.
        /// </summary>
        private void Start(uint processId) {
            if (this.Loaded) {
                _server.Start(new IPEndPoint(IPAddress.Loopback, GetPort(processId)));
            }
        }

        /// <summary>
        /// Stops the socket listener for the arc dps bridge.
        /// </summary>
        private void Stop() {
            if (this.Loaded) {
                this._server.Stop();
            }
        }

        /// <summary>
        /// Restarts the socket listener for the arc dps bridge.
        /// </summary>
        public void Restart() {
            this.Stop();
            this.Start(Gw2Mumble.Info.ProcessId);
        }

        private static int GetPort(uint processId) {
            ushort pid;

            unchecked {
                pid = (ushort) processId;
            }

            return pid | (1 << 14) | (1 << 15);
        }

        protected override void Unload() {
            Gw2Mumble.Info.ProcessIdChanged -= Start;
            _stopwatch.Stop();
            _server.Stop();
            this.RenderPresent = false;
        }

        protected override void Update(GameTime gameTime) {
            TimeSpan elapsed;

            lock (WatchLock) {
                elapsed = _stopwatch.Elapsed;
            }

            this.RenderPresent = elapsed < _leeway;
        }

        private void MessageHandler(object sender, MessageData data) {
            switch (data.Message[0]) {
                case (byte) MessageType.ImGui:
                    this.HudIsActive = data.Message[1] != 0;
                    break;
                case (byte) MessageType.CombatArea:
                    ProcessCombat(data.Message, RawCombatEventArgs.CombatEventType.Area);
                    break;
                case (byte) MessageType.CombatLocal:
                    ProcessCombat(data.Message, RawCombatEventArgs.CombatEventType.Local);
                    break;
            }
        }

        private void SocketErrorHandler(object sender, SocketError socketError) {
            var listener = (SocketListener)sender;
            listener.Stop();

            Logger.Error("ArcDpsService stopped with socket error: {0}", socketError.ToString());

            this.Error?.Invoke(this, socketError);
        }

        private void ProcessCombat(byte[] data, RawCombatEventArgs.CombatEventType eventType) {
            var message = CombatParser.ProcessCombat(data);
            OnRawCombatEvent(new RawCombatEventArgs(message, eventType));
        }

        private void OnRawCombatEvent(RawCombatEventArgs e) {
            this.RawCombatEvent?.Invoke(this, e);
        }

        private enum MessageType {

            ImGui       = 1,
            CombatArea  = 2,
            CombatLocal = 3

        }

    }

}