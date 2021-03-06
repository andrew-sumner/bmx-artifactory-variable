﻿// -----------------------------------------------------------------------
// <copyright file="SetVariableToArtifactoryPathAction.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ArtifactoryExtension.Actions
{
    using System;
    using System.Linq;
    using System.Xml;
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Web;
    using ArtifactoryExtension.Variables;
    using Inedo.Serialization;
    using System.ComponentModel;
    using Inedo.Documentation;
    using Inedo.Data;

    /// <summary>
    /// Populates a variable with the value path to a build in artifactory chosen in selected artifactory variable
    /// </summary>
    [DisplayName("Set Variable to Artifiactory Path")]
    [Description("Changes the value on an existing variable or creates a runtime variable with the path to the build folder in artifactory as determined by the selected artifactory variable and its properties.")]
    [Tag("Artifactory")]
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
                string selectedVersion = this.Context.Variables[this.ArtifactoryVariable];

                var variable = ArtifactoryVersionVariable.GetVariableDeclaration(this.Context.ApplicationId, this.ArtifactoryVariable);
                string path = variable.GetRepositoryPath(selectedVersion);

                this.LogInformation(string.Format("Get Artifactory path for selected build '{0}' from {1}'s variable properties", selectedVersion, this.ArtifactoryVariable));
                this.LogInformation(string.Format("Set {0}={1}", this.VariableName, path));

                DB.Variables_CreateOrUpdateVariableDefinition(
                                Variable_Name: this.VariableName,
                                Environment_Id: this.Context.EnvironmentId,
                                ServerRole_Id: null,
                                Server_Id: null,
                                ApplicationGroup_Id: this.Context.ApplicationGroupId,
                                Application_Id: this.Context.ApplicationId,
                                Deployable_Id: null,
                                Release_Number: this.Context.ReleaseNumber,
                                Build_Number: this.Context.BuildNumber,
                                Promotion_Id: this.Context.PromotionId,
                                Execution_Id: this.Context.ExecutionId,
                                Value_Text: path,
                                Sensitive_Indicator: YNIndicator.No
                            );
                        //.Execute();
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }
    }
}