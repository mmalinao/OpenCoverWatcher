using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;

using SystemInterface.IO;
using SystemWrapper.IO;

namespace Client
{
    /*
     * MVVM Pattern
     * 
     * The View and the ViewModel communicate via data-binding, method calls, properties, 
     * events, and messages.
     * 
     * The ViewModel exposes not only models, but other properties (such as state information,
     * like the "is busy" indicator) and commands.
     * 
     * The View handles its own UI events, then maps them to the ViewModel via commands
     * 
     * The Models and properties on the ViewModel are updated from the via via two-way databinding
     * 
     * The Model should be "clean" - only consist of fields/properties and no business logic.
     */

    public interface IWatcherViewModel
    {
        string SolutionDirectoryPath { get; set; }
        IEnumerable<IDirectoryInfo> TestProjectDirectories { get; set; }
        string BuildConfig { get; set; }

        ICommand BrowseFolderCommand { get; }
        ICommand WatchCommand { get; }
        ICommand GenerateCommand { get; }
    }



    public class WatcherViewModel : ViewModel, IWatcherViewModel, IDisposable
    {
        public WatcherViewModel(IWatcherModel model, IFolderBrowserDialogWrap folderBrowserDialog)
        {
            _model = model;
            _folderBrowserDialog = folderBrowserDialog;

            foreach (var name in Enum.GetNames(typeof(ExecutableKey)).Where(name => ConfigurationManager.AppSettings[name] == null))
                throw new ApplicationException(String.Format("Invalid configuration - missing {0} key in appSettings", name));            

            _executables = new Dictionary<ExecutableKey, FileInfo>()
            {
                { ExecutableKey.NUnit, new FileInfo(ConfigurationManager.AppSettings[ExecutableKey.NUnit.ToString()]) },
                { ExecutableKey.OpenCover, new FileInfo(ConfigurationManager.AppSettings[ExecutableKey.OpenCover.ToString()]) },
                { ExecutableKey.ReportGen, new FileInfo(ConfigurationManager.AppSettings[ExecutableKey.ReportGen.ToString()]) }
            };

            foreach (var executable in _executables.Where(executable => !executable.Value.Exists))
                throw new ApplicationException(String.Format("{0} executable could not be found", executable.Value.FullName));

            _availableProjects = new List<ProjectItem>();
        }

        #region Fields

        public static readonly string TempBuildConfig = "Local";    // TODO: Temporary static variable

        private readonly IWatcherModel _model;         
        private readonly IFolderBrowserDialogWrap _folderBrowserDialog;
        private readonly Dictionary<ExecutableKey, FileInfo> _executables;         
        private List<FileSystemWatcher> _watchers;

        private IEnumerable<ProjectItem> _availableProjects; 

        #endregion 

        #region Properties - these are exposed to the View via Binding

        public string SolutionDirectoryPath
        {
            //get { return _model.SolutionDirectory.FullName; }
            get { return (_model.SolutionDirectory == null) ? String.Empty : _model.SolutionDirectory.FullName; }
            set
            {
                if (value == String.Empty) 
                    return;

                var dir = new DirectoryInfoWrap(value);

                // TODO: Need to wait for Window loaded before instantiating this class in order for Validation to work properly
                //if (!dir.IsSolutionDirectory())
                //    throw new ApplicationException("Not a VisualStudio directory");

                _model.SolutionDirectory = dir;
                NotifyChange(() => SolutionDirectoryPath);
            }
        }

        public IEnumerable<IDirectoryInfo> TestProjectDirectories
        {
            get { return _model.TestProjectDirectories; }
            set
            {
                _model.TestProjectDirectories = value;
                NotifyChange(() => TestProjectDirectories);
            }
        }

        public IEnumerable<ProjectItem> AvailableProjects
        {
            get { return _availableProjects; }
            set
            {
                _availableProjects = value;
                NotifyChange(() => AvailableProjects);
            }
        }

        public string BuildConfig
        {
            get { return _model.BuildConfig; }
            set
            {
                _model.BuildConfig = value;
                NotifyChange(() => BuildConfig);
            }
        }

        public ICommand BrowseFolderCommand
        {
            get { return new DelegateCommand(BrowseFolder); }
        }

        public ICommand WatchCommand
        {
            get { return new DelegateCommand(Watch); }
        }

        public ICommand GenerateCommand
        {
            get { return new DelegateCommand(GenerateCodeCoverage); }
        }

        #endregion

        #region Commands

        private void BrowseFolder(object obj)
        {
            SolutionDirectoryPath = _folderBrowserDialog.PromptFolderBrowserDialog();

            //TestProjectDirectories = _model.SolutionDirectory.GetProjectDirectories();
            
            var dirInfos = _model.SolutionDirectory.GetProjectDirectories();
            AvailableProjects = (from dirInfo in dirInfos
                                 select new ProjectItem()
                                     {
                                         DirInfo = dirInfo, 
                                         IsSelected = dirInfo.Name.ToLower().Contains("test")
                                     }).ToList();
        }

        private void Watch(object obj)
        {
            _model.TestProjectDirectories = _availableProjects.Where(x => x.IsSelected)
                                                              .Select(x => new DirectoryInfoWrap(x.DirInfo.FullName))
                                                              .ToList();

            var configDirs = _model.TestProjectDirectories.GetConfigDirectories(TempBuildConfig).ToList();
            _watchers = new List<FileSystemWatcher>();

            foreach (var configDir in configDirs)
            {
                var watcher = new FileSystemWatcher();

                watcher.Path = configDir.Value.FullName;
                watcher.Filter = configDir.Key + ".dll";
                watcher.NotifyFilter = NotifyFilters.LastWrite;

                watcher.Changed += (sender, args) => GenerateCodeCoverage(sender);

                watcher.EnableRaisingEvents = true;

                _watchers.Add(watcher);
            }
            
        }

        private void GenerateCodeCoverage(object obj)
        {
            _watchers.ForEach(w => w.EnableRaisingEvents = false);  // stop listening for changes while processes run

            // Create temporary folder to hold targeted .dll & .pdb files
            var tempFolder = new DirectoryInfoWrap("temp");

            if (tempFolder.Exists)
                tempFolder.Delete(true);

            tempFolder.Create();

            // Create output folder to hold Code Coverage reporting
            // TODO: Optional user specified output folder
            var outputDir = new DirectoryInfoWrap("CodeCoverage");

            if (outputDir.Exists)
                outputDir.Delete(true);

            outputDir.Create();

            // Copy all files based on given Build Configuration
            var configDirs = _model.TestProjectDirectories.GetConfigDirectories(TempBuildConfig).ToList();
            foreach (var configDir in configDirs)
            {
                var files = configDir.Value.GetFiles().ToList();
                foreach (var file in files)
                {
                    var fileInfo = new FileInfoWrap(tempFolder.FullName + "\\" + file.Name);
                    if (!fileInfo.Exists)
                        file.CopyTo(fileInfo.FullName);
                }
            }

            // Run OpenCover process
            // TODO: Abstract out
            var openCoverProcess = new Process();

            var openCoverArgs = String.Format(
                "-target:\"{0}\" -targetdir:\"{1}\" -targetargs:\"{2}\" -filter:\"{3}\" -output:\"{4}\" -register:user",
                _executables[ExecutableKey.NUnit].FullName,
                tempFolder.FullName,
                tempFolder.GetFiles("*.Test.dll").ToOpenCoverTargetArgs(),
                tempFolder.GetFiles("*.Test.dll").ToOpenCoverFilters(),
                outputDir.FullName + "\\coverage.xml"
            );

            openCoverProcess.StartInfo.FileName = _executables[ExecutableKey.OpenCover].FullName;
            openCoverProcess.StartInfo.UseShellExecute = false;
            openCoverProcess.StartInfo.RedirectStandardOutput = true;
            openCoverProcess.StartInfo.Arguments = openCoverArgs;

            openCoverProcess.Start();

            var ccOutput = openCoverProcess.StandardOutput.ReadToEnd();
            openCoverProcess.WaitForExit();

            // Run ReportGenerator process
            // TODO: Abstract out
            var reportGeneratorProcess = new Process();

            var reportGeneratorArgs = String.Format(
                "-reports:\"{0}\" -targetdir:\"{1}\"",
                outputDir.FullName + @"\coverage.xml",
                outputDir.FullName
            );

            reportGeneratorProcess.StartInfo.FileName = _executables[ExecutableKey.ReportGen].FullName;
            reportGeneratorProcess.StartInfo.UseShellExecute = false;
            reportGeneratorProcess.StartInfo.RedirectStandardOutput = true;
            reportGeneratorProcess.StartInfo.Arguments = reportGeneratorArgs;

            reportGeneratorProcess.Start();

            var rgOutput = reportGeneratorProcess.StandardOutput.ReadToEnd();
            reportGeneratorProcess.WaitForExit();


            tempFolder.Delete(true);
            _watchers.ForEach(w => w.EnableRaisingEvents = true);   // start listening now that processes are finished
        }

        #endregion

        #region Processes



        #endregion

        public void Dispose()
        {
            _watchers.ForEach(x => x.Dispose());
        }
    }

    public class ProjectItem
    {
        public IDirectoryInfo DirInfo { get; set; }
        public bool IsSelected { get; set; }
    }

    public enum ExecutableKey
    {
        NUnit = 1,
        OpenCover = 2,
        ReportGen = 3
    }

    internal static class Extensions
    {
        public static bool IsSolutionDirectory(this IDirectoryInfo dir)
        {
            if (dir == null)
                return false;

            return dir.Exists && dir.GetFiles().Any(x => x.Name.EndsWith(".sln"));
        }

        public static bool IsProjectDirectory(this IDirectoryInfo dir)
        {
            if (dir == null)
                return false;

            return dir.Exists && dir.GetFiles().Any(x => x.Name.EndsWith(".csproj"));
        }

        public static IEnumerable<IDirectoryInfo> GetProjectDirectories(this IDirectoryInfo solDir)
        {
            // if not a valid solution directory, return empty list
            if (!solDir.IsSolutionDirectory())
                return new List<IDirectoryInfo>();  
            // otherwise, return all valid sub project directories
            return solDir.GetDirectories().Where(x => x.IsProjectDirectory()).ToList();
        }

        public static Dictionary<string, IDirectoryInfo> GetConfigDirectories(this IEnumerable<IDirectoryInfo> projDirs,
                                                                              string buildConfig)
        {
            var dict = new Dictionary<string, IDirectoryInfo>();

            foreach (var dir in projDirs)
            {
                if (!dir.IsProjectDirectory())
                    continue;

                var bin = dir.GetDirectories().SingleOrDefault(x => x.Name == "bin");
                if (bin == null)
                    throw new ApplicationException(String.Format("Could not find 'bin' folder in project {0}", dir.Name));

                var configDir = bin.GetDirectories().SingleOrDefault(x => x.Name == buildConfig);
                if (configDir == null)
                    throw new ApplicationException(String.Format("Could not find '{0}' build config folder in project {1}", buildConfig, dir.Name));

                dict.Add(dir.Name, configDir);
            }

            return dict;
        }

        public static string ToOpenCoverTargetArgs(this IEnumerable<IFileInfo> files)
        {
            var str = String.Empty;
            files.ToList().ForEach(file => str += (file.Name + " "));
            return str;
        }

        public static string ToOpenCoverFilters(this IEnumerable<IFileInfo> files)
        {
            var str = "";
            files.ToList().ForEach(file => str += String.Format("-[{0}]* ", file.Name.Replace(".dll", "")));
            str += " +[*]*";
            return str;
        }

    }    

}
