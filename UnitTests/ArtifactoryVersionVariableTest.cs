// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariableTest.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace UnitTests
{
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NZCustomsServiceExtension.Variables;
    
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
            RepositoryPath = "mygroup.co.nz",
            Filter = "%RELNO%",
            TrimFromPath = "mygroup.co.nz",
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
            Assert.AreEqual("libs-release-local/mygroup.co.nz", variableReleasesGrouped.ExpandTrimFromPath("0.1", "0.1"));
            Assert.AreEqual("libs-release-local/myapp", variableReleasePerFolder.ExpandTrimFromPath("0.1", "0.1"));
        }

        [TestMethod]
        public void ExpandRepositoryPath()
        {
            Assert.AreEqual("libs-release-local/mygroup.co.nz", variableReleasesGrouped.ExpandRepositoryPath("0.1", "0.1"));
            Assert.AreEqual("libs-release-local/myapp/0.1", variableReleasePerFolder.ExpandRepositoryPath("0.1", "0.1"));
        }

        [TestMethod]
        public void ExpandRepositoryPathWithValue()
        {
            // Releases Grouped
            Assert.AreEqual("libs-release-local/mygroup.co.nz/0.1.33", variableReleasesGrouped.ExpandRepositoryPathWithValue("0.1", "0.1", "0.1.33"));

            string trim = variableReleasesGrouped.TrimFromPath;

            variableReleasesGrouped.TrimFromPath = String.Empty;

            Assert.AreEqual("libs-release-local/mygroup.co.nz/0.1.33", variableReleasesGrouped.ExpandRepositoryPathWithValue("0.1", "0.1", "mygroup.co.nz/0.1.33"));

            variableReleasesGrouped.TrimFromPath = trim;


            // Release Per Folder
            Assert.AreEqual("libs-release-local/myapp/0.1/33", variableReleasePerFolder.ExpandRepositoryPathWithValue("0.1", "0.1", "0.1.33"));

            trim = variableReleasesGrouped.TrimFromPath;

            variableReleasesGrouped.TrimFromPath = "myapp/";

            Assert.AreEqual("libs-release-local/myapp/0.1/33", variableReleasePerFolder.ExpandRepositoryPathWithValue("0.1", "0.1", "0.1.33"));

            variableReleasesGrouped.TrimFromPath = trim;
        }
    }
}
