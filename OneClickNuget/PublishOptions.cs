using System;
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
            if (string.IsNullOrEmpty(targetPackageVersion))
                throw new Exception("Please specify a version for nuget package.");

            try
            {
                TargetPackageVersion = SemanticVersion.Parse(targetPackageVersion);
            }
            catch (Exception)
            {
                throw new Exception($"{targetPackageVersion} is not a valid version.");
            }
            
            ReleaseNotes = releaseNotes;
        }

        public SemanticVersion TargetPackageVersion { get; set; }

        public string ReleaseNotes { get; set; }
    }
}
