using System;
using System.IO;
using System.Threading.Tasks;

namespace Blish_HUD.Content {
    public sealed class DirectoryReader : IDataReader {

        private readonly string _directoryPath;

        public string PhysicalPath => _directoryPath;

        public DirectoryReader(string directoryPath) {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory path {directoryPath} not found.");

            _directoryPath = directoryPath;
        }
        
        public IDataReader GetSubPath(string subPath) {
            if (subPath.StartsWith(_directoryPath, StringComparison.OrdinalIgnoreCase))
                return new DirectoryReader(subPath);

            return new DirectoryReader(Path.Combine(_directoryPath, subPath));
        }
        
        public string GetPathRepresentation(string relativeFilePath = null) {
            return Path.Combine(_directoryPath, relativeFilePath ?? "");
        }
        
        public void LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension = "", IProgress<string> progress = null) {    
            foreach (string filePath in Directory.EnumerateFiles(_directoryPath, $"*{fileExtension}", SearchOption.AllDirectories)) {
                progress?.Report($"Loading {Path.GetFileName(filePath)}");
                loadFileFunc.Invoke(this.GetFileStream(filePath), this);
            }
        }
        
        public bool FileExists(string filePath) {
            return File.Exists(Path.Combine(_directoryPath, filePath));
        }
        
        public Stream GetFileStream(string filePath) {
            if (!this.FileExists(filePath)) return null;

            return File.Open(Path.Combine(_directoryPath, filePath), FileMode.Open);
        }
        
        public byte[] GetFileBytes(string filePath) {
            if (!this.FileExists(filePath)) return null;

            return File.ReadAllBytes(Path.Combine(_directoryPath, filePath));
        }
        
        public int GetFileBytes(string filePath, out byte[] fileBuffer) {
            fileBuffer = GetFileBytes(filePath);

            return fileBuffer?.Length ?? 0;
        }
        
        public async Task<Stream> GetFileStreamAsync(string filePath) {
            return await Task.FromResult(this.GetFileStream(filePath));
        }
        
        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            if (!FileExists(filePath)) return null;

            byte[] fileData;

            using (var fileStream = File.OpenRead(Path.Combine(_directoryPath, filePath))) {
                fileData = new byte[fileStream.Length];
                await fileStream.ReadAsync(fileData, 0, (int) fileStream.Length);
            }

            return fileData;
        }

        public void DeleteRoot() {
            this.Dispose();

            Directory.Delete(_directoryPath, true);
        }

        public void Dispose() { /* NOOP */ }

    }
}
