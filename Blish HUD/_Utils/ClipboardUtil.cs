using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using AsyncWindowsClipboard;

namespace Blish_HUD {
    /// <summary>
    /// Contains reference to a shared <see cref="WindowsClipboardService"/>
    /// used to make async calls to the Windows clipboard.
    /// </summary>
    public static class ClipboardUtil {

        private static readonly Logger Logger = Logger.GetLogger(typeof(ClipboardUtil));

        private static readonly WindowsClipboardService _clipboardService;

        static ClipboardUtil() {
            _clipboardService = new WindowsClipboardService(TimeSpan.FromMilliseconds(20));
        }

        /// <summary>
        /// A shared <see cref="WindowsClipboardService"/>
        /// used to make async calls to the Windows clipboard.
        /// </summary>
        public static WindowsClipboardService WindowsClipboardService => _clipboardService;
        /// <summary>
        /// Returns the clipboard's content as a Dictionary of objects indexed by their data format or null if the clipboard is empty.
        /// </summary>
        /// <returns>Dictionary of objects indexed by their DataFormat (System.Windows.Forms.DataFormats).</returns>
        public static Dictionary<string, object> GetClipboardContent() {
            try {
                var dataObject = Clipboard.GetDataObject();
                if (dataObject == null) return null;
                var formats = dataObject.GetFormats(false);
                var backup = new Dictionary<string, object>();
                foreach (var format in formats) 
                    backup.Add(format, dataObject.GetData(format, false));
                return backup;
            } catch (ExternalException e) {
                Logger.Error("Data could not be retrieved from the Clipboard. This typically occurs when the Clipboard is being used by another process. " + e.Message);
            } catch (ThreadStateException e) {
                Logger.Error("The current thread is not in single-threaded apartment (STA) mode and the MessageLoop property value is true. Add the STAThreadAttribute to your application's Main method. " + e.Message);
            }
            return null;
        }
        /// <summary>
        /// Sets the clipboard's content by assigning it a new DataObject with the given content.
        /// </summary>
        /// <param name="content">Dictionary of objects indexed by their DataFormat (System.Windows.Forms.DataFormats).</param>
        public static void SetClipboardContent(Dictionary<string, object> content) {
            if (content == null) return;
            try {
                var dataObject = Clipboard.GetDataObject() ?? new DataObject();
                var formats = content.Keys.ToList();
                foreach (var format in formats) 
                    dataObject.SetData(content[format]);
                Clipboard.SetDataObject(dataObject);
            } catch (ExternalException e) {
                Logger.Error("Data could not be retrieved from the Clipboard. This typically occurs when the Clipboard is being used by another process. " + e.Message);
            } catch (ThreadStateException e) {
                Logger.Error("The current thread is not in single-threaded apartment (STA) mode and the MessageLoop property value is true. Add the STAThreadAttribute to your application's Main method. " + e.Message);
            }
        }
    }
}
