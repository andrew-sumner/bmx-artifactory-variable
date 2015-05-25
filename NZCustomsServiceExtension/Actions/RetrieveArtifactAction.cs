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

            string variableValue = this.Context.Variables[this.ArtifactoryVariable];

            StringBuilder uri = new StringBuilder(config.Server);

            variable.AppendPath(uri, variable.ExpandRepositoryPathWithValue(this.Context.ReleaseNumber, GetReleaseName(), variableValue));
            variable.AppendPath(uri, this.ArtifactName);

            var srcFileOps = GetLocalFileOps();
            var destFileOps = GetRemoteFileOps();

            if (String.IsNullOrEmpty(this.DestinationFileName)) this.DestinationFileName = null;

            string onlyFileName = Path.GetFileName(this.DestinationFileName ?? this.ArtifactName);
                        
            string srcFileName = srcFileOps.CombinePath(srcFileOps.GetBaseWorkingDirectory(), onlyFileName);
            string destFileName = destFileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.DestinationFileName ?? this.ArtifactName);

            if (!DownloadFile(config, uri.ToString(), srcFileOps, srcFileName)) return;
            TransferFile(srcFileOps, srcFileName, destFileOps, destFileName);

            if (this.MarkAsExecutable) 
            {
                string fileName = Path.GetFileName(destFileName);
                string filePath = Path.GetFullPath(destFileName);

                int chmodRet = this.ExecuteCommandLine("/bin/bash", "-c \"if [[ \\\"`ls -1 *.sh 2>/dev/null`\\\" ]]; then chmod 0755 " + fileName + "; else exit 0; fi\"", filePath);
                if (chmodRet != 0)
                {
                    this.LogWarning("chmod return code indicates error: {0} (0x{0:X8})", chmodRet);
                }
            }
        }

        private string GetReleaseName()
        {
            var execution = StoredProcs
                     .Builds_GetExecution(this.Context.ExecutionId)
                     .Execute().FirstOrDefault();

            return execution.Release_Name;
        }

        private void TransferFile(IFileOperationsExecuter srcFileOps, string srcFileName, IFileOperationsExecuter destFileOps, string destFileName)
        {
            this.LogInformation("Transfer {0} to {1} over SSH", srcFileName, destFileName);
            
            Stream srcStream = null;
            Stream destStream = null;

            try
            {
                srcStream = srcFileOps.OpenFile(srcFileName, FileMode.Open, FileAccess.Read);
                destStream = destFileOps.OpenFile(destFileName, FileMode.Create, FileAccess.Write);

                //const int ONE_MB = 1048576;
                const int TEN_KB = 10240;

                byte[] buffer = new byte[TEN_KB];
                int bytesRead;

                while ((bytesRead = srcStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destStream.Write(buffer, 0, bytesRead);
                }
            }
            finally
            {
                if (srcStream != null) srcStream.Close();
                if (destStream != null) destStream.Close();
            }

            srcFileOps.DeleteFile(srcFileName);
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