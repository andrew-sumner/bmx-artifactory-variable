using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ArtifactoryExtension.Actions;
using Inedo.BuildMaster.Extensibility.Actions.Testing;
using Inedo.BuildMaster.Data;

namespace UnitTests
{
    [TestClass]
    public class CleanupArtifactoryActionTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            MockActionExecutionContext context = new MockActionExecutionContext()
            {
                ApplicationId = 27,      // SmartViewer
                ReleaseNumber = "0.1.0",
                BuildNumber = "564",
                ExecutionId = 96051
                
            };


            

            CleanupArtifactoryAction action = new CleanupArtifactoryAction(context);
            
            action.TestExecute();

            //action.ArtifactoryVariable = "SMARTVIEWER_VERSION";
            
            //PrivateObject po = new PrivateObject(action);
            //po.Invoke("Execute");

            

            
        }


    }
}
