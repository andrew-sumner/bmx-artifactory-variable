// -----------------------------------------------------------------------
// <copyright file="WGetFromArtifactoryActionTest.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace UnitTests
{
    using System;
    using Inedo.BuildMaster.Extensibility.Variables;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using NZCustomsServiceExtension.Actions;

    /// <summary>
    /// This is a test class for WGetFromArtifactoryActionTest and is intended
    /// to contain all WGetFromArtifactoryActionTest Unit Tests
    /// </summary>
    [TestClass]
    public class SaveVariableToArtifactoryPathTest
    {
        /// <summary>
        /// A test for getArguments
        /// </summary>
        [TestMethod]
        
        public void SaveVariableToArtifactoryTest()
        {
            SetVariableToArtifactoryPathAction var = new SetVariableToArtifactoryPathAction();

            var privObj = new PrivateObject(var);

            privObj.SetProperty("VariableName", "CLIENT_PATH");
            privObj.SetProperty("ArtifactoryVariable", "CLIENT_VERSION");

            privObj.Invoke("Execute");

            Assert.Equals(true, true);
        }
    }

    /// <summary>
    /// This is a test class for WGetFromArtifactoryActionTest and is intended
    /// to contain all WGetFromArtifactoryActionTest Unit Tests
    /// </summary>
    [TestClass]
    public class WGetFromArtifactoryActionTest
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
        /// A test for getArguments
        /// </summary>
        [TestMethod]
        
        public void GetArgumentsTest()
        {
            string actual;
            WGetFromArtifactoryAction target;
            
            // A folder path must end with a forward slash
            target = new WGetFromArtifactoryAction()
            {
                //OverrideArtifactoryServer = "http://nowhere",
                AcceptList = "*.sh",
                RepositoryPath = "Cusmod/fred"
            };
            
            actual = target.GetWGetArguments();

            Assert.AreEqual("-m -e robots=off -nd -np -A*.sh http://nowhere/Cusmod/fred/", actual);

            // A file path must not end with a forward slash
            target = new WGetFromArtifactoryAction()
            {
                //OverrideArtifactoryServer = "http://nowhere",
                RepositoryPath = "Cusmod/fred/file.sh"
            };

            actual = target.GetWGetArguments();

            Assert.AreEqual("-m -e robots=off -nd -np http://nowhere/Cusmod/fred/file.sh", actual);
        }
    }
}
