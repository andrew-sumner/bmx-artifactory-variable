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
using Inedo.BuildMaster.Data;

namespace NZCustomsServiceExtension.Actions
{
    [ActionProperties(
    "Retrieve Artifact Over SSH",
    "Retrieves an atifact from an Artifactory repository. Actually downloads to BuildMaster Server and then transfers file.")]
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

        [Persistent]
        public bool MarkAsExecutable { get; set; }

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
                    String message = "The extension 'NZCustomsService' global configuration needs setting.";
                    this.LogError(message);
                    throw new Exception(message);
                }

                return this.GetExtensionConfigurer() as ArtifactoryConfigurer;
            }
        }

        protected override void Execute()
        {
            ArtifactoryVersionVariable variable = ArtifactoryVersionVariable.GetVariableDeclaration(this.Context.ApplicationId, this.ArtifactoryVariable);
            ArtifactoryConfigurer config = this.GlobalConfig;

            string selectedVersion = this.Context.Variables[this.ArtifactoryVariable];

            StringBuilder uri = new StringBuilder(config.Server);

            variable.AppendPath(uri, variable.GetRepositoryPath(selectedVersion));
            variable.AppendPath(uri, this.ArtifactName);

            var srcFileOps = GetLocalFileOps();
            var destFileOps = GetRemoteFileOps();

            if (String.IsNullOrEmpty(this.DestinationFileName)) this.DestinationFileName = null;

            string onlyFileName = Path.GetFileName(this.DestinationFileName ?? this.ArtifactName);
                        
            string srcFileName = srcFileOps.CombinePath(srcFileOps.GetBaseWorkingDirectory(), onlyFileName);
            string destFileName = destFileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.DestinationFileName ?? this.ArtifactName);

            if (!DownloadFile(config, uri.ToString(), srcFileOps, srcFileName)) return;
            TransferFile(srcFileOps, srcFileName, destFileOps, destFileName);
            MakeExecutable(destFileName);
        }

        private bool DownloadFile(ArtifactoryConfigurer config, string url, IFileOperationsExecuter srcFileOps, string srcFileName)
        {
            this.LogInformation("Downloading {0} artifact to {1}", url, srcFileName);

            var req = new WebClient();
            req.Credentials = new NetworkCredential(config.Username, config.Password);

            try
            {
                srcFileOps.DeleteFile(srcFileName);
                req.DownloadFile(url, srcFileName);
            }
            catch (Exception ex)
            {
                this.LogError("Error retrieving the artifact from {0}. Error: {1}", url, ex.ToString());
                return false;
            }

            return true;
        }
        
        private void TransferFile(IFileOperationsExecuter srcFileOps, string srcFileName, IFileOperationsExecuter destFileOps, string destFileName)
        {
            this.LogInformation("Transfer {0} to {1} over SSH", srcFileName, destFileName);
            
            FileStream srcStream = null;
            Stream destStream = null;

            try
            {
                srcStream = File.Open(srcFileName, FileMode.Open, FileAccess.Read);
                destStream = destFileOps.OpenFile(destFileName, FileMode.Create, FileAccess.Write);

                srcStream.CopyTo(destStream);
            }
            finally
            {
                if (srcStream != null) srcStream.Close();
                if (destStream != null) destStream.Close();
            }

            srcFileOps.DeleteFile(srcFileName);
        }

        private void MakeExecutable(string destFileName)
        {
            if (!this.MarkAsExecutable) return;

            this.LogInformation("Make {0} executable", destFileName);

            string fileName = Path.GetFileName(destFileName);
            string filePath = destFileName.Remove(destFileName.Length - fileName.Length);

            int chmodRet = this.ExecuteCommandLine("chmod", "0755 " + fileName, filePath);
            if (chmodRet != 0)
            {
                this.LogWarning("chmod return code indicates error: {0}", chmodRet);
            }
        }

        protected IFileOperationsExecuter GetRemoteFileOps()
        {
            return this.Context.Agent.GetService<IFileOperationsExecuter>();
        }

        protected IFileOperationsExecuter GetLocalFileOps()
        {
            var sourceAgent = Util.Agents.CreateLocalAgent();
            return sourceAgent.GetService<IFileOperationsExecuter>();
        }
    }

    
}