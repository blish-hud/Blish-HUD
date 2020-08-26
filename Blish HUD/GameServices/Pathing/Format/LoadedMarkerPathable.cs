using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;
using Blish_HUD.Pathing.Markers;
using Blish_HUD.Pathing.Content;
using Blish_HUD.Pathing.Entities;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Format {

    public class LoadedPathableAttributeDescription {

        public Func<PathableAttribute, bool> LoadAttributeFunc { get; }

        public bool Required { get; }

        public bool Loaded { get; set; }

        public LoadedPathableAttributeDescription(Func<PathableAttribute, bool> loadAttributeFunc, bool required) {
            this.LoadAttributeFunc = loadAttributeFunc;
            this.Required = required;
            this.Loaded = false;
        }

    }

    public abstract class LoadedMarkerPathable : LoadedPathable<Entities.Marker>, IMarker {
        
        private float  _minimumSize = 1.0f;
        private float  _maximumSize = 1.0f;
        private string _text;
        private string _iconReferencePath;

        public float MinimumSize {
            get => _minimumSize;
            set => SetProperty(ref _minimumSize, value);
        }

        public float MaximumSize {
            get => _maximumSize;
            set => SetProperty(ref _maximumSize, value);
        }

        public override float Scale {
            get => this.ManagedEntity.Scale;
            set {
                this.ManagedEntity.Scale = value;
                OnPropertyChanged();
            }
        }

        public bool FadeCenter {
            get => this.ManagedEntity.FadeCenter;
            set {
                this.ManagedEntity.FadeCenter = value;
                OnPropertyChanged();
            }
        }

        public string Text {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string IconReferencePath {
            get => _iconReferencePath;
            set {
                if (SetProperty(ref _iconReferencePath, value) && this.Active) {
                    LoadIcon();
                }
            }
        }

        public Texture2D Icon {
            get => this.ManagedEntity.Texture;
            set {
                this.ManagedEntity.Texture = value;
                OnPropertyChanged();
            }
        }

        public LoadedMarkerPathable(PathableResourceManager pathableManager) : base(new Marker(), pathableManager) { }

        protected override void PrepareAttributes() {
            base.PrepareAttributes();

            // IMarker:MinimumSize
            RegisterAttribute("minSize", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.MinimumSize = fOut;
                return true;
            });

            // IMarker:MaximumSize
            RegisterAttribute("maxSize", delegate (PathableAttribute attribute) {
                if (!InvariantUtil.TryParseFloat(attribute.Value, out float fOut)) return false;

                this.MaximumSize = fOut;
                return true;
            });

            // IMarker:Icon
            RegisterAttribute("iconFile", delegate (PathableAttribute attribute) {
                if (this.IconReferencePath != null) return true;

                if (!string.IsNullOrEmpty(attribute.Value)) {
                    this.IconReferencePath = attribute.Value.Trim();

                    return true;
                }

                return false;
            });

            // Marker:FadeCenter
            RegisterAttribute("fadeCenter", attribute => {
                if (bool.TryParse(attribute.Value, out bool bOut)) {
                    this.FadeCenter = bOut;
                    return true;
                } else {
                    this.FadeCenter = false;
                    return false;
                }
            });

            // IMarker:Text
            RegisterAttribute("text", attribute => (!string.IsNullOrEmpty(this.Text = attribute.Value)));
        }

        public override void OnLoading(EventArgs e) {
            base.OnLoading(e);

            LoadIcon();
        }

        public override void OnUnloading(EventArgs e) {
            base.OnUnloading(e);

            UnloadIcon();
        }

        private void LoadIcon() {
            if (!string.IsNullOrEmpty(_iconReferencePath)) {
                this.Icon = this.PathableManager.LoadTexture(_iconReferencePath);
            }
        }
        
        private void UnloadIcon() {
            this.Icon = null;
            this.PathableManager.MarkTextureForDisposal(_iconReferencePath);
        }

    }
}
