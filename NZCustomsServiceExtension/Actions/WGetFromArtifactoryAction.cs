// -----------------------------------------------------------------------
// <copyright file="WGetFromArtifactoryAction.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
//
// This is a tweaked version of Indeo's CommandLineAction.cs
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Actions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Extensibility.Agents;
    using Inedo.BuildMaster.Web;

    /// <summary>
    /// Represents an action that runs a command line with arguments on the target server
    /// </summary>
    [ActionProperties("WGet Artifact", "Runs a wget download via command line on the target server.")]
    [Tag("NZCustomsService")]
    [CustomEditor(typeof(WGetFromArtifactoryActionEditor))]
    public sealed class WGetFromArtifactoryAction : AgentBasedActionBase
    {
        #region Properties
        /// <summary>
        /// Gets or sets the path to the w-get application
        /// </summary>
        [Persistent]
        public string WGetPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the Artifactory repository folder or file that the artifact(s) can be found
        /// </summary>
        [Persistent]
        public string RepositoryPath { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of accepted extensions, for the -A parameter of w-get
        /// </summary>
        [Persistent]
        public string AcceptList { get; set; }

        /// <summary>
        /// Gets a value indicating whether the executed process's Standard Error should be logged
        /// with a <see cref="Inedo.BuildMaster.Diagnostics.MessageLevels"/> of <see cref="Inedo.BuildMaster.Diagnostics.MessageLevels.Error"/>
        /// or <see cref="Inedo.BuildMaster.Diagnostics.MessageLevels.Information"/>.
        /// </summary>
        protected override bool LogProcessStandardErrorAsError
        {
            get
            {
                return false;
            }
        }
     
        #endregion

        private ArtifactoryConfigurer GlobalConfig
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

        /// <summary>
        /// See <see cref="object.ToString()"/>
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format(
                "Execute {0} downloading {1}{2} to {3}",
                this.WGetPath,
                this.RepositoryPath,
                string.IsNullOrEmpty(this.AcceptList) ? string.Empty : " where files match " + this.AcceptList,
                string.IsNullOrEmpty(this.OverriddenWorkingDirectory) ? "the default directory" : this.OverriddenWorkingDirectory);
        }

        /// <summary>
        /// Perform w-get download
        /// </summary>
        protected override void Execute()
        {
            var arguments = this.GetWGetArguments();
            var fileOps = this.Context.Agent.GetService<IFileOperationsExecuter>();
            var absExePath = fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.WGetPath);
            var absWorkingDirectory = fileOps.GetWorkingDirectory(this.Context.ApplicationId, this.Context.DeployableId ?? 0, this.OverriddenWorkingDirectory);
            
            int exitCode = this.ExecuteCommandLine(absExePath, arguments, absWorkingDirectory);
                    
            // Shell scripts need to be made executable
            if (exitCode == 0)
            {
                // Only windows agents implement IRemoteCommandExecuter
                // TODO: this may stop working if ssh agents get support for IRemoteCommandExecuter
                bool isWindowsAgent = this.Context.Agent.TryGetService<IRemoteCommandExecuter>() != null;

                if (!isWindowsAgent)
                {
//                    int chmodRet = this.ExecuteCommandLine("chmod", "0755 *.sh", absWorkingDirectory);
                    int chmodRet = this.ExecuteCommandLine("/bin/bash", "-c \"if [[ \\\"`ls -1 *.sh 2>/dev/null`\\\" ]]; then chmod 0755 *.sh; else exit 0; fi\"", absWorkingDirectory);
                    if (chmodRet != 0)
                    {
                        this.LogWarning("chmod return code indicates error: {0} (0x{0:X8})", chmodRet);
                    }
                }
            }
            
            if (exitCode == 0)
            {
                this.LogInformation("Process Exit Code: {0} (0x{0:X8})", exitCode);
            }
            else
            {
                this.LogError("Process Exit Code indicates error: {0} (0x{0:X8})", exitCode);
            }
        }

        /// <summary>
        /// Get the arguments required for w-get
        /// </summary>
        /// <returns>List of arguments</returns>
        private string GetWGetArguments()
        {
            string arguments = "-m -e robots=off -nd -np";

            if (!string.IsNullOrEmpty(this.AcceptList))
            {
                arguments += " -A" + this.AcceptList;
            }


            string url = this.GlobalConfig.Server;            
            if (!url.EndsWith("/"))
            {
                url += "/";
            }

            // Remove beginning / if set so don't conflict with base url
            if (this.RepositoryPath.StartsWith("/"))
            {
                this.RepositoryPath = this.RepositoryPath.Substring(1);
            }

            // If repository path refers to a folder ensure it ends with / otherwise find 
            // will download lots of files that don't want
            if (!this.RepositoryPath.EndsWith("/") && !string.IsNullOrEmpty(this.AcceptList))
            {
                this.RepositoryPath += "/";
            }

            url += this.RepositoryPath;

            return arguments + " " + url;
        }
    }
}
