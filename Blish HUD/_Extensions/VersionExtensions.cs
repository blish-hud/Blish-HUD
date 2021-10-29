using System;

namespace Blish_HUD {
    internal static class VersionExtensions {

        public static string BaseAndPrerelease(this SemVer.Version version) {
            return version.ToString().Split(new char[]{ '+' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

    }
}
