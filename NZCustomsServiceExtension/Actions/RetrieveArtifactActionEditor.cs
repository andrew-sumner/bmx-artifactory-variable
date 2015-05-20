using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using Inedo.BuildMaster.Extensibility.Actions;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace NZCustomsServiceExtension.Actions
{
    internal sealed class RetrieveArtifactActionEditor : ActionEditorBase 
    {
        private ValidatingTextBox txtItemName;
        private ValidatingTextBox txtProperties;
        private SourceControlFileFolderPicker ffpFileName;

        public RetrieveArtifactActionEditor () 
        {
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            
            var action = (RetrieveArtifactAction) extension;
            this.txtItemName.Text = action.ItemName;
            this.txtProperties.Text = action.Properties;
            this.ffpFileName.Text = action.FileName;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new RetrieveArtifactAction
            {
                ItemName = this.txtItemName.Text,
                Properties = this.txtProperties.Text,
                FileName = this.ffpFileName.Text
            };
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.txtItemName = new ValidatingTextBox() { Width = 300 };
            this.txtProperties = new ValidatingTextBox() { Width = 300, TextMode = TextBoxMode.MultiLine };
            this.ffpFileName = new SourceControlFileFolderPicker() { ServerId = 1 };
            this.Controls.Add(new FormFieldGroup("Artifact","Artifact Information",true,
                new StandardFormField("Item Name:", txtItemName),
                new StandardFormField("Properties:",txtProperties),
                new StandardFormField("Output File:",ffpFileName)
                )
            );
        }
    }
}
