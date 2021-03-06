// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariableEditor.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ArtifactoryExtension.Variables
{
    using System;
    using System.Text.RegularExpressions;
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster.Extensibility.Variables;
    using Inedo.BuildMaster.Web.Controls;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Inedo.Web.Controls;
    //using Inedo.Web.Controls.ButtonLinks;
    using ArtifactoryExtension.Artifactory;
    using ArtifactoryExtension.Artifactory.Domain;
    using System.Collections.Generic;
    using Inedo.BuildMaster;

    /// <summary>
    /// Variable editor.
    /// </summary>
    internal sealed class ArtifactoryVersionVariableEditor : VariableEditorBase
    {
        private DropDownList repositoryKey;

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

        public ArtifactoryConfigurer GlobalConfig 
        { 
            get
            {
                return new ArtifactoryVersionVariable().GlobalConfig;
            }
        }
        
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
            var v = (ArtifactoryVersionVariable)extension;
            
            this.EnsureChildControls();

            this.repositoryKey.Text = v.RepositoryKey;
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
                RepositoryKey = this.repositoryKey.Text,
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
            this.repositoryKey = new DropDownList();
            this.repositoryPath = new ValidatingTextBox() { Width = 250 };
            this.filter = new ValidatingTextBox { Width = 150 };
            this.trimFromPath = new ValidatingTextBox { Width = 250 };
            this.replaceSlashWithDot = new CheckBox { Width = 250, Text = "Replace '/' with '.'" };
            this.defaultToNotIncluded = new CheckBox { Width = 250, Text = "Default to '" + ArtifactoryVersionVariableSetter.NotIncluded + "'" };

            //new SlimFormField("Help", server) { "Ensure 'Scope' is set to Build and 'Default Value is Required' is checked." },
            this.Controls.Add(
                new SlimFormField("IMPORTANT") { InnerHtml = "<i>Ensure 'Scope' is set to Build and 'Default Value is Required' is checked.</i>" },
                new SlimFormField("Repository:", this.repositoryKey) { HelpText = "The name of the repository the artifact is stored in." },
                new SlimFormField("Repository Path:", this.repositoryPath) { HelpText = "The path to the folder that contains the list of builds.  Supports use of the %RELNO% and %RELNAME% variables (token for active release(s) rather than current release) eg if builds are in a seperate folder per release then you might use: group/name/%RELNO%" },
                new SlimFormField("Filter:", this.filter) { HelpText = "If builds for all releases placed in one folder, use this filter to only include active release related items (eg %RELNO%..*).  Note: This is a regular expression" },
                new SlimFormField("Trim From Path:", this.trimFromPath) { HelpText = "To strip the artifactory path details from the list items that this control displays, enter the text that you wish removed from the start of the item.  Often this will be identical to the Repository Path setting." },
                new SlimFormField("Version Format:", this.replaceSlashWithDot) { HelpText = "When checked will replace / with . in list item, eg 1.0/22 will become to 1.0.22.  Should only be set when there is a / in the list being displayed to the user otherwise there could be problems decoding the version number later." },
                new SlimFormField("Default Value: ", this.defaultToNotIncluded) { HelpText = "When unchceck will default to the latest build (last item in list), when checked will default to (Not included)." }
            );

            ArtifactoryApi artifactory = new ArtifactoryApi(this.GlobalConfig);

            this.repositoryKey.Items.Add(new ListItem(null));
            List<Repository> repositories = artifactory.GetLocalRepositories();
            foreach (var repository in repositories)
            {
                this.repositoryKey.Items.Add(new ListItem(repository.Key));
            }            
        }
    }
}
