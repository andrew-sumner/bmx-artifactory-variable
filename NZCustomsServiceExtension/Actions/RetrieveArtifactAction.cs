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
    "Retrieves an atifact from a repository (Supports SSH).")]
    [Tag("NZCustomsService")]
    [CustomEditor(typeof(RetrieveArtifactActionEditor))]
    public class RetrieveArtifactAction : AgentBasedActionBase 
    {
        [Persistent]
        public string ItemName { get; set; }

        [Persistent]
        public string FileName { get; set; }

        public RetrieveArtifactAction()
        {
        }

        public override string ToString()
        {
            return string.Format("Retrieve the {0} artifact {1}", this.ItemName, this.FileName);
        }

        protected override void Execute()
        {
            this.ProcessRemoteCommand();
            //this.ExecuteRemoteCommand(null);
        }

        protected string ResolveDirectory(string FilePath)
        {
            this.LogInformation("ResolveDirectory=" + FilePath);
            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var absWorkingDirectory = fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);
            this.LogInformation("absWorkingDirectory=" + absWorkingDirectory);

            return "/tmp/buildmaster/art.txt";
            
            //using (var sourceAgent2 = Util.Agents.CreateLocalAgent())
            //{
            //    var sourceAgent = sourceAgent2.GetService<IFileOperationsExecuter>();

            //    char srcSeparator = sourceAgent.GetDirectorySeparator();
            //    var srcPath = sourceAgent.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);

            //    LogInformation("Source Path: " + srcPath);
            //    return srcPath;
            //}
        }

        protected string ProcessRemoteCommand()
        {
            this.LogInformation("ProcessRemoteCommand");
            string fname = this.ResolveDirectory(this.FileName);

            this.LogInformation("fname=" + fname);
            string onlyFileName = Path.GetFileName(fname);
            this.LogInformation("onlyFileName=" + onlyFileName);
            StringBuilder url = new StringBuilder();

            ArtifactoryConfigurer config = this.GetExtensionConfigurer() as ArtifactoryConfigurer;
            this.LogInformation("config=" + config.Server);
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
                this.LogDebug("trydownlaod");
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
