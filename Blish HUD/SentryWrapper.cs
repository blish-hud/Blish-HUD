using System;
using Sentry;

namespace Blish_HUD {

    // TODO: Migrate this all into the Debug Service
    public static class SentryWrapper {

        public static void CaptureException(Exception err) {
#if SENTRY
            SentrySdk.CaptureException(err);
#endif
        }

    }
}
