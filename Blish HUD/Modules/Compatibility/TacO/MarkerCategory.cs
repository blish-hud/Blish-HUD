using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace Blish_HUD.Modules.Compatibility.TacO {
    public class MarkerCategory {

        public string Name { get; set; }
        public string DisplayName { get; set; }

        private string _iconFile;
        public string IconFile {
            get => _iconFile;
            set {
                //if (this.IconTexture != null) this.IconTexture.Dispose();

                if (this._iconFile == value || value == null) return;

                GameService.Content.PurgeTextureCache(value);
                
                string texturePath = Path.Combine("taco", value);
                if (File.Exists(texturePath)) {
                    // TODO: Have this use the content class' LoadTexture method
                    this.IconTexture = Utils.Pipeline.TextureFromFile(GameService.Graphics.GraphicsDevice, texturePath);
                    //this.IconTexture = GameService.Content.GetTexture("footsteps");
                }

                _iconFile = texturePath;
            }
        }

        public Texture2D IconTexture { get; set; }

        //public Texture2D IconTexture {
        //    get {
        //        return 
        //            GameServices.GetService<ContentService>().GetTexture(
        //                File.Exists(this.IconFile) ? 
        //                    this.IconFile : 
        //                    "footsteps");
        //    }
        //}

        protected float _iconSize;
        public float IconSize { get { return _iconSize; } set { _iconSize = value; } }

        protected float _heightOffset;
        public float HeightOffset { get { return _heightOffset; } set { _heightOffset = value; } }

        public int Behavior { get; set; } // TODO: Implement

        protected float _fadeFar;
        public float FadeFar { get { return _fadeFar; } set { _fadeFar = value; } }

        protected float _fadeNear;
        public float FadeNear { get { return _fadeNear; } set { _fadeNear = value; } }

        public int ResetLength { get; set; } // TODO: Implement

        protected float _minSize;
        public float MinSize { get { return _minSize; } set { _minSize = value; } }

        protected float _alpha;
        public float Alpha { get { return _alpha; } set { _alpha = value; } }


        public Dictionary<string, MarkerCategory> SubCategories { get; protected set; }

        public void AppendCategories(MarkerCategory newCategory) {
            if (this.SubCategories.ContainsKey(newCategory.Name)) {
                foreach (var subCat in newCategory.SubCategories.Values) {
                    this.SubCategories[newCategory.Name].AppendCategories(subCat);
                }
            } else {
                this.SubCategories.Add(newCategory.Name, newCategory);
            }
        }

        public MarkerCategory() {
            this.SubCategories = new Dictionary<string, MarkerCategory>();
        }

        public static MarkerCategory FromXmlNode(XmlNode node) {
            string name = node.Attributes["name"]?.InnerText.ToLower();
            string displayName = node.Attributes["DisplayName"]?.InnerText;

            if (name == null || displayName == null) return null;

            var tacoCategory = new MarkerCategory() {
                Name = name,
                DisplayName = displayName,
                IconFile = node.Attributes["iconFile"]?.InnerText
            };

            float.TryParse(node.Attributes["iconSize"]?.InnerText, out tacoCategory._iconSize);
            float.TryParse(node.Attributes["heightOffset"]?.InnerText, out tacoCategory._heightOffset);
            float.TryParse(node.Attributes["fadeFar"]?.InnerText, out tacoCategory._fadeFar);
            float.TryParse(node.Attributes["FadeStart"]?.InnerText, out tacoCategory._fadeNear);
            float.TryParse(node.Attributes["alpha"]?.InnerText, out tacoCategory._alpha);

            tacoCategory._fadeFar /= 50;
            tacoCategory._fadeNear /= 50;

            return tacoCategory;
        }

    }
}
