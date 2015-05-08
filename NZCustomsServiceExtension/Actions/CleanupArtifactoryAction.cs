// -----------------------------------------------------------------------
// <copyright file="SetVariableToArtifactoryPathAction.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Actions
{
    using System;
    using System.Linq;
    using System.Xml;
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Actions;
    using Inedo.BuildMaster.Web;
    using System.Web.UI.WebControls;
    using System.Collections;
    using System.Collections.Generic;
    using NZCustomsServiceExtension.Artifactory;
    using NZCustomsServiceExtension.Variables;
    using NZCustomsServiceExtension.Artifactory.Domain;

    /// <summary>
    /// Populates a variable with the value path to a build in artifactory chosen in selected artifactory variable
    /// </summary>
    [ActionProperties(
        "Cleanup artifacts in Artifiactory", "Deletes artifacts in artifactory for the selected artifactory variable.")]
    [Tag("NZCustomsService")]
    [CustomEditor(typeof(CleanupArtifactoryActionEditor))] 
    public class CleanupArtifactoryAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetVariableToArtifactoryPathAction"/> class.
        /// </summary>
        public CleanupArtifactoryAction()
        {   
        }

        /// <summary>
        /// Gets or sets string to find in value
        /// </summary>
        [Persistent]
        public string ArtifactoryVariable { get; set; }

        /// <summary>
        /// Set true to only report on what artifacts would be deleted from Artifactory, false to delete artifacts.
        /// </summary>
        [Persistent]
        public bool DryRun { get; set; }

        /// <summary>
        /// Returns a string displayed in the BuildMaster deployment plan
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return string.Format("Cleanup artifacts from Artifactory for the variable {0}", this.ArtifactoryVariable);
        }

        /// <summary>
        /// This method is called to execute the Action.
        /// </summary>
        protected override void Execute()
        {
            try
            {
                if (ArtifactoryVariable.Equals(CleanupArtifactoryActionEditor.ALL_ARTIFACTORY_VARIABLES))
                {
                    this.LogInformation("Cleaning up artifacts from Artifactory for all Artifactory Build Variables that have been created for this application and have a scope of Build.");
                    ListItem[] variables = CleanupArtifactoryActionEditor.GetVariables(this.Context.ApplicationId);
                    foreach (var item in variables)
                    {
                        CleanupArtifacts(item.Value);
                        
                    }
                }
                else
                {
                    CleanupArtifacts(ArtifactoryVariable);
                }
            }
            catch (Exception ex)
            {
                this.LogError(ex.Message);
            }
        }

        private void CleanupArtifacts(string ArtifactoryVariable)
        {
            // TODO remove this line
            DryRun = true;

            this.LogInformation("Cleanup artifacts from Artifactory for Artifactory Build Variable " + ArtifactoryVariable);
                    
            List<BuildInfo> builds = new List<BuildInfo>();
            ArtifactoryVersionVariable variable = GetArtifactoryVersionVariableDeclaration(ArtifactoryVariable);

            // 1. Get List of all envrionment for this Application
            //    NOTE: To get these in correct order order we have to query the workflows instead of calling getEnvironments()
            var workflows = StoredProcs.Workflows_GetWorkflows(Application_Id: this.Context.ApplicationId).Execute();

            foreach (var workflowItem in workflows)
            {
                this.LogInformation(String.Format("Application: {0} - {1}", workflowItem.Single_Application_Id, workflowItem.Single_Application_Name));

                var workflow = StoredProcs.Workflows_GetWorkflow(Workflow_Id: workflowItem.Workflow_Id).Execute();

                // 2. For each environment get a list of the last 'X' builds and add to a list of of versions to keep.
                //    NOTE: A build may get reapplied to the same envrionment several times to repair a failed deployment. Treat these as one.
                foreach (var step in workflow.WorkflowSteps_Extended)
                {
                    int keep = (step.Environment_Name.Equals(workflow.WorkflowSteps_Extended.Last().Environment_Name) ? 10 : 5);
		            int unique = 0;

                    var executions = StoredProcs.Builds_GetExecutions(Application_Id: this.Context.ApplicationId, Release_Number: null, 
                                Build_Number: null, Environment_Id: step.Environment_Id, Execution_Count: keep).Execute();

                    foreach (var execution in executions)
	                {
                        var variableValue = StoredProcs.Variables_GetVariableValues(
                                                Environment_Id: execution.Environment_Id,
                                                Server_Id: null,
                                                ApplicationGroup_Id: null,
                                                Application_Id: this.Context.ApplicationId,
                                                Deployable_Id: null,
                                                Release_Number: execution.Release_Number,
                                                Build_Number: execution.Build_Number,
                                                Execution_Id: execution.Execution_Id)
                                            .Execute()
                                            .Where(v => v.Variable_Name.Equals(ArtifactoryVariable))
                                            .FirstOrDefault();
                        
                        if (variableValue != null) {
				            this.LogInformation(String.Format("\tWorkflow: {0} - {1}", workflowItem.Workflow_Id), workflowItem.Workflow_Name);
				            this.LogInformation(String.Format("\tEnvironment: {0} - {1}", step.Environment_Id, step.Environment_Name));
				            this.LogInformation(String.Format("\tExecution: Version = {0}.{1}, Date = {2}", execution.Release_Number, execution.Build_Number, execution.ExecutionStart_Date));
                            this.LogInformation(String.Format("\tVariable: {0} = '{1}'", variableValue.Variable_Name, variableValue.Value_Text));
				
				            if (executions.FirstOrDefault(it => it.Release_Number == execution.Release_Number && it.Build_Number == execution.Build_Number && it.Execution_Id > execution.Execution_Id) != null) {
					            // same build has been deployed to this enviornment more than once, skip this one
					            this.LogInformation("\tResult: Repeated build, skip");
				            } else {
					            unique ++;
					            if (unique <= keep) {
						            if (builds.FirstOrDefault(it => it.releaseNumber == execution.Release_Number && it.buildNumber == execution.Build_Number) == null) {
                                        var buildInfo = new BuildInfo();
									
							            buildInfo.variableValue = variableValue.Value_Text;
							            buildInfo.releaseNumber = execution.Release_Number;
                                        buildInfo.releaseName = execution.Release_Name;
							            buildInfo.buildNumber = execution.Build_Number;
							            buildInfo.buildStatus = execution.BuildStatus_Name;
							            buildInfo.executionStatus = execution.ExecutionStatus_Name;
							
							            builds.Add(buildInfo);
							
							            this.LogInformation("\tResult: Keep");
						            } else {
							            this.LogInformation("\tResult: Already stored, ignore");
						            }
					            } else {
						            this.LogInformation("\tResult: Discard");
					            }
				            }				
			            } else {
				            this.LogWarning(String.Format("Variable '{0}' was not found", ArtifactoryVariable));
			            }
	                }
                }
            }
                        
            // 3. Get a list of folders from Artifactory for our application and for each folder returned:
            //      check its in our keep list, if not delete it - but do not delete the newest (last) folder 
            //      from artifactory as it may not have had a chance to build yet!
            int numberRemoved = 0;

            if (variable.RepositoryPathRequiresExpanding()) 
            {
                // Release builds are stored in a different folder per release
                var releases = builds.Select(b => new {b.releaseNumber, b.releaseName}).Distinct().OrderBy(b => b.releaseNumber);

                foreach (var release in releases)
	            {
                    numberRemoved += DeleteFromArtifactory(builds, variable, release.releaseNumber, release.releaseName);
                }
            } else {
                // All builds, regardless of release are stored in the same folder
                numberRemoved += DeleteFromArtifactory(builds, variable, String.Empty, String.Empty);
            }

            this.LogInformation("");
            this.LogInformation(String.Format("{0} folders {1} removed from Artifactory", numberRemoved, (this.DryRun ? "would have been" : "were")));
        }

        private int DeleteFromArtifactory(List<BuildInfo> builds, ArtifactoryVersionVariable variable, string releaseNumber, string releaseName)
        {
            int numberRemoved = 0;

            ArtifactoryApi artifactory = new ArtifactoryApi(variable.GetBaseURL());
            FolderInfo folderInfo = artifactory.GetFolderInfo(variable.ExpandRepositoryPath(releaseNumber, releaseName));

            foreach (var child in folderInfo.Children)
	        {
		        if(child.Folder) {
		            var artifactoryVersion = child.Uri.StartsWith("/") ? child.Uri.Substring(1) : child.Uri;
		
		            this.LogInformation(String.Format("Artifactory Version: {0}", artifactoryVersion));
		
		            if (child == folderInfo.Children.Last()) {
			            this.LogInformation(String.Format("\tKeep Newest Version: {0} in Artifactory", artifactoryVersion));
		            } else {
			            int index = artifactoryVersion.LastIndexOf('.');
			            string artifactoryReleaseNumber = artifactoryVersion.Substring(0, index);
			            string artifactoryBuildNumber = artifactoryVersion.Substring(index + 1);
			
			            if (builds.Find(it => it.variableValue == artifactoryVersion) == null) {
				            this.LogInformation(String.Format("\tDelete Version: {0} from Artifactory", artifactoryVersion));
                            if(!this.DryRun) 
                            {
				                artifactory.DeleteItem(folderInfo.Uri + child.Uri);
                            }
				            numberRemoved ++;			
			            } else {
				            this.LogInformation(String.Format("\tKeep Version: {0} in Artifactory", artifactoryVersion));
			            }
		            }
	            }
            }
            
            return numberRemoved;
        }

        /// <summary>
        /// Create and populate ArtifactoryVersionVariable from settings in database for this application
        /// </summary>
        /// <param name="version">Version selected for the variable</param>
        /// <returns>Artifactory path</returns>
        private ArtifactoryVersionVariable GetArtifactoryVersionVariableDeclaration(string artifactoryVariableName)
        {   
            // Get variable properties
            var settings = StoredProcs
                     .Variables_GetVariableDeclarations("B", this.Context.ApplicationId, null)
                     .Execute()
                     .Where(s => s.Variable_Name == artifactoryVariableName)
                     .FirstOrDefault()
                     .Variable_Configuration;

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(settings);

            return new ArtifactoryVersionVariable
            {
                ActionServer = xml.SelectSingleNode("//Properties/@ActionServer").Value,
                RepositoryPath = xml.SelectSingleNode("//Properties/@RepositoryPath").Value,
                Filter = xml.SelectSingleNode("//Properties/@Filter").Value,
                TrimFromPath = xml.SelectSingleNode("//Properties/@TrimFromPath").Value,
                ReplaceSlashWithDot = bool.Parse(xml.SelectSingleNode("//Properties/@ReplaceSlashWithDot").Value),
                DefaultToNotIncluded = bool.Parse(xml.SelectSingleNode("//Properties/@DefaultToNotIncluded").Value)
            };
        }
    }

    protected class BuildInfo {
	    public String variableValue { get; set; }
        public String releaseNumber { get; set; }
        public String releaseName { get; set; }
        public String buildNumber { get; set; }
        public String buildStatus { get; set; }
        public String executionStatus { get; set; }
    }
}