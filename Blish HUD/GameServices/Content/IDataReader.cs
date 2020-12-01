using System;
using System.IO;
using System.Threading.Tasks;

namespace Blish_HUD.Content {

    /// <summary>
    /// Provides methods to read from various file systems, endpoints, and formats.
    /// </summary>
    public interface IDataReader : IDisposable {

        /// <summary>
        /// The physical path or root of what the <see cref="IDataReader"/> represents.
        /// </summary>
        string PhysicalPath { get; }

        /// <summary>
        /// Gets a new <see cref="IDataReader"/> with the root path set to the provided <see cref="subPath"/>.
        /// </summary>
        /// <param name="subPath">A sub path within the context of the <see cref="IDataReader"/>.</param>
        IDataReader GetSubPath(string subPath);

        /// <summary>
        /// Returns a string representation of the current path the DataReader is reading from. This won't necessarily be a valid file path.
        /// </summary>
        /// <param name="relativeFilePath">If provided, the path to a file within the <see cref="IDataReader"/> will be returned.</param>
        string GetPathRepresentation(string relativeFilePath = null);

        /// <summary>
        /// Enumerates all available files. Files that have the extension <see cref="fileExtension"/>
        /// will be passed to the provided <see cref="loadFileFunc"/>.
        /// </summary>
        /// <param name="loadFileFunc">The method to call on all files within the context of the <see cref="IDataReader"/> that have the required file extension.</param>
        /// <param name="fileExtension">The file extension criteria. Should contain the '.' before the extension. If no fileExtension is provided, all files will meet the criteria.</param>
        void LoadOnFileType(Action<Stream, IDataReader> loadFileFunc, string fileExtension = "", IProgress<string> progress = null);

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="filePath">A path to a file within the context of the <see cref="IDataReader"/>.</param>
        bool FileExists(string filePath);

        /// <summary>
        /// Retreives a file stream of the file.
        /// </summary>
        /// <param name="filePath">A path to a file within the context of the <see cref="IDataReader"/>.</param>
        /// <returns>
        /// A task that represents the file's opened stream.
        /// If the file does not exist or cannot be read, <see cref="T:null"/> will be returned instead of a <see cref="Stream"/>.
        /// </returns>
        Stream GetFileStream(string filePath);

        /// <summary>
        /// Opens a file and returns the raw data in a byte array.
        /// </summary>
        /// <param name="filePath">A path to a file within the context of the <see cref="IDataReader"/>.</param>
        /// <returns>
        /// A byte array of the file's data.
        /// If the file does not exist or cannot be read, <see cref="T:null"/> will be returned instead of <see cref="T:byte[]"/>.
        /// </returns>
        byte[] GetFileBytes(string filePath);

        /// <summary>
        /// Opens a file, writes the raw data to the provided <see cref="fileBuffer"/> and returns the length of the data read.
        /// </summary>
        /// <param name="filePath">A path to a file within the context of the <see cref="IDataReader"/>.</param>
        /// <param name="fileBuffer">The buffer to write the file's data into.</param>
        /// <returns>
        /// The total number of bytes successfully read into the <see cref="fileBuffer"/>.
        /// If the file does not exist or cannot be read, the buffer will be empty and the return value will be 0.
        /// </returns>
        int GetFileBytes(string filePath, out byte[] fileBuffer);

        /// <summary>
        /// Asynchronously retreives a stream of the file.
        /// </summary>
        /// <param name="filePath">A path to a file within the context of the <see cref="IDataReader"/>.</param>
        /// <returns>
        /// A task that represents the file's opened stream.
        /// If the file does not exist or cannot be read, the <see cref="Task"/> will result in <see cref="T:null"/> instead of a <see cref="Stream"/>.
        /// </returns>
        Task<Stream> GetFileStreamAsync(string filePath);

        /// <summary>
        /// Asynchronously opens a file and returns the raw data in a byte array.
        /// </summary>
        /// <param name="filePath">A path to a file within the context of the <see cref="IDataReader"/>.</param>
        /// <returns>
        /// A task that represents a byte array of the file's data.
        /// If the file does not exist or cannot be read, the <see cref="Task"/> will result in <see cref="T:null"/> instead of a <see cref="T:byte[]"/>.
        /// </returns>
        Task<byte[]> GetFileBytesAsync(string filePath);

    }

}
