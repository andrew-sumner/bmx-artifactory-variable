// -----------------------------------------------------------------------
// <copyright file="SetVariableToArtifactoryPathActionEditor.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Actions
{
    using System.Linq;
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Web.Controls;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Inedo.Web.Controls;
    using NZCustomsServiceExtension.Variables;
    
    /// <summary>
    /// Editor for <see cref="SetVariableToArtifactoryPathAction"/> 
    /// </summary>
    public sealed class SetVariableToArtifactoryPathActionEditor : ActionEditorBase
    {
        /// <summary>
        /// Name of variable to set
        /// </summary>
        private ValidatingTextBox variableName;

        /// <summary>
        /// Name of Artifactory variable to gather properties from
        /// </summary>
        private DropDownList artifactoryVariable;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetVariableToArtifactoryPathActionEditor" /> class
        /// </summary>
        public SetVariableToArtifactoryPathActionEditor()
        {
        }

        /// <summary>
        /// Bind variable to form
        /// </summary>
        /// <param name="action">Base control</param>
        public override void BindToForm(ActionBase action)
        {
            this.EnsureChildControls();

            var cmd = (SetVariableToArtifactoryPathAction)action;
            this.variableName.Text = cmd.VariableName;
            this.artifactoryVariable.Text = cmd.ArtifactoryVariable;
        }

        /// <summary>
        /// Create action from form
        /// </summary>
        /// <returns>new WGetFromArtifactoryAction</returns>
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new SetVariableToArtifactoryPathAction
            {
                VariableName = this.variableName.Text,
                ArtifactoryVariable = this.artifactoryVariable.Text
            };
        }

        /// <summary>
        /// Create controls
        /// </summary>
        protected override void CreateChildControls()
        {
            this.variableName = new ValidatingTextBox() { Width = 300, Required = true };
            this.artifactoryVariable = new DropDownList { Width = 300 };

            this.Controls.Add(
                new SlimFormField("Name:", this.variableName) { HelpText = "The name of the variable." },
                new SlimFormField("Variable:", this.artifactoryVariable) { HelpText = "The name of the artifactory variable - requires that you have defined a build scoped Artifactory variable." }
            );

            //this.Controls.Add(
            //     new FormFieldGroup(
            //        "Variable Name",
            //        "The name of the variable.",
            //        false,
            //        new StandardFormField("Name:", this.variableName)),
            //    new FormFieldGroup(
            //        "Artifactory Variable",
            //        "The name of the artifactory variable.",
            //        false,
            //        new StandardFormField("Name:", this.artifactoryVariable)));

            this.artifactoryVariable.Items.Add(new ListItem(string.Empty));

            this.artifactoryVariable.Items.AddRange(ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(this.ApplicationId));
        } 
    }
}
