using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemInterface.IO;
using SystemWrapper.IO;
using NUnit.Framework;
using NUnitCategories;
using Moq;
using Client;

namespace Client.Test
{
    [TestFixture]
    public class WatcherViewModelTest
    {
        private Mock<IWatcherModel> _mockModel;
        private Mock<IFolderBrowserDialogWrap> _mockFolderBrowserDialog; 
        private Mock<IWatcherViewModel> _mockViewModel;

        #region Setup and Teardown

        [SetUp]
        public void Setup()
        {
            _mockModel = new Mock<IWatcherModel>();
            _mockViewModel = new Mock<IWatcherViewModel>();
            _mockFolderBrowserDialog = new Mock<IFolderBrowserDialogWrap>();
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class MockAppConfig : Attribute, ITestAction
        {
            private Configuration _config;

            public void BeforeTest(TestDetails testDetails)
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Clear();
                config.AppSettings.Settings.Add(ExecutableKey.NUnit.ToString(), "nunit-console.exe");
                config.AppSettings.Settings.Add(ExecutableKey.OpenCover.ToString(), "OpenCover.Console.exe");
                config.AppSettings.Settings.Add(ExecutableKey.ReportGen.ToString(), "ReportGenerator.exe");
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }

            public void AfterTest(TestDetails testDetails)
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings.Clear();
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }

            public ActionTargets Targets
            {
                get { return ActionTargets.Test; }
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class CreateFakeSolutionDirectory : Attribute, ITestAction
        {
            private DirectoryInfo _solDir;

            public void BeforeTest(TestDetails testDetails)
            {
                _solDir = new DirectoryInfo("FakeSolution");
                
                if (_solDir.Exists)
                    _solDir.Delete(true);

                _solDir.Create();
            }

            public void AfterTest(TestDetails testDetails)
            {
                _solDir.Delete(true);
            }

            public ActionTargets Targets
            {
                get { return ActionTargets.Test; }
            }
        }

        [AttributeUsage(AttributeTargets.Method)]
        private class CreateFakeCsProjectDirectory : Attribute, ITestAction
        {            
            private DirectoryInfo _projDir;

            public void BeforeTest(TestDetails testDetails)
            {
                _projDir = new DirectoryInfo("FakeCsProj");

                if (_projDir.Exists)
                    _projDir.Delete(true);

                _projDir.Create();
            }

            public void AfterTest(TestDetails testDetails)
            {
                _projDir.Delete(true);
            }

            public ActionTargets Targets
            {
                get { return ActionTargets.Test; }
            }
        }

        #endregion

        #region Constructor Tests

        private static void ModifyAppSetting(string key, string value)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(key);
            config.AppSettings.Settings.Add(key, value);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private static void RemoveAppSetting(string key)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings.Remove(key);
            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void Constructor_throws_ApplicationException_if_NUnit_executable_path_not_found()
        {
            // ASSEMBLE
            const string path = "C:\\badpath\\nunit-console.exe";
            ModifyAppSetting(ExecutableKey.NUnit.ToString(), path);

            //ACT
            //ASSERT
            var e = Assert.Throws<ApplicationException>(() => new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object));
            Assert.AreEqual(path + " executable could not be found", e.Message);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void Constructor_throws_ApplicationException_if_NUnit_key_in_appSettings_is_missing()
        {
            //ASSEMBLE
            RemoveAppSetting(ExecutableKey.NUnit.ToString());

            //ACT
            //ASSERT
            var e = Assert.Throws<ApplicationException>(() => new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object));
            Assert.AreEqual(String.Format("Invalid configuration - missing {0} key in appSettings", 
                            ExecutableKey.NUnit), e.Message);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void Constructor_throws_ApplicationException_if_OpenCover_executable_path_not_found()
        {
            //ASSEMBLE
            const string path = "C:\\badpath\\OpenCover.Console.exe";
            ModifyAppSetting(ExecutableKey.OpenCover.ToString(), path);
            //ACT
            //ASSERT
            var e = Assert.Throws<ApplicationException>(() => new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object));
            Assert.AreEqual(path + " executable could not be found", e.Message);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void Constructor_throws_ApplicationException_if_OpenCover_key_in_appSettings_is_missing()
        {
            //ASSEMBLE
            RemoveAppSetting(ExecutableKey.OpenCover.ToString());

            //ACT
            //ASSERT
            var e = Assert.Throws<ApplicationException>(() => new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object));
            Assert.AreEqual(String.Format("Invalid configuration - missing {0} key in appSettings",
                            ExecutableKey.OpenCover), e.Message);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void Constructor_throws_ApplicationException_if_ReportGenerator_executable_path_not_found()
        {
            //ASSEMBLE
            const string path = "C:\\badpath\\ReportGenerator.exe";
            ModifyAppSetting(ExecutableKey.ReportGen.ToString(), path);

            //ACT
            //ASSERT
            var e = Assert.Throws<ApplicationException>(() => new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object));
            Assert.AreEqual(path + " executable could not be found", e.Message);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void Constructor_throws_ApplicationException_if_ReportGenerator_key_in_appSettings_is_missing()
        {
            //ASSEMBLE
            RemoveAppSetting(ExecutableKey.ReportGen.ToString());

            //ACT
            //ASSERT
            var e = Assert.Throws<ApplicationException>(() => new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object));
            Assert.AreEqual(String.Format("Invalid configuration - missing {0} key in appSettings",
                            ExecutableKey.ReportGen), e.Message);
        }

        #endregion

        #region Properties Tests

        [Test]
        [UnitTest, MockAppConfig]
        public void SolutionDirectoryPath_getter_returns_full_path_when_models_SolutionDirectory_set()
        {
            // ASSEMBLE
            const string expected = "C:\\fullpath";
            _mockModel.Setup(x => x.SolutionDirectory.FullName).Returns(expected);
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);

            // ACT            
            var actual = vm.SolutionDirectoryPath;

            // ASSERT
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void SolutionDirectoryPath_getter_returns_empty_string_when_models_SolutionDirectory_not_set()
        {         
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);

            // ACT            
            var actual = vm.SolutionDirectoryPath;

            // ASSERT
            Assert.AreEqual(String.Empty, actual);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void SolutionDirectoryPath_setter_does_nothing_if_given_an_empty_string()
        {
            // ASSEMBLE     
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            string actualPropertyName = null;

            vm.PropertyChanged += (sender, args) => { actualPropertyName = args.PropertyName; };

            // ACT            
            vm.SolutionDirectoryPath = String.Empty;

            // ASSERT
            _mockModel.VerifySet(x => x.SolutionDirectory, Times.Never());
            Assert.IsNull(actualPropertyName);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void SolutionDirectoryPath_setter_replaces_models_SolutionDirectory_with_given_value()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            var expectedDirInfo = new DirectoryInfoWrap("somepath");

            _mockModel.SetupProperty(x => x.SolutionDirectory);

            // ACT
            vm.SolutionDirectoryPath = "somepath";

            // ASSERT
            _mockModel.VerifySet(x => x.SolutionDirectory);
            Assert.That(_mockModel.Object.SolutionDirectory, Is.EqualTo(expectedDirInfo).Using(new DirectoryInfoWrapComparer()));
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void SolutionDirectoryPath_setter_raises_PropertyChangedEvent()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            const string expected = "SolutionDirectoryPath";
            string actual = null;

            vm.PropertyChanged += (sender, args) => { actual = args.PropertyName; };

            // ACT
            vm.SolutionDirectoryPath = "somepath";

            // ASSERT
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void TestProjectDirectories_getter_returns_models_ProjectDirectories()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            var expectedDirectories = new List<IDirectoryInfo>()
            {
                new DirectoryInfoWrap("somedirectory1"),
                new DirectoryInfoWrap("somedirectory2")
            };
            _mockModel.SetupGet(x => x.TestProjectDirectories).Returns(expectedDirectories);

            // ACT
            var actual = vm.TestProjectDirectories;

            // ASSERT
            _mockModel.VerifyGet(x => x.TestProjectDirectories);
            Assert.That(_mockModel.Object.TestProjectDirectories, Is.EqualTo(expectedDirectories).Using(new DirectoryInfoWrapComparer()));
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void TestProjectDirectories_setter_replaces_models_ProjectDirectories_with_given_value()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            var expectedDirectories = new List<IDirectoryInfo>()
            {
                new DirectoryInfoWrap("somedirectory1"),
                new DirectoryInfoWrap("somedirectory2")
            };
            _mockModel.SetupProperty(x => x.TestProjectDirectories);

            // ACT
            vm.TestProjectDirectories = new List<IDirectoryInfo>()
            {
                new DirectoryInfoWrap("somedirectory1"),
                new DirectoryInfoWrap("somedirectory2")
            };

            // ASSERT
            _mockModel.VerifySet(x => x.TestProjectDirectories);
            Assert.That(_mockModel.Object.TestProjectDirectories, Is.EqualTo(expectedDirectories).Using(new DirectoryInfoWrapComparer()));
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void TestProjectDirectories_setter_raises_PropertyChanged_event()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            const string expected = "ProjectDirectories";
            string actual = null;

            vm.PropertyChanged += (sender, args) => { actual = args.PropertyName; };

            // ACT
            vm.TestProjectDirectories = new List<IDirectoryInfo>();

            // ASSERT
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region Commands

        [Test]
        [UnitTest, MockAppConfig]
        public void BrowseFolderCommand_sets_SolutionDirectoryPath_by_prompting_user()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            _mockFolderBrowserDialog.Setup(x => x.PromptFolderBrowserDialog()).Returns("somepath");

            var command = vm.BrowseFolderCommand;

            // ACT
            command.Execute(null);

            // ASSERT
            _mockModel.VerifySet(x => x.SolutionDirectory);
        }

        [Test]
        [UnitTest, MockAppConfig]
        public void BrowseFolderCommand_sets_ProjectDirectories_based_on_selected_solution_directory()
        {
            // ASSEMBLE
            var vm = new WatcherViewModel(_mockModel.Object, _mockFolderBrowserDialog.Object);
            _mockFolderBrowserDialog.Setup(x => x.PromptFolderBrowserDialog()).Returns("somepath");

            var command = vm.BrowseFolderCommand;

            // ACT
            command.Execute(null);

            // ASSERT
            _mockModel.VerifySet(x => x.TestProjectDirectories);
        }


        #endregion

        #region Extensions Tests

        [Test]
        [UnitTest]
        public void IsSolutionDirectory_returns_false_if_given_null_argument()
        {
            // ASSEMBLE
            IDirectoryInfo dir = null;

            // ACT
            // ASSERT
            Assert.IsFalse(dir.IsSolutionDirectory());
        }

        [Test]
        [UnitTest]
        public void IsSolutionDirectory_returns_false_if_given_directory_does_not_exist()
        {
            // ASSEMBLE
            var solDir = new DirectoryInfoWrap("FakeSolution");

            // ACT            
            // ASSERT
            Assert.IsFalse(solDir.IsSolutionDirectory());
        }

        [Test]
        [UnitTest, CreateFakeSolutionDirectory]
        public void IsSolutionDirectory_returns_false_if_no_sln_file_in_given_directory()
        {
            // ASSEMBLE
            var solDir = new DirectoryInfoWrap("FakeSolution");

            // ACT
            // ASSERT
            Assert.IsFalse(solDir.IsSolutionDirectory());
        }

        [Test]
        [UnitTest, CreateFakeSolutionDirectory]
        public void IsSolutionDirectory_returns_true_if_sln_file_in_given_directory()
        {         
            // ASSEMBLE
            var solDir = new DirectoryInfoWrap("FakeSolution");
            var solFile = new FileInfo(solDir.FullName + "\\FakeSolution.sln");
            solFile.Create().Close();

            // ACT
            // ASSERT
            Assert.IsTrue(solDir.IsSolutionDirectory());
        }

        [Test]
        [UnitTest]
        public void IsProjectDirectory_returns_false_if_given_null_argument()
        {
            // ASSEMBLE
            IDirectoryInfo dir = null;

            // ACT
            // ASSERT
            Assert.IsFalse(dir.IsProjectDirectory());
        }

        [Test]
        [UnitTest]
        public void IsProjectDirectory_returns_false_if_given_directory_does_not_exist()
        {
            // ASSEMBLE
            var csProjDir = new DirectoryInfoWrap("FakeCsProj");

            // ACT
            // ASSERT
            Assert.IsFalse(csProjDir.IsProjectDirectory());
        }

        [Test]
        [UnitTest, CreateFakeCsProjectDirectory]
        public void IsProjectDirectory_returns_false_if_no_csproj_file_in_given_directory()
        {
            // ASSEMBLE
            var csProjDir = new DirectoryInfoWrap("FakeCsProj");

            // ACT
            // ASSERT
            Assert.IsFalse(csProjDir.IsProjectDirectory());
        }

        [Test]
        [UnitTest, CreateFakeCsProjectDirectory]
        public void IsProjectDirectory_returns_true_if_csproj_file_in_given_directory()
        {
            // ASSEMBLE
            var csProjDir = new DirectoryInfoWrap("FakeCsProj");
            var csProjFile = new FileInfo(csProjDir.FullName + "\\FakeCsProj.csproj");
            csProjFile.Create().Close();

            // ACT
            // ASSERT
            Assert.IsTrue(csProjDir.IsProjectDirectory());
        }

        [Test]
        [UnitTest]
        public void GetProjectDirectories_returns_empty_list_if_given_invalid_solution_directory()
        {
            // ASSEMBLE
            var solDir = new DirectoryInfoWrap("FakeSolution");
            
            // ACT
            var actual = solDir.GetProjectDirectories();

            // ASSERT
            Assert.IsEmpty(actual);
        }

        [Test]
        [UnitTest, CreateFakeSolutionDirectory]
        public void GetProjectDirectories_returns_empty_list_if_given_valid_solution_directory_with_no_sub_project_directories()
        {
            // ASSEMBLE
            var solDir = new DirectoryInfoWrap("FakeSolution");
            var solFile = new FileInfo(solDir.FullName + "\\FakeSolution.sln");
            solFile.Create().Close();

            // ACT
            var actual = solDir.GetProjectDirectories();

            // ASSERT
            Assert.IsEmpty(actual);
        }

        [Test]
        [UnitTest, CreateFakeSolutionDirectory]
        public void GetProjectDirectories_returns_list_given_valid_solution_directory_with_sub_project_directories()
        {
            // ASSEMBLE
            var solDir = new DirectoryInfoWrap("FakeSolution");
            var solFile = new FileInfo(solDir.FullName + "\\FakeSolution.sln");
            solFile.Create().Close();

            var csProjDir = new DirectoryInfoWrap("FakeSolution\\FakeCsProj");
            csProjDir.Create();
            var csProjFile = new FileInfo(csProjDir.FullName + "\\FakeCsProj.csproj");
            csProjFile.Create().Close();

            const int expectedCount = 1;

            // ACT
            var actual = solDir.GetProjectDirectories();

            // ASSERT
            Assert.AreEqual(expectedCount, actual.Count());
        }

        #endregion

        #region IEqualityComparer

        internal class DirectoryInfoWrapComparer : IEqualityComparer<DirectoryInfoWrap>
        {
            public bool Equals(DirectoryInfoWrap x, DirectoryInfoWrap y)
            {
                return (x.Exists == y.Exists) && (x.FullName == y.FullName);
            }

            public int GetHashCode(DirectoryInfoWrap obj)
            {
                return (obj.Exists.ToString() + obj.FullName).GetHashCode();
            }
        }

        #endregion

    }
}
