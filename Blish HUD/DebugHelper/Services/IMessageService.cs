using System;
using Blish_HUD.DebugHelper.Models;

namespace Blish_HUD.DebugHelper.Services {

    public interface IMessageService {

        public void Start();

        public void Stop();

        public void Register<T>(Action<T> callback) where T : Message;

        public void Unregister<T>() where T : Message;

        public void Send(Message message);

        public T SendAndWait<T>(Message message) where T : Message;

        public T SendAndWait<T>(Message message, TimeSpan timeout) where T : Message;

    }
}
