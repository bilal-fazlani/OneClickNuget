using NuGet;

namespace OneClickNuget
{
    public class PublishOptions : PackageRetrieveOptions
    {
        public PublishOptions(
            string projectFilePath,
            string targetPackageVersion, 
            string releaseNotes):base(projectFilePath)
        {
            TargetPackageVersion = SemanticVersion.Parse(targetPackageVersion);
            ReleaseNotes = releaseNotes;
        }

        public SemanticVersion TargetPackageVersion { get; set; }

        public string ReleaseNotes { get; set; }
    }
}
