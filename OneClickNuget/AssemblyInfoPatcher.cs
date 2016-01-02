using System.IO;
using System.Threading.Tasks;
using OneClickNuget.Data;

namespace OneClickNuget
{
    public class AssemblyInfoPatcher
    {
        public async Task PatchVersion(PublishOptions options)
        {
            await Task.Run(() =>
            {
                string assemblyInfoFileName = Path.Combine(options.ProjectDirectory, "Properties", "AssemblyInfo.cs");
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

                allLines[assemblyVersionIndex] = $"[assembly: AssemblyVersion(\"{options.TargetPackageVersion.Version}\")]";
                allLines[assemblyFileVersionIndex] = $"[assembly: AssemblyFileVersion(\"{options.TargetPackageVersion.Version}\")]";

                File.WriteAllLines(assemblyInfoFileName, allLines);
            });
        }
    }
}
