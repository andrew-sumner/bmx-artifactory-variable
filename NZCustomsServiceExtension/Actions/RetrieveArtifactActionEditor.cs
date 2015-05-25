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
    internal sealed class RetrieveArtifactActionEditor : ActionEditorBase
    {
        private DropDownList artifactoryVariable;        
        private ValidatingTextBox artifactName;
        private SourceControlFileFolderPicker destinationFileName;
        //private ValidatingTextBox ffpFileName;

        public RetrieveArtifactActionEditor()
        {
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();

            var action = (RetrieveArtifactAction)extension;

            this.artifactoryVariable.Text = action.ArtifactoryVariable;
            this.artifactName.Text = action.ArtifactName;
            this.destinationFileName.Text = action.DestinationFileName;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new RetrieveArtifactAction
            {
                ArtifactoryVariable = this.artifactoryVariable.Text,
                ArtifactName = this.artifactName.Text,
                DestinationFileName = this.destinationFileName.Text 
            };
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.artifactoryVariable = new DropDownList();
            this.artifactName = new ValidatingTextBox() { Width = 300 };
            this.destinationFileName = new SourceControlFileFolderPicker() { ServerId = this.ServerId, DefaultText = "Current Directory and Item Name" };

            this.Controls.Add(
                new SlimFormField("Artifactory Variable:", this.artifactoryVariable) { HelpText = "Choose a specific artifactory variable, or all artifactory variables in build scope." },
                new SlimFormField("Item Name:", this.artifactName) { HelpText = "Checking this option means that this action will only log what it would delete, unchecking it will cause the action to actually delete artifacts from Artifactory." },
                new SlimFormField("Output File:", this.destinationFileName)
                
            );
         
            //this.artifactoryVariable.Items.Add(new ListItem("(All Artifactory Variables in Build Scope)", ALL_ARTIFACTORY_VARIABLES));
            this.artifactoryVariable.Items.AddRange(ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(this.ApplicationId));
        }
    }
}