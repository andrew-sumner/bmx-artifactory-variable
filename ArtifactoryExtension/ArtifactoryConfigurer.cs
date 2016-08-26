using Inedo.BuildMaster;
using Inedo.BuildMaster.Extensibility.Configurers.Extension;
using Inedo.BuildMaster.Web;
using Inedo.Serialization;

[assembly: ExtensionConfigurer(typeof(ArtifactoryExtension.ArtifactoryConfigurer))]

namespace ArtifactoryExtension
{

    [CustomEditor(typeof(ArtifactoryConfigurerEditor))]
    public class ArtifactoryConfigurer : ExtensionConfigurerBase 
    {
        [Persistent]
        public string Server { get; set; }

        [Persistent]
        public string Username { get; set; }

        [Persistent]
        public string Password { get; set; }

        public override string ToString()
        {
            return string.Empty;
        }
    }
}
