using System.Collections.Generic;
using Blish_HUD.DebugHelperLib.Models;
using Blish_HUD.DebugHelperLib.Services;

namespace Blish_HUD.Input {

    internal abstract class DebugHelperInputHookManager<THandlerDelegate, TEventMessage>
        where TEventMessage : Message {

        private static readonly Logger Logger = Logger.GetLogger<DebugHelperMouseHookManager>();

        private bool isEnabled = false;


        public DebugHelperInputHookManager(IMessageService debugHelperMessageService) {
            this.DebugHelperMessageService = debugHelperMessageService;
            debugHelperMessageService.Register<TEventMessage>(DummyHookCallback);
        }


        protected IList<THandlerDelegate> Handlers { get; } = new List<THandlerDelegate>();

        protected IMessageService DebugHelperMessageService { get; }


        public virtual bool EnableHook() {
            if (isEnabled)
                return false;

            Logger.Debug("Enabling");

            DebugHelperMessageService.Unregister<TEventMessage>();
            DebugHelperMessageService.Register<TEventMessage>(HookCallback);

            isEnabled = true;
            return true;
        }

        public virtual void DisableHook() {
            if (!isEnabled)
                return;

            Logger.Debug("Disabling");

            DebugHelperMessageService.Unregister<TEventMessage>();
            DebugHelperMessageService.Register<TEventMessage>(DummyHookCallback);

            isEnabled = false;
        }

        public virtual void RegisterHandler(THandlerDelegate handleInputCallback) {
            Handlers.Add(handleInputCallback);
        }

        public virtual void UnregisterHandler(THandlerDelegate handleInputCallback) {
            Handlers.Remove(handleInputCallback);
        }

        protected abstract void HookCallback(TEventMessage message);

        protected abstract void DummyHookCallback(TEventMessage message);
    }
}
