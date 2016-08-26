using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestSharp;
using System.Collections.Generic;
using ArtifactoryExtension.Variables;
using ArtifactoryExtension.Artifactory;
using ArtifactoryExtension.Artifactory.Domain;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using ArtifactoryExtension;

namespace UnitTests
{
    [TestClass]
    public class ArtifactoryApiTest
    {
        //const String baseUrl = "http://192.168.59.103:8081/artifactory";
        //const String baseUrl = "http://artifactory:8081/artifactory";

        ArtifactoryConfigurer configurer = new ArtifactoryConfigurer 
        {
            Server =  "http://192.168.59.103:8081/artifactory",  // Home
            //Server = "http://artifactory:8081/artifactory",     // Work
            Username = "admin",
            Password = "password"
        };
        
            
        [TestMethod]
        public void GetFolderInfo()
        {
            String repositoryPath = "libs-release-local";

            ArtifactoryApi artifactory = new ArtifactoryApi(configurer);

            FolderInfo folderInfo = artifactory.GetFolderInfo(repositoryPath);

            Assert.IsTrue(folderInfo.Children.Count > 0, "Expect folder to contain files");
        }

        [TestMethod]
        public void DeleteItem()
        {
            ArtifactoryConfigurer deleteConfig = new ArtifactoryConfigurer
            {
                Server = configurer.Server
            };

            // Test will fail with artifact not found if this doesn't actually exist
            String itemPath = "libs-release-local/org.example/SmartViewer/0.1.0.462";

            ArtifactoryApi artifactory = new ArtifactoryApi(deleteConfig); // This will fail with no permission error
            //ArtifactoryApi artifactory = new ArtifactoryApi(configurer); // This will delete object

            artifactory.DeleteItem(itemPath);
        }

        [TestMethod]
        public void GetLocalRepositories()
        {
            ArtifactoryApi artifactory = new ArtifactoryApi(configurer); // This will delete object

            List<Repository> repositories = artifactory.GetLocalRepositories();

            Assert.IsTrue(repositories.Count > 0, "Expect local repositories to exist");
            Assert.IsTrue(repositories.FindIndex(r => r.Key == "libs-release-local") >= 0, "Expect local repositories to exist");
        }
    }
}
