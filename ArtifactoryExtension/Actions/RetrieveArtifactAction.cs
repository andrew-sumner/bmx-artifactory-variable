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
using ArtifactoryExtension;
using ArtifactoryExtension.Variables;
using System.ComponentModel;
using Inedo.Documentation;
using Inedo.Serialization;
using Inedo.Agents;

namespace ArtifactoryExtension.Actions
{
    [DisplayName("Retrieve Artifact")]
    [Description("Retrieves an atifact from an Artifactory repository.")]
    [Tag("Artifactory")]
    [CustomEditor(typeof(RetrieveArtifactActionEditor))]
    public class RetrieveArtifactAction : RemoteActionBase 
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
            return string.Format("Retrieve {0} artifact from Artifactory", this.ArtifactName, this.DestinationFileName);
        }

        protected ArtifactoryConfigurer GlobalConfig
        {
            get
            {
                if (this.IsConfigurerSettingRequired())
                {
                    String message = "The extension 'Artifactory' global configuration needs setting.";
                    this.LogError(message);
                    throw new Exception(message);
                }

                return this.GetExtensionConfigurer() as ArtifactoryConfigurer;
            }
        }

        protected override void Execute()
        {
            this.ExecuteRemoteCommand(null);
        }

        protected override string ProcessRemoteCommand(string name, string[] args)
        {
            ArtifactoryVersionVariable variable = ArtifactoryVersionVariable.GetVariableDeclaration(this.Context.ApplicationId, this.ArtifactoryVariable);
            ArtifactoryConfigurer config = this.GlobalConfig;

            string selectedVersion = this.Context.Variables[this.ArtifactoryVariable];

            StringBuilder uri = new StringBuilder(config.Server);

            variable.AppendPath(uri, variable.GetRepositoryPath(selectedVersion));
            variable.AppendPath(uri, this.ArtifactName);

            var destFileOps = GetLocalFileOps();
            
            if (String.IsNullOrEmpty(this.DestinationFileName)) this.DestinationFileName = null;

            //TODO test this
            //string destFileName = destFileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.DestinationFileName ?? this.ArtifactName);
            string destFileName = destFileOps.GetBaseWorkingDirectory();

            if (!DownloadFile(config, uri.ToString(), destFileOps, destFileName)) return null;

            return "OK";
        }

        private IFileOperationsExecuter GetLocalFileOps()
        {
            var sourceAgent = Util.Agents.CreateLocalAgent();
            return sourceAgent.GetService<IFileOperationsExecuter>();
        }

        private bool DownloadFile(ArtifactoryConfigurer config, string url, IFileOperationsExecuter destFileOps, string destFileName)
        {
            this.LogInformation("Downloading {0} artifact to {1}", url, destFileName);

            var req = new WebClient();
            req.Credentials = new NetworkCredential(config.Username, config.Password);

            try
            {
                destFileOps.DeleteFile(destFileName);
                req.DownloadFile(url, destFileName);
            }
            catch (Exception ex)
            {
                this.LogError("Error retrieving the artifact from {0}. Error: {1}", url, ex.ToString());
                return false;
            }

            return true;
        }
    }
}
