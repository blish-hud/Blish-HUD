using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Blish_HUD
{
    public class ArcDpsService : GameService
    {
        public bool HudIsActive = false;
        private NamedPipeServerStream Server;
        private Stopwatch Watch;
        private CancellationToken Cancel;
        private Mutex Mut;

        protected override void Initialize()
        {
            Server = new NamedPipeServerStream("BHUDrender");
            Mut = new Mutex(false);
        }

        protected override void Load()
        {
            Cancel = new CancellationToken(false);
            Watch = Stopwatch.StartNew();
            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        Server.WaitForConnection();

                        var reader = new StreamReader(Server);
                        reader.ReadLine();

                        Server.Disconnect();
                        Mut.WaitOne();
                        try
                        {
                            Watch.Restart();
                        }
                        finally
                        {
                            Mut.ReleaseMutex();
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }, Cancel);
        }

        protected override void Unload()
        {
            Cancel = new CancellationToken(true);
        }

        protected override void Update(GameTime gameTime)
        {
            Mut.WaitOne();
            try
            {
                HudIsActive = Watch.ElapsedMilliseconds < 0.5 * 1000.0;
            }
            finally
            {
                Mut.ReleaseMutex();
            }
        }
    }
}