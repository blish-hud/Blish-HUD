using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blish_HUD._Utils {
    public class DebugHelpers {
        public static string CaptureProcessStackTrace() {
            StringBuilder output = new StringBuilder("Process Threads:\n");

            using (DataTarget dataTarget = DataTarget.CreateSnapshotAndAttach(Process.GetCurrentProcess().Id)) {
                ClrInfo runtimeInfo = dataTarget.ClrVersions[0];
                ClrRuntime runtime = runtimeInfo.CreateRuntime();

                foreach (ClrThread thread in runtime.Threads) {
                    if (!thread.IsAlive) {
                        continue;
                    }

                    output.AppendLine($"Thread {thread.ManagedThreadId}({thread.OSThreadId}):\n");
                    output.AppendLine($"    CurrentAppDomain: {thread.CurrentAppDomain}");
                    output.AppendLine($"    LockCount: {thread.LockCount}");
                    output.AppendLine($"    Stack:");

                    foreach (ClrStackFrame frame in thread.EnumerateStackTrace()) {
                        output.AppendLine($"        {frame}");
                    }

                }
            }

            return output.ToString();
        }
    }
}
