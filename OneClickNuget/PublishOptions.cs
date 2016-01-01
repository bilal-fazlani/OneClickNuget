using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickNuget
{
    public class PublishOptions : PackageRetrieveOptions
    {
        public PublishOptions(
            string projectFilePath,
            string targetPackageVersion, 
            string releaseNotes):base(projectFilePath)
        {
            TargetPackageVersion = targetPackageVersion;
            ReleaseNotes = releaseNotes;
        }

        public string TargetPackageVersion { get; set; }

        public string ReleaseNotes { get; set; }
    }
}
