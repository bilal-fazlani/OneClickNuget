using System.IO;

namespace OneClickNuget
{
    public class AssemblyInfoPatcher
    {
        private readonly string _projectDirectory;

        public AssemblyInfoPatcher(string projectDirectory)
        {
            _projectDirectory = projectDirectory;
        }

        public void PatchVersion(string version)
        {
            string assemblyInfoFileName = Path.Combine(_projectDirectory, "Properties", "AssemblyInfo.cs");
            var allLines = File.ReadAllLines(assemblyInfoFileName);

            int index = 0;
            int assemblyVersionIndex = 0;
            int assemblyFileVersionIndex = 0;

            foreach (var line in allLines)
            {
                if (line.StartsWith("[assembly: AssemblyVersion("))
                {
                    assemblyVersionIndex = index;
                }

                if (line.StartsWith("[assembly: AssemblyFileVersion("))
                {
                    assemblyFileVersionIndex = index;
                }

                index++;
            }

            allLines[assemblyVersionIndex] = $"[assembly: AssemblyVersion(\"{version}\")]";
            allLines[assemblyFileVersionIndex] = $"[assembly: AssemblyFileVersion(\"{version}\")]";

            File.WriteAllLines(assemblyInfoFileName, allLines);
        }
    }
}
