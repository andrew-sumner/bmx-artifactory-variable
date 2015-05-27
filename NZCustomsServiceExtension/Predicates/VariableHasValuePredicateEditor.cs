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
    using System.Web;
    using System.Linq;

    /// <summary>
    /// Predicate editor.
    /// </summary>
    internal sealed class VariableHasValuePredicateEditor : PredicateEditorBase
    {
        /// <summary>
        /// Variable name text box
        /// </summary>
        private DropDownList variableNameDd = null;
        private ValidatingTextBox variableNameTxt = null;

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

            if (this.variableNameDd != null) this.variableNameDd.Text = v.VariableName;
            if (this.variableNameTxt != null) this.variableNameTxt.Text = v.VariableName;
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
                VariableName = (this.variableNameDd == null ? this.variableNameTxt.Text : this.variableNameDd.Text)
            };
        }

        /// <summary>
        /// Create form
        /// </summary>
        protected override void CreateChildControls()
        {
            int id;
            int? applicationId = null;

            //Predicates are not directly tied to applications anymore as of v4.3 because of global plans. However, if the plan is in a single application you can get application id as follows:
            if (int.TryParse(HttpContext.Current.Request.QueryString["planActionGroupId"], out id))
            {
                applicationId = StoredProcs.Plans_GetActionGroup(id)
                                  .Execute()
                                  .ActionGroupUsage_Slim
                                  .FirstOrDefault()
                                  .Application_Id;
            }
 
            if (applicationId != null)
            {
                this.variableNameDd = new DropDownList();

                this.Controls.Add(
                    new SlimFormField("Variable:", this.variableNameDd) { HelpText = "The name of the artifactory variable - requires that you have defined a build scoped Artifactory variable." }
                );


                this.variableNameDd.Items.AddRange(ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(applicationId.Value));
            }
            else
            {
                this.variableNameTxt = new ValidatingTextBox();

                this.Controls.Add(
                    new SlimFormField("Variable:", this.variableNameTxt) { HelpText = "The name of the artifactory variable - requires that you have defined a build scoped Artifactory variable." }
                );
            }
        }
    }
}
