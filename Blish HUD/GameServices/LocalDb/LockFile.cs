using System;
using System.IO;

#nullable enable

namespace Blish_HUD.LocalDb {
    internal class LockFile : IDisposable {
        private readonly string _path;
        private readonly FileStream _stream;

        private LockFile(string path) {
            _path = path;
            _stream = File.Open(_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
        }

        public static LockFile? TryAcquire(string path) {
            try {
                return new LockFile(path);
            } catch (IOException e) when (e.GetType() == typeof(IOException)) {
                return null;
            }
        }

        public void Dispose() {
            _stream.Dispose();
            try {
                File.Delete(_path);
            } catch {
                // Ignore
            }
        }
    }
}
