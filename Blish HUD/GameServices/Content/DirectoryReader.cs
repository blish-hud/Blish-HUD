using System;
using System.IO;
using System.Threading.Tasks;

namespace Blish_HUD.Content {
    public sealed class DirectoryReader : IDataReader {
        public string PhysicalPath { get; }

        public DirectoryReader(string directoryPath) {
            if (!Directory.Exists(directoryPath)) {
                throw new DirectoryNotFoundException($"Directory path {directoryPath} not found.");
            }

            this.PhysicalPath = directoryPath;
        }

        public IDataReader GetSubPath(string subPath) {
            if (subPath.StartsWith(this.PhysicalPath, StringComparison.OrdinalIgnoreCase)) {
                return new DirectoryReader(subPath);
            }

            return new DirectoryReader(Path.Combine(this.PhysicalPath, subPath));
        }

        public string GetPathRepresentation(string relativeFilePath = null) => Path.Combine(this.PhysicalPath, relativeFilePath ?? "");

        public void LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension = "", IProgress<string> progress = null) {
            foreach (string filePath in Directory.EnumerateFiles(this.PhysicalPath, $"*{fileExtension}", SearchOption.AllDirectories)) {
                progress?.Report($"Loading {Path.GetFileName(filePath)}");
                loadFileFunc.Invoke(this.GetFileStream(filePath), this);
            }
        }

        public bool FileExists(string filePath) => File.Exists(Path.Combine(this.PhysicalPath, filePath));

        public Stream GetFileStream(string filePath) {
            if (!this.FileExists(filePath)) {
                return null;
            }

            return File.Open(Path.Combine(this.PhysicalPath, filePath), FileMode.Open);
        }

        public byte[] GetFileBytes(string filePath) {
            if (!this.FileExists(filePath)) {
                return null;
            }

            return File.ReadAllBytes(Path.Combine(this.PhysicalPath, filePath));
        }

        public int GetFileBytes(string filePath, out byte[] fileBuffer) {
            fileBuffer = GetFileBytes(filePath);

            return fileBuffer?.Length ?? 0;
        }

        public async Task<Stream> GetFileStreamAsync(string filePath) => await Task.FromResult(this.GetFileStream(filePath));

        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            if (!FileExists(filePath)) {
                return null;
            }

            byte[] fileData;

            using (var fileStream = File.OpenRead(Path.Combine(this.PhysicalPath, filePath))) {
                fileData = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileData, 0, (int)fileStream.Length);
            }

            return fileData;
        }

        public void DeleteRoot() {
            this.Dispose();

            Directory.Delete(this.PhysicalPath, true);
        }

        public void Dispose() { /* NOOP */ }
    }
}
