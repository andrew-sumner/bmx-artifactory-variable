// -----------------------------------------------------------------------
// <copyright file="VariableHasValuePredicate.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace ArtifactoryExtension.Predicates
{
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Extensibility.Predicates;
    using Inedo.BuildMaster.Web;
    using System.Linq;
    using ArtifactoryExtension.Variables;
    using System.ComponentModel;
    using Inedo.Documentation;
    using Inedo.Serialization;

    /// <summary>
    /// BuildMaster predicate to check that a variable has a value
    /// </summary>
    [DisplayName("Artifactory Variable Has Value")]
    [Description("A predicate that tests whether a artifactory variable has a proper value rather than " + ArtifactoryVersionVariableSetter.NotIncluded + ".")]
    [Tag("Artifactory")]
    [CustomEditor(typeof(VariableHasValuePredicateEditor))]
    public sealed class VariableHasValuePredicate : PredicateBase
    {
        /// <summary>
        /// Gets or sets the value returned when the predicate is evaluated
        /// </summary>
        [Persistent]
        public string VariableName { get; set; }

        /// <summary>
        /// Check variable value
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <returns>True if variable contains a value</returns>
        public override bool Evaluate(IActionExecutionContext context)
        {
            var variable = ArtifactoryVersionVariable.GetVariableValue(context.ExecutionId, this.VariableName);
            
            if (variable == null) return false;
            if (string.IsNullOrEmpty(variable.Value_Text)) return false;
            if (variable.Value_Text == ArtifactoryVersionVariableSetter.NotIncluded) return false;

            return true;
        }

        /// <summary>
        /// Description of this object
        /// </summary>
        /// <returns>A string</returns>
        public override string ToString()
        {
            return "Check that \"" + this.VariableName + "\" has a value";
        }
    }
}
