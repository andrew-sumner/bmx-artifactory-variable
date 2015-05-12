// -----------------------------------------------------------------------
// <copyright file="WGetFromArtifactoryActionEditor.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
//
// This is a tweaked version of Indeo's CommandLineActionEditor.cs
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Actions
{
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Web.Controls;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Inedo.Web.Controls;
    
    /// <summary>
    /// Editor for WGetFromArtifactoryAction
    /// </summary>
    public sealed class WGetFromArtifactoryActionEditor : ActionEditorBase
    {
        /// <summary>
        /// Override server
        /// </summary>
        private ValidatingTextBox artifactoryServer;

        /// <summary>
        /// Artifactory repository
        /// </summary>
        private ValidatingTextBox repositoryPath;

        /// <summary>
        /// Working directory override
        /// </summary>
        private SourceControlFileFolderPicker workingDirectory;

        /// <summary>
        /// Path to w-get
        /// </summary>
        private SourceControlFileFolderPicker wgetPath;

        /// <summary>
        /// Accept list argument for w-get
        /// </summary>
        private ValidatingTextBox acceptList;

        /// <summary>
        /// Initializes a new instance of the <see cref="WGetFromArtifactoryActionEditor" /> class
        /// </summary>
        public WGetFromArtifactoryActionEditor()
        {
        }

        /// <summary>
        /// Bind variable to form
        /// </summary>
        /// <param name="action">Base control</param>
        public override void BindToForm(ActionBase action)
        {
            this.EnsureChildControls();

            var cmd = (WGetFromArtifactoryAction)action;
            this.artifactoryServer.Text = cmd.OverrideArtifactoryServer;
            this.repositoryPath.Text = cmd.RepositoryPath;            
            this.wgetPath.Text = cmd.WGetPath;
            this.acceptList.Text = cmd.AcceptList;
            this.workingDirectory.Text = cmd.OverriddenWorkingDirectory;            
        }

        /// <summary>
        /// Create action from form
        /// </summary>
        /// <returns>new WGetFromArtifactoryAction</returns>
        public override ActionBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new WGetFromArtifactoryAction
            {
                OverrideArtifactoryServer = this.artifactoryServer.Text,
                RepositoryPath = this.repositoryPath.Text,
                WGetPath = this.wgetPath.Text,
                AcceptList = this.acceptList.Text,
                OverriddenWorkingDirectory = this.workingDirectory.Text
            };
        }

        /// <summary>
        /// Create controls
        /// </summary>
        protected override void CreateChildControls()
        {
            this.wgetPath = new SourceControlFileFolderPicker { Required = true, ServerId = this.ServerId };
            this.workingDirectory = new SourceControlFileFolderPicker { ServerId = this.ServerId };
            this.artifactoryServer = new ValidatingTextBox() { Width = 300 };
            this.repositoryPath = new ValidatingTextBox() { Width = 300, Required = true };
            this.acceptList = new ValidatingTextBox() { Width = 300 };

            this.Controls.Add(
                new SlimFormField("WGet Path:", this.wgetPath) { HelpText = "The wget path and working directory are relative to the default directory." },
                new SlimFormField("Working Directory:", 
                        this.workingDirectory, 
                        new RenderJQueryDocReadyDelegator(w =>
                        {
                            w.WriteLine(
                                "$('#{0}').inedojq_defaulter();",
                                this.workingDirectory.ClientID);
                        }))
                        { HelpText = "The name of the artifactory variable - requires that you have defined a build scoped Artifactory variable." },
                new SlimFormField("Artifactory Server:", this.artifactoryServer) { HelpText = "If server not populated then the server defined for the Artifactory extension will be used." },
                new SlimFormField("Repository Path:", this.repositoryPath) { HelpText = "The repository path is the folder or file to download excluding artifactory server url." },
                new SlimFormField("Accept List:", this.acceptList) { HelpText = "The accept list can contain a comma-separated list of accepted file extensions to download." }
            );

            //this.Controls.Add(
            //     new FormFieldGroup(
            //        "WGet",
            //        "The wget path and working directory are relative to the default directory.",
            //        false,
            //        new StandardFormField("WGet Path:", this.wgetPath),
            //        new StandardFormField(
            //            "Working Directory:",
            //            this.workingDirectory,
            //            new RenderJQueryDocReadyDelegator(w =>
            //            {
            //                w.WriteLine(
            //                    "$('#{0}').inedojq_defaulter();",
            //                    this.workingDirectory.ClientID);
            //            }))),
            //    new FormFieldGroup(
            //        "Artifactory",
            //        "If server not populated then the server defined for the Artifactory extension will be used.<br><br>The repository path is the folder or file to download excluding artifactory server url.<br><br>The accept list can contain a comma-separated list of accepted file extensions to download.",
            //        false,
            //        new StandardFormField("Server:", this.artifactoryServer),
            //        new StandardFormField("Repository Path:", this.repositoryPath),
            //        new StandardFormField("Accept List:", this.acceptList)));
        } 
    }
}
