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
using System.Threading;

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
        public string ArtifactoryVariable { get; set; }

        [Persistent]
        public string ArtifactName { get; set; }

        [Persistent]
        public string DestinationFileName { get; set; }

        public RetrieveArtifactAction()
        {
        }

        public override string ToString()
        {
            return string.Format("Retrieve the {0} artifact {1}", this.ArtifactName, this.DestinationFileName);
        }

        protected ArtifactoryConfigurer GlobalConfig
        {
            get
            {
                if (this.IsConfigurerSettingRequired())
                {
                    String message = "The extension 'NZCustomsService' global configuration needs setting.";
                    this.LogError(message);
                    throw new Exception(message);
                }

                return this.GetExtensionConfigurer() as ArtifactoryConfigurer;
            }
        }

        protected override void Execute()
        {
            this.LogInformation("ProcessRemoteCommand");

            ArtifactoryConfigurer config = this.GlobalConfig;

            // Source
            StringBuilder url = new StringBuilder();

            this.LogInformation("config=" + config.Server);

            url.Append(config.Server.EndsWith("/") ? config.Server : config.Server + "/");
            url.Append(this.ArtifactName);
            
            
            // Destination
            string fname = this.ResolveDirectory(this.DestinationFileName);
            this.LogInformation("fname=" + fname);

            string onlyFileName = Path.GetFileName(fname);
            this.LogInformation("onlyFileName=" + onlyFileName);

            
            var req = new WebClient();
            req.Credentials = new NetworkCredential(config.Username, config.Password);

            //TODO: get temp path and filename

            this.LogInformation("OverriddenWorkingDirectory=" + this.OverriddenWorkingDirectory);

            try
            {
                req.DownloadFile(url.ToString(), "C:\\temp\\" + onlyFileName);
            }
            catch (Exception ex)
            {
                this.LogError("Error retrieving the {0} artifact in repository {1}. Error: {2}", fname, url.ToString(), ex.ToString());
                return;
            }
            

            var sshFileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            sshFileOps.FileCopyBatch("C:\\temp\\", new String[] {onlyFileName}, fname, new String[] {onlyFileName}, true, true);

        }

        protected string ResolveDirectory(string FilePath)
        {
            
            this.LogInformation("ResolveDirectory=" + FilePath);

            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var absWorkingDirectory = fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);
            this.LogInformation("absWorkingDirectory=" + absWorkingDirectory);

            return absWorkingDirectory;

            //using (var sourceAgent2 = Util.Agents.CreateLocalAgent())
            //{
            //    var sourceAgent = sourceAgent2.GetService<IFileOperationsExecuter>();

            //    char srcSeparator = sourceAgent.GetDirectorySeparator();
            //    var srcPath = sourceAgent.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, FilePath);

            //    LogInformation("Source Path: " + srcPath);
            //    return srcPath;
            //}
        }

      
    }

    
}