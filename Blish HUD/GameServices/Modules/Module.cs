using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Blish_HUD.Settings;
using Microsoft.Xna.Framework;

namespace Blish_HUD.Modules {

    public abstract class Module : IDisposable {

        private static readonly Logger Logger = Logger.GetLogger<Module>();

        #region Module Events

        public event EventHandler<ModuleRunStateChangedEventArgs> ModuleRunStateChanged;
        public event EventHandler<EventArgs>                      ModuleLoaded;

        public event EventHandler<UnobservedTaskExceptionEventArgs> ModuleException;

        internal void OnModuleRunStateChanged(ModuleRunStateChangedEventArgs e) {
            Logger.Debug("Module {moduleName} run state changed to {runState}", ModuleParameters.Manifest.GetDetailedName(), e.RunState);
            this.ModuleRunStateChanged?.Invoke(this, e);

            if (e.RunState == ModuleRunState.Loaded) {
                OnModuleLoaded(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Allows you to perform an action once your module has finished loading (once
        /// <see cref="LoadAsync"/> has completed).  You must call "base.OnModuleLoaded(e)" at the
        /// end for the <see cref="Module.ModuleLoaded"/> event to fire.
        /// </summary>
        protected virtual void OnModuleLoaded(EventArgs e) {
            ModuleLoaded?.Invoke(this, e);
        }

        protected void OnModuleException(UnobservedTaskExceptionEventArgs e) {
            ModuleException?.Invoke(this, e);

            if (!e.Observed) {
                this.RunState = ModuleRunState.FatalError;
            }
        }

        #endregion

        protected readonly ModuleParameters ModuleParameters;

        private ModuleRunState _runState = ModuleRunState.Unloaded;
        public ModuleRunState RunState {
            get => _runState;
            private set {
                if (_runState == value) return;

                _runState = value;
                OnModuleRunStateChanged(new ModuleRunStateChangedEventArgs(_runState));
            }
        }

        public bool Loaded => _runState == ModuleRunState.Loaded;

        #region Manifest & Parameter Aliases

        // Manifest

        public string Name => ModuleParameters.Manifest.Name;

        public string Namespace => ModuleParameters.Manifest.Namespace;

        public SemVer.Version Version => ModuleParameters.Manifest.Version;

        #endregion

        private Task _loadTask;

        [ImportingConstructor]
        protected Module([Import("ModuleParameters")] ModuleParameters moduleParameters) {
            ModuleParameters = moduleParameters;
        }

        #region Module Method Interface

        public void DoInitialize() {
            DefineSettings(ModuleParameters.SettingsManager.ModuleSettings);

            Initialize();
        }

        public void DoLoad() {
            this.RunState = ModuleRunState.Loading;

            _loadTask = Task.Run(LoadAsync);
        }

        private void CheckForLoaded() {
            switch (_loadTask.Status) {
                case TaskStatus.Faulted:
                    var loadError = new UnobservedTaskExceptionEventArgs(_loadTask.Exception);
                    OnModuleException(loadError);

                    if (!loadError.Observed && _loadTask.Exception != null) {
                        Logger.Error(_loadTask.Exception, "Module {module} had an unhandled exception while loading.", ModuleParameters.Manifest.GetDetailedName());

                        if (ApplicationSettings.Instance.DebugEnabled) {
                            throw _loadTask.Exception;
                        }
                    } else {
                        RunState = ModuleRunState.Loaded;
                    }
                    break;

                case TaskStatus.RanToCompletion:
                    RunState = ModuleRunState.Loaded;
                    Logger.Info("Module {module} finished loading.", ModuleParameters.Manifest.GetDetailedName());
                    break;

                case TaskStatus.Canceled:
                    Logger.Warn("Module {module} was cancelled before it could finish loading.", ModuleParameters.Manifest.GetDetailedName());
                    break;

                case TaskStatus.WaitingForActivation:
                    break;

                default:
                    Logger.Warn("Module {module} load state of {loadTaskStatus} was unexpected.", ModuleParameters.Manifest.GetDetailedName(), _loadTask.Status.ToString());
                    break;
            }
        }

        public void DoUpdate(GameTime gameTime) {
            if (_runState == ModuleRunState.Loaded) {
                Update(gameTime);
            } else {
                CheckForLoaded();
            }
        }

        private void DoUnload() {
            this.RunState = ModuleRunState.Unloading;
            Unload();
            this.RunState = ModuleRunState.Unloaded;

            this.ModuleLoaded          = null;
            this.ModuleRunStateChanged = null;
            this.ModuleException       = null;
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
        protected virtual void DefineSettings(ISettingCollection settings) { /* NOOP */ }

        /// <summary>
        /// Load content and more here. This call is asynchronous, so it is a good time to
        /// run any long running steps for your module. Be careful when instancing
        /// <see cref="Blish_HUD.Entities.Entity"/> and <see cref="Blish_HUD.Controls.Control"/>.
        /// Setting their parent is not thread-safe and can cause the application to crash.
        /// You will want to queue them to add later while on the main thread or in a delegate queued
        /// with <see cref="OverlayService.QueueMainThreadUpdate(Action{GameTime})"/>.
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

        protected virtual void Dispose(bool disposing) {
            DoUnload();
            this.ModuleParameters.ContentsManager?.Dispose();
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
