using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Blish_HUD.ArcDps;
using Blish_HUD.GameServices.ArcDps;
using Blish_HUD.GameServices.ArcDps.V2;
using Blish_HUD.GameServices.ArcDps.V2.Models;
using Microsoft.Xna.Framework;

namespace Blish_HUD {

    public class ArcDpsServiceV2 : GameService {
        private static readonly Logger Logger = Logger.GetLogger<ArcDpsServiceV2>();

        /// <summary>
        /// The timespan after which ArcDPS is treated as not responding.
        /// </summary>
        private readonly TimeSpan _leeway = TimeSpan.FromMilliseconds(1000);
        private readonly CancellationTokenSource _arcDpsClientCancellationTokenSource = new CancellationTokenSource();
        private IArcDpsClient _arcDpsClient;
        private bool _hudIsActive;
        private Stopwatch _stopwatch;
        private bool _subscribed;

#if DEBUG
        public static long Counter => ArcDpsClient.Counter;
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
        ///     Indicates if the socket listener for the arcdps service is running and arcdps sent an update in the last second.
        /// </summary>
        public bool Running => this._arcDpsClient?.Client.Connected ?? false && this.RenderPresent;

        /// <summary>
        ///     Indicates if arcdps currently draws its HUD (not in character select, cut scenes or loading screens)
        /// </summary>
        public bool HudIsActive {
            get {
                lock (_stopwatch) {
                    return _hudIsActive;
                }
            }
            private set {
                lock (_stopwatch) {
                    _stopwatch.Restart();
                    _hudIsActive = value;
                }
            }
        }

        public void RegisterMessageType<T>(int type, Func<T, CancellationToken, Task> listener)
            where T : struct {
            _arcDpsClient.RegisterMessageTypeListener(type, listener);
        }

        protected override void Initialize() {
            this.Common             = new CommonFields();
            _stopwatch              = new Stopwatch();
            _arcDpsClient           = new ArcDpsClient(ArcDpsBridgeVersion.V1);
            _arcDpsClient.Error += SocketErrorHandler;
        }

        protected override void Load() {
            Gw2Mumble.Info.ProcessIdChanged += Start;
            _stopwatch.Start();
            this.RegisterMessageType<CombatCallback>(0, async (combatEvent, ct) => {
                System.Diagnostics.Debug.WriteLine("");
            });
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
                _arcDpsClient.Initialize(new IPEndPoint(IPAddress.Loopback, GetPort(processId)), _arcDpsClientCancellationTokenSource.Token);
            }
        }

        private static int GetPort(uint processId) {
            ushort pid;

            unchecked {
                pid = (ushort) processId;
            }

            // +1 for V2 and +0 for V1
            //return (pid | (1 << 14) | (1 << 15)) + 1;
            return (pid | (1 << 14) | (1 << 15));
        }

        protected override void Unload() {
            Gw2Mumble.Info.ProcessIdChanged -= Start;
            _arcDpsClientCancellationTokenSource.Cancel();

            _stopwatch.Stop();
            _arcDpsClient.Disconnect();
            _arcDpsClient.Error -= SocketErrorHandler;
            this.RenderPresent = false;
        }

        protected override void Update(GameTime gameTime) {
            TimeSpan elapsed;

            lock (_stopwatch) {
                elapsed = _stopwatch.Elapsed;
            }

            this.RenderPresent = elapsed < _leeway;
        }

        private void SocketErrorHandler(object sender, SocketError socketError) {
            // Socketlistener stops by itself.
            Logger.Error("Encountered socket error: {0}", socketError.ToString());

            this.Error?.Invoke(this, socketError);
        }
    }

}