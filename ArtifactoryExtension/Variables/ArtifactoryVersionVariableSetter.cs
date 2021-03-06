// -----------------------------------------------------------------------
// <copyright file="ArtifactoryVersionVariableSetter.cs" company="Inedo">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ArtifactoryExtension.Variables
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Web.UI.WebControls;
    using Inedo.BuildMaster.Data;
    using Inedo.BuildMaster.Web.Controls.Extensions;
    using ArtifactoryExtension.Artifactory;
    using ArtifactoryExtension.Artifactory.Domain;
    using Inedo.BuildMaster;

    /// <summary>
    /// Artifactory build variable setter.
    /// </summary>
    internal sealed class ArtifactoryVersionVariableSetter : DropDownList, IVariableSetter<ArtifactoryVersionVariable>
    {
        /// <summary>
        /// Value to use for not included option
        /// </summary>
        public const string NotIncluded = "(Not Included)";

        /// <summary>
        /// Initializes a new instance of the <see cref="ArtifactoryVersionVariableSetter"/> class.
        /// </summary>
        public ArtifactoryVersionVariableSetter()
        {
        }

        /// <summary>
        /// Gets or sets the selected value
        /// </summary>
        string IVariableSetter.VariableValue
        {
            get { return this.SelectedValue; }
            set { this.SelectedValue = value == NotIncluded ? NotIncluded : value; }
        }
        
        /// <summary>
        /// Implement method
        /// </summary>
        /// <param name="variable">the variable</param>
        /// <param name="defaultValue">default value (not used)</param>
        void IVariableSetter<ArtifactoryVersionVariable>.BindToVariable(ArtifactoryVersionVariable variable, string defaultValue)
        {
            this.EnsureChildControls();

            int applicationId = GetApplicationIdFromUrl();
            
            // Get a list of all active releases for the specified appliation
            var releases = DB
                            .Releases_GetReleases(applicationId, null, null)
                            .Where(s => s.ReleaseStatus_Name == "Active")
                            .OrderBy(s => s.Release_Sequence)
                            .Select(s => new { s.Release_Number, s.Release_Name }).ToArray();
            
            // Add empty element so can select nothing
            this.Items.Add(NotIncluded);

            if (!variable.RepositoryPath.Contains("%RELNO%") && !variable.RepositoryPath.Contains("%RELNAME%") && 
                !variable.Filter.Contains("%RELNO%") && !variable.Filter.Contains("%RELNAME%") )
            {
                this.Items.AddRange(this.GetReleaseBuilds(variable, "", ""));
            }
            else
            {
                // Add all builds for the active release(s)            
                foreach (var release in releases.OrderBy(s => s.Release_Name))
                {
                    this.Items.AddRange(this.GetReleaseBuilds(variable, release.Release_Number, release.Release_Name));
                }
            }

            if (this.Items.Count == 1)
            {
                this.Items.Add(string.Format("<<< No builds found in Artifactory for application id {0} >>>", applicationId));
            }

            // Select the last build as the default
            if ( variable.DefaultToNotIncluded )
                this.SelectedValue = NotIncluded;
            else
                this.SelectedValue = this.Items[this.Items.Count - 1].Text;
        }

        /// <summary>
        /// Extract the application id from the URL
        /// </summary>
        /// <returns>Application Id</returns>
        private static int GetApplicationIdFromUrl()
        {
            string url = HttpContext.Current.Request.Url.AbsolutePath;

            string[] parts = url.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (parts[i].ToLower() == "applications")
                {
                    return int.Parse(parts[i + 1]);
                }
            }

            return -1;
        }

        public static ListItemCollection BubbleSortList(ListItemCollection listsub)
        {
            ListItemCollection list = listsub;
            bool swapped = false;
            int n = list.Count;
            for (int y = 0; y < n; y++)
            {
                for (int j = n - 1; j > 1; j--)
                {
                    if (list[j - 1].Value.CompareTo(list[j].Value) > 0)
                    {
                        string tempv = list[j - 1].Value; string temps = list[j - 1].Text;
                        list[j - 1].Text = list[j].Text;
                        list[j - 1].Value = list[j].Value;
                        list[j].Text = temps;
                        list[j].Value = tempv;
                        swapped = true;
                    }
                }
            }
            if (swapped == true)
                return BubbleSortList(list);

            return list;
        }

        /// <summary>
        /// Create list of builds for a release
        /// </summary>
        /// <param name="variable">Artifactory variable</param>
        /// <param name="releaseNumber">Release number</param>
        /// <returns>List of builds</returns>
        internal ListItem[] GetReleaseBuilds(ArtifactoryVersionVariable variable, string releaseNumber, string releaseName)
        {

            List<ListItem> folders = new List<ListItem>();

            string repositoryPath = variable.ExpandRepositoryPath(releaseNumber, releaseName);
            string filter = variable.ExpandFilter(releaseNumber, releaseName);
            string trimFromPath = variable.ExpandTrimFromPath(releaseNumber, releaseName);

            FolderInfo folderInfo;

            ArtifactoryConfigurer config = variable.GlobalConfig;

            try
            {
                ArtifactoryApi artifactory = new ArtifactoryApi(config);
                folderInfo = artifactory.GetFolderInfo(repositoryPath);
            }
            catch (Exception ex)
            {
                folders.Add(new ListItem(string.Format("<< storageURL: {0}/{1} >>", config.Server, repositoryPath)));
                folders.Add(new ListItem(string.Format("<< Failed: {0} >>", ex.Message)));                
                return folders.ToArray();
            }

            foreach (var child in folderInfo.Children) {
                if (child.Folder) {
                    if (string.IsNullOrEmpty(filter) || Regex.IsMatch(child.Uri, filter, RegexOptions.IgnoreCase | RegexOptions.Singleline))
                    {
                        folders.Add(new ListItem(GetDisplayPath(variable, repositoryPath + child.Uri, trimFromPath)));
                    }
                }
            }

            return folders.OrderBy(s => ConvertToSortableNumber(s.Value)).ToArray();
        }


        /// <summary>
        /// Format artifact path according rules set for this ArtifactoryVersionVariable
        /// </summary>
        /// <param name="path"></param>
        /// <param name="trimFromPath"></param>
        /// <returns></returns>
        private string GetDisplayPath(ArtifactoryVersionVariable variable, string path, string trimFromPath)
        {
            if (!string.IsNullOrEmpty(trimFromPath))
            {
                if (path.StartsWith(trimFromPath))
                {
                    path = path.Substring(trimFromPath.Length);
                }
            }

            if (variable.ReplaceSlashWithDot)
            {
                int index = path.LastIndexOf('/');

                if (index > -1)
                {
                    path = path.Remove(index, 1).Insert(index, ".");
                }
            }

            return path;
        }

        private int ConvertToSortableNumber(String value)
        {
            int result;

            result = value.LastIndexOfAny(new char[] { '/', '.' });
            if (result >= 0)
            {
                value = value.Substring(value.LastIndexOfAny(new char[] { '/', '.' }) + 1);                
            }

            int.TryParse(value, out result);
            
            return result;
        }
    }
}
