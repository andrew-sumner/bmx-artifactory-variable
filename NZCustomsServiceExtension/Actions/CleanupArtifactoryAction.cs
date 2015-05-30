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
    using System.ComponentModel;
    using System.Web.UI.WebControls;
    using System.Collections;
    using System.Collections.Generic;
    using NZCustomsServiceExtension.Artifactory;
    using NZCustomsServiceExtension.Variables;
    using NZCustomsServiceExtension.Artifactory.Domain;
    using System.Reflection;

    /// <summary>
    /// Populates a variable with the value path to a build in artifactory chosen in selected artifactory variable
    /// </summary>
    [ActionProperties(
        "Cleanup Artifacts in Artifiactory", "Deletes artifacts in artifactory for the selected artifactory variable.")]
    [Tag("NZCustomsService")]
    [CustomEditor(typeof(CleanupArtifactoryActionEditor))] 
    public class CleanupArtifactoryAction : ActionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CleanupArtifactoryAction"/> class.
        /// </summary>
        public CleanupArtifactoryAction()
        {   
        }

        protected ArtifactoryConfigurer GlobalConfig
        {
            get
            {
                if (this.IsConfigurerSettingRequired())
                {
                    String message = "The extension 'NZCustomsService' global configuration needs setting.";
                    this.LogError(message);
                    throw new Exception(message);
                }

                return this.GetExtensionConfigurer() as ArtifactoryConfigurer;
            }
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
        //[DefaultValue(true)]
        public bool DryRun { get; set; }

        /// <summary>
        /// The number of rejected builds to keep per environment.
        /// </summary>
        [Persistent]
        //[DefaultValue(5)]
        public int BuildsToKeep { get; set; }

        /// <summary>
        /// The number of rejected builds to keep in the final environment (ie those deployed to production).
        /// </summary>
        [Persistent]
        //[DefaultValue(15)]
        public int BuildsToKeepFinal { get; set; }

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
            if (this.BuildsToKeep == 0)
            {
                this.LogInformation("BuildsToKeep has been set to zero, defaulting to 5");
                this.BuildsToKeep = 5;
            }

            if (this.BuildsToKeepFinal == 0)
            {
                this.LogInformation("BuildsToKeepFinal has been set to zero, defaulting to 15");
                this.BuildsToKeepFinal = 15;
            }

            if (ArtifactoryVariable.Equals(CleanupArtifactoryActionEditor.ALL_ARTIFACTORY_VARIABLES))
            {
                this.LogInformation("Cleaning up artifacts from Artifactory for all Artifactory Build Variables that have been created for this application and have a scope of Build.");
                ListItem[] variables = ArtifactoryVersionVariable.GetArtifactoryVariablesInBuildScope(this.Context.ApplicationId);
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

        private void CleanupArtifacts(string artifactoryVariable)
        {
            this.LogInformation("Gather variable usage from BuildMaster for Artifactory Build Variable {0}", artifactoryVariable);

            List<BuildInfo> builds = GetBuildMasterBuildsToKeep(artifactoryVariable);
            DeleteArtifactsFromArtifactory(artifactoryVariable, builds);
        }

        private List<BuildInfo> GetBuildMasterBuildsToKeep(string artifactoryVariable)
        {
            List<BuildInfo> builds = new List<BuildInfo>();
            //TODO I can help thinking that it might be simpler just to get the active builds and releases and keep x number of rejected, I'm just concerned
            // that won't keep the right builds that way.  If using BuildMasters built in clean up rejected builds then possibly could just keep everything 
            // still recorded in buildmaster?

            // 1. Get List of all envrionment for this Application
            //    NOTE: To get these in correct order order we have to query the workflows instead of calling getEnvironments()
            var workflows = StoredProcs.Workflows_GetWorkflows(Application_Id: this.Context.ApplicationId).Execute();
            bool checkRunningExecutions = true;

            foreach (var workflowItem in workflows)
            {
                this.LogDebug("Application: {0} - {1}", workflowItem.Single_Application_Id, workflowItem.Single_Application_Name);
                this.LogDebug("Workflow: {0} - {1}", workflowItem.Workflow_Id, workflowItem.Workflow_Name);

                var workflow = StoredProcs.Workflows_GetWorkflow(Workflow_Id: workflowItem.Workflow_Id).Execute();

                // 2. For each environment get a list of the last 'X' builds and add to a list of of versions to keep.
                //    NOTE: A build may get reapplied to the same envrionment several times to repair a failed deployment. Treat these as one.
                foreach (var step in workflow.WorkflowSteps_Extended)
                {
                    int keep = (step.Environment_Name == workflow.WorkflowSteps_Extended.Last().Environment_Name ? this.BuildsToKeepFinal : this.BuildsToKeep);

                    this.LogDebug("Keep: {0} builds in environment '{1}'", keep, step.Environment_Name);

                    var executions = StoredProcs.Builds_GetExecutions(Application_Id: this.Context.ApplicationId, Release_Number: null,
                                Build_Number: null, Environment_Id: step.Environment_Id, Execution_Count: keep * 3).Execute();


                    if (checkRunningExecutions)
                    {
                        // Get any variables declared in the execution of the current build
                        var runningExecutions = StoredProcs.Builds_GetExecutionsInProgress(this.Context.ApplicationId).Execute();
                        executions = runningExecutions.Concat(executions);
                        checkRunningExecutions = false;
                    }

                    foreach (var execution in executions)
                    {
                        //TODO: ArtifactoryVersionVariable.GetVariableValue ? 
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
                                            .Where(v => v.Variable_Name.Equals(artifactoryVariable))
                                            .FirstOrDefault();

                        if (variableValue != null && !String.IsNullOrEmpty(variableValue.Value_Text))
                        {
                            ArtifactVersion version = ArtifactoryVersionVariable.ExtractReleaseAndBuildNumbers(variableValue.Value_Text);
                            string result = String.Empty;

                            if (executions.FirstOrDefault(it => it.Release_Number == execution.Release_Number && it.Build_Number == execution.Build_Number && it.Execution_Id > execution.Execution_Id) != null)
                            {
                                // same build has been deployed to this enviornment more than once, skip this one
                                result = "Repeated execution, skip";
                            }
                            else
                            {
                                if (builds.Count() <= keep)
                                {
                                    if (builds.FirstOrDefault(bld => bld.variableReleaseNameOrNumber == version.ReleaseNameOrNumber && bld.variableBuildNumber == version.BuildNumber) == null)
                                    {
                                        var buildInfo = new BuildInfo();

                                        buildInfo.variableValue = variableValue.Value_Text;
                                        buildInfo.variableReleaseNameOrNumber = version.ReleaseNameOrNumber;
                                        buildInfo.variableBuildNumber = version.BuildNumber;

                                        builds.Add(buildInfo);

                                        result = "Keep";
                                    }
                                    else
                                    {
                                        result = "Already keeping";
                                    }
                                }
                                else
                                {
                                    result = "Discard, we've kept all we want";
                                }
                            }

                            this.LogDebug("\tVariable {0}='{1}': {2}. Artifactory Release {3} Build {4}, BuildMaster Release {5}, Build {6}",
                                    variableValue.Variable_Name, variableValue.Value_Text, result,
                                    version.ReleaseNameOrNumber, version.BuildNumber, execution.Release_Number, execution.Build_Number);
                        }
                        else
                        {
                            this.LogDebug("\tVariable '{0}' was not found for Release {1} Build {2}", artifactoryVariable, execution.Release_Number, execution.Build_Number);
                        }

                        // Found all the builds we need for this environment, move on to the next
                        if (builds.Count() >= keep)
                        {
                            break;                            
                        }
                    }
                }
            }

            return builds;
        }

        private void DeleteArtifactsFromArtifactory(string ArtifactoryVariable, List<BuildInfo> builds)
        {
            ArtifactoryVersionVariable variable = ArtifactoryVersionVariable.GetVariableDeclaration(this.Context.ApplicationId, ArtifactoryVariable);

            // 3. Get a list of folders from Artifactory for our application and for each folder returned:
            //      check its in our keep list, if not delete it - but do not delete the newest (last) folder 
            //      from artifactory as it may not have had a chance to build yet!
            this.LogInformation("{0} artifacts from Artifactory for Artifactory Build Variable {1}", this.DryRun ? "Perform a dry run of the cleanup of" : "Cleanup", ArtifactoryVariable);

            int numberRemoved = 0;

            if (variable.RepositoryPathRequiresExpanding())
            {
                // TODO: It may be better to get the base folder and look at all child folders rather than rely on kept builds to 
                //       give us the full list of folders
                // Release builds are stored in a different folder per release
                var releases = builds.Select(b => b.variableReleaseNameOrNumber).Distinct().OrderBy(b => b);

                foreach (var release in releases)
                {
                    numberRemoved += DeleteFromArtifactory(builds, variable, release);
                }
            }
            else
            {
                // All builds, regardless of release are stored in the same folder
                numberRemoved += DeleteFromArtifactory(builds, variable, String.Empty);
            }

            this.LogInformation("");
            this.LogInformation("{0} folders {1} removed from Artifactory", numberRemoved, (this.DryRun ? "would have been" : "were"));
        }

        private int DeleteFromArtifactory(List<BuildInfo> builds, ArtifactoryVersionVariable variable, string releaseNameOrNumber)
        {
            int numberRemoved = 0;

            ArtifactoryApi artifactory = new ArtifactoryApi(this.GlobalConfig);
            FolderInfo folderInfo = artifactory.GetFolderInfo(variable.ExpandRepositoryPath(releaseNameOrNumber, releaseNameOrNumber));

            foreach (var child in folderInfo.Children)
            {
                if (child.Folder)
                {
                    ArtifactVersion artifactoryVersion = ExtractArtifactoryVersion(folderInfo.Path, child.Uri);

                    string result = String.Empty;
                    bool delete = false;

                    // TODO: Ensure this is the last, not convinced artifactory is ordering builds or what will happen for multiple active releases
                    // Could sort by number, date or just keep any builds less than a day old
                    // TODO: Haven't tested with builds that seperated into different releases folders, rather than placed in big bucket
                    if (builds.Find(bld => bld.variableReleaseNameOrNumber == artifactoryVersion.ReleaseNameOrNumber && 
                                            bld.variableBuildNumber == artifactoryVersion.BuildNumber) != null)
                    {
                        result = "Keep as is used by BuildMaster";
                    }
                    else if (child == folderInfo.Children.Last())
                    {
                        result = "Keep as is newest build in Artifactory and may be required";
                    }
                    else
                    {
                        result = "Delete as BuildMaster has no use for it";
                        delete = true;
                    }
                    
                    this.LogDebug("Artifactory Release: {0} Build {1} {2} - Path: {3} Child {4}", artifactoryVersion.ReleaseNameOrNumber, artifactoryVersion.BuildNumber, result, folderInfo.Path, child.Uri);
                    if (delete)
                    {
                        if (!this.DryRun)
                        {
                            artifactory.DeleteItem(folderInfo.Uri + child.Uri);
                        }
                        numberRemoved++;
                    }
                }
            }

            return numberRemoved;
        }

        private ArtifactVersion ExtractArtifactoryVersion(string parentFolder, string childFolder)
        {
            parentFolder = RemoveSlashes(parentFolder);
            childFolder = RemoveSlashes(childFolder);

            if (childFolder.Contains(".") || childFolder.Contains("/"))
            {
                // Assume childFolder contains both release and build details
                return ArtifactoryVersionVariable.ExtractReleaseAndBuildNumbers(childFolder);
            }

            // Assume childFolder contains build details only and parentFolder has release details
            ArtifactVersion value = new ArtifactVersion();

            int index = parentFolder.LastIndexOf("/");

            if (index > 0)
            {
                value.ReleaseNameOrNumber = parentFolder.Substring(index + 1);
            }
            else
            {
                value.ReleaseNameOrNumber = parentFolder;
            }

            value.BuildNumber = childFolder;
        
            return value;
        }

        private string RemoveSlashes(string value)
        {
            if (value.StartsWith("/")) value = value.Substring(1);
            if (value.EndsWith("/")) value = value.Substring(0, value.Length - 1);

            return value;
        }
    }

    public class BuildInfo
    {
        public String variableValue { get; set; }
        public String variableReleaseNameOrNumber { get; set; }
        public String variableBuildNumber { get; set; }
    }
}