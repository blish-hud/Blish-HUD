using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blish_HUD._Extensions;
using Microsoft.Xna.Framework.Graphics;

namespace Blish_HUD.Modules.MarkersAndPaths {

    public interface IPackFileSystemContext : IDisposable {

        void LoadOnXmlPack(Action<string, IPackFileSystemContext> loadXmlFunc);

        bool FileExists(string filePath);

        void RunTextureDisposal();
        void MarkTextureForDisposal(string texturePath);
        Texture2D LoadTexture(string texturePath);
        Texture2D LoadTexture(string texturePath, Texture2D fallbackTexture);

        Stream LoadFileStream(string filePath);

    }

    public class DirectoryPackContext : IPackFileSystemContext {

        private readonly string _markerDir;

        private Dictionary<string, Texture2D> _textureCache;
        private HashSet<string>               _pendingTextureRemoval;
        private HashSet<string>               _isUsedOnNextMap;

        public DirectoryPackContext(string directoryRoot) {
            //if (!Directory.Exists(directoryRoot))
            //    throw new DirectoryNotFoundException($"The marker directory could not be found: {directoryRoot}");

            _markerDir = directoryRoot;

            _textureCache          = new Dictionary<string, Texture2D>();
            _pendingTextureRemoval = new HashSet<string>();
            _isUsedOnNextMap       = new HashSet<string>();
        }

        private void RunOnAllXmlPacks(string directory, Action<string, IPackFileSystemContext> loadXmlFunc) {
            foreach (var mFile in Directory.EnumerateFiles(directory, "*.xml")) {
                Console.WriteLine($"[{nameof(DirectoryPackContext)}] Loading pack file {mFile}");
                loadXmlFunc.Invoke(File.ReadAllText(mFile), this);
            }

            foreach (var mDir in Directory.EnumerateDirectories(directory)) {
                RunOnAllXmlPacks(mDir, loadXmlFunc);
            }
        }

        public void LoadOnXmlPack(Action<string, IPackFileSystemContext> loadXmlFunc) {
            RunOnAllXmlPacks(_markerDir, loadXmlFunc);
        }

        public bool FileExists(string filePath) {
            return File.Exists(Path.Combine(_markerDir, filePath));
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
                    _textureCache.Add(texturePath, Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream));
                }
            }

            return _textureCache[texturePath];
        }

        public Stream LoadFileStream(string filePath) {
            return new FileStream(Path.Combine(_markerDir, filePath), FileMode.Open);
        }
        
        public void Dispose() {
            foreach (var textures in _textureCache) {
                textures.Value?.Dispose();
            }
        }

    }

    public class ZipPackContext : IPackFileSystemContext {

        private ZipArchive _packArchive;

        private Dictionary<string, Texture2D> _textureCache;
        private HashSet<string>               _pendingTextureRemoval;
        private HashSet<string>               _isUsedOnNextMap;

        public ZipPackContext(string zipPackPath) {
            //if (!File.Exists(zipPackPath))
            //    throw new FileNotFoundException("The ZIP pathing pack could not be loaded.", zipPackPath);

            _packArchive = ZipFile.OpenRead(zipPackPath);

            _textureCache          = new Dictionary<string, Texture2D>();
            _pendingTextureRemoval = new HashSet<string>();
            _isUsedOnNextMap       = new HashSet<string>();
        }

        public void LoadOnXmlPack(Action<string, IPackFileSystemContext> loadXmlFunc) {
            //if (_packArchive == null)
            //    throw new ObjectDisposedException(nameof(_packArchive), "The pack archive reader was unable to be opened, or has already been disposed.");

            foreach (var entry in _packArchive.Entries) {
                if (entry.Name.ToLower().EndsWith(".xml")) {
                    Console.WriteLine($"[{nameof(ZipPackContext)}] Loading pack file {entry.FullName}");

                    using (var entryReader = new StreamReader(entry.Open())) {
                        string rawPackXml = entryReader.ReadToEnd();
                        loadXmlFunc.Invoke(rawPackXml, this);
                    }
                }
            }
        }

        public bool FileExists(string filePath) {
            return _packArchive.Entries.Any(entry => 
                string.Equals(entry.FullName, filePath, StringComparison.CurrentCultureIgnoreCase)
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
            //if (_packArchive == null)
            //    throw new ObjectDisposedException(nameof(_packArchive), "The pack archive reader was unable to be opened, or has already been disposed.");
            
            _isUsedOnNextMap.Add(texturePath);

            if (!_textureCache.ContainsKey(texturePath)) {
                using (var textureStream = LoadFileStream(texturePath)) {
                    _textureCache.Add(texturePath, Texture2D.FromStream(GameService.Graphics.GraphicsDevice, textureStream));
                }
            }

            return _textureCache[texturePath];
        }

        public Stream LoadFileStream(string filePath) {
            ZipArchiveEntry textureEntry;

            //foreach (var textureEntry in _packArchive.Entries) {
            //    if (textureEntry.FullName.EndsWith(filePath)) {
            //        return textureEntry.Open().ToMemoryStream();
            //    }
            //}

            if ((textureEntry = _packArchive.GetEntry(filePath)) != null) {
                return textureEntry.Open().ToMemoryStream();
            }
            //} else {
            //    throw new FileNotFoundException("File was not found inside of archive.", filePath);

            // Can't find it, so just send back an empty memory stream
            return new MemoryStream(new byte[] {});
        }

        public void Dispose() {
            foreach (var textures in _textureCache) {
                textures.Value?.Dispose();
            }
            _packArchive?.Dispose();
        }

    }



}
