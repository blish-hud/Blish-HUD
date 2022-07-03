using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD._Utils {
    public class ProcessResourceMonitor {
        private static readonly Logger Logger = Logger.GetLogger<ProcessResourceMonitor>();

        private const int POLL_INTERVAL = 3000;
        private const int HIGH_CPU_COUNT_THRESHOLD = 5;
        private const double HIGH_CPU_THRESHOLD = 0.7;

        private Thread _testThread;

        private CancellationTokenSource _monitorTaskCancellationSource = null;

        public ProcessResourceMonitor() {
            _testThread = new Thread(() => TestThread());
            _testThread.Start();
        }

        private void TestThread() {
            Thread.Sleep(20 * 1000);
            while (true) ;
        }

        public void StartMonitor() {
            StopMonitor();
            _monitorTaskCancellationSource = new CancellationTokenSource();

            Task.Factory.StartNew(
                () => MonitorThread(_monitorTaskCancellationSource.Token),
                _monitorTaskCancellationSource.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
        }

        public void StopMonitor() {
            if (_monitorTaskCancellationSource != null && !_monitorTaskCancellationSource.IsCancellationRequested) {
                _monitorTaskCancellationSource.Cancel();
            }
        }

        private async Task MonitorThread(CancellationToken cancellationToken) {
            await Task.Delay(POLL_INTERVAL, cancellationToken);

            Process process = Process.GetCurrentProcess();
            TimeSpan lastProcessCpuTime = process.TotalProcessorTime;
            int lastClockTime = Environment.TickCount;
            Dictionary<int, TimeSpan> lastThreadCpuTimes = new Dictionary<int, TimeSpan>();
            int highCpuCount = 0;

            while (!cancellationToken.IsCancellationRequested) {
                var currentCpuTime = process.TotalProcessorTime;
                var deltaCpuTime = currentCpuTime - lastProcessCpuTime;

                var currentClockTime = Environment.TickCount;
                var deltaClockTime = currentClockTime - lastClockTime;

                // Intentionally not accounting for # of processors so that we don't miss high CPU usage
                // on systems with high # of processors
                var cpuUsage = 1.0 * deltaCpuTime.TotalMilliseconds / deltaClockTime;
                Logger.Debug($"CPU usage: {cpuUsage}");

                // If we have stored thread CPU times, calculate the CPU usage and log it
                if (lastThreadCpuTimes.Count > 0) {
                    StringBuilder threadOutput = new StringBuilder($"High CPU usage: {cpuUsage}\nThread CPU Usage:\n");
                    foreach (ProcessThread thread in process.Threads) {
                        if (lastThreadCpuTimes.TryGetValue(thread.Id, out var lastThreadCpuTime)) {
                            try {
                                var deltaThreadCpuTime = thread.TotalProcessorTime - lastThreadCpuTime;
                                var threadCpuUsage = 1.0 * deltaThreadCpuTime.TotalMilliseconds / deltaClockTime;

                                if (threadCpuUsage > 0) {
                                    threadOutput.AppendLine($"    Thread {thread.Id}: {threadCpuUsage}");
                                }
                            } catch {
                                // May encounter exceptions due to thread exiting. Ignore
                            }
                        }
                    }
                    lastThreadCpuTimes.Clear();

                    try {
                        string stackTraces = DebugHelpers.CaptureProcessStackTrace();
                        threadOutput.AppendLine(stackTraces);
                    } catch (Exception ex) {
                        Logger.Error(ex, "Failed to capture stack traces");
                    }

                    Logger.Error(threadOutput.ToString());
                }

                if (cpuUsage > HIGH_CPU_THRESHOLD) {
                    highCpuCount++;
                    Logger.Warn($"High CPU usage {highCpuCount}: {cpuUsage}");
                } else {
                    highCpuCount = 0;
                }

                // If we've exceeded CPU usage threshold, log and record thread CPU times to calculate thread
                // CPU usage on next interval.
                if (highCpuCount >= HIGH_CPU_COUNT_THRESHOLD) {
                    highCpuCount = 0;
                    Dictionary<int, TimeSpan> threadCpuTimes = new Dictionary<int, TimeSpan>();
                    foreach (ProcessThread thread in process.Threads) {
                        try {
                            threadCpuTimes.Add(thread.Id, thread.TotalProcessorTime);
                        } catch {
                            // May encounter exceptions due to thread exiting. Ignore
                        }
                    }
                    lastThreadCpuTimes = threadCpuTimes;
                }

                lastProcessCpuTime = process.TotalProcessorTime;
                lastClockTime = Environment.TickCount;

                await Task.Delay(POLL_INTERVAL, cancellationToken);
            }
        }
    }
}
