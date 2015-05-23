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
            this.repositoryPath = new ValidatingTextBox() { Width = 250 };
            this.filter = new ValidatingTextBox { Width = 150 };
            this.trimFromPath = new ValidatingTextBox { Width = 250 };
            this.replaceSlashWithDot = new CheckBox { Width = 250, Text = "Replace '/' with '.'" };
            this.defaultToNotIncluded = new CheckBox { Width = 250, Text = "Default to (Not Included)" };

            //new SlimFormField("Help", server) { "Ensure 'Scope' is set to Build and 'Default Value is Required' is checked." },
            this.Controls.Add(
                new SlimFormField("IMPORTANT") { InnerHtml = "<i>Ensure 'Scope' is set to Build and 'Default Value is Required' is checked.</i>" },
                new SlimFormField("Repository Path:", this.repositoryPath) { HelpText = "The path to the folder that contains the list of builds.<br>Supports use of the %RELNO% and %RELNAME% variables <i>(token for active release(s) rather than current release)</i>." },
                new SlimFormField("Filter:", this.filter) { HelpText = "If builds for all releases placed in one folder, use this filter to only include active release related items (eg %RELNO%..*).<br><br><i>Note: This is a regular expression.</i>" },
                new SlimFormField("Trim From Path:", this.trimFromPath) { HelpText = "To strip the artifactory path details from the list items that this control displays, enter the text that you wish removed from the start of the item.  Often this will be identical to the Repository Path setting." },
                new SlimFormField("Version Format:", this.replaceSlashWithDot) { HelpText = "When checked will replace / with . in list item, eg 1.0/22 will become to 1.0.22 - only useful when builds are kept in seperate release folders rather than all in one folder" },
                new SlimFormField("Default Value: ", this.defaultToNotIncluded) { HelpText = "When unchceck will default to the latest build (last item in list), when checked not select a build." }
            );

            //string helpText =
            //        "Filter: only include release related items (eg %RELNO%..*)<br>" +
            //        "Trim: text removed from list item.<br>" +
            //        "Replace: check to replace / with . in list item.<br>" +
            //        "NOTE: Ensure Scope set to build and default value is required is checked.";

            //this.Controls.Add(
            //    new FormFieldGroup(
            //        "Server",
            //        "Server used for this action. If not populated then the server defined for the Artifactory extension will be used.",
            //        false,
            //        new StandardFormField("Server:", this.server))
            //        {
            //            Narrow = true
            //        },
            //    new FormFieldGroup(
            //        "Repository Path",
            //        "The path to the folder that contains the list of builds.<br>Supports use of the %RELNO% and %RELNAME% variables <i>(token for active release(s) rather than current release)</i>.",
            //        false,
            //        new StandardFormField("Repository Path:", this.repositoryPath))
            //        {
            //            Narrow = true
            //        },
            //    new FormFieldGroup(
            //        "Options",
            //        helpText,
            //        false,
            //        new StandardFormField("Filter (regular expression):", this.filter),
            //        new StandardFormField("Trim From Path:", this.trimFromPath),
            //        new StandardFormField(string.Empty, this.replaceSlashWithDot),
            //        new StandardFormField(string.Empty, this.defaultToNotIncluded))
            //        {
            //            Narrow = true
            //        });
        }
    }
}
