using Blish_HUD.ArcDps;
using Blish_HUD.GameServices.Modules.Managers;
using Gw2Sharp.WebApi.V2.Models;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD.GameServices {
    public class EventBusService : GameService {
        protected static readonly Logger Logger = Logger.GetLogger<EventBusService>();

        private ConcurrentDictionary<string, ConcurrentBag<Func<object, Task>>> _subscriptions;

        protected override void Initialize() {
            this._subscriptions = new ConcurrentDictionary<string, ConcurrentBag<Func<object, Task>>>();
        }

        protected override void Load() { }

        protected override void Unload() {
            this._subscriptions.Clear();
            this._subscriptions = null;
        }

        protected override void Update(GameTime gameTime) { }

        /// <summary>
        ///     Sends the content to all subscribers for the specified event.
        /// </summary>
        /// <param name="moduleNamespace">The namespace of the module.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="content">To content to send to all subscribers.</param>
        /// <param name="silentNoSubscribers">False, if an exception should be thrown if no subscribers are found; Otherwise false.</param>
        /// <returns>The task that represents the current method.</returns>
        /// <exception cref="InvalidOperationException">If the service is in an invalid state.</exception>
        internal async Task SendToSubscribers(string moduleNamespace, string eventId, object content, bool silentNoSubscribers = true) {
            if (this._subscriptions == null) throw new InvalidOperationException($"The {typeof(EventBusService).Name} is currently in an invalid state.");

            var identifier = this.GetIdentifier(moduleNamespace, eventId);

            var tasks = new List<Task>();

            if ((!_subscriptions.TryGetValue(identifier, out var subscriberFunctions) || subscriberFunctions == null || subscriberFunctions.Count == 0) && !silentNoSubscribers) {
                throw new Exception("No module listens to this event.");
            }

            if (subscriberFunctions != null) {
                foreach (var subscriberFunction in subscriberFunctions) {
                    tasks.Add(Task.Run(async () => {
                        try {
                            await subscriberFunction?.Invoke(content);
                        } catch (Exception ex) {
                            // Failed to execute subscriber, don't throw.
                            Logger.Warn(ex, $"Failed to execute subscriber for event \"{identifier}\".");
                        }
                    }));
                }
            }

            await Task.WhenAll(tasks);
        }

        private string GetIdentifier(string moduleNamespace, string eventId) {
            return $"{moduleNamespace}_{eventId}";
        }

        /// <summary>
        ///     Subscribe to an event to handle cases another module triggers.
        /// </summary>
        /// <param name="moduleNamespace">The namespace of the module.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="action">The action which should be executed when the event is dispatched.</param>
        /// <exception cref="InvalidOperationException">If the service is in an invalid state.</exception>
        public void Subscribe(string moduleNamespace, string eventId, Func<object, Task> action) {
            if (this._subscriptions == null) throw new InvalidOperationException($"The {typeof(EventBusService).Name} is currently in an invalid state.");

            var identifier = this.GetIdentifier(moduleNamespace, eventId);

            if (!this._subscriptions.ContainsKey(identifier)) {
                this._subscriptions.TryAdd(identifier, new ConcurrentBag<Func<object, Task>>());
            }

            this._subscriptions[identifier].Add(action);
        }

        /// <summary>
        ///     Unsubscribes all listeners for the given event. This should be used when a module self registers listeners and wants to prevent collisions on disable and enable again.
        /// </summary>
        /// <param name="moduleNamespace">The namespace of the module.</param>
        /// <param name="eventId">The event id.</param>
        /// <exception cref="InvalidOperationException">If the service is in an invalid state.</exception>
        internal void Unsubscribe(string moduleNamespace, string eventId) {
            if (this._subscriptions == null) throw new InvalidOperationException($"The {typeof(EventBusService).Name} is currently in an invalid state.");

            var identifier = this.GetIdentifier(moduleNamespace, eventId);

            _ = this._subscriptions.TryRemove(identifier, out _);
        }

        /// <summary>
        ///     Calls a function inside a different module.
        /// </summary>
        /// <param name="moduleNamespace">The namespace of the module.</param>
        /// <param name="eventId">The event id.</param>
        /// <param name="content">The content to send to all subscribers.</param>
        /// <returns>The task that represents the current method.</returns>
        public async Task Dispatch(string moduleNamespace, string eventId, object content) {
            await this.SendToSubscribers(moduleNamespace, eventId, content, false); // Don't call this silent. We want the caller to handle this unexpected case.
        }
    }
}
