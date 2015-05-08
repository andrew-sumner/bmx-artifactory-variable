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

        public ArtifactoryApi(String baseUrl)
        {
            this.client = new RestClient(baseUrl);
        }

        public ArtifactoryApi(String baseUrl, String username, String password)
        {
            this.client = new RestClient(baseUrl);
            this.client.Authenticator = new HttpBasicAuthenticator(username, password);

            //username: admin
    	    //password: password
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
    }
}
