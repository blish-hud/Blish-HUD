using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Blish_HUD.Pathing.Behaviors;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Pathing {
    
    public enum UserAccess {
        /// <summary>
        /// The user should not be provided the option to toggle the visibility of
        /// the <see cref="ManagedPathable{TEntity}"/> or makes changes to it.
        /// </summary>
        None,
        /// <summary>
        /// The user may be provided the option to toggle the visibility of the
        /// <see cref="ManagedPathable{TEntity}"/> or its sub-pathables.
        /// </summary>
        View,
        /// <summary>
        /// The user may be provided the option to modify the <see cref="ManagedPathable{TEntity}"/>.
        /// </summary>
        Modify,
        /// <summary>
        /// The user may be provided the option to modify or toggle the visibility
        /// of the <see cref="ManagedPathable{TEntity}"/> or its sub pathables.
        /// </summary>
        ViewAndModify,
    }

    public abstract class ManagedPathable<TEntity> : IPathable<TEntity>, INotifyPropertyChanged
        where TEntity : Blish_HUD.Entities.Entity {

        private readonly TEntity _managedEntity;
        //private List<PathingBehavior<ManagedPathable<TEntity>, TEntity>> _behaviors = new List<PathingBehavior<ManagedPathable<TEntity>, TEntity>>();
        private List<PathingBehavior> _behaviors = new List<PathingBehavior>();
        private int                   _mapId     = -1;
        private UserAccess            _access    = UserAccess.None;
        private string                _guid;

        /// <summary>
        /// Used by the Pathing service to either add or remove this path
        /// from the list of renderables whenever a map change occurs.  A
        /// MapId of -1 will prevent the pathing service from managing if
        /// this entity renders or not.
        /// </summary>
        public int MapId {
            get => _mapId;
            set => SetProperty(ref _mapId, value);
        }

        public string Guid {
            // Load pre-set GUID or lazy-load new one
            get => _guid ?? (_guid = GetGuid());
            set => SetProperty(ref _guid, value);
        }

        public UserAccess Access {
            get => _access;
            set => SetProperty(ref _access, value);
        }

        public abstract bool Active { get; set; }

        public abstract float Scale { get; set; }

        public float Opacity {
            get => this.ManagedEntity.Opacity;
            set { this.ManagedEntity.Opacity = value; OnPropertyChanged(); }
        }

        public Vector3 Position {
            get => this.ManagedEntity.Position;
            set { this.ManagedEntity.Position = value; OnPropertyChanged(); }
        }

        public List<PathingBehavior> Behavior {
            get => _behaviors;
            set => SetProperty(ref _behaviors, value);
        }

        public TEntity ManagedEntity => _managedEntity;

        private string GetGuid() {
            string uniqueName = $"{this.Position.X}{this.Position.Y}{this.Position.Z}{this.MapId}";

            byte[] input = Encoding.Unicode.GetBytes(uniqueName);
            return Convert.ToBase64String(MD5.Create().ComputeHash(input));
        }

        public ManagedPathable(TEntity managedEntity) {
            _managedEntity = managedEntity;
        }

        public virtual void Update(GameTime gameTime) {
            for (int i = 0; i < _behaviors.Count; i++) {
                _behaviors[i].Update(gameTime);
            }
        }

        #region Property Management and Binding

        protected bool SetProperty<T>(ref T property, T newValue, [CallerMemberName] string propertyName = null) {
            if (Equals(property, newValue) || propertyName == null) return false;

            property = newValue;

            OnPropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
