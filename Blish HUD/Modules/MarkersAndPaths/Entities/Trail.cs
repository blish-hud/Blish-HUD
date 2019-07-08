using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Modules.MarkersAndPaths;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Readers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.MarkersAndPaths.Entities.Paths {

    [Serializable]
    public class Trail {

        private static Effect _trailEffect;

        public List<Vector3> PathPoints { get; set; } = new List<Vector3>();
        public Texture2D PathTexture { get; set; }
        public int MapId { get; set; }
        public PathingCategory RefCategory { get; set; }


        public VertexPositionColorTexture[] VertexData  { get; set; }

        public string      IconFile;
        public float       Size         = 1.0f;
        public float       Alpha        = 1.0f;
        public float       FadeNear     = -1.0f;
        public float       FadeFar      = -1.0f;
        public float       Height       = 1.5f;
        //public POIBehavior Behavior     = POIBehavior.AlwaysVisible;
        public int         ResetLength  = 0;
        public int         ResetOffset  = 0;
        public int         AutoTrigger  = 0;
        public int         HasCountdown = 0;
        public float       TriggerRange = 2.0f;
        public int         MinSize      = 5;
        public int         MaxSize      = 2048;
        public Color       Color        = Color.White;
        public string      TrailData;
        public int       AnimSpeed = 10;
        public string      Texture;
        public float       TrailScale = 1;
        public string      ToggleCategory;

        private float _distance;
        public float Distance {
            get {
                // Lazy load the trail length
                if (_distance < 1) {
                    _distance = 0;

                    for (int i = 0; i < this.PathPoints.Count - 1; i++) {
                        _distance += Vector3.Distance(this.PathPoints[i], this.PathPoints[i + 1]);
                    }
                }

                return _distance;
            }
        }

        public Trail() {
            //_trailEffect = _trailEffect ?? Overlay.cm.Load<Effect>("effects\\trail");
            _trailEffect = _trailEffect ?? Overlay.cm.Load<Effect>("effects\\tacotrail");
            //this.Visible = true;
        }
        
        public void Update(GameTime gameTime) {
            _trailEffect.Parameters["TotalMilliseconds"].SetValue((float)gameTime.TotalGameTime.TotalMilliseconds);
        }

        //public void Draw(GraphicsDevice graphicsDevice) {
        //    _trailEffect.Parameters["WorldViewProjection"].SetValue(GameService.Camera.View * GameService.Camera.Projection * Matrix.Identity);
        //    _trailEffect.Parameters["Texture"].SetValue(this.PathTexture);
        //    _trailEffect.Parameters["FlowSpeed"].SetValue(this.AnimSpeed);
        //    _trailEffect.Parameters["PlayerPosition"].SetValue(GameService.Player.Position);
        //    _trailEffect.Parameters["FadeOutDistance"].SetValue(3000f);
        //    _trailEffect.Parameters["FullClip"].SetValue(3500f);
        //    _trailEffect.Parameters["FadeDistance"].SetValue(this.PathTexture.Height / 256.0f / 2.0f);
        //    _trailEffect.Parameters["TotalLength"].SetValue(this.Distance                     / this.PathTexture.Height);

        //    foreach (EffectPass trailPass in _trailEffect.CurrentTechnique.Passes) {
        //        trailPass.Apply();

        //        graphicsDevice.DrawUserPrimitives(
        //                                          PrimitiveType.TriangleStrip,
        //                                          this.VertexData,
        //                                          0,
        //                                          this.VertexData.Length - 2
        //                                         );
        //    }
        //}

        public void Draw(GraphicsDevice graphicsDevice) {
            _trailEffect.Parameters["camera"].SetValue(GameService.Camera.View * GameService.Camera.Projection * Matrix.Identity);
            _trailEffect.Parameters["persp"].SetValue(this.PathTexture);
            _trailEffect.Parameters["charpos"].SetValue(GameService.Player.Position);
            _trailEffect.Parameters["data"].SetValue(this.AnimSpeed);
            _trailEffect.Parameters["nearFarFades"].SetValue(new Vector2());
            _trailEffect.Parameters["FullClip"].SetValue(3500f);
            _trailEffect.Parameters["FadeDistance"].SetValue(this.PathTexture.Height / 256.0f / 2.0f);
            _trailEffect.Parameters["TotalLength"].SetValue(this.Distance                     / this.PathTexture.Height);

            foreach (EffectPass trailPass in _trailEffect.CurrentTechnique.Passes) {
                trailPass.Apply();

                graphicsDevice.DrawUserPrimitives(
                                                  PrimitiveType.TriangleStrip,
                                                  this.VertexData,
                                                  0,
                                                  this.VertexData.Length - 2
                                                 );
            }
        }

        public static Trail FromPositions(Texture2D pathTexture, List<Vector3> posData, int mapId) {
            if (!(posData.Count > 1)) {
                Console.WriteLine("Trail data did not contain enough points.");
                return null;
            }

            var trailSection = new Trail();

            trailSection.PathTexture = pathTexture;
            trailSection.VertexData = new VertexPositionColorTexture[posData.Count * 2];
            trailSection.PathPoints = new List<Vector3>(posData);
            trailSection.MapId = mapId;

            float imgScale = pathTexture.Width / 2f / 256f;

            float pastDistance = 0;

            var offsetDirection = new Vector3(0, 0, -1);
            var prevPoint = posData[0];

            for (int i = 1; i < posData.Count; i++) {
                var currPoint = posData[i];

                pastDistance += Vector3.Distance(currPoint, prevPoint);

                var pathDirection = currPoint - prevPoint;

                pathDirection.Normalize();

                var offset = Vector3.Cross(pathDirection, offsetDirection);

                if (i == 1) {
                    var firstLeftPoint = prevPoint + (offset * imgScale);
                    var firstRightPoint = prevPoint + (offset * -imgScale);

                    trailSection.VertexData[1] = new VertexPositionColorTexture(firstLeftPoint,  Color.White, new Vector2(0, trailSection.Distance));
                    trailSection.VertexData[0] = new VertexPositionColorTexture(firstRightPoint, Color.White, new Vector2(1, trailSection.Distance));
                }

                // expanded points
                var leftPoint  = currPoint + (offset * imgScale);
                var rightPoint = currPoint + (offset * -imgScale);

                trailSection.VertexData[i * 2 + 1] = new VertexPositionColorTexture(leftPoint,  Color.White, new Vector2(0, trailSection.Distance - pastDistance / imgScale / 2));
                trailSection.VertexData[i * 2]     = new VertexPositionColorTexture(rightPoint, Color.White, new Vector2(1, trailSection.Distance - pastDistance / imgScale / 2));

                prevPoint = currPoint;
            }

            return trailSection;
        }

        public static List<Trail> FromTrlFile(string trlFile, Texture2D pathTexture, PathingCategory refCategory) {
            if (!File.Exists(trlFile)) {
                Console.WriteLine("No trl file found at " + trlFile);
                return new List<Trail>();
            }

            var trlSections = new List<Trail>();

            byte[] rawTacoTrlData = File.ReadAllBytes(trlFile);

            // 32 bit, little-endian
            using (var mReader = new MemoryStream(rawTacoTrlData)) {
                using (var bReader = new BinaryReader(mReader, Encoding.ASCII)) {
                    // First four bytes are just 0000 to signify the first path section
                    bReader.ReadInt32();

                    int mapId = bReader.ReadInt32();

                    var trailPoints = new List<Vector3>();

                    while (bReader.PeekChar() != -1) {
                        float x = bReader.ReadSingle();
                        float z = bReader.ReadSingle();
                        float y = bReader.ReadSingle();

                        // Indicates a new section in the trail (trails do not always consist of only one "section")
                        if (x == 0 && y == 0 && z == 0) {
                            // Copy current trail to current trailSet
                            var nTrlSection = Trail.FromPositions(pathTexture, trailPoints, mapId);
                            if (nTrlSection != null) {
                                trlSections.Add(nTrlSection);
                            }
                            trailPoints.Clear();
                        } else {
                            trailPoints.Add(new Vector3(x, y, z));
                            //var t = new Blish_HUD.Entities.Marker(pathTexture, new Vector3(x, y, z), Vector2.One);
                            //t.MapId = mapId;
                            //GameService.Pathing.RegisterMarker(t);
                        }
                    }

                    if (trailPoints.Count > 0) {
                        var endTrlSection = Trail.FromPositions(pathTexture, trailPoints, mapId);
                        if (endTrlSection != null) {
                            endTrlSection.RefCategory = refCategory;
                            trlSections.Add(endTrlSection);
                        }

                        trailPoints.Clear();
                    }
                }
            }

            return trlSections;
        }

    }
}
