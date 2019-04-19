using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Entities.Primitives;
using Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Behavior;
using Blish_HUD.Pathing;
using Blish_HUD.Pathing.Entities;
using Blish_HUD.Pathing.Format;
using Blish_HUD.Pathing.Markers;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules.MarkersAndPaths.PackFormat.TacO.Pathables {
    public class TacOMarkerPathable : LoadedMarkerPathable, ITacOPathable {


        /*
        public int    Behavior     { get; set; }
        */

        private string _type;
        private float  _fadeNear     = -1;
        private float  _fadeFar      = -1;
        private float  _heightOffset = 1.5f;
        private int    _resetLength;
        private bool   _autoTrigger;
        private bool   _hasCountdown;
        private float  _triggerRange;
        private int    _tacOBehavior;

        public string Type {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public float FadeNear {
            get => Math.Min(_fadeNear, _fadeFar);
            set => SetProperty(ref _fadeNear, value);
        }

        public float FadeFar {
            get => Math.Max(_fadeNear, _fadeFar);
            set => SetProperty(ref _fadeFar, value);
        }

        public float HeightOffset {
            get => this.ManagedEntity.VerticalOffset;
            set { this.ManagedEntity.VerticalOffset = value; OnPropertyChanged(); }
        }

        public int ResetLength {
            get => _resetLength;
            set => SetProperty(ref _resetLength, value);
        }

        public bool AutoTrigger {
            get => _autoTrigger;
            set => SetProperty(ref _autoTrigger, value);
        }

        public bool HasCountdown {
            get => _hasCountdown;
            set => SetProperty(ref _hasCountdown, value);
        }

        public float TriggerRange {
            get => _triggerRange;
            set => SetProperty(ref _triggerRange, value);
        }

        public int TacOBehavior {
            get => _tacOBehavior;
            set {
                if (SetProperty(ref _tacOBehavior, value))
                    this.Behavior = new BasicTOBehavior<ManagedPathable<Marker>, Marker>(this, (TacO.Behavior.TacOBehavior)_tacOBehavior);
            }
        }

        public TacOMarkerPathable(XmlNode sourceNode, IPackFileSystemContext packContext) : base(sourceNode, packContext) { }

        protected override void PrepareAttributes() {
            // Type
            RegisterAttribute("type",
                              attribute => (!string.IsNullOrWhiteSpace(this.Type = attribute.Value.Trim())),
                              false);

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

            // IconSize
            RegisterAttribute("iconSize", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.ManagedEntity.AutoResizeBillboard = false;
                this.ManagedEntity.Size = new Vector2(fOut * 2);
                return true;
            });

            // HeightOffset
            RegisterAttribute("heightOffset", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.HeightOffset = fOut;
                return true;
            });

            // ResetLength
            RegisterAttribute("resetLength", delegate (XmlAttribute attribute) {
                if (!int.TryParse(attribute.Value, out int iOut)) return false;

                this.ResetLength = iOut;
                return true;
            });

            // AutoTrigger
            RegisterAttribute("autoTrigger", delegate (XmlAttribute attribute) {
                this.AutoTrigger = (attribute.Value == "0");
                return true;
            });

            // AutoTrigger
            RegisterAttribute("hasCountdown", delegate (XmlAttribute attribute) {
                this.HasCountdown = (attribute.Value == "0");
                return true;
            });

            // TriggerRange
            RegisterAttribute("triggerRange", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.TriggerRange = fOut;
                return true;
            });

            // Taco Behavior
            RegisterAttribute("behavior", delegate (XmlAttribute attribute) {
                if (!int.TryParse(attribute.Value, out int iOut)) return false;

                this.TacOBehavior = iOut;
                return true;
            });
            
            base.PrepareAttributes();
        }

        protected override bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {
            // Process attributes from type category first
            var refCategory = GameService.Pathing.Categories.GetOrAddCategoryFromNamespace(this.Type);

            if (refCategory?.SourceXmlNode?.Attributes != null) {
                ProcessAttributes(refCategory.SourceXmlNode.Attributes);
            }

            refCategory?.AddPathable(this);

            // Finalize attributes
            if (attributeLoaders.ContainsKey("heightoffset")) {
                if (!attributeLoaders["heightoffset"].Loaded) {
                    this.HeightOffset = 1.5f;
                    this.ManagedEntity.VerticalConstraint = BillboardVerticalConstraint.CameraPosition;
                }
            }
            if (attributeLoaders.ContainsKey("iconsize")) {
                if (!attributeLoaders["iconsize"].Loaded) {
                    this.ManagedEntity.Size = new Vector2(2f);
                }
            }

            // Let base finalize attributes
            return base.FinalizeAttributes(attributeLoaders);
        }

        public override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if (this.FadeFar >= 0 && this.FadeNear >= 0) {
                float baseLeft = Utils.World.WorldToGameCoord(this.ManagedEntity.DistanceFromCamera) - this.FadeNear;
                float baseRange = this.FadeFar - this.FadeNear;
                this.ManagedEntity.Opacity = 1 - MathHelper.Clamp(baseLeft / baseRange, 0f, 1f);
            }
        }

    }
}
