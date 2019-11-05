using System;
using Blish_HUD.Graphics.UI;

namespace Blish_HUD.Common.UI {
    public interface IReturningView<TReturn> : IView {

        void ReturnWith(Action<TReturn> returnAction);

    }
}
