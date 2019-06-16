using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD.Content;
using Blish_HUD.Modules.Managers;
using Gw2Sharp.WebApi;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules {

    public abstract class Module : IDisposable {

        #region Module Events

        public event EventHandler<EventArgs>                        ModuleLoaded;
        public event EventHandler<UnobservedTaskExceptionEventArgs> ModuleException;

        /// <summary>
        /// Allows you to perform an action once your module has finished loading (once
        /// <see cref="LoadAsync"/> has completed).  You must call "base.OnModuleLoaded(e)" at the
        /// end for the <see cref="ExternalModule.ModuleLoaded"/> event to fire and for
        /// <see cref="ExternalModule.Loaded" /> to update correctly.
        /// </summary>
        protected virtual void OnModuleLoaded(EventArgs e) {
            _loaded = true;

            ModuleLoaded?.Invoke(this, e);
        }

        protected virtual void OnModuleException(UnobservedTaskExceptionEventArgs e) {
            ModuleException?.Invoke(this, e);
        }

        #endregion

        private readonly ModuleParameters _moduleParameters;

        private bool _loaded = false;
        public bool Loaded => _loaded;

        #region Manifest & Parameter Aliases

        // Manifest

        public string Name => _moduleParameters.Manifest.Name;

        public string Namespace => _moduleParameters.Manifest.Namespace;

        public SemVer.Version Version => _moduleParameters.Manifest.Version;

        // Service Managers

        protected SettingsManager SettingsManager => _moduleParameters.SettingsManager;

        protected ContentsManager ContentsManager => _moduleParameters.ContentsManager;

        protected DirectoriesManager DirectoriesManager => _moduleParameters.DirectoriesManager;

        protected Gw2ApiManager Gw2ApiManager => _moduleParameters.GW2ApiManager;

        #endregion

        private Task _loadTask;

        [ImportingConstructor]
        public Module([Import("ModuleParameters")] ModuleParameters moduleParameters) {
            _moduleParameters = moduleParameters;
        }

        #region Module Method Interface

        public void DoInitialize() {
            DefineSettings(this.SettingsManager.ModuleSettings);

            Initialize();
        }

        public void DoLoad() {
            _loadTask = LoadAsync();
        }

        private void CheckForLoaded() {
            switch (_loadTask.Status) {
                case TaskStatus.Faulted:
                    OnModuleException(new UnobservedTaskExceptionEventArgs(_loadTask.Exception));
                    OnModuleLoaded(EventArgs.Empty);
                    break;

                case TaskStatus.RanToCompletion:
                    OnModuleLoaded(EventArgs.Empty);
                    break;

                default:
                    GameService.Debug.WriteErrorLine($"Unexpected module load result status '{_loadTask.Status.ToString()}'.");
                    break;
            }
        }

        public void DoUpdate(GameTime gameTime) {
            if (_loaded)
                Update(gameTime);
            else
                CheckForLoaded();
        }

        private void DoUnload() {
            Unload();
        }

        #endregion

        #region Virtual Methods

        /// <summary>
        /// Allows your module to perform any initialization it needs before starting to run.
        /// Please note that Initialize is NOT asynchronous and will block Blish HUD's update
        /// and render loop, so be sure to not do anything here that takes too long.
        /// </summary>
        protected virtual void Initialize() { /* NOOP */ }

        /// <summary>
        /// Define the settings you would like to use in your module.  Settings are persistent
        /// between updates to both Blish HUD and your module.
        /// </summary>
        protected virtual void DefineSettings(SettingCollection settings) { /* NOOP */ }

        /// <summary>
        /// Load content and more here. This call is asynchronous, so it is a good time to
        /// run any long running steps for your module. Be careful when instancing
        /// <see cref="Blish_HUD.Entities.Entity"/> and <see cref="Blish_HUD.Controls.Control"/>.
        /// Setting their parent is not thread-safe and can cause the application to crash.
        /// You will want to queue them to add later while on the main thread or in a delegate queued
        /// with <see cref="Blish_HUD.DirectorService.QueueMainThreadUpdate(Action{GameTime})"/>.
        /// </summary>
        protected virtual async Task LoadAsync() { /* NOOP */ }

        /// <summary>
        /// Allows your module to run logic such as updating UI elements,
        /// checking for conditions, playing audio, calculating changes, etc.
        /// This method will block the primary Blish HUD loop, so any long
        /// running tasks should be executed on a separate thread to prevent
        /// slowing down the overlay.
        /// </summary>
        protected virtual void Update(GameTime gameTime) { /* NOOP */ }

        /// <summary>
        /// For a good module experience, your module should clean up ANY and ALL entities
        /// and controls that were created and added to either the World or SpriteScreen.
        /// Be sure to remove any tabs added to the Director window, CornerIcons, etc.
        /// </summary>
        protected virtual void Unload() { /* NOOP */ }

        #endregion

        #region IDispose

        protected void Dispose(bool disposing) {
            DoUnload();
        }

        /// <inheritdoc />
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        ~Module() {
            Dispose(false);
        }

        #endregion

    }

}
