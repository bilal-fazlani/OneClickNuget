using System;
using System.IO;

namespace OneClickNuget.Data
{
    public class PackageRetrieveOptions
    {
        public PackageRetrieveOptions(string projectFilePath)
        {
            if(string.IsNullOrEmpty(projectFilePath))
                throw new Exception("Please specify a project file.");
            ProjectFilePath = projectFilePath;
        }

        public string ProjectFilePath { get; set; }

        public string ProjectName => Path.GetFileNameWithoutExtension(ProjectFilePath);

        public string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

        public string NuspecFilePath => Path.Combine(ProjectDirectory, ProjectName + ".nuspec");

        public string NugetUrl => $@"http://nuget.org/api/v2/package/{ProjectName}";
    }
}
