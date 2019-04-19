using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Readers;
using Blish_HUD.Pathing.Entities;
using Blish_HUD.Pathing.Format;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Pathables {
    public class TacOTrailPathable : LoadedTrailPathable, ITacOPathable {

        private string _type;
        private string _trlFilePath;

        public string Type {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public float FadeNear {
            get => this.ManagedEntity.FadeNear;
            set => this.ManagedEntity.FadeNear = value;
        }

        public float FadeFar {
            get => this.ManagedEntity.FadeFar;
            set => this.ManagedEntity.FadeFar = value;
        }

        public string TrlFilePath {
            get => _trlFilePath;
            set => SetProperty(ref _trlFilePath, value);
        }

        public TacOTrailPathable(XmlNode sourceNode, IPackFileSystemContext packContext) : base(sourceNode, packContext) { }

        protected override void PrepareAttributes() {
            base.PrepareAttributes();

            // Type
            RegisterAttribute("type",
                              attribute => (!string.IsNullOrWhiteSpace(this.Type = attribute.Value.Trim())),
                              false);

            // TrailData
            RegisterAttribute("trailData",
                              attribute => (!string.IsNullOrWhiteSpace(
                                   this.TrlFilePath = attribute.Value.Trim()
                                  )
                              ),
                              true);

            // Alpha (alias:Opacity)
            RegisterAttribute("alpha", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.Opacity = fOut;
                return true;
            });

            // FadeNear
            RegisterAttribute("fadeNear", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.FadeNear = fOut;
                return true;
            });

            // FadeFar
            RegisterAttribute("fadeFar", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.FadeFar = fOut;
                return true;
            });

            // AnimationSpeed
            RegisterAttribute("animSpeed", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.AnimationSpeed = fOut;
                return true;
            });

            // TrailScale
            RegisterAttribute("trailScale", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.Scale = fOut;
                return true;
            });

        }

        protected override bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {
            // Process attributes from type category first
            var refCategory = GameService.Pathing.Categories.GetOrAddCategoryFromNamespace(this.Type);

            if (refCategory?.SourceXmlNode?.Attributes != null) {
                ProcessAttributes(refCategory.SourceXmlNode.Attributes);
            }

            refCategory?.AddPathable(this);
            
            // Load trl file
            var sectionData = TrlReader.ReadStream(this.PackContext.LoadFileStream(this.TrlFilePath));

            if (!sectionData.Any()) return false;

            sectionData.ForEach(t => {
                this.MapId = t.MapId;
                this.ManagedEntity.AddSection(t.TrailPoints);
            });

            // Finalize attributes
            if (attributeLoaders.ContainsKey("trailscale")) {
                if (!attributeLoaders["trailscale"].Loaded) {
                    this.Scale = 1f;
                }
            }

            if (attributeLoaders.ContainsKey("animspeed")) {
                if (!attributeLoaders["animspeed"].Loaded) {
                    this.ManagedEntity.AnimationSpeed = 0.5f;
                }
            }

            // Let base finalize attributes
            return base.FinalizeAttributes(attributeLoaders);
        }

    }
}
