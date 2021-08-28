using System;
using System.Collections.Generic;
using System.Linq;
using Blish_HUD.Input;
using Microsoft.Xna.Framework.Input;

namespace Blish_HUD.Controls {
    public interface IWindow : IContainer {

        /// <summary>
        /// If the window should be forced on top of all other windows.
        /// </summary>
        bool TopMost { get; }

        /// <summary>
        /// The last time the window was made active from a click or shown.
        /// </summary>
        double LastInteraction { get; }

        /// <summary>
        /// Brings the window to the front of all other windows.
        /// </summary>
        void BringWindowToFront();

        /// <summary>
        /// If <c>true</c> the window can support closing itself.  Otherwise, an external action will be required to close it.
        /// </summary>
        bool CanClose { get; }

        void Hide();

        void Show();

    }

    public static class IWindowImpl {

        private static readonly List<IWindow> _windows = new List<IWindow>();

        internal static void EnableWindows() {
            GameService.Input.Keyboard.KeyPressed += KeyboardOnKeyPressed;
        }

        private static void KeyboardOnKeyPressed(object sender, KeyboardEventArgs e) {
            if (e.Key == Keys.Escape) {
                if (Control.ActiveControl == null || _windows.Count < 1) return;

                var window = _windows.OrderByDescending(w => w.LastInteraction).First();

                foreach (var childControl in window.GetDescendants()) {
                    if (Control.ActiveControl == childControl) {
                        window.Hide();
                        return;
                    }
                }
            }
        }

        public static void RegisterWindow(IWindow window) {
            _windows.Add(window);
        }

        public static void UnregisterWindow(IWindow window) {
            _windows.Remove(window);
        }

        public static int GetZIndex(IWindow thisWindow) {
            if (!_windows.Contains(thisWindow)) {
                throw new InvalidOperationException($"{nameof(thisWindow)} must be registered with {nameof(RegisterWindow)} before ZIndex can automatically be calculated.");
            }

            return Screen.WINDOW_BASEZINDEX + _windows.OrderBy(window => window.TopMost)
                                                      .ThenBy(window => window.LastInteraction)
                                                      .TakeWhile(window => window != thisWindow)
                                                      .Count();
        }

    }
}
