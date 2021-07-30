using Blish_HUD.DebugHelper.Services;
using Blish_HUD.DebugHelperLib.Services;
using System;
using System.Windows.Forms;

namespace Blish_HUD.DebugHelper {

    internal static class Program {

        [STAThread]
        internal static void Main(string[] args) {

            using var inStream  = Console.OpenStandardInput();
            using var outStream = Console.OpenStandardOutput();

            ProcessService? processService      = (args.Length > 0) && int.TryParse(args[0], out int blishHudProcessId) ? new ProcessService(blishHudProcessId) : null;
            using var       messageService      = new StreamMessageService(inStream, outStream);
            using var       mouseHookService    = new MouseHookService(messageService);
            using var       keyboardHookService = new KeyboardHookService(messageService);
            using var       inputManagerService = new InputManagerService(messageService, mouseHookService, keyboardHookService);

            processService?.Start();
            messageService.Start();
            mouseHookService.Start();
            keyboardHookService.Start();
            inputManagerService.Start();

            Application.Run();
        }

    }

}
