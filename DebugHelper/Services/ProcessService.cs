using System;
using System.Diagnostics;

namespace Blish_HUD.DebugHelper.Services {

    internal class ProcessService : IDebugService {

        private readonly Process process;

        public ProcessService(int blishHudProcessId) {
            process                     = Process.GetProcessById(blishHudProcessId);
            process.EnableRaisingEvents = true;
        }

        public void Start() { process.Exited += Process_Exited; }

        public void Stop() { process.Exited -= Process_Exited; }

        private void Process_Exited(object sender, EventArgs e) { Environment.Exit(0); }

    }

}
