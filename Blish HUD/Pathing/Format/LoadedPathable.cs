﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Blish_HUD.Modules.MarkersAndPaths;
using Blish_HUD.Pathing.Behaviors;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing.Format {

    public abstract class LoadedPathable<TEntity> : ManagedPathable<TEntity>
        where TEntity : Blish_HUD.Entities.Entity {

        public event EventHandler<EventArgs> Loading;
        public event EventHandler<EventArgs> Loaded;
        public event EventHandler<EventArgs> Unloading;
        public event EventHandler<EventArgs> Unloaded;

        private bool _active = false;

        public bool SuccessfullyLoaded { get; private set; } = false;

        protected IPackFileSystemContext PackContext { get; }

        public override bool Active {
            get => _active;
            set {
                if (SetProperty(ref _active, value)) {
                    if (_active)
                        LoadResources();
                    else 
                        UnloadResources();
                }
            }
        }

        private Dictionary<string, LoadedPathableAttributeDescription> _attributeLoaders;
        private List<XmlAttribute> _leftOverAttributes;

        public LoadedPathable(TEntity pathableEntity, IPackFileSystemContext packContext) : base(pathableEntity) {
            this.PackContext = packContext;
        }

        protected abstract void BeginLoad();

        protected void LoadAttributes(XmlNode sourceNode) {
            _attributeLoaders = new Dictionary<string, LoadedPathableAttributeDescription>();
            _leftOverAttributes = new List<XmlAttribute>();

            PrepareAttributes();

            if (sourceNode.Attributes != null) {
                ProcessAttributes(sourceNode.Attributes);
            }

            var requiredAttributesRemain = false;
            foreach (KeyValuePair<string, LoadedPathableAttributeDescription> attributeDescription in _attributeLoaders) {
                if (attributeDescription.Value.Required && !attributeDescription.Value.Loaded) {
                    // Required attribute wasn't found in the node
                    Console.WriteLine($"Required attribute '{attributeDescription.Key}' could not be found in the pathable, so it will not be displayed:");
                    Console.WriteLine(sourceNode.ToString(3));
                    requiredAttributesRemain = true;
                }
            }

            if (!requiredAttributesRemain) {
                this.SuccessfullyLoaded = FinalizeAttributes(_attributeLoaders);
            }

            if (this.SuccessfullyLoaded) {
                AssignBehaviors();
            }

            _attributeLoaders = null;
            _leftOverAttributes = null;
        }

        protected void ProcessAttributes(XmlAttributeCollection attributes) {
            foreach (XmlAttribute attribute in attributes) {
                if (_attributeLoaders.TryGetValue(attribute.Name.ToLower(), out LoadedPathableAttributeDescription attributeDescription)) {
                    if (attributeDescription.LoadAttributeFunc.Invoke(attribute)) {
                        attributeDescription.Loaded = true;
                    } else if (attributeDescription.Required) {
                        // This was a required attribute and it failed to load
                        // We can stop loading it since it is no longer valid
                        Console.WriteLine($"[🛑] Required attribute '{attribute.Name}' failed to load for pathable, so it will not be displayed.");
                        break;
                    } else {
                        // Attribute was optional, so we report and move along
                        Console.WriteLine($"[⚠] Optional attribute '{attribute.Name}' could not be loaded for the pathable.");
                    }
                } else {
                    // Attribute was never defined for loading
                    //Console.WriteLine($"[ℹ] Attribute '{attribute.Name}' does not have a marker description to load it, so it will be added to left overs.");
                    _leftOverAttributes.Add(attribute);
                }
            }
        }

        protected void RegisterAttribute(string attributeName, Func<XmlAttribute, bool> loadAttribute, bool required = false) {
            _attributeLoaders.Add(attributeName.ToLower(),
                                  new LoadedPathableAttributeDescription(loadAttribute, required));
        }

        protected virtual void PrepareAttributes() {
            // IPathable:MapId
            RegisterAttribute("MapId", delegate (XmlAttribute attribute) {
                if (!int.TryParse(attribute.Value, out int iOut)) return false;

                this.MapId = iOut;
                return true;
            });

            // IPathable:GUID
            RegisterAttribute("GUID",
                              attribute => (!string.IsNullOrEmpty(this.Guid = attribute.Value.TrimEnd('='))),
                              false);

            // IPathable:Opacity
            RegisterAttribute("opacity", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.Opacity = fOut;
                return true;
            });

            // IPathable:Position (X)
            RegisterAttribute("xPos", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                _xPos = fOut;
                return true;
            });

            // IPathable:Position (Y)
            RegisterAttribute("yPos", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                _yPos = fOut;
                return true;
            });

            // IPathable:Position (Z)
            RegisterAttribute("zPos", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                _zPos = fOut;
                return true;
            });

            // IPathable:Scale
            RegisterAttribute("scale", delegate (XmlAttribute attribute) {
                if (!float.TryParse(attribute.Value, out float fOut)) return false;

                this.Scale = fOut;
                return true;
            });
        }

        private float _xPos;
        private float _yPos;
        private float _zPos;

        protected virtual bool FinalizeAttributes(Dictionary<string, LoadedPathableAttributeDescription> attributeLoaders) {
            this.Position = new Vector3(_xPos, _zPos, _yPos);

            return true;
        }

        protected virtual void AssignBehaviors() {
            var attrNames = _leftOverAttributes.Select(xmlAttr => xmlAttr.Name.ToLower());

            foreach (var autoBehavior in PathingBehavior.AllAvailableBehaviors) {
                var checkBehavior = IdentifyingBehaviorAttributePrefixAttribute.GetAttributesOnType(autoBehavior);

                if (attrNames.Any(sa => sa.StartsWith(checkBehavior.AttributePrefix))) {
                    var loadedBehavior = Activator.CreateInstance(autoBehavior.MakeGenericType(this.GetType(), typeof(TEntity)), this) as ILoadableBehavior;

                    loadedBehavior.LoadWithAttributes(_leftOverAttributes.Where(sa => sa.Name.ToLower().StartsWith(checkBehavior.AttributePrefix)));
                    this.Behavior.Add((PathingBehavior)loadedBehavior);
                }
            }
        }

        private void LoadResources() {
            OnLoading(EventArgs.Empty);
            OnLoaded(EventArgs.Empty);
        }

        private void UnloadResources() {
            OnUnloading(EventArgs.Empty);
            OnUnloaded(EventArgs.Empty);
        }

        public virtual void OnLoading(EventArgs e) {
            this.Loaded?.Invoke(this, e);
        }

        public virtual void OnLoaded(EventArgs e) {
            this.Loaded?.Invoke(this, e);
        }

        public virtual void OnUnloading(EventArgs e) {
            this.Unloaded?.Invoke(this, e);
        }

        public virtual void OnUnloaded(EventArgs e) {
            this.Unloaded?.Invoke(this, e);
        }

    }

}
