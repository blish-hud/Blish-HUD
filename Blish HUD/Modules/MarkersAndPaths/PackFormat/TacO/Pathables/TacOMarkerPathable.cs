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

        private const float DEFAULT_HEIGHTOFFSET = 1.5f;
        private const float DEFAULT_ICONSIZE     = 2f;

        private string          _type;
        private PathingCategory _category;
        private float           _fadeNear     = -1;
        private float           _fadeFar      = -1;
        private int             _resetLength;
        private bool            _autoTrigger;
        private bool            _hasCountdown;
        private float           _triggerRange;
        private int             _tacOBehaviorId;

        private BasicTOBehavior<ManagedPathable<Marker>, Marker> _tacOBehavior;

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

        public int TacOBehaviorId {
            get => _tacOBehaviorId;
            set {
                if (SetProperty(ref _tacOBehaviorId, value)) {
                    this.Behavior.Remove(_tacOBehavior);

                    _tacOBehavior = new BasicTOBehavior<ManagedPathable<Marker>, Marker>(this, (TacOBehavior)_tacOBehaviorId);

                    this.Behavior.Add(_tacOBehavior);
                }
            }
        }

        private readonly XmlNode         _sourceNode;
        private readonly PathingCategory _rootCategory;

        public TacOMarkerPathable(XmlNode sourceNode, IPackFileSystemContext packContext, PathingCategory rootCategory) : base(packContext) {
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
            // Type
            RegisterAttribute("type", attribute => (!string.IsNullOrEmpty(this.Type = attribute.Value.Trim())));

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

                this.TacOBehaviorId = iOut;
                return true;
            });
            
            base.PrepareAttributes();
        }

        protected override bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {
            // Process attributes from type category first
            if (_category?.SourceXmlNode?.Attributes != null) {
                ProcessAttributes(_category.SourceXmlNode.Attributes);
            }

            _category?.AddPathable(this);

            // Finalize attributes
            if (attributeLoaders.ContainsKey("heightoffset")) {
                if (!attributeLoaders["heightoffset"].Loaded) {
                    this.HeightOffset = DEFAULT_HEIGHTOFFSET;
                    this.ManagedEntity.VerticalConstraint = BillboardVerticalConstraint.CameraPosition;
                }
            }
            if (attributeLoaders.ContainsKey("iconsize")) {
                if (!attributeLoaders["iconsize"].Loaded) {
                    this.ManagedEntity.Size = new Vector2(DEFAULT_ICONSIZE);
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
