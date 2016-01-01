using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneClickNuget
{
    public class PackageRetrieveOptions
    {
        public PackageRetrieveOptions(string projectFilePath)
        {
            ProjectFilePath = projectFilePath;
        }

        public string ProjectFilePath { get; set; }

        public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFilePath);

        public string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

        public string NuspecFilePath => Path.Combine(ProjectDirectory, ProjectName + ".nuspec");
    }
}
