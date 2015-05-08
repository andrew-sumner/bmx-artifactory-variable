// -----------------------------------------------------------------------
// <copyright file="VariableHasValuePredicate.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
/*
namespace NZCustomsServiceExtension.Predicates
{
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Extensibility.Predicates;
    using Inedo.BuildMaster.Web;
    using NZCustomsServiceExtension.Variables;

    /// <summary>
    /// BuildMaster predicate to check that a variable has a value
    /// </summary>
    [PredicateProperties(
        "Variable Has Value Predicate (NZCustomsService)",
        "A predicate that tests whether a variable contains a value.")]
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
            if (context.Variables.ContainsKey(this.VariableName))
            {
                // TODO: How get default value for variable?
                if (context.Variables[this.VariableName] == ArtifactoryVersionVariableSetter.NotIncluded)
                {
                    return false;
                }

                return !string.IsNullOrEmpty(context.Variables[this.VariableName]);
            }

            return false;
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
*/