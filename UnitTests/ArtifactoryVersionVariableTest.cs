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
        
        /// <summary>
        /// A test for GetArtifactoryExtensionServer
        /// </summary>
        [TestMethod]
        [DeploymentItem("NZCustomsServiceExtension.dll")]
        public void GetArtifactoryExtensionServerTest()
        {
            ArtifactoryVersionVariable_Accessor target = new ArtifactoryVersionVariable_Accessor(); 

            string settings = "<Inedo.BuildMasterExtensions.Artifactory.ArtifactoryConfigurer Assembly=\"Artifactory\"><Properties Server=\"http://artifactory:8081/artifactory/\" Username=\"admin\" Password=\"password\" /></Inedo.BuildMasterExtensions.Artifactory.ArtifactoryConfigurer>";            
            string actual = target.GetArtifactoryExtensionServer(settings);

            Assert.AreEqual("http://artifactory:8081/artifactory/", actual);
            
            settings = "<Inedo.BuildMasterExtensions.Artifactory.ArtifactoryConfigurer Assembly=\"Artifactory\"><Properties NoServer=\"http://artifactory:8081/artifactory/\" Username=\"admin\" Password=\"password\" /></Inedo.BuildMasterExtensions.Artifactory.ArtifactoryConfigurer>";
            actual = target.GetArtifactoryExtensionServer(settings);

            Assert.AreEqual(null, actual);
        }
    }
}
