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
    
    /// <summary>
    /// Editor for WGetFromArtifactoryAction
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
        /// Initializes a new instance of the <see cref="SetVariableToArtifactoryPathActionEditor" /> class
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
        }

        /// <summary>
        /// Create action from form
        /// </summary>
        /// <returns>new WGetFromArtifactoryAction</returns>
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new CleanupArtifactoryAction
            {
                ArtifactoryVariable = this.artifactoryVariable.Text,
                DryRun = this.dryRun.Checked
            };
        }

        /// <summary>
        /// Create controls
        /// </summary>
        protected override void CreateChildControls()
        {
            this.artifactoryVariable = new DropDownList { Width = 300 };
            
            this.Controls.Add(
                new FormFieldGroup(
                    "Artifactory Variable",
                    "Choose specific artifactory variable, or all artifactory variables in build scope.",
                    false,
                    new StandardFormField("Name:", this.artifactoryVariable),
                    new StandardFormField("Dry Run:", this.dryRun)
            ));

            // TODO: Should this be in a Setter?
            this.artifactoryVariable.Items.Add(new ListItem("[All Artifactory Variables in Build Scope]", ALL_ARTIFACTORY_VARIABLES));
            this.artifactoryVariable.Items.AddRange(GetVariables(this.ApplicationId));
        }

        public static ListItem[] GetVariables(int ApplicationId)
        {
            return StoredProcs
                    .Variables_GetVariableDeclarations("B", ApplicationId, null)
                    .Execute()
                    .Where(s => s.Variable_Configuration.Contains("NZCustomsServiceExtension.Variables.ArtifactoryVersionVariable"))
                    .Select(s => new ListItem(s.Variable_Name))
                    .ToArray();
        }
    }
}
