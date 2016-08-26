using System.Reflection;
using System.Runtime.InteropServices;
using Inedo.BuildMaster.Extensibility;

[assembly: AssemblyTitle("ArtifactoryExtension")]
[assembly: AssemblyDescription("BuildMaster Extension for Artifactory.")]
[assembly: AssemblyCompany("Inedo")]
[assembly: AssemblyCopyright("Copyright © 2016")]

[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

//[assembly: BuildMasterAssembly]
[assembly: RequiredBuildMasterVersion("5.3.0")]

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("UnitTests")]