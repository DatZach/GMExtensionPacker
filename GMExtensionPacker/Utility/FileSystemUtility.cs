using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GMExtensionPacker.Utility
{
    public static class FileSystemUtility
    {
        
    }

    internal sealed class WorkingDirectory : IDisposable
    {
        private readonly string directory;

        private WorkingDirectory(string directory)
        {
            this.directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        public static implicit operator string(WorkingDirectory workingDirectory)
        {
            return workingDirectory.directory;
        }

        public void Dispose()
        {
            Directory.Delete(directory, true);
        }

        public static WorkingDirectory Create()
        {
            string directory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(directory);

            return new WorkingDirectory(directory);
        }
    }
}
