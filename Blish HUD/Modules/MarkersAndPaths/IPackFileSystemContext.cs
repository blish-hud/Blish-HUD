using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.MarkersAndPaths {

    public interface IPackFileSystemContext : IDisposable {

        void LoadOnFileType(Action<Stream, IPackFileSystemContext> loadFileFunc, string fileExtension, IProgress<string> progressIndicator = null);

        bool FileExists(string filePath);

        void RunTextureDisposal();
        void MarkTextureForDisposal(string texturePath);
        Texture2D LoadTexture(string texturePath);
        Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture);

        Stream LoadFileStream(string filePath);

    }

    public class DirectoryPackContext : IPackFileSystemContext {

        private static Dictionary<string, DirectoryPackContext> _cachedContexts = new Dictionary<string, DirectoryPackContext>();

        private readonly string _packDir;

        private Dictionary<string, Texture2D> _textureCache;
        private HashSet<string>               _pendingTextureRemoval;
        private HashSet<string>               _isUsedOnNextMap;

        public DirectoryPackContext(string directoryRoot) {
            _packDir = directoryRoot;

            _textureCache          = new Dictionary<string, Texture2D>();
            _pendingTextureRemoval = new HashSet<string>();
            _isUsedOnNextMap       = new HashSet<string>();
        }

        public static DirectoryPackContext GetCachedContext(string directoryRoot) {
            if (!_cachedContexts.ContainsKey(directoryRoot)) {
                _cachedContexts.Add(directoryRoot, new DirectoryPackContext(directoryRoot));
            } else { Console.WriteLine("Returned existing Directory Context!"); }

            return _cachedContexts[directoryRoot];
        }

        private void RunOnAllOfFileType(string directory, Action<Stream, IPackFileSystemContext> loadFileFunc, string fileExtension, IProgress<string> progressIndicator = null) {
            foreach (var mFile in Directory.EnumerateFiles(directory, $"*.{fileExtension}")) {
                progressIndicator?.Report($"Loading pack file {mFile}");

                Console.WriteLine($"[{nameof(DirectoryPackContext)}] Loading file {mFile}");
                loadFileFunc.Invoke(LoadFileStream(mFile), this);
            }

            foreach (var mDir in Directory.EnumerateDirectories(directory)) {
                RunOnAllOfFileType(mDir, loadFileFunc, fileExtension);
            }
        }

        public void LoadOnFileType(Action<Stream, IPackFileSystemContext> loadFileFunc, string fileExtension, IProgress<string> progressIndicator = null) {
            RunOnAllOfFileType(_packDir, loadFileFunc, fileExtension, progressIndicator);
        }

        public bool FileExists(string filePath) {
            return File.Exists(Path.Combine(_packDir, filePath));
        }

        public void RunTextureDisposal() {
            // Prevent us from removing textures that are needed on the next map as well
            _pendingTextureRemoval.RemoveWhere(t => _isUsedOnNextMap.Contains(t));
            
            foreach (string textureToRemove in _pendingTextureRemoval) {
                if (_textureCache.TryGetValue(textureToRemove, out Texture2D textureToDispose)) {
                    textureToDispose?.Dispose();
                }
                _textureCache.Remove(textureToRemove);
                Console.WriteLine($"Disposed of texture '{textureToRemove}'");
            }

            _pendingTextureRemoval.Clear();
        }

        public void MarkTextureForDisposal(string texturePath) {
            if (texturePath == null) return;

            if (_textureCache.ContainsKey(texturePath)) {
                _pendingTextureRemoval.Add(texturePath);
            }
        }

        public Texture2D LoadTexture(string texturePath) {
            return LoadTexture(texturePath, null);
        }

        public Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture) {
            _isUsedOnNextMap.Add(texturePath);

            if (!_textureCache.ContainsKey(texturePath)) {
                using (var textureStream = LoadFileStream(texturePath)) {
                    if (textureStream != Stream.Null) {
                        _textureCache.Add(texturePath, Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream));
                    } else {
                        return fallbackTexture;
                    }
                }
            }

            return _textureCache[texturePath];
        }

        public Stream LoadFileStream(string filePath) {
            var fullFilePath = Path.Combine(_packDir, filePath);

            if (File.Exists(fullFilePath)) {
                try {
                    return new FileStream(fullFilePath, FileMode.Open);
                } catch (Exception ex) {
                    return Stream.Null;
                }
            }

            return Stream.Null;
        }
        
        public void Dispose() {
            foreach (var textures in _textureCache) {
                textures.Value?.Dispose();
            }
        }

    }

    public class ZipPackContext : IPackFileSystemContext {

        private static Dictionary<string, ZipPackContext> _cachedContexts = new Dictionary<string, ZipPackContext>();

        private readonly string _archivePath;
        private ZipArchive _packArchive;

        private Mutex _exclusiveStreamAccessMutex;

        private Dictionary<string, Texture2D> _textureCache;
        private HashSet<string>               _pendingTextureRemoval;
        private HashSet<string>               _isUsedOnNextMap;

        public ZipPackContext(string zipPackPath) {
            _archivePath = zipPackPath;
            _packArchive = ZipFile.OpenRead(zipPackPath);

            _exclusiveStreamAccessMutex = new Mutex(false);

            _textureCache          = new Dictionary<string, Texture2D>();
            _pendingTextureRemoval = new HashSet<string>();
            _isUsedOnNextMap       = new HashSet<string>();
        }

        public static ZipPackContext GetCachedContext(string zipPackPath) {
            if (!_cachedContexts.ContainsKey(zipPackPath)) {
                _cachedContexts.Add(zipPackPath, new ZipPackContext(zipPackPath));
            } else { Console.WriteLine("Returned existing Zip Context!"); }

            return _cachedContexts[zipPackPath];
        }

        public void LoadOnFileType(Action<Stream, IPackFileSystemContext> loadFileFunc, string fileExtension, IProgress<string> progressIndicator = null) {
            foreach (var entry in _packArchive.Entries) {
                if (entry.Name.EndsWith($".{fileExtension}", StringComparison.OrdinalIgnoreCase)) {
                    progressIndicator?.Report($"Loading {entry.FullName}");

                    var entryStream = LoadFileStream(entry.FullName);
                    loadFileFunc.Invoke(entryStream, this);
                }
            }
        }

        public bool FileExists(string filePath) {
            return _packArchive.Entries.Any(entry => 
                string.Equals(entry.FullName, filePath.Replace(@"\", "/"), StringComparison.CurrentCultureIgnoreCase)
            );
        }

        public void RunTextureDisposal() {
            // Prevent us from removing textures that are needed on the next map as well
            _pendingTextureRemoval.RemoveWhere(t => _isUsedOnNextMap.Contains(t));

            foreach (string textureToRemove in _pendingTextureRemoval) {
                if (_textureCache.TryGetValue(textureToRemove, out Texture2D textureToDispose)) {
                    textureToDispose?.Dispose();
                }
                _textureCache.Remove(textureToRemove);
                Console.WriteLine($"Disposed of texture '{textureToRemove}'");
            }

            _pendingTextureRemoval.Clear();
            _isUsedOnNextMap.Clear();
        }

        public void MarkTextureForDisposal(string texturePath) {
            if (texturePath == null) return;

            if (_textureCache.ContainsKey(texturePath)) {
                _pendingTextureRemoval.Add(texturePath);
            }
        }

        public Texture2D LoadTexture(string texturePath) {
            return LoadTexture(texturePath, null);
        }

        public Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture) {
            _isUsedOnNextMap.Add(texturePath);

            if (!_textureCache.ContainsKey(texturePath)) {
                using (var textureStream = LoadFileStream(texturePath)) {
                    if (textureStream != Stream.Null) {
                        _textureCache.Add(texturePath, Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream));
                    } else {
                        return fallbackTexture;
                    }
                }
            }

            return _textureCache[texturePath];
        }

        public Stream LoadFileStream(string filePath) {
            //if (!FileExists(filePath))
            //    throw new FileNotFoundException("File could not be found inside of archive.", filePath);

            ZipArchiveEntry fileEntry;

            //if ((fileEntry = _packArchive.GetEntry(filePath)) != null) {
                //_exclusiveStreamAccessMutex.WaitOne();
            using (var exclusiveReader = ZipFile.OpenRead(_archivePath)) {
                if ((fileEntry = exclusiveReader.GetEntry(filePath.Replace(@"\", "/"))) != null) {


                    var memStream = new MemoryStream();

                    using (var entryStream = fileEntry.Open()) {
                        entryStream.CopyTo(memStream);
                    }

                    memStream.Position = 0;

                    //_exclusiveStreamAccessMutex.ReleaseMutex();
                    return memStream;
                }
            }
            //}

            // Can't find it, so just send back an empty stream
            return Stream.Null;
        }

        public void Dispose() {
            foreach (var textures in _textureCache) {
                textures.Value?.Dispose();
            }
            _packArchive?.Dispose();
        }

    }



}
