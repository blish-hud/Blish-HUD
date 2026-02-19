using Blish_HUD.Modules;
using Blish_HUD.Modules.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices.Modules.Managers {
    public class EventBusManager : IDisposable {
        protected static readonly Logger Logger = Logger.GetLogger<EventBusManager>();
        private List<string> _selfSubscribedEvents = new List<string>();

        private readonly string _moduleNamespace;

        internal static EventBusManager GetModuleInstance(ModuleManager module) {
            return new EventBusManager(module.Manifest.Namespace);
        }

        private EventBusManager(string moduleNamespace) {
            this._moduleNamespace = moduleNamespace;
        }

        /// <inheritdoc cref="EventBusService.Dispatch(string, string, object)"/>
        public async Task Dispatch(string eventId, object content, bool silentNoSubscribers = true) {
            await GameService.EventBus.SendToSubscribers(this._moduleNamespace, eventId, content, silentNoSubscribers);
        }

        /// <inheritdoc cref="EventBusService.Subscribe(string, string, Func{object, Task})"/>
        public void Subscribe(string eventId, Func<object, Task> action) {
            GameService.EventBus.Subscribe(this._moduleNamespace, eventId, action);

            this._selfSubscribedEvents?.Add(eventId);
        }

        /// <summary>
        ///     Unsubscribes all event subscribers that got registered by this module instance.
        /// </summary>
        public void Dispose() {
            if (this._selfSubscribedEvents != null) {
                foreach (var eventId in this._selfSubscribedEvents) {
                    GameService.EventBus.Unsubscribe(this._moduleNamespace, eventId);
                }

                this._selfSubscribedEvents.Clear();
            }
        }
    }
}
