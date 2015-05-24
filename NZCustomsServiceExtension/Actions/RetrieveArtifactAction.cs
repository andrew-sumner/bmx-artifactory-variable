﻿using System;
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

            
            ArtifactoryVersionVariable variable = ArtifactoryVersionVariable.GetVariableDeclaration(this.Context.ApplicationId, this.ArtifactoryVariable);
            ArtifactoryConfigurer config = this.GlobalConfig;


            Uri uri = new Uri(config.Server);
            
            Uri relativeUri1 = new Uri(uri, this.ArtifactName);
            Uri relativeUri = new Uri("fred", UriKind.Relative);

            // Create a new Uri from an absolute Uri and a relative Uri.
            Uri combinedUri = new Uri(uri, relativeUri);


            // Source
            StringBuilder url = new StringBuilder();
            
            this.LogInformation("config=" + config.Server);

            url.Append(config.Server.EndsWith("/") ? config.Server : config.Server + "/");
            url.Append(this.ArtifactName);



            var srcFileOps = GetLocalFileOps();
            var destFileOps = GetRemoteFileOps();
            
            string onlyFileName = Path.GetFileName(this.DestinationFileName);
            string folder = Path.GetDirectoryName(this.DestinationFileName);

            string srcFileName = srcFileOps.CombinePath(srcFileOps.GetBaseWorkingDirectory(), onlyFileName);
            string destFileName = destFileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.DestinationFileName); ;
            
                        
            if (!DownloadFile(config, url.ToString(), srcFileName)) return;
            TransferFile(srcFileOps, srcFileName, destFileOps, destFileName);            
        }

        private void TransferFile(IFileOperationsExecuter srcFileOps, string srcFileName, IFileOperationsExecuter destFileOps, string destFileName)
        {
            this.LogInformation("Transfer {0} to {1} over SSH", srcFileName, destFileName);
            var sshFileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();

            Stream srcStream = null;
            Stream destStream = null;

            try
            {
                srcStream = srcFileOps.OpenFile(srcFileName, FileMode.Open, FileAccess.Read);
                destStream = destFileOps.OpenFile(destFileName, FileMode.Create, FileAccess.Write);

                byte[] buffer = new byte[2048];
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

        }

        private bool DownloadFile(ArtifactoryConfigurer config, string url, string srcFileName)
        {
            this.LogInformation("Downloading {0} artifact to {1}", url, srcFileName);

            var req = new WebClient();
            req.Credentials = new NetworkCredential(config.Username, config.Password);

            try
            {
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