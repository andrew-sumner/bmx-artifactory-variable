using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Extensibility.Agents;
using Inedo.BuildMaster.Web;
using NZCustomsServiceExtension.Variables;

namespace NZCustomsServiceExtension.Actions
{
    [ActionProperties(
    "Retrieve Artifact",
    "Retrieves an atifact from a repository.")]
    [Tag("Artifactory")]
    [CustomEditor(typeof(RetrieveArtifactActionEditor))]
    public class RetrieveArtifactAction : RemoteActionBase 
    {
        //[Persistent]
        //public int FileServerID { get; set; }

        [Persistent]
        public string FileName { get; set; }

        [Persistent]
        public string ItemName { get; set; }

        [Persistent]
        public string Properties { get; set; }

        public RetrieveArtifactAction()
        {
        }

        public override string ToString()
        {
            return string.Format("Retrieve the {0} artifact from the {1} repository to {2}", this.ItemName, this.FileName);
        }

        internal string Test()
        {
            return ProcessRemoteCommand(null, null);
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }

        protected string ResolveDirectory(string FilePath)
        {
            using (var sourceAgent2 = Util.Agents.CreateLocalAgent())
            {
                var sourceAgent = sourceAgent2.GetService<IFileOperationsExecuter>();

                char srcSeparator = sourceAgent.GetDirectorySeparator();
                var srcPath = sourceAgent.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);

                LogInformation("Source Path: " + srcPath);
                return srcPath;
            }
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            string fname = this.ResolveDirectory(this.FileName);
            string onlyFileName = Path.GetFileName(fname);
            StringBuilder url = new StringBuilder();

            ArtifactoryConfigurer config = this.GetExtensionConfigurer() as ArtifactoryConfigurer;

            string server = config.Server;

            url.Append(server.EndsWith("/") ? server : server + "/");
            //if (!string.IsNullOrEmpty(this.Properties))
            //{
            //    foreach (var item in this.Properties.ParseNameValue())
            //    {
            //        url.AppendFormat(";{0}={1}", item.Key, item.Value);
            //    }
            //}

            url.Append(this.ItemName);

            var uri = new Uri(url.ToString());
            var req = new WebClient();
            req.Credentials = new NetworkCredential(config.Username, config.Password);

            try
            {
                req.DownloadFile(uri, fname);
                return "OK";
            }
            catch (Exception ex)
            {
                LogError("Error retrieving the {0} artifact. Error: {1}", this.ItemName, ex.ToString());
            }
            return null;
        }        
    }
}
