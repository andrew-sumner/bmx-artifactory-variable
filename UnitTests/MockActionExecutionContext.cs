// -----------------------------------------------------------------------
// <copyright file="MockActionExecutionContext.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace UnitTests
{
    using System;
    using Inedo.BuildMaster.Extensibility.Actions;

    /// <summary>
    /// Mock object for creating an IActionExecutionContext object for passing through to 
    /// BuildMaster functions that require it.
    /// </summary>
    public class MockActionExecutionContext : IActionExecutionContext
    {
        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public int? ApplicationGroupId { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public int ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public int? DeployableId { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public int EnvironmentId { get; private set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public int ExecutionId { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public int ExecutionPlanActionId { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public long NumericReleaseNumber { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public string ReleaseNumber { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public string BuildNumber { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public System.Collections.Generic.IDictionary<string, string> Variables { get; set; }

        /// <summary>
        /// Gets or sets value for interface
        /// </summary>
        public IActionCancellationToken CancellationToken { get; set; }

        public int PromotionId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
