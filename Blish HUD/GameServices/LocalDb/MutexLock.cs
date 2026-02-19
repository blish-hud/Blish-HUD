using System;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class MutexLock : IDisposable {
        private static readonly Logger _logger = Logger.GetLogger<MutexLock>();

        // FIXME: This is duplicated from Program.APP_GUID... should it be made public there?
        private const string APP_GUID = "{5802208e-71ca-4745-ab1b-d851bc17a460}";

        public readonly bool HasHandle = false;
        public readonly bool TimedOut = false;
        public readonly bool Abandoned = false;

        private readonly Mutex _mutex;

        public MutexLock(string name, TimeSpan? timeout = null)
        {
            {
                _mutex = new Mutex(false, $"{APP_GUID}.{name}");

                var allowEveryoneRule = new MutexAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    MutexRights.FullControl,
                    AccessControlType.Allow);
                var securitySettings = new MutexSecurity();
                securitySettings.AddAccessRule(allowEveryoneRule);
                _mutex.SetAccessControl(securitySettings);
            }

            try {
                HasHandle = _mutex.WaitOne(timeout ?? Timeout.InfiniteTimeSpan, false);

                if (!HasHandle) {
                    TimedOut = true;
                }
            } catch (AbandonedMutexException) {
                _logger.Warn($"Abandoned mutex encountered: {name}");
                HasHandle = true;
                Abandoned = true;
            }
        }

        public void Dispose() {
            if (_mutex == null) {
                return;
            }

            if (HasHandle) {
                _mutex.ReleaseMutex();
            }

            _mutex.Close();
        }
    }
}
