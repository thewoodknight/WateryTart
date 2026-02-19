using System;
using System.IO;

namespace WateryTart.Core.Utilities
{
    /// <summary>
    /// Cross-platform single-instance lock using an exclusive filesystem lock on a file.
    /// Keeps the FileStream open while held so other processes cannot open it with FileShare.None.
    /// </summary>
    public sealed class SingleInstanceLock : IDisposable
    {
        private readonly FileStream _stream;
        private readonly string _path;

        private SingleInstanceLock(FileStream stream, string path)
        {
            _stream = stream;
            _path = path;
        }

        /// <summary>
        /// Try to acquire an exclusive lock on the specified file path.
        /// Returns true and an instance when lock is acquired, otherwise false.
        /// </summary>
        public static bool TryAcquire(string path, out SingleInstanceLock? instance)
        {
            instance = null;

            try
            {
                var dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);

                // Open or create the file with no sharing so other processes cannot open it.
                var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);

                // Write the current process id for debugging and leave the stream open.
                using (var writer = new StreamWriter(stream, leaveOpen: true))
                {
                    writer.Write(Environment.ProcessId);
                    writer.Flush();
                }

                // Rewind so the file contents remain readable if needed
                stream.Seek(0, SeekOrigin.Begin);

                instance = new SingleInstanceLock(stream, path);
                return true;
            }
            catch (IOException)
            {
                // Another process probably holds the file open exclusively
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            try
            {
                _stream?.Dispose();
            }
            catch { }

            try
            {
                if (!string.IsNullOrEmpty(_path) && File.Exists(_path))
                    File.Delete(_path);
            }
            catch { }
        }
    }
}
