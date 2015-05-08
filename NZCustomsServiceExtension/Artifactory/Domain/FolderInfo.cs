using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NZCustomsServiceExtension.Artifactory.Domain
{
    public class FolderInfo
    {
        public string Path { get; set; }
        public string Uri { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public DateTime LastModified { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<FolderChild> Children { get; set; }
    }
}
