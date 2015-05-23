using System;

using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web.Controls;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace NZCustomsServiceExtension
{
    internal sealed class ArtifactoryConfigurerEditor : ExtensionConfigurerEditorBase
    {
        ValidatingTextBox txtUserName;
        ValidatingTextBox txtPassword;
        ValidatingTextBox txtServer;

        public ArtifactoryConfigurerEditor()
        {
            txtUserName = new ValidatingTextBox() { Width = 300 };
            txtPassword = new ValidatingTextBox() { Width = 300, TextMode = System.Web.UI.WebControls.TextBoxMode.Password };
            txtServer = new ValidatingTextBox() { Width = 300 };
        }

        public override void InitializeDefaultValues()
        {
            BindToForm(new ArtifactoryConfigurer());
        }

        /// <summary>
        /// Binds to form.
        /// </summary>
        /// <param name="extension">The extension.</param>
        public override void BindToForm(ExtensionConfigurerBase extension)
        {
            var configurer = (ArtifactoryConfigurer)extension;
            if (!string.IsNullOrEmpty(configurer.Username))
            {
                this.txtUserName.Text = configurer.Username;
                this.txtPassword.Text = configurer.Password;
            }
            this.txtServer.Text = configurer.Server;
        }

        /// <summary>
        /// Creates from form.
        /// </summary>
        /// <returns></returns>
        public override ExtensionConfigurerBase CreateFromForm()
        {
            var configurer = new ArtifactoryConfigurer()
            {
                Server = this.txtServer.Text                
            };
            if (!string.IsNullOrEmpty(this.txtUserName.Text))
            {
                configurer.Username = this.txtUserName.Text;
                configurer.Password = this.txtPassword.Text;
            }
            return configurer;
        }

        protected override void CreateChildControls()
        {
            this.Controls.Add(
               new SlimFormField("Server:", this.txtServer) { HelpText = "The URL of the Artifactory server." },
               new SlimFormField("Username:", this.txtUserName) { HelpText = "Authentication Information used for all Artifactory actions." },
               new SlimFormField("Password:", this.txtPassword) { HelpText = "Authentication Information used for all Artifactory actions." }
            );
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);
        //}
  
       
        //protected override void OnInit(EventArgs e)
        //{
            
        //    //CUtil.Add(this,
        //    //    new FormFieldGroup("Server", "The URL of the Artifactory server.", false,
        //    //        new StandardFormField("Server:",txtServer )
        //    //    ),
        //    //    new FormFieldGroup("Authentication","Authentication Information used for all Artifactory actions unless overridden in the action.",
        //    //        true,
        //    //        new StandardFormField("Username:",txtUserName),
        //    //        new StandardFormField("Password:",txtPassword)
        //    //    )
        //    //);

        //    base.OnInit(e);
        //}

    }
}
