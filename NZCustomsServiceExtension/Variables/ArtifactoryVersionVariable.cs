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


        internal bool RepositoryPathRequiresExpanding()
        {
            return RepositoryPath.Contains("%RELNO%") || RepositoryPath.Contains("%RELNAME%");
        }

        internal string ExpandRepositoryPathWithValue(string releaseNumber, string releaseName, string selectedVersion)
        {
            StringBuilder path = new StringBuilder();
            path.Append(ExpandRepositoryPath(releaseNumber, releaseName));

            String trimFromPath = ExpandTrimFromPath(releaseNumber, releaseName);
            if (!String.IsNullOrEmpty(trimFromPath) && path.ToString().StartsWith(trimFromPath))
            {
                path = new StringBuilder(path.ToString().Substring(0, trimFromPath.Length));
            }

            AppendPath(path, selectedVersion);

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

        internal void AppendPath(StringBuilder path, string section)
        {
            section = RemoveSurroundingSlashes(section);

            if (section.Length > 0)
            {
                path.Append(path.Length > 0 ? "/" : String.Empty);
                path.Append(section);
            }
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

            //return path;
        }

        private string RemoveSurroundingSlashes(string path)
        {
            if (path == null) path = String.Empty;

            if (path.StartsWith("/"))
            {
                path = path.Remove(1, 1);
            }

            if (path.EndsWith("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            return path;
        }

        internal string GetTrimmedPath(string path, string trimFromPath)
        {
            if (!string.IsNullOrEmpty(trimFromPath))
            {
                if (path.StartsWith(trimFromPath))
                {
                    path = path.Substring(trimFromPath.Length);
                }
            }

            return path;
        }

        internal string GetReplaceSlashWithDot(string path, string trimFromPath)
        {
            if (this.ReplaceSlashWithDot)
            {
                int index = path.LastIndexOf('/');

                if (index > -1)
                {
                    path = path.Remove(index, 1).Insert(index, ".");
                }
            }

            return path;
        }

        ///// <summary>
        ///// Gets the server name from the XML returned by the ExtensionConfiguration_GetConfiguration call for the Artifactory extension
        ///// </summary>
        //private string GetArtifactoryExtensionArtifactoryUrl()
        //{
        //    var settings = StoredProcs
        //                   .ExtensionConfiguration_GetConfiguration("Inedo.BuildMasterExtensions.Artifactory.ArtifactoryConfigurer,Artifactory", null)
        //                   .Execute()
        //                   .FirstOrDefault()
        //                   .Extension_Configuration;

        //    XmlDocument xml = new XmlDocument();
        //    xml.LoadXml(settings);

        //    XmlNode node = xml.SelectSingleNode("//Properties/@Server");

        //    if (node == null)
        //    {
        //        return null;
        //    }

        //    return node.Value;
        //}


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

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(settings);

            return new ArtifactoryVersionVariable
            {
                RepositoryKey = xml.SelectSingleNode("//Properties/@RepositoryKey").Value,
                RepositoryPath = xml.SelectSingleNode("//Properties/@RepositoryPath").Value,
                Filter = xml.SelectSingleNode("//Properties/@Filter").Value,
                TrimFromPath = xml.SelectSingleNode("//Properties/@TrimFromPath").Value,
                ReplaceSlashWithDot = bool.Parse(xml.SelectSingleNode("//Properties/@ReplaceSlashWithDot").Value),
                DefaultToNotIncluded = bool.Parse(xml.SelectSingleNode("//Properties/@DefaultToNotIncluded").Value)
            };
        }

        public static ArtifactVersion ExtractReleaseAndBuildNumbers(string version)
        {
            ArtifactVersion value = new ArtifactVersion();

            int index = version.LastIndexOf('/');
            if (index <= 0) index = version.LastIndexOf('.'); 

            if (index > 0)
            {
                value.ReleaseNameOrNumber = version.Substring(0, index);
                value.BuildNumber = version.Substring(index + 1);
            }

            return value;
        }

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
