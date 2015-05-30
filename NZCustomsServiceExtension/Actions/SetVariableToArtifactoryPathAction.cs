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

                var variable = ArtifactoryVersionVariable.GetVariableDeclaration(this.Context.ApplicationId, this.ArtifactoryVariable);
                string path = variable.ExpandRepositoryPathWithVersion(version);

                this.LogInformation(string.Format("Get Artifactory path for selected build '{0}' from {1}'s variable properties", version, this.ArtifactoryVariable));
                this.LogInformation(string.Format("Set {0}={1}", this.VariableName, path));

                StoredProcs.Variables_CreateOrUpdateVariableDefinition(
                                Variable_Name: this.VariableName, 
                                Environment_Id: this.Context.EnvironmentId, 
                                Server_Id: null, 
                                ApplicationGroup_Id: this.Context.ApplicationGroupId, 
                                Application_Id: this.Context.ApplicationId,
                                Deployable_Id: this.Context.DeployableId,
                                Release_Number: this.Context.ReleaseNumber,
                                Build_Number: this.Context.BuildNumber,
                                Execution_Id: this.Context.ExecutionId,
                                Value_Text: path,
                                Sensitive_Indicator: "N"
                            ).Execute();
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }
    }
}