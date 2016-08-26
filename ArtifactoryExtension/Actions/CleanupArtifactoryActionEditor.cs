// -----------------------------------------------------------------------
// <copyright file="SetVariableToArtifactoryPathActionEditor.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ArtifactoryExtension.Actions
{
    using System.Linq;
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Web.Controls;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Inedo.Web.Controls;
    using System.Web.UI;
    using System;
    using ArtifactoryExtension.Variables;
        
    /// <summary>
    /// Editor for <see cref="CleanupArtifactoryAction"/> 
    /// </summary>
    public sealed class CleanupArtifactoryActionEditor : ActionEditorBase
    {
        /// <summary>
        /// All artifactory variables in build scope
        /// </summary>
        public const string ALL_ARTIFACTORY_VARIABLES = "ALL";

        /// <summary>
        /// Name of Artifactory variable to gather properties from
        /// </summary>
        private DropDownList artifactoryVariable;

        /// <summary>
        /// To delete or not to delete, this answers that question.
        /// </summary>
        private CheckBox dryRun;

        /// <summary>
        /// When to delete.
        /// </summary>
        private ValidatingTextBox buildsToKeep;
        private ValidatingTextBox buildsToKeepFinal;

        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupArtifactoryActionEditor" /> class
        /// </summary>
        public CleanupArtifactoryActionEditor()
        {
        }

        /// <summary>
        /// Bind variable to form
        /// </summary>
        /// <param name="action">Base control</param>
        public override void BindToForm(ActionBase action)
        {
            this.EnsureChildControls();

            var cmd = (CleanupArtifactoryAction)action;
            this.artifactoryVariable.Text = cmd.ArtifactoryVariable;
            this.dryRun.Checked = cmd.DryRun;
            this.buildsToKeep.Text = cmd.BuildsToKeep.ToString();
            this.buildsToKeepFinal.Text = cmd.BuildsToKeepFinal.ToString();
        }

        /// <summary>
        /// Create action from form
        /// </summary>
        /// <returns>new ActionBase</returns>
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new CleanupArtifactoryAction
            {
                ArtifactoryVariable = this.artifactoryVariable.Text,
                DryRun = this.dryRun.Checked,
                BuildsToKeep = int.Parse(String.IsNullOrEmpty(this.buildsToKeep.Text) ? this.buildsToKeep.DefaultText : this.buildsToKeep.Text),
                BuildsToKeepFinal = int.Parse(String.IsNullOrEmpty(this.buildsToKeepFinal.Text) ? this.buildsToKeepFinal.DefaultText : this.buildsToKeepFinal.Text)
            };
        }

        /// <summary>
        /// Create controls
        /// </summary>
        protected override void CreateChildControls()
        {
            this.artifactoryVariable = new DropDownList { Width = 300 };
            this.dryRun = new CheckBox() { Checked = true };

            this.buildsToKeep = new ValidatingTextBox() { 
                Type = ValidationDataType.Integer,
                MaxLength = 2,
                DefaultText = "5",
                Width = 50 
            };

            this.buildsToKeepFinal = new ValidatingTextBox()
            {
                Type = ValidationDataType.Integer,
                MaxLength = 2,
                DefaultText = "15",
                Width = 50
            };

            this.Controls.Add(
                new SlimFormField("Artifactory Variable:", this.artifactoryVariable) { HelpText = "Choose a specific artifactory variable, or all artifactory variables in build scope." },
                new SlimFormField("Dry Run:", this.dryRun) { HelpText = "Checking this option means that this action will only log what it would delete, unchecking it will cause the action to actually delete artifacts from Artifactory." },
                new SlimFormField("Builds To Keep"),
                new SlimFormField("Per Envrionment:", this.buildsToKeep) { HelpText = "Number of builds to keep per envrionment.  Keep this at a level that will allow reverting to a previous builds if required, even if that build is no longer active." },
                new SlimFormField("Final Envrionment:", this.buildsToKeepFinal) { HelpText = "Could be read as how many production releases should we keep in the archive." }
            );

            this.artifactoryVariable.Items.Add(new ListItem("(All Artifactory Variables in Build Scope)", ALL_ARTIFACTORY_VARIABLES));
            this.artifactoryVariable.Items.AddRange(ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(this.ApplicationId));
        }
    }
}
