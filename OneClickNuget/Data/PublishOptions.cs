using System;
using System.IO;
using NuGet;

namespace OneClickNuget.Data
{
    public class PublishOptions : PackageRetrieveOptions
    {
        public PublishOptions(
            string projectFilePath,
            string targetPackageVersion,
            string releaseNotes,
            string apiKey
            ) : base(projectFilePath)
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
            ApiKey = apiKey;
        }

        public SemanticVersion TargetPackageVersion { get; set; }

        public string PackagePath
            => Path.Combine(ProjectDirectory, $"{ProjectName} {TargetPackageVersion.ToNormalizedString()}.nupkg");

        public string ReleaseNotes { get; set; }
        public string ApiKey { get; set; }
    }
}
