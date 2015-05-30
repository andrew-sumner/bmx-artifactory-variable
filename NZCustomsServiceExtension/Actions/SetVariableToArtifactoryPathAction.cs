// -----------------------------------------------------------------------
// <copyright file="SetVariableToArtifactoryPathAction.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Actions
{
    using System;
    using System.Linq;
    using System.Xml;
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Web;
    using NZCustomsServiceExtension.Variables;

    /// <summary>
    /// Populates a variable with the value path to a build in artifactory chosen in selected artifactory variable
    /// </summary>
    [ActionProperties(
        "Set Variable to Artifiactory Path", "Changes the value on an existing variable or creates a runtime variable with the path to the build folder in artifactory as determined by the selected artifactory variable and its properties.")]
    [Tag("NZCustomsService")]
    [CustomEditor(typeof(SetVariableToArtifactoryPathActionEditor))] 
    public class SetVariableToArtifactoryPathAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetVariableToArtifactoryPathAction"/> class.
        /// </summary>
        public SetVariableToArtifactoryPathAction()
        {   
        }

        /// <summary>
        /// Gets or sets name of the variable
        /// </summary>
        [Persistent]
        public string VariableName { get; set; }

        /// <summary>
        /// Gets or sets string to find in value
        /// </summary>
        [Persistent]
        public string ArtifactoryVariable { get; set; }

        /// <summary>
        /// Returns a string displayed in the BuildMaster deployment plan
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Set variable {0} with artifactory path for the selected build from the {1} variable", this.VariableName, this.ArtifactoryVariable);
        }

        /// <summary>
        /// This method is called to execute the Action.
        /// </summary>
        protected override void Execute()
        {
            try
            {
                string version = this.Context.Variables[this.ArtifactoryVariable];
                
                var exec = StoredProcs
                        .Builds_GetExecution(this.Context.ExecutionId)
                        .Execute()
                        .First();
     
                if (!version.Contains(exec.Release_Number.ToString()) && !version.Contains(exec.Release_Name))
                {
                    this.LogError(string.Format("The version '{0}' selected for artifactory variable '{1}' is not valid for this the current release '{2} {3}' ", version, this.ArtifactoryVariable, exec.Release_Number, exec.Release_Name));
                    return;
                }

                string path = this.GetArtifactoryVariablePath(version, exec.Release_Number, exec.Release_Name);

                this.LogInformation(string.Format("Get Artifactory path for selected build '{0}' from {1}'s variable properties", version, this.ArtifactoryVariable));
                this.LogInformation(string.Format("Set {0}={1}", this.VariableName, path));

                StoredProcs.Variables_CreateOrUpdateVariableDefinition(
                                this.VariableName, 
                                exec.Environment_Id, 
                                null, 
                                null, 
                                exec.Application_Id,
                                null, 
                                exec.Release_Number, 
                                exec.Build_Number, 
                                exec.Execution_Id, 
                                path,
                                "N")
                            .Execute();
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }
                
        /// <summary>
        /// Gets the artifactory path from the properties and value of the selected artifactory variable
        /// </summary>
        /// <param name="version">Version selected for the variable</param>
        /// <returns>Artifactory path</returns>
        private string GetArtifactoryVariablePath(string version, string releaseNumber, string releaseName)
        {
            // Get variable properties
            var settings = StoredProcs
                     .Variables_GetVariableDeclarations("B", this.Context.ApplicationId, null)
                     .Execute()
                     .Where(s => s.Variable_Name == this.ArtifactoryVariable)
                     .FirstOrDefault()
                     .Variable_Configuration;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(settings);

            string repositoryPath; 
            string trimFromPath = xml.SelectSingleNode("//Properties/@TrimFromPath").Value;
            bool replaceSlashWithDot = bool.Parse(xml.SelectSingleNode("//Properties/@ReplaceSlashWithDot").Value);

            // Reconstruct path to build folder from properties and version selected
            repositoryPath = trimFromPath + version;

            if (replaceSlashWithDot)
            {
                repositoryPath = repositoryPath.Replace(releaseNumber + ".", releaseNumber + "/");
                repositoryPath = repositoryPath.Replace(releaseName + ".", releaseName + "/");
            }

            if (repositoryPath.EndsWith("/"))
            {
                repositoryPath = repositoryPath.Remove(repositoryPath.Length - 1);
            }

            return repositoryPath;
        }
    }
}