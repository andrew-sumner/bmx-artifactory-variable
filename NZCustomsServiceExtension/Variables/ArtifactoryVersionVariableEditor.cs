// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariableEditor.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Variables
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster.Extensibility.Variables;
    using Inedo.BuildMaster.Web.Controls;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Inedo.Web.Controls;
    using Inedo.Web.Controls.ButtonLinks;

    /// <summary>
    /// Variable editor.
    /// </summary>
    internal sealed class ArtifactoryVersionVariableEditor : VariableEditorBase
    {
        /// <summary>
        /// Override server
        /// </summary>
        private ValidatingTextBox server;

        /// <summary>
        /// Artifactory repository
        /// </summary>
        private ValidatingTextBox repositoryPath;

        /// <summary>
        /// Regular expression pattern to filter to limit list to builds for a release 
        /// </summary>
        private ValidatingTextBox filter;

        /// <summary>
        /// String to remove from list item 
        /// </summary>
        private ValidatingTextBox trimFromPath;
                
        /// <summary>
        /// Flag to replace / with . or not
        /// </summary>
        private CheckBox replaceSlashWithDot;

        /// <summary>
        /// Flag for default to (Not Included)
        /// </summary>
        private CheckBox defaultToNotIncluded;

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactoryVersionVariableEditor"/> class.
        /// </summary>
        public ArtifactoryVersionVariableEditor()
        {
        }

        /// <summary>
        /// Apply current variable settings to form
        /// </summary>
        /// <param name="extension">The variable</param>
        public override void BindToForm(VariableBase extension)
        {
            this.EnsureChildControls();

            var v = (ArtifactoryVersionVariable)extension;

            /*
            this.userName.Text = v.ActionUsername;
            this.password.Text = v.ActionPassword;
            */
            this.server.Text = v.ActionServer;
            this.repositoryPath.Text = v.RepositoryPath;
            this.filter.Text = v.Filter;
            this.trimFromPath.Text = v.TrimFromPath;
            this.replaceSlashWithDot.Checked = v.ReplaceSlashWithDot;
            this.defaultToNotIncluded.Checked = v.DefaultToNotIncluded;
        }

        /// <summary>
        /// Create variable from form
        /// </summary>
        /// <returns>New variable</returns>
        public override VariableBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new ArtifactoryVersionVariable
            {
                ActionServer = this.server.Text,
                RepositoryPath = this.repositoryPath.Text,
                Filter = this.filter.Text,
                TrimFromPath = this.trimFromPath.Text,
                ReplaceSlashWithDot = this.replaceSlashWithDot.Checked,
                DefaultToNotIncluded = this.defaultToNotIncluded.Checked
            };
        }

        /// <summary>
        /// Define form
        /// </summary>
        protected override void CreateChildControls()
        {
            this.server = new ValidatingTextBox() { Width = 250 };
            this.repositoryPath = new ValidatingTextBox() { Width = 250 };
            this.filter = new ValidatingTextBox { Width = 150 };
            this.trimFromPath = new ValidatingTextBox { Width = 250 };
            this.replaceSlashWithDot = new CheckBox { Width = 250, Text = "Replace '/' with '.'" };
            this.defaultToNotIncluded = new CheckBox { Width = 250, Text = "Default to (Not Included)" };

            string helpText = 
                    "Filter: only include release related items (eg %RELNO%..*)<br>" +
                    "Trim: text removed from list item.<br>" +
                    "Replace: check to replace / with . in list item.<br>" +
                    "NOTE: Ensure Scope set to build and default value is required is checked.";
            
            this.Controls.Add(
                new FormFieldGroup(
                    "Server",
                    "Server used for this action. If not populated then the server defined for the Artifactory extension will be used.",
                    false,
                    new StandardFormField("Server:", this.server))
                    {
                        Narrow = true
                    },
                new FormFieldGroup(
                    "Repository Path",
                    "The path to the folder that contains the list of builds.<br>Supports use of the %RELNO% and %RELNAME% variables <i>(token for active release(s) rather than current release)</i>.",
                    false,
                    new StandardFormField("Repository Path:", this.repositoryPath))
                    {
                        Narrow = true
                    },
                new FormFieldGroup(
                    "Options",
                    helpText,
                    false,
                    new StandardFormField("Filter (regular expression):", this.filter),
                    new StandardFormField("Trim From Path:", this.trimFromPath),
                    new StandardFormField(string.Empty, this.replaceSlashWithDot),
                    new StandardFormField(string.Empty, this.defaultToNotIncluded))
                    {
                        Narrow = true
                    });
        }
    }
}
