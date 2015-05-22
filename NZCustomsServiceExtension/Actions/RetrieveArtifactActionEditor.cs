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
        private SourceControlFileFolderPicker ffpFileName;
        //private ValidatingTextBox ffpFileName;

        public RetrieveArtifactActionEditor () 
        {
        }

        public override void BindToForm(ActionBase extension)
        {
            this.EnsureChildControls();
            
            var action = (RetrieveArtifactAction) extension;
            this.txtItemName.Text = action.ItemName;
            this.ffpFileName.Text = action.FileName;
        }

        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new RetrieveArtifactAction
            {
                ItemName = this.txtItemName.Text,
                FileName = this.ffpFileName.Text
            };
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            this.txtItemName = new ValidatingTextBox() { Width = 300 };
            this.ffpFileName = new SourceControlFileFolderPicker() { ServerId = this.ServerId };
            this.Controls.Add(new FormFieldGroup("Artifact","Artifact Information",true,
                new StandardFormField("Item Name:", txtItemName),
                new StandardFormField("Output File:",ffpFileName)
                )
            );
        }
    }
}
