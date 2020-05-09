using System;
using System.Windows.Forms;
using Blish_HUD.DebugHelper.Services;
using Blish_HUD.DebugHelperLib.Services;

namespace Blish_HUD.DebugHelper {

    internal static class Program {

        internal static void Main(string[] args) {
            using var inStream = Console.OpenStandardInput();
            using var outStream = Console.OpenStandardOutput();

            using var messageService = new StreamMessageService(inStream, outStream);
            using var mouseHookService = new MouseHookService(messageService);
            using var keyboardHookService = new KeyboardHookService(messageService);
            var inputManagerService = new InputManagerService(messageService, mouseHookService, keyboardHookService);
            messageService.Start();
            mouseHookService.Start();
            keyboardHookService.Start();
            inputManagerService.Start();

            Application.Run();
        }
    }
}
