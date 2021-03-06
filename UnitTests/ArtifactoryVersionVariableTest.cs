﻿// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariableTest.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ArtifactoryExtension.Variables;
    using System.IO;
    
    /// <summary>
    /// This is a test class for ArtifactoryVersionVariableTest and is intended
    /// to contain all ArtifactoryVersionVariableTest Unit Tests
    /// </summary>
    [TestClass]
    public class ArtifactoryVersionVariableTest
    {
        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext)
        // {
        // }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup()
        // {
        // }
        //
        // Use TestInitialize to run code before running each test
        // [TestInitialize()]
        // public void MyTestInitialize()
        // {
        // }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup()
        // {
        // }
        #endregion

        ArtifactoryVersionVariable variableReleasesGrouped = new ArtifactoryVersionVariable
        {
            RepositoryKey = "libs-release-local",
            RepositoryPath = "example.com",
            Filter = "%RELNO%",
            TrimFromPath = "example.com",
            ReplaceSlashWithDot = false,
            DefaultToNotIncluded = false
        };

        ArtifactoryVersionVariable variableReleasePerFolder = new ArtifactoryVersionVariable
        {
            RepositoryKey = "libs-release-local",
            RepositoryPath = "myapp/%RELNO%",
            Filter = String.Empty,
            TrimFromPath = "myapp",
            ReplaceSlashWithDot = true,
            DefaultToNotIncluded = false
        };

        //TODO
        // predicate working
        // transfer file prepending characters
        // location of "current directory"
        [TestMethod]
        public void test()
        {
            FileStream srcStream = null;
            Stream destStream = null;

            try
            {
                srcStream = File.Open("c:/temp/trial.sh", FileMode.Open, FileAccess.Read);
                destStream = File.Open("c:/temp/trial2.sh", FileMode.Create, FileAccess.Write);

                //const int ONE_MB = 1048576;
                //const int TEN_KB = 10240;

                Byte[] buffer = new Byte[32 * 1024]; // a 32KB-sized buffer is the most efficient
                int bytesRead;

                while ((bytesRead = srcStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destStream.Write(buffer, 0, bytesRead);
                }
            }
            finally
            {
                if (srcStream != null) srcStream.Close();
                if (destStream != null) destStream.Close();
            }   
        }

        [TestMethod]
        public void RepositoryPathRequiresExpanding()
        {
            Assert.AreEqual(false, variableReleasesGrouped.RepositoryPathRequiresExpanding());
            Assert.AreEqual(true, variableReleasePerFolder.RepositoryPathRequiresExpanding());
            
        }

        [TestMethod]
        public void ExpandFilter()
        {
            Assert.AreEqual("0.1", variableReleasesGrouped.ExpandFilter("0.1", "0.1"));
            Assert.AreEqual(String.Empty, variableReleasePerFolder.ExpandFilter("0.1", "0.1"));

        }

        [TestMethod]
        public void ExpandTrimFromPath()
        {
            Assert.AreEqual("libs-release-local/example.org", variableReleasesGrouped.ExpandTrimFromPath("0.1", "0.1"));
            Assert.AreEqual("libs-release-local/myapp", variableReleasePerFolder.ExpandTrimFromPath("0.1", "0.1"));
        }

        [TestMethod]
        public void ExpandRepositoryPath()
        {
            Assert.AreEqual("libs-release-local/example.org", variableReleasesGrouped.ExpandRepositoryPath("0.1", "0.1"));
            Assert.AreEqual("libs-release-local/myapp/0.1", variableReleasePerFolder.ExpandRepositoryPath("0.1", "0.1"));
        }

        [TestMethod]
        public void GetRepositoryPath()
        {
            // Releases Grouped
            Assert.AreEqual("libs-release-local/example.org/0.1.33", variableReleasesGrouped.GetRepositoryPath("0.1.33"));

            string trim = variableReleasesGrouped.TrimFromPath;

            variableReleasesGrouped.TrimFromPath = String.Empty;

            Assert.AreEqual("libs-release-local/example.org/0.1.33", variableReleasesGrouped.GetRepositoryPath("example.org/0.1.33"));

            variableReleasesGrouped.TrimFromPath = trim;


            // Release Per Folder
            Assert.AreEqual("libs-release-local/myapp/0.1/33", variableReleasePerFolder.GetRepositoryPath("0.1.33"));

            trim = variableReleasePerFolder.TrimFromPath;
            bool replace = variableReleasePerFolder.ReplaceSlashWithDot;

            try
            {
                variableReleasePerFolder.TrimFromPath = String.Empty;
                Assert.AreEqual("libs-release-local/myapp/0.1/33", variableReleasePerFolder.GetRepositoryPath("/myapp/0.1.33"));

                variableReleasePerFolder.ReplaceSlashWithDot = false;
                Assert.AreEqual("libs-release-local/myapp/0.1/33", variableReleasePerFolder.GetRepositoryPath("/myapp/0.1/33"));
            }
            finally
            {
                variableReleasePerFolder.TrimFromPath = trim;
                variableReleasePerFolder.ReplaceSlashWithDot = replace;
            }
        }
    }
}
