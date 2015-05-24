using Newtonsoft.Json;
using NZCustomsServiceExtension.Artifactory.Domain;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NZCustomsServiceExtension.Artifactory
{
    public class ArtifactoryApi
    {
        private RestClient client;
        
        public ArtifactoryApi(ArtifactoryConfigurer configurer)
        {
            this.client = new RestClient(configurer.Server);

            if (!String.IsNullOrEmpty(configurer.Username) && !String.IsNullOrEmpty(configurer.Password))
            {
                this.client.Authenticator = new HttpBasicAuthenticator(configurer.Username, configurer.Password);
            }
        }

        /**
         * Return information about the provided path
         */
        public FolderInfo GetFolderInfo(String repositoryPath)
        {
            RestRequest request = new RestRequest("/api/storage/{repoPath}", Method.GET);
            request.AddUrlSegment("repoPath", repositoryPath);

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            if (((int)response.StatusCode) > 399)
            {
                throw new ApplicationException(response.StatusDescription);
            }

            return JsonConvert.DeserializeObject<FolderInfo>(response.Content);
        }

        /**
         * Deletes a file or a folder from the specified destination.
         * 
         * Requries Authentication
         */
        public void DeleteItem(String itemPath)
        {
            RestRequest request = new RestRequest(itemPath, Method.DELETE);

            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            if (((int)response.StatusCode) > 399)
            {
                throw new ApplicationException(response.StatusDescription);
            }
        }

        /**
         * Return information about the provided path
         */
        public List<Repository> GetLocalRepositories()
        {
            RestRequest request = new RestRequest("/api/repositories?type=local", Method.GET);
            
            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                const string message = "Error retrieving response.  Check inner details for more info.";
                throw new ApplicationException(message, response.ErrorException);
            }

            if (((int)response.StatusCode) > 399)
            {
                throw new ApplicationException(response.StatusDescription);
            }

            return JsonConvert.DeserializeObject<List<Repository>>(response.Content);
        }
    }
}
