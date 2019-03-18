using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.Compatibility.TacO {

    public class TrailSection {

        public TrailOld AssociatedTrail { get; protected set; }

        public Vector3[] SectionData { get; protected set; }
        public VertexPositionColorTexture[] VertexData { get; protected set; }

        private float _distance;
        public float Distance {
            get {
                // Lazy load the trail length
                if (_distance < 1) {
                    _distance = 0;

                    for (int i = 0; i < this.SectionData.Length - 1; i++) {
                        _distance += Vector3.Distance(this.SectionData[i], this.SectionData[i + 1]);
                    }
                }

                return _distance;
            }
        }

        public TrailSection(TrailOld assocTrail, IEnumerable<Vector3> posData) {
            this.AssociatedTrail = assocTrail;
            this.SectionData = posData.ToArray();

            // TODO: This actually needs to be part of a factory function and not the constructor - this won't give the desired result
            // All trail sections must contain at least two points in order to render - trash anything else
            if (!(this.SectionData.Length > 1)) return;

            this.VertexData = new VertexPositionColorTexture[this.SectionData.Length * 2];

            // TODO: Move this into the shader itself (assuming the distance can be calculated for the texture norms)
            float imgScale = this.AssociatedTrail.Texture.Width / 2f / 256f;

            float pastDistance = 0;

            // First point is actually handled last
            var offsetDirection = new Vector3(0, 0, -1);
            var prevPoint = this.SectionData[0];

            for (int i = 1; i < this.SectionData.Length; i++) {
                var currPoint = this.SectionData[i];

                pastDistance += Vector3.Distance(currPoint, prevPoint);

                var pathDirection = currPoint - prevPoint;

                pathDirection.Normalize();

                var offset = Vector3.Cross(pathDirection, offsetDirection);

                if (i == 1) {
                    var firstLeftPoint = prevPoint + (offset * imgScale);
                    var firstRightPoint = prevPoint + (offset * -imgScale);

                    this.VertexData[1] = new VertexPositionColorTexture(firstLeftPoint, Color.White, new Vector2(0, this.Distance));
                    this.VertexData[0] = new VertexPositionColorTexture(firstRightPoint, Color.White, new Vector2(1, this.Distance));
                }

                // expanded points
                var leftPoint = currPoint + (offset * imgScale);
                var rightPoint = currPoint + (offset * -imgScale);

                this.VertexData[i * 2 + 1] = new VertexPositionColorTexture(leftPoint, Color.White, new Vector2(0, this.Distance - pastDistance / imgScale / 2));
                this.VertexData[i * 2]     = new VertexPositionColorTexture(rightPoint, Color.White, new Vector2(1, this.Distance - pastDistance / imgScale / 2));

                prevPoint = currPoint;
            }
        }
    }

    public class TrailOld {

        public string Type { get; set; }
        public int MapId { get; set; }
        public string GUID { get; set; }
        public int FadeNear { get; set; }
        public int FadeFar { get; set; }

        public Vector3[][] TrailData { get; set; }
        public VertexPositionColorTexture[] vertData { get; set; }
        public TrailSection[] Sections { get; set; }

        public float AnimSpeed { get; set; }
        public Texture2D Texture { get; set; }

        public static TrailOld FromTrlFile(string trlFile, Texture2D pathTexture) {
            if (!File.Exists(trlFile)) {
                Console.WriteLine("No trl file found at " + trlFile);
                return null;
            }

            var tacoTrl = new TrailOld() {Texture = pathTexture};

            byte[] rawTacoTrlData = File.ReadAllBytes(trlFile);

            // 32 bit, little-endian
            using (var mReader = new MemoryStream(rawTacoTrlData)) {
                using (var bReader = new BinaryReader(mReader, Encoding.ASCII)) {
                    // First four bytes are just 0000 to signify the first path section
                    bReader.ReadInt32();

                    tacoTrl.MapId = bReader.ReadInt32();

                    var trlSections = new List<TrailSection>();

                    var trailPoints = new List<Vector3>();

                    while (bReader.PeekChar() != -1) {
                        float x = bReader.ReadSingle();
                        float z = bReader.ReadSingle();
                        float y = bReader.ReadSingle();

                        // Indicates a new section in the trail (trails do not always consist of only one "section")
                        if (x == 0 && y == 0 && z == 0) {
                            // Copy current trail to current trailSet
                            var nTrlSection = new TrailSection(tacoTrl, trailPoints);
                            if (nTrlSection.VertexData != null) {
                                trlSections.Add(nTrlSection);
                            }
                            trailPoints.Clear();
                        } else {
                            trailPoints.Add(new Vector3(x, y, z));
                        }
                    }

                    if (trailPoints.Count > 0) {
                        trlSections.Add(new TrailSection(tacoTrl, trailPoints));
                        trailPoints.Clear();
                    }

                    tacoTrl.Sections = trlSections.ToArray();
                }
            }

            return tacoTrl;
        }

    }
}
