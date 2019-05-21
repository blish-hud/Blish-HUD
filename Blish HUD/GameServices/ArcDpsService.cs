using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using ProtoBuf;

namespace Blish_HUD
{
    public class ArcDpsService : GameService
    {
        public bool ArcPresent;
        private CancellationToken Cancel;
        public bool HudIsActive;
        private Mutex Mut;
        private NamedPipeServerStream Server;
        private Stopwatch Watch;

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
                    try
                    {
                        Server.WaitForConnection();
                        var obj = Serializer.Deserialize<Arc>(Server);
                        Server.Disconnect();

                        if (obj.Msgtype == Mtype.Imgui)
                        {
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
                        else

                        {
                            ProcessArc(obj);
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
            }, Cancel);
        }

        private void ProcessArc(Arc obj)
        {
            switch (obj.Msgtype)
            {
                case Mtype.NoMsg:
                    break;
                case Mtype.Greeting:
                    ArcPresent = obj.Greeting;
                    break;
                case Mtype.Imgui:
                    break;
            }
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