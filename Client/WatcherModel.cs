using System.Collections.Generic;
using SystemInterface.IO;

namespace Client
{
    public interface IWatcherModel
    {
        IDirectoryInfo SolutionDirectory { get; set; }
        IEnumerable<IDirectoryInfo> TestProjectDirectories { get; set; }
        string BuildConfig { get; set; }
    }

    public class WatcherModel : IWatcherModel
    {
        public IDirectoryInfo SolutionDirectory { get; set; }
        public IEnumerable<IDirectoryInfo> TestProjectDirectories { get; set; }
        public string BuildConfig { get; set; }
    }

}
