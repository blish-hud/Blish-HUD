using System;
using AsyncWindowsClipboard;

namespace Blish_HUD {
    /// <summary>
    /// Contains reference to a shared <see cref="WindowsClipboardService"/>
    /// used to make async calls to the Windows clipboard.
    /// </summary>
    public static class ClipboardUtil {

        /// <summary>
        /// A shared <see cref="WindowsClipboardService"/>
        /// used to make async calls to the Windows clipboard.
        /// </summary>
        public static WindowsClipboardService WindowsClipboardService { get; } = new WindowsClipboardService(TimeSpan.FromMilliseconds(20));
    }
}
