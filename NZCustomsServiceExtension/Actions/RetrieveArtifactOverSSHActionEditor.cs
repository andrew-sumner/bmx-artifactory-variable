using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;
using NZCustomsServiceExtension.Variables;

namespace NZCustomsServiceExtension.Actions
{
    internal sealed class RetrieveArtifactOverSSHActionEditor : ActionEditorBase
    {
        private DropDownList artifactoryVariable;        
        private ValidatingTextBox artifactName;
        private SourceControlFileFolderPicker destinationFileName;
        private CheckBox markAsExecutable;

        public RetrieveArtifactOverSSHActionEditor()
        {
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (RetrieveArtifactOverSSHAction)extension;

            this.artifactoryVariable.Text = action.ArtifactoryVariable;
            this.artifactName.Text = action.ArtifactName;
            this.destinationFileName.Text = action.DestinationFileName;
            this.markAsExecutable.Checked = action.MarkAsExecutable;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new RetrieveArtifactOverSSHAction
            {
                ArtifactoryVariable = this.artifactoryVariable.Text,
                ArtifactName = this.artifactName.Text,
                DestinationFileName = this.destinationFileName.Text,
                MarkAsExecutable = this.markAsExecutable.Checked
            };
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.artifactoryVariable = new DropDownList();
            this.artifactName = new ValidatingTextBox() { Width = 300 };
            this.destinationFileName = new SourceControlFileFolderPicker() { ServerId = this.ServerId, DefaultText = "Working Directory and Item Name" };
            this.markAsExecutable = new CheckBox() { Text = "Mark As Executable" };

            this.Controls.Add(
                new SlimFormField("Artifactory Variable:", this.artifactoryVariable) { HelpText = "Select the previously declared Artifactory Variable that specifies the location of the file you wish to download." },
                new SlimFormField("Artifact Name:", this.artifactName) { HelpText = "The name of the file you wish to download." },
                new SlimFormField("Output File:", this.destinationFileName) { HelpText = "The destination, defaults to the working directory plus artifact name." },
                new SlimFormField("Make Executable:", this.markAsExecutable) { HelpText = "Give the file executable permission (run chmod 0755). Unfortunately permissions are not kept with this method of copy files." }
            );
         
            this.artifactoryVariable.Items.AddRange(ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(this.ApplicationId));
        }
    }
}