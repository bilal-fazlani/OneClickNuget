using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickNuget
{
    public class PublishOptions
    {
        public PublishOptions(
            string projectFilePath,
            string targetPackageVersion, 
            string releaseNotes)
        {
            ProjectFilePath = projectFilePath;
            TargetPackageVersion = targetPackageVersion;
            ReleaseNotes = releaseNotes;
        }

        public string TargetPackageVersion { get; set; }

        public string ReleaseNotes { get; set; }

        public string ProjectFilePath { get; set; }

        public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFilePath);

        public string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

        public string NuspecFilePath => Path.Combine(ProjectDirectory, ProjectName + ".nuspec");
    }
}
