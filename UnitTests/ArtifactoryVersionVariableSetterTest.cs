// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariableSetterTest.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace UnitTests
{
    using System;
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster.Extensibility.Variables;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ArtifactoryExtension.Variables;
    
    /// <summary>
    /// This is a test class for ArtifactoryVersionVariableSetterTest and is intended
    /// to contain all ArtifactoryVersionVariableSetterTest Unit Tests
    /// </summary>
    [TestClass]
    public class ArtifactoryVersionVariableSetterTest
    {
        /// <summary>
        /// The test version.
        /// </summary>
        private const string TestVersion = "482";

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

        /// <summary>
        /// A test for GetReleaseBuilds using release number
        /// </summary>
        [TestMethod]        
        public void GetReleaseBuildsTestUsingReleaseNumber()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory/",
                RepositoryPath = "Cusmod/client/%RELNO%"
            };

            string releaseNumber = "9.4.0";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.IsTrue(actual.Length > 0, "Expect folders for this release");
            Assert.IsTrue(actual[0].Text.StartsWith("Cusmod/client/9.4.0/"), "Showing full path to artifact");
        }

        /// <summary>
        /// A test for GetReleaseBuilds using release number
        /// </summary>
        [TestMethod]
        public void GetReleaseBuildsTestUsingReleaseNumber2()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "libs-release-local/org.example/MiddlewareJBMSInterface"
            };

            string releaseNumber = "1.1";
            ListItem[] allReleases = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.IsTrue(allReleases.Length > 0, "Expect folders for all release");

            variable.Filter = "%RELNO%.*"; // Limit result set to one version
            ListItem[] specificRelease = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.IsTrue(specificRelease.Length > 0, "Expect folders for release " + releaseNumber);
         
            Assert.AreNotEqual(allReleases.Length, specificRelease.Length, "Expect that all releases return more builds than specific release");
            Assert.IsTrue(specificRelease[0].Text.StartsWith("libs-release-local/org.example/MiddlewareJBMSInterface/" + releaseNumber), "Showing full path to artifact");
        }

        /// <summary>
        /// A test for GetReleaseBuilds using release number
        /// </summary>
        [TestMethod]
        public void GetReleaseBuildsTestUsingReleaseNumber3()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/sql/CM%RELNO%",
                Filter = "",  
                TrimFromPath = "Cusmod/sql/",
                ReplaceSlashWithDot = true
            };

            string releaseNumber = "9.6.2";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.IsTrue(actual.Length > 0, "Expect folders for release " + releaseNumber);
            Assert.IsTrue(actual[0].Text.StartsWith("CM" + releaseNumber + "."), "Showing artifact name only");
            Assert.IsTrue(actual[0].Value.StartsWith("CM" + releaseNumber + "."), "Showing artifact name only");
        }

        /// <summary>
        /// A test for GetReleaseBuilds using release number
        /// </summary>
        [TestMethod]        
        public void GetReleaseBuildsTestDefaultToNotIncluded()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/client/%RELNO%",
                Filter = ".*" + TestVersion,   // Limit result set to one item
                DefaultToNotIncluded = true
            };

            string releaseNumber = "9.4.0";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            // TODO: how to test this ???
            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual("Cusmod/client/9.4.0/" + TestVersion, actual[0].Text);
            Assert.AreEqual("Cusmod/client/9.4.0/" + TestVersion, actual[0].Value);
        }

        /// <summary>
        /// A test for GetReleaseBuilds using release name
        /// </summary>
        [TestMethod]
        
        public void GetReleaseBuildsTestUsingReleaseName()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/sql/%RELNAME%",
                Filter = "228"   // Limit result set to one item
            };

            string releaseName = "SG_CashQ_StateCode";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, null, releaseName);

            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual("Cusmod/sql/SG_CashQ_StateCode/228", actual[0].Text);
            Assert.AreEqual("Cusmod/sql/SG_CashQ_StateCode/228", actual[0].Value);
        }

        /// <summary>
        /// A test for GetReleaseBuilds using release name
        /// </summary>
        [TestMethod]
        
        public void GetReleaseBuildsTestUsingReleaseName2()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/server/%RELNAME%",
                //Filter = "9.4.1"   // Limit result set to one item
            };

            string releaseName = "9.4.1";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, null, releaseName);

//            Assert.AreEqual(1, actual.Length);
//            Assert.AreEqual("Cusmod/sql/SG_US_UK_Arrivals/133", actual[0].Text);
//            Assert.AreEqual("Cusmod/sql/SG_US_UK_Arrivals/133", actual[0].Value);
        }

        /// <summary>
        /// A test for GetReleaseBuilds
        /// </summary>
        [TestMethod]
        
        public void GetReleaseBuildsTrimResultTest()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/client/%RELNO%",
                Filter = ".*" + TestVersion,                   // Limit result set to one item
                TrimFromPath = "Cusmod/client/",    // Remove string from result
                ReplaceSlashWithDot = true,          // Replace / with .
                DefaultToNotIncluded = false
            };

            string releaseNumber = "9.4.0";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual("9.4.0." + TestVersion, actual[0].Text);

            variable.TrimFromPath = "Cusmod/client/%RELNO%/";
            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.AreEqual(1, actual.Length);
            Assert.AreEqual(TestVersion, actual[0].Text);
        }

        /// <summary>
        /// A test for GetReleaseBuilds using an invalid url
        /// </summary>
        [TestMethod]
        
        public void GetReleaseBuildsInvalidURLTest()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/client/%RELNO%"
            };

            string releaseNumber = "0.0.0";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.AreEqual(0, actual.Length, "Did not expect to find any builds for release " + releaseNumber);
        }

        /// <summary>
        /// A test for GetReleaseBuilds checking order
        /// </summary>
        [TestMethod]
        
        public void GetReleaseBuildsCheckOrder()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "Cusmod/sql/CM%RELNO%",
                TrimFromPath = "Cusmod/sql/"
            };

            string releaseNumber = "9.4.0";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.AreNotEqual(0, actual.Length, "Expected to get some rows returned");

            // check that CM9.4.0.100 comes after CM9.4.0.2
            for (int i = 0; i < actual.Length - 1; i++)
            {
                int ione, itwo;
                string sone = actual[i].Value.Substring(actual[i].Value.LastIndexOfAny(new char[] { '/', '.' }) + 1);
                string stwo = actual[i + 1].Value.Substring(actual[i + 1].Value.LastIndexOfAny(new char[] { '/', '.' }) + 1);

                Assert.IsTrue(int.TryParse(sone, out ione), "Expected numeric value");
                Assert.IsTrue(int.TryParse(stwo, out itwo), "Expected numeric value");

                Assert.IsTrue(ione <= itwo, string.Format("Array must be sorted in numeric order: {0} > {1}", ione, itwo));
            }

            // Also test when only return build number
            variable.TrimFromPath = "Cusmod/sql/CM%RELNO%/";

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.AreNotEqual(0, actual.Length, "Expected to get some rows returned");

            // check that CM9.4.0.100 comes after CM9.4.0.2
            for (int i = 0; i < actual.Length - 1; i++)
            {
                int ione, itwo;
                
                Assert.IsTrue(int.TryParse(actual[i].Value, out ione), "Expected numeric value");
                Assert.IsTrue(int.TryParse(actual[i + 1].Value, out itwo), "Expected numeric value");

                Assert.IsTrue(ione <= itwo, string.Format("Array must be sorted in numeric order: {0} > {1}", ione, itwo));
            }
        }

        /// <summary>
        /// A test for GetReleaseBuilds checking order
        /// </summary>
        [TestMethod]
        
        public void GetReleaseBuildsNoReleaseNumber()
        {
            // This test is using live data which may change at any time!
            ArtifactoryVersionVariableSetter target = new ArtifactoryVersionVariableSetter();
            ArtifactoryVersionVariable variable = new ArtifactoryVersionVariable
            {
                //ActionServer = "http://artifactory:8081/artifactory",
                RepositoryPath = "libs-release-local/org/example/application/",
                TrimFromPath = "libs-release-local/org/example/application/"
            };

            string releaseNumber = "9.4.0";
            ListItem[] actual;

            actual = target.GetReleaseBuilds(variable, releaseNumber, null);

            Assert.AreNotEqual(0, actual.Length, "Expected to get some rows returned");

            // check that CM9.4.0.100 comes after CM9.4.0.2
            for (int i = 0; i < actual.Length - 1; i++)
            {
                int ione, itwo;
                string sone = actual[i].Value.Substring(actual[i].Value.LastIndexOfAny(new char[] { '/', '.' }) + 1);
                string stwo = actual[i + 1].Value.Substring(actual[i + 1].Value.LastIndexOfAny(new char[] { '/', '.' }) + 1);

                Assert.IsTrue(int.TryParse(sone, out ione), "Expected numeric value");
                Assert.IsTrue(int.TryParse(stwo, out itwo), "Expected numeric value");

                Assert.IsTrue(ione <= itwo, string.Format("Array must be sorted in numeric order: {0} > {1}", ione, itwo));
            }
        }
    }
}
