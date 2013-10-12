using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NUnitCategories;
using Moq;

namespace Console.Test
{
    [TestFixture]
    public class WatcherTest
    {
        private Mock<ISettings> _mockSettings;
        private Watcher _watcher;
        
        #region Setup and Teardown

        [SetUp]
        public void Setup()
        {
            _mockSettings = new Mock<ISettings>();
            _mockSettings.Setup(x => x.GetNUnitPath()).Returns(@"C:\Program Files (x86)\NUnit 2.6.2\bin\nunit-console.exe");
            _mockSettings.Setup(x => x.GetOpenCoverPath()).Returns(@"C:\Dev\OpenCover\OpenCover.Console.exe");
            _mockSettings.Setup(x => x.GetReportGeneratorPath()).Returns(@"C:\Dev\ReportGenerator\bin\ReportGenerator.exe");
        }

        #endregion

        #region Constructor Tests

        [Test]
        [UnitTest]
        public void Constructor_throws_UsageException_if_executable_dependency_could_not_be_found()
        {
            // ASSEMBLE
            var args = new string[] { };
            _mockSettings.Setup(x => x.GetNUnitPath()).Returns(@"C:\fake-nunit.exe");
            _mockSettings.Setup(x => x.GetOpenCoverPath()).Returns(@"C:\fake-opencover.exe");
            _mockSettings.Setup(x => x.GetReportGeneratorPath()).Returns(@"C:\fake-reportgenerator.exe");
            
            // ACT
            // ASSERT
            var e = Assert.Throws<UsageException>(() => new Watcher(_mockSettings.Object, args));
            Assert.AreEqual("Executable dependencies could not be found", e.Message);
        }

        #endregion

        #region Init Tests

        [Test]
        [UnitTest]
        public void Init_throws_UsageException_if_no_arguments_provided()
        {         
            // ASSEMBLE
            var watcher = new Watcher(_mockSettings.Object, new string[] {});

            // ACT
            // ASSERT
            var e = Assert.Throws<UsageException>(() => watcher.Init());
            Assert.AreEqual("Error: No arguments provided", e.Message);
        }

        [Test]
        [UnitTest]
        public void Init_throws_UsageException_if_first_argument_is_help()
        {
            // ASSEMBLE
            var watcher = new Watcher(_mockSettings.Object, new string[] {"-?"});

            // ACT
            // ASSERT
            var e = Assert.Throws<UsageException>(() => watcher.Init());
            Assert.AreEqual(String.Empty, e.Message);
        }

        [Test]
        [UnitTest]
        public void Init_throws_UsageException_if_not_all_required_arguments_are_specified()
        {
            // ASSEMBLE
            var args = new string[] { "-buildconfig:Debug" }; 
            var watcher = new Watcher(_mockSettings.Object, args); 

            // ACT
            // ASSERT
            var e = Assert.Throws<UsageException>(() => watcher.Init());
            Assert.AreEqual("Must specify all required arguments", e.Message);
        }

        [Test]
        [UnitTest]
        public void Init_throws_UsageException_if_given_target_directory_does_not_exist()
        {
            // ASSEMBLE
            var args = new string[] { @"-targetdir:C:\Does Not Exist", @"-outputdir:C:\Does Not Exist\Coverage" };
            var watcher = new Watcher(_mockSettings.Object, args);

            // ACT
            // ASSERT
            var e = Assert.Throws<UsageException>(() => watcher.Init());
            Assert.AreEqual("Given target directory does not exist", e.Message);
        }

        [Test]
        [UnitTest]
        public void Init_throws_UsageException_if_no_test_projects_found_under_target_directory()
        {
            // ASSEMBLE
            var args = new string[] { @"-targetdir:" };

            // ACT
            // ASSERT
        }

        #endregion


    }
}
