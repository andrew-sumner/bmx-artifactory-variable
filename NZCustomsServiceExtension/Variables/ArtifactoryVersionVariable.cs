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


        ///// <summary>
        ///// Gets the Base URL based on the settings configured for the artifactory extension and this variable
        ///// in the format http://artifactory:8081/artifactory
        ///// </summary>
        //[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed.")]
        //public string GetBaseURL()
        //{
        //    ArtifactoryConfigurer config = this.GetExtensionConfigurer();
            
        //    string url = config.Server;

        //    if (url.EndsWith("/"))
        //    {
        //        url = url.Remove(url.Length - 1);
        //    }
                
        //    return url;
        //}

        public bool RepositoryPathRequiresExpanding()
        {
            return RepositoryPath.Contains("%RELNO%") || RepositoryPath.Contains("%RELNAME%");
        }
        /// <summary>
        /// Gets the RepositoryPath (without BaseURL) based on the settings configured for this variable
        /// in the format libs-release-local/nz.govt.customs/SmartViewer - with any %RELNO% or %RELNAME% 
        /// replaced with actual values
        /// </summary>
        public string ExpandRepositoryPath(string releaseNumber, string releaseName)
        {
            StringBuilder path = new StringBuilder();

            path.Append(this.RepositoryPath ?? String.Empty);
            path.Append("/");

            string rp = this.RepositoryPath;

            if (rp.EndsWith("/"))
            {
                rp = rp.Substring(1);
            }

            if (rp.EndsWith("/"))
            {
                rp = rp.Remove(path.Length - 1);
            }

            path.Append(rp);

            // Format variables
            if (!string.IsNullOrEmpty(releaseNumber))
            {
                path = path.Replace("%RELNO%", releaseNumber);
            }

            if (!string.IsNullOrEmpty(releaseName))
            {
                path = path.Replace("%RELNAME%", releaseName);
            }

            return path.ToString();
        }

        public string ExpandFilter(string releaseNumber, string releaseName)
        {
            string filter = string.IsNullOrEmpty(this.Filter) ? string.Empty : this.Filter;

            // Format variables
            if (!string.IsNullOrEmpty(releaseNumber))
            {
                filter = filter.Replace("%RELNO%", releaseNumber);
            }

            if (!string.IsNullOrEmpty(releaseName))
            {
                filter = filter.Replace("%RELNAME%", releaseName);
            }

            return filter;
        }

        public string ExpandTrimFromPath(string releaseNumber, string releaseName)
        {
            string trimFromPath = string.IsNullOrEmpty(this.TrimFromPath) ? string.Empty : this.TrimFromPath;

            // Format variables
            if (!string.IsNullOrEmpty(releaseNumber))
            {
                trimFromPath = trimFromPath.Replace("%RELNO%", releaseNumber);
            }

            if (!string.IsNullOrEmpty(releaseName))
            {
                trimFromPath = trimFromPath.Replace("%RELNAME%", releaseName);
            }

            return trimFromPath;
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
        public static ArtifactoryVersionVariable GetVariableDeclaration(int applicationId, string artifactoryVariableName)
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
