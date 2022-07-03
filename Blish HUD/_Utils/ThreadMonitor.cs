using Microsoft.Diagnostics.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Blish_HUD._Utils {
    public class ThreadMonitor {
        private static readonly Logger Logger = Logger.GetLogger<ThreadMonitor>();

        private const int POLL_INTERVAL = 1000;
        private const int THREAD_HANG_THRESHOLD = 15000;

        private readonly object _watchedThreadsLock = new object();
        private Dictionary<int, int> _watchedThreads = new Dictionary<int, int>();

        private CancellationTokenSource _monitorTaskCancellationSource = null;

        public ThreadMonitor() {
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

        public void MonitorCurrentThread() {
            lock (_watchedThreadsLock) {
                _watchedThreads.Add(Thread.CurrentThread.ManagedThreadId, Environment.TickCount);
            }
        }

        public void StopMonitorCurrentThread() {
            lock (_watchedThreadsLock) {
                _watchedThreads.Remove(Thread.CurrentThread.ManagedThreadId);
            }
        }

        public void Signal() {
            lock (_watchedThreadsLock) {
                var threadId = Thread.CurrentThread.ManagedThreadId;
                if (_watchedThreads.ContainsKey(threadId)) {
                    _watchedThreads[threadId] = Environment.TickCount;
                } else {
                    Logger.Error($"Signal called on thread {threadId} not subscribed to be monitored.");
                }
            }
        }

        private async Task MonitorThread(CancellationToken cancellationToken) {
            await Task.Delay(POLL_INTERVAL, cancellationToken);

            List<int> badThreads = new List<int>();
            while (!cancellationToken.IsCancellationRequested) {
                Thread.Sleep(POLL_INTERVAL);

                lock (_watchedThreadsLock) {
                    var currentTicks = Environment.TickCount;
                    Logger.Debug($"Thread monitor polling at {currentTicks}");
                    foreach (var thread in _watchedThreads) {
                        var duration = currentTicks - thread.Value;
                        if (duration > THREAD_HANG_THRESHOLD) {
                            Logger.Error($"Thread {thread.Key} is unresponsive since {thread.Value}({duration})");
                            badThreads.Add(thread.Key);
                        }
                    }

                    foreach (var thread in badThreads) {
                        _watchedThreads.Remove(thread);
                    }
                }

                // Handle bad threads
                if (badThreads.Count > 0) {
                    try {
                        string stackTraces = DebugHelpers.CaptureProcessStackTrace();
                        Logger.Error(stackTraces);
                    } catch (Exception ex) {
                        Logger.Error(ex, "Failed to capture stack traces");
                    }
                    badThreads.Clear();
                }
            }
        }
    }
}
