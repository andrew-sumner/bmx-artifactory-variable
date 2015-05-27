// -----------------------------------------------------------------------
// <copyright file="VariableHasValuePredicate.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------
namespace NZCustomsServiceExtension.Predicates
{
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Extensibility.Predicates;
    using Inedo.BuildMaster.Web;
    using System.Linq;
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
            var iter = Util.Variables.EnumerateVariables().GetEnumerator();
            while (iter.MoveNext()) 
            {
                string name = iter.Current.Name;
            }

            var variable = StoredProcs.Variables_GetVariableValues(
                Environment_Id: context.EnvironmentId, Server_Id: null, 
                ApplicationGroup_Id: context.ApplicationGroupId, Application_Id: context.ApplicationId, 
                Deployable_Id: context.DeployableId,
                Release_Number: context.ReleaseNumber, Build_Number: context.BuildNumber,
                Execution_Id: context.ExecutionId).Execute();

            var iter2 = variable.GetEnumerator();
            while (iter2.MoveNext())
            {
                string name = iter2.Current.Variable_Name;
            }

            //var variable = Util.Variables.EnumerateVariables().FirstOrDefault(s => s.Name == this.VariableName);
            
            //if (variable == null) return false;
            //if (string.IsNullOrEmpty(variable.Value)) return false;
            //if (variable.Value == ArtifactoryVersionVariableSetter.NotIncluded) return false;

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
