using System;
using Blish_HUD.DebugHelper.Models;

namespace Blish_HUD.DebugHelper.Services {

    public interface IMessageService {

        void Start();

        void Stop();

        void Register<T>(Action<T> callback) where T : Message;

        void Unregister<T>() where T : Message;

        void Send(Message message);

        T SendAndWait<T>(Message message) where T : Message;

        T SendAndWait<T>(Message message, TimeSpan timeout) where T : Message;

    }

}
