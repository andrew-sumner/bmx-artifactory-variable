// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariable.cs" company="NZ Customs Service">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace NZCustomsServiceExtension.Variables
{
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Xml;
    using Inedo.BuildMaster;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Extensibility.Variables;
    using Inedo.BuildMaster.Web;
    using System;
    using System.Collections.Generic;
    using System.Web.UI.WebControls;
    using System.Text;
    using Inedo.BuildMaster.Data.StoredProcedures;
    using Inedo.BuildMaster.Extensibility.Actions;

    /// <summary>
    /// Artifactory build variable.  Allows selection of a specific build in Artifactory.
    /// </summary>
    [VariableProperties(
        "Artifactory Build Variable (NZCustomsService)",
        "Variable that represents a deployable found in artifactory, uses api artifactories storage to display a list of folders from a particular location in artifactory.")]
    [CustomEditor(typeof(ArtifactoryVersionVariableEditor))]
    [CustomSetter(typeof(ArtifactoryVersionVariableSetter))]
    public sealed class ArtifactoryVersionVariable : VariableBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactoryVersionVariable"/> class.
        /// </summary>
        public ArtifactoryVersionVariable()
        {
            this.ReplaceSlashWithDot = false;
        }

        /// <summary>
        /// Gets or sets the name of the repository repository the item can be found in
        /// </summary>
        [Persistent]
        public string RepositoryKey { get; set; }

        /// <summary>
        /// Gets or sets the path to the Artifactory repository folder that the build folders can be found in
        /// </summary>
        [Persistent]
        public string RepositoryPath { get; set; }

        /// <summary>
        /// Gets or sets a filter to limit artifacts to those built for the current release
        /// </summary>
        [Persistent]
        public string Filter { get; set; }

        /// <summary>
        /// Gets or sets a string to remove from the list item
        /// </summary>
        [Persistent]
        public string TrimFromPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether '/' should be replace with '.' or not
        /// </summary>
        [Persistent]
        public bool ReplaceSlashWithDot { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the variable should default to (Not Included)
        /// </summary>
        [Persistent]
        public bool DefaultToNotIncluded { get; set; }

        /// <summary>
        /// Get the Artifactory Server configuration for this Extension
        /// </summary>
        public ArtifactoryConfigurer GlobalConfig
        {
            get {
                ArtifactoryConfigurer configurer = Util.ExtensionConfigurers.GetExtensionConfigurer(this) as ArtifactoryConfigurer;
            
                if (configurer == null)
                {
                    String message = "The extension 'NZCustomsService' global configuration needs setting.";
                    throw new Exception(message);
                }

                return configurer;
            }            
        }

        /// <summary>
        /// Does the path contain %RELNO% or %RELNAME%
        /// </summary>
        /// <returns></returns>
        internal bool RepositoryPathRequiresExpanding()
        {
            return RepositoryPath.Contains("%RELNO%") || RepositoryPath.Contains("%RELNAME%");
        }

        /// <summary>
        /// Get the full atifactory url to the selected folder 
        /// </summary>
        /// <param name="selectedVersion"></param>
        /// <returns></returns>
        internal string GetRepositoryPath(string selectedVersion)
        {
            StringBuilder path = new StringBuilder();

            // Append repository
            AppendPath(path, this.RepositoryKey);

            // Append the part of the path that not provided by the selected version
            int remove = String.IsNullOrEmpty(this.TrimFromPath) ? 0 : this.TrimFromPath.Length;            
            string repositoryPath = this.RepositoryPath;

            if (remove < this.RepositoryPath.Length)
            {
                repositoryPath = repositoryPath.Remove(remove);
            }
            AppendPath(path, repositoryPath);
            
            // Append the selected version
            AppendPath(path, selectedVersion);

            // Replace . with / if required
            string value = path.ToString();

            if (this.ReplaceSlashWithDot)
            {
                int index = value.LastIndexOf(".");

                if (index > -1)
                {
                    value = value.Remove(index, 1).Insert(index, "/");
                }
            }

            return value;
        }

        /// <summary>
        /// Gets the RepositoryPath (without BaseURL) based on the settings configured for this variable
        /// in the format libs-release-local/nz.govt.customs/SmartViewer - with any %RELNO% or %RELNAME% 
        /// replaced with actual values
        /// </summary>
        internal string ExpandRepositoryPath(string releaseNumber, string releaseName)
        {
            StringBuilder path = new StringBuilder();

            AppendPath(path, this.RepositoryKey);
            AppendPath(path, this.RepositoryPath);

            ExpandVariables(path, releaseNumber, releaseName);

            return path.ToString();
        }

        internal string ExpandFilter(string releaseNumber, string releaseName)
        {
            StringBuilder filter = new StringBuilder();

            AppendPath(filter, this.Filter);
            ExpandVariables(filter, releaseNumber, releaseName);

            return filter.ToString();
        }

        internal string ExpandTrimFromPath(string releaseNumber, string releaseName)
        {
            StringBuilder trim = new StringBuilder();

            AppendPath(trim, this.RepositoryKey);
            AppendPath(trim, this.TrimFromPath);

            if ((this.TrimFromPath ?? String.Empty).EndsWith("/")) 
            {
                trim.Append("/");
            }

            ExpandVariables(trim, releaseNumber, releaseName);

            return trim.ToString();
        }

        private void ExpandVariables(StringBuilder path, string releaseNumber, string releaseName)
        {
            // Format variables
            if (!string.IsNullOrEmpty(releaseNumber))
            {
                path = path.Replace("%RELNO%", releaseNumber);
            }

            if (!string.IsNullOrEmpty(releaseName))
            {
                path = path.Replace("%RELNAME%", releaseName);
            }
        }

        internal void AppendPath(StringBuilder path, string section)
        {
            section = RemoveSurroundingSlashes(section);

            if (section.Length > 0)
            {
                path.Append(path.Length > 0 ? "/" : String.Empty);
                path.Append(section);
            }
        }

        private string RemoveSurroundingSlashes(string path)
        {
            if (path == null) path = String.Empty;

            if (path.StartsWith("/"))
            {
                path = path.Remove(0, 1);
            }

            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            return path;
        }

        public static ArtifactVersion ExtractReleaseAndBuildNumbers(string version)
        {
            ArtifactVersion value = new ArtifactVersion();

            int index = 0;

            int indexSlash = version.LastIndexOf('/');
            int indexDot = version.LastIndexOf('.');

            if (indexSlash > 0 && indexSlash < indexDot)
            {
                version = version.Substring(indexSlash + 1);
                indexSlash = version.LastIndexOf('/');
                indexDot = version.LastIndexOf('.');
            }

            if (indexSlash <= 0)
            {
                index = indexDot;
            }
            
            if (index > 0)
            {
                value.ReleaseNameOrNumber = version.Substring(0, index);
                value.BuildNumber = version.Substring(index + 1);
            }

            return value;
        }

        /// <summary>
        /// Create and populate ArtifactoryVersionVariable from settings in database for this application
        /// </summary>
        /// <param name="version">Version selected for the variable</param>
        /// <returns>Artifactory path</returns>
        internal static ArtifactoryVersionVariable GetVariableDeclaration(int applicationId, string artifactoryVariableName)
        {
            // Get variable properties
            var settings = StoredProcs
                     .Variables_GetVariableDeclarations("B", applicationId, null)
                     .Execute()
                     .Where(s => s.Variable_Name == artifactoryVariableName)
                     .FirstOrDefault()
                     .Variable_Configuration;

            return (ArtifactoryVersionVariable)Util.Persistence.DeserializeFromPersistedObjectXml(settings);
        }

        /// <summary>
        /// Get the value of an ArtifactoryVersionVariable for an execution
        /// </summary>
        /// <param name="executionId"></param>
        /// <param name="variableName"></param>
        /// <returns></returns>
        public static Tables.Variable_Values GetVariableValue(int executionId, string variableName)
        {
            return StoredProcs.Variables_GetVariableValues(
                        Environment_Id: null, Server_Id: null,
                        ApplicationGroup_Id: null, Application_Id: null, Deployable_Id: null,
                        Release_Number: null, Build_Number: null,
                        Execution_Id: executionId
                    ).Execute().FirstOrDefault(v => v.Variable_Name == variableName);
        }

        /// <summary>
        /// Get a list of all ArtifactoryVersionVariable in the build scope
        /// </summary>
        /// <param name="applicationId"></param>
        /// <returns></returns>
        public static ListItem[] GetArtifactoryVariablesInBuildScope(int applicationId)
        {
            return StoredProcs
                    .Variables_GetVariableDeclarations("B", applicationId, null)
                    .Execute()
                    .Where(s => s.Variable_Configuration.Contains("NZCustomsServiceExtension.Variables.ArtifactoryVersionVariable"))
                    .Select(s => new ListItem(s.Variable_Name))
                    .ToArray();
        }
    }

    public class ArtifactVersion
    {
        public String ReleaseNameOrNumber { get; set; }
        public String BuildNumber { get; set; }

    }
}
