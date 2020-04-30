using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Blish_HUD.ArcDps;
using Blish_HUD.ArcDps.Common;
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

        private readonly ConcurrentDictionary<uint, ConcurrentBag<Action<object, RawCombatEventArgs>>> _subscriptions =
            new ConcurrentDictionary<uint, ConcurrentBag<Action<object, RawCombatEventArgs>>>();

        private bool _hudIsActive;

        private SocketListener _server;

        private Stopwatch _stopwatch;

        private bool _subscribed;

        /// <summary>
        ///     Provides common fields that multiple modules might want to track
        /// </summary>
        public CommonFields Common { get; private set; }

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

        public void SubscribeToCombatEventId(Action<object, RawCombatEventArgs> func, params uint[] skillIds)
        {
            if (!_subscribed)
            {
                RawCombatEvent += DispatchSkillSubscriptions;
                _subscribed = true;
            }

            foreach (var skillId in skillIds)
            {
                if (!_subscriptions.ContainsKey(skillId))
                    _subscriptions.TryAdd(skillId, new ConcurrentBag<Action<object, RawCombatEventArgs>>());

                _subscriptions[skillId].Add(func);
            }
        }

        private void DispatchSkillSubscriptions(object sender, RawCombatEventArgs eventHandler)
        {
            if (eventHandler.CombatEvent.Ev == null)
                return;
            var skillId = eventHandler.CombatEvent.Ev.SkillId;
            if (!_subscriptions.ContainsKey(skillId))
                return;

            foreach (var action in _subscriptions[skillId]) action(sender, eventHandler);
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
            Common = new CommonFields();
            _stopwatch = new Stopwatch();
            _server = new SocketListener(10, 200_000);
            _server.ReceivedMessage += MessageHandler;
#if DEBUG
            RawCombatEvent += (a, b) => { Interlocked.Increment(ref Counter); };
#endif
        }

        protected override void Load()
        {
            //_server.Start(new IPEndPoint(IPAddress.Loopback, 8214));
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
                case (byte) MessageType.CombatArea:
                    ProcessCombat(data.Message, RawCombatEventArgs.CombatEventType.Area);
                    break;
                case (byte) MessageType.CombatLocal:
                    ProcessCombat(data.Message, RawCombatEventArgs.CombatEventType.Local);
                    break;
            }
        }

        private void ProcessCombat(byte[] data, RawCombatEventArgs.CombatEventType eventType)
        {
            var message = CombatParser.ProcessCombat(data);
            OnRawCombatEvent(new RawCombatEventArgs(message, eventType));
        }

        private void OnRawCombatEvent(RawCombatEventArgs e)
        {
            RawCombatEvent?.Invoke(this, e);
        }

        private enum MessageType
        {
            ImGui = 1,
            CombatArea = 2,
            CombatLocal = 3
        }
    }
}