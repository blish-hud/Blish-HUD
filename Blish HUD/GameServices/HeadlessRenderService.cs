using Blish_HUD.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MouseEventArgs = Blish_HUD.Input.MouseEventArgs;

namespace Blish_HUD.GameServices {
    public class HeadlessRenderService: GameService {
        private readonly Logger Logger = Logger.GetLogger(typeof(HeadlessRenderService));

        public MemoryMappedFile HeaderMMF = null;
        public MemoryMappedViewAccessor HeaderAccesor = null;
        public int Width = 0 ;
        public int Height = 0;
        const string HEADERMAPNAME = "BlishHUD_Header";
        const int HEADERSIZE = 28;

        private RenderTarget2D _front;
        private RenderTarget2D _back;
        private uint _textureIdx = 1;

        private Mutex _isAliveMtx = new Mutex(true, "Global\\blish_isalive_mutex");

        public void CreateBuffers(GraphicsDevice device) {
            Width = device.PresentationParameters.BackBufferWidth;
            Height = device.PresentationParameters.BackBufferHeight;

            _front = new RenderTarget2D(device, Width, Height, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.PreserveContents, true);
            _back = new RenderTarget2D(device, Width, Height, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.PreserveContents, true);
            _textureIdx = 1;

            HeaderAccesor.Write(0, Width);
            HeaderAccesor.Write(4, Height);
            HeaderAccesor.Write(8, _textureIdx);
            HeaderAccesor.Write(12, _front.GetSharedHandle().ToInt64());
            HeaderAccesor.Write(20, _back.GetSharedHandle().ToInt64());

            Logger.Debug("Created {}x{} textures", Width, Height);
        }

        protected override void Initialize() {
            if (!ApplicationSettings.Instance.Headless) {
                return;
            }

            Logger.Warn("Going headless: this is very experimental.");

            HeaderMMF = MemoryMappedFile.CreateOrOpen(HEADERMAPNAME, HEADERSIZE, MemoryMappedFileAccess.ReadWrite);
            HeaderAccesor = HeaderMMF.CreateViewAccessor(0, HEADERSIZE, MemoryMappedFileAccess.ReadWrite);

            BlishHud.Instance.IsHeadless = true;

            Graphics.BeforeRender += (sender, e) => {
                var device = e.Device;
                if (_front == null || _back == null) {
                    CreateBuffers(device);
                }

                var pp = device.PresentationParameters;
                if (pp.BackBufferWidth != Width || pp.BackBufferHeight != Height) {
                    CreateBuffers(device);
                }

                device.SetRenderTarget(_back);
            };

            Graphics.AfterRender += (sender, e) => {
                e.Device.SetRenderTarget(null);

                (_front, _back) = (_back, _front);
                _textureIdx ^= 1;
                HeaderAccesor.Write(8, _textureIdx);
            };
        }

        protected override void Load() {}

        protected override void Unload() {}

        protected override void Update(GameTime gameTime) {}
    }
}