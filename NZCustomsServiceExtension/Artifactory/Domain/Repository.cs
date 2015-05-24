using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NZCustomsServiceExtension.Artifactory.Domain
{
    public class Repository
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
    }
}
