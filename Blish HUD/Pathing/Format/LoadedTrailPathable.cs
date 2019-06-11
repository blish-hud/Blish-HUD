using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Modules.MarkersAndPaths;
using Blish_HUD.Pathing.Content;
using Blish_HUD.Pathing.Entities;
using Blish_HUD.Pathing.Trails;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Pathing.Format {

    public abstract class LoadedTrailPathable : LoadedPathable<Entities.ScrollingTrail> {

        private string _textureReferencePath;

        public override float Scale {
            get => this.ManagedEntity.Scale;
            set {
                this.ManagedEntity.Scale = value;
                OnPropertyChanged();
            }
        }

        public string TextureReferencePath {
            get => _textureReferencePath;
            set {
                if (SetProperty(ref _textureReferencePath, value) && this.Active) {
                    LoadTexture();
                }
            }
        }

        public Texture2D Texture {
            get => this.ManagedEntity.TrailTexture;
            set {
                this.ManagedEntity.TrailTexture = value;
                OnPropertyChanged();
            }
        }

        public LoadedTrailPathable(PathableResourceManager pathableContext) : base(new ScrollingTrail(), pathableContext) { }

        protected override void PrepareAttributes() {
            base.PrepareAttributes();

            // ITrail:Texture
            RegisterAttribute("texture", delegate(XmlAttribute attribute) {
                                  if (!string.IsNullOrEmpty(attribute.Value)) {
                                      this.TextureReferencePath = attribute.Value.Trim();
                                                                           //.Replace('\\', Path.DirectorySeparatorChar)
                                                                           //.Replace('/',  Path.DirectorySeparatorChar);

                                      return true;
                                  }

                                  return false;
                              });

        }

        public override void OnLoading(EventArgs e) {
            base.OnLoading(e);

            LoadTexture();
        }

        public override void OnUnloading(EventArgs e) {
            base.OnUnloading(e);

            UnloadTexture();
        }

        private void LoadTexture() {
            if (!string.IsNullOrEmpty(_textureReferencePath)) {
                this.Texture = this.PathableManager.LoadTexture(_textureReferencePath);
            }
        }

        private void UnloadTexture() {
            this.Texture = null;
            this.PathableManager.MarkTextureForDisposal(_textureReferencePath);
        }

    }

}
