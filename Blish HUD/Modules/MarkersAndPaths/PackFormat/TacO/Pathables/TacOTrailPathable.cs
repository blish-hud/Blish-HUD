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

        private const float DEFAULT_TRAILSCALE     = 1f;
        private const float DEFAULT_ANIMATIONSPEED = 0.5f;

        private string          _type;
        private PathingCategory _category;
        private string          _trlFilePath;

        public string Type {
            get => _type;
            set {
                if (SetProperty(ref _type, value)) {
                    _category = _rootCategory.GetOrAddCategoryFromNamespace(_type);
                    OnPropertyChanged(nameof(Category));
                }
            }
        }

        public PathingCategory Category {
            get => _category;
            set {
                if (SetProperty(ref _category, value)) {
                    _type = _category.Namespace;
                    OnPropertyChanged(nameof(Type));
                }
            }
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

        private readonly XmlNode         _sourceNode;
        private          PathingCategory _rootCategory;

        public TacOTrailPathable(XmlNode sourceNode, IPackFileSystemContext packContext, PathingCategory rootCategory) : base(packContext) {
            _sourceNode   = sourceNode;
            _rootCategory = rootCategory;

            BeginLoad();
        }

        // TODO: Use this method as an opportunity to convert attributes to some sort of IPathingAttribute to keep things
        // consistent between imported file formats
        protected override void BeginLoad() {
            LoadAttributes(_sourceNode);
        }

        protected override void PrepareAttributes() {
            base.PrepareAttributes();

            // Type
            RegisterAttribute("type",
                              attribute => (!string.IsNullOrEmpty(this.Type = attribute.Value.Trim())),
                              false);

            // TrailData
            RegisterAttribute("trailData",
                              attribute => (!string.IsNullOrEmpty(
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
            if (_category?.SourceXmlNode?.Attributes != null) {
                ProcessAttributes(_category.SourceXmlNode.Attributes);
            }

            _category?.AddPathable(this);

            // Load trl file
            using (var trlStream = this.PackContext.LoadFileStream(this.TrlFilePath)) {
                var sectionData = TrlReader.ReadStream(trlStream);

                if (!sectionData.Any()) return false;

                sectionData.ForEach(t => {
                    this.MapId = t.MapId;
                    this.ManagedEntity.AddSection(t.TrailPoints);
                });
            }

            // Finalize attributes
            if (attributeLoaders.ContainsKey("trailscale")) {
                if (!attributeLoaders["trailscale"].Loaded) {
                    this.Scale = DEFAULT_TRAILSCALE;
                }
            }

            if (attributeLoaders.ContainsKey("animspeed")) {
                if (!attributeLoaders["animspeed"].Loaded) {
                    this.ManagedEntity.AnimationSpeed = DEFAULT_ANIMATIONSPEED;
                }
            }

            // Let base finalize attributes
            return base.FinalizeAttributes(attributeLoaders);
        }

    }
}
