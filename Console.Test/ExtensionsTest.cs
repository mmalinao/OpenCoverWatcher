using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;
using NUnitCategories;
using Console;

namespace Console.Test
{
    [TestFixture]
    public class ExtensionsTest
    {

        #region ParseKeysFromArgs Tests

        [Test]
        [UnitTest]
        public void ParseKeysFromArgs_returns_array_of_valid_keys_from_arguments()
        {
            // ASSEMBLE
            var args = new[] {"-targetdir:\"target dir\"", "-outputdir:\"output dir\""};
            var expected = new[] {"-targetdir", "-outputdir"};

            // ACT
            var actual = args.ParseKeysFromArgs();
            
            // ASSERT
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        [UnitTest]
        public void ParseKeysFromArgs_returns_empty_array_given_empty_array()
        {
            //ASSEMBLE
            var args = new string[] {};
            var expected = new string[] {};

            //ACT
            var actual = args.ParseKeysFromArgs();

            //ASSERT
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        #endregion

        #region ParseValueFromArg Tests

        [Test]
        [UnitTest]
        public void ParseValueFromArg_returns_argument_string_parsed_from_array_of_arguments()
        {
            // ASSEMBLE
            string[] args = new[] {"-testarg:test arg"};
            const string expected = "test arg";

            // ACT
            var actual = args.ParseValueFromArg("-testarg");

            // ASSERT
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [UnitTest]
        public void ParseValueFromArg_returns_null_given_argument_that_does_not_exist_in_array()
        {
            // ASSEMBLE
            string[] args = new[] {"-testarg:test arg"};

            // ACT
            var actual = args.ParseValueFromArg("-somearg");

            // ASSERT
            Assert.IsNull(actual);
        }

        [Test]
        [UnitTest]
        public void ParseValueFromArg_throws_InvalidOperationException_test()
        {
            // ASSEMBLE
            string[] args = new[] {"-testarg:test arg", "-testarg:duplicate arg"};

            // ACT
            // ASSERT
            var e = Assert.Throws<InvalidOperationException>(() => args.ParseValueFromArg("-testarg"));
            Assert.AreEqual("Sequence contains more than one matching element", e.Message);
        }

        #endregion

        #region ToOpenCoverTargetArgs Tests

        [Test]
        [UnitTest]
        public void ToOpenCoverTargetArgs_returns_empty_string_given_empty_array_of_files()
        {
            //ASSEMBLE
            var files = new FileInfo[] {};

            //ACT
            var actual = files.ToOpenCoverTargetArgs();

            //ASSERT
            Assert.AreEqual(String.Empty, actual);
        }

        [Test]
        [UnitTest]
        public void ToOpenCoverTargetArgs_returns_string_of_full_filenames()
        {
            //ASSEMBLE
            var files = new FileInfo[] { new FileInfo(@"C:\path1\file1.dll"), new FileInfo(@"C:\path2\file2.dll"), };
            const string expected = @"C:\path1\file1.dll C:\path2\file2.dll ";

            //ACT
            var actual = files.ToOpenCoverTargetArgs();

            //ASSERT
            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region ToOpenCoverFilters Tests

        [Test]
        [UnitTest]
        public void ToOpenCoverFilters_returns_string_filter_that_includes_all_namespaces_and_classes_given_empty_array_of_files()
        {
            //ASSEMBLE
            var files = new FileInfo[] {};
            const string expected = " +[*]*";
            
            //ACT
            var actual = files.ToOpenCoverFilters();

            //ASSERT
            Assert.AreEqual(expected, actual);
        }

        [Test]
        [UnitTest]
        public void ToOpenCoverFilters_returns_string_filter_that_excludes_classes_found_in_given_array_of_files()
        {
            //ASSEMBLE
            var files = new FileInfo[] {new FileInfo(@"C:\path1\file1.dll"), new FileInfo(@"C:\path2\file2.dll")};
            const string expected = "-[file1]* -[file2]*  +[*]*";

            //ACT
            var actual = files.ToOpenCoverFilters();

            //ASSERT
            Assert.AreEqual(expected, actual);
        }


        #endregion
    }
}
