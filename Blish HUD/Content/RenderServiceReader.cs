using System;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using Flurl.Http;

namespace Blish_HUD.Content {

    public sealed class RenderServiceReader : IDataReader {

        #region Not Supported

        /// <inheritdoc />
        IDataReader IDataReader.GetSubPath(string subPath) {
            throw new ActionNotSupportedException("The Render Service does not have sub paths.");
        }

        /// <inheritdoc />
        public string GetPathRepresentation(string relativeFilePath = null) {
            return relativeFilePath ?? "";
        }

        /// <inheritdoc />
        void IDataReader.LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension) {
            throw new ActionNotSupportedException("Can't enumerate all images from render service.");
        }

        /// <inheritdoc />
        bool IDataReader.FileExists(string filePath) {
            throw new ActionNotSupportedException("The Render Service does not support determining if a file exists prior to a request for that file.");
        }

        #endregion

        /// <inheritdoc />
        public Stream GetFileStream(string filePath) {
            var getFileRes = GetFileStreamAsync(filePath);
            getFileRes.Wait();

            if (getFileRes.Status == TaskStatus.RanToCompletion) {
                return getFileRes.Result;
            }

            return null;
        }

        /// <inheritdoc />
        public byte[] GetFileBytes(string filePath) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int GetFileBytes(string filePath, out byte[] fileBuffer) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public async Task<Stream> GetFileStreamAsync(string filePath) {
            string requestUrl = $"https://darthmaim-cdn.de/gw2treasures/icons/{filePath}.png";
            //string requestUrl = $"https://render.guildwars2.com/file/{filePath}.png";

            return await requestUrl.AllowAnyHttpStatus().GetStreamAsync().ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<byte[]> GetFileBytesAsync(string filePath) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Dispose() {
            
        }

    }

}
