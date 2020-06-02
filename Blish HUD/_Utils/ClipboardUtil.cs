using System;
using AsyncWindowsClipboard;

namespace Blish_HUD {
    /// <summary>
    /// Contains reference to a shared <see cref="WindowsClipboardService"/>
    /// used to make async calls to the Windows clipboard.
    /// </summary>
    public static class ClipboardUtil {

        private static readonly WindowsClipboardService _clipboardService;

        static ClipboardUtil() {
            _clipboardService = new WindowsClipboardService(TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// A shared <see cref="WindowsClipboardService"/>
        /// used to make async calls to the Windows clipboard.
        /// </summary>
        public static WindowsClipboardService WindowsClipboardService => _clipboardService;
    }
}
