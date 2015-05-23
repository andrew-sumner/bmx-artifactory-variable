using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Collections.Generic;
using NZCustomsServiceExtension.Variables;
using NZCustomsServiceExtension.Artifactory;
using NZCustomsServiceExtension.Artifactory.Domain;

namespace UnitTests
{
    [TestClass]
    public class ArtifactoryApiTest
    {
        const String baseUrl = "http://192.168.59.103:8081/artifactory";
        //const String baseUrl = "http://artifactory:8081/artifactory";
            
        [TestMethod]
        public void GetFolderInfo()
        {
            //String repositoryPath = "libs-release-local";

            //ArtifactoryApi artifactory = new ArtifactoryApi(baseUrl);

            //FolderInfo folderInfo = artifactory.GetFolderInfo(repositoryPath);

            //Assert.IsTrue(folderInfo.Children.Count > 0, "Expect folder to contain files");
        }

        [TestMethod]
        public void DeleteItem()
        {
            // Test will fail with artifact not found if this doesn't actually exist
            //String itemPath = "libs-release-local/nz.govt.customs/SmartViewer/0.1.0.462";

            //ArtifactoryApi artifactory = new ArtifactoryApi(baseUrl); // This will fail with no permission error
            ////ArtifactoryApi artifactory = new ArtifactoryApi(baseUrl, "admin", "password"); // This will delete object
            
            //artifactory.DeleteItem(itemPath);
        }

    }
}
