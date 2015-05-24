// -----------------------------------------------------------------------
// <copyright file="VariableHasValuePredicateEditor.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace NZCustomsServiceExtension.Predicates
{
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Extensibility.Predicates;
    using Inedo.BuildMaster.Extensibility.Variables;
    using Inedo.BuildMaster.Web.Controls;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using Inedo.Web.Controls;
    using NZCustomsServiceExtension.Variables;

    /// <summary>
    /// Predicate editor.
    /// </summary>
    internal sealed class VariableHasValuePredicateEditor : PredicateEditorBase
    {
        /// <summary>
        /// Variable name text box
        /// </summary>
        private DropDownList variableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="VariableHasValuePredicateEditor"/> class.
        /// </summary>
        public VariableHasValuePredicateEditor()
        {
        }

        /// <summary>
        /// Apply current variable settings to form
        /// </summary>
        /// <param name="extension">The variable</param>
        public override void BindToForm(PredicateBase extension)
        {
            this.EnsureChildControls();

            var v = (VariableHasValuePredicate)extension;
            this.variableName.Text = v.VariableName;
        }

        /// <summary>
        /// Create variable from form
        /// </summary>
        /// <returns>New variable</returns>
        public override PredicateBase CreateFromForm()
        {
            this.EnsureChildControls();

            return new VariableHasValuePredicate
            {
                VariableName = this.variableName.Text
            };
        }

        /// <summary>
        /// Create form
        /// </summary>
        protected override void CreateChildControls()
        {
            this.variableName = new DropDownList();// { Width = 300 };
            
            this.Controls.Add(
                new SlimFormField("Variable:", this.variableName) { HelpText = "The name of the artifactory variable - requires that you have defined a build scoped Artifactory variable." }
            );

            this.variableName.Items.AddRange(ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(ArtifactoryVersionVariableSetter.GetApplicationIdFromUrl()));
        }
    }
}
