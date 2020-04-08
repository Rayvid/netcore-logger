using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Planet.Library.Common
{
    public class TempDir : IDisposable
    {
        private ILogger _logger;
        private bool _disposed = false;
        private bool _autoDeleteAfterDispose = true;
        public DirectoryInfo Dir { get; private set; }

        public TempDir(ILogger logger, bool autoDeleteAfterDispose = true)
        {
            _logger = logger;
            _autoDeleteAfterDispose = autoDeleteAfterDispose;
            try
            {
                Dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
            }
            catch
            {
                try
                {
                    Dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                }
                catch
                {
                    Dir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (_autoDeleteAfterDispose)
            {
                Task.Run(() =>
                {
                    try
                    {
                        Dir.Delete(true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Cleanup of temp dir `{Dir.FullName}` failed", ex);
                    }
                });
            }

            if (disposing)
            {
                // Nothing unmanaged
            }

            _disposed = true;
        }
    }
}
