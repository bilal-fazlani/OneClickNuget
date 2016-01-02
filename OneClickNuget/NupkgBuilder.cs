using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;

namespace OneClickNuget
{
    public class NupkgBuilder
    {
        public async Task CreateNugetPackage(PublishOptions options)
        {
            await Task.Run(() =>
            {
                using (FileStream fileStream = new FileStream(options.NuspecFilePath, FileMode.Open))
                {
                    var nuspec = Manifest.ReadFrom(fileStream, true);
                    var pacakgeBuilder = new PackageBuilder();
                    pacakgeBuilder.Populate(nuspec.Metadata);

                    var manifestFiles = GetManifestFiles(nuspec.Files, options);

                    pacakgeBuilder.PopulateFiles(options.ProjectDirectory, manifestFiles);

                    string nupkgPath = Path.Combine(options.ProjectDirectory, $"{options.ProjectName} {nuspec.Metadata.Version}.nupkg");
                    FileStream nupkgStream = new FileStream(nupkgPath, FileMode.Create);

                    pacakgeBuilder.Save(nupkgStream);
                }
            });
        }

        public async Task PublishNugetPackage()
        {
            await Task.Delay(new TimeSpan(0, 0, 3));
        }

        private List<ManifestFile> GetManifestFiles(List<ManifestFile> onlineManifestFileEntries,
            PublishOptions options)
        {
            if (onlineManifestFileEntries == null || !onlineManifestFileEntries.Any())
            {
                List<ManifestFile> files = new List<ManifestFile>();
                AddDefaultManifestFileEntry(files, options);
                return files;
            }

            if (!ContainsDefaultEnty(onlineManifestFileEntries, options))
            {
                AddDefaultManifestFileEntry(onlineManifestFileEntries, options);
                return onlineManifestFileEntries;
            }

            return onlineManifestFileEntries;
        }

        private bool ContainsDefaultEnty(IEnumerable<ManifestFile> files, PublishOptions options)
        {
            return files.Any(x => x.Target.ToLower() == $"lib\\net45\\{options.ProjectName}.dll".ToLower());
        }

        private void AddDefaultManifestFileEntry(List<ManifestFile> files, PublishOptions options)
        {
            files.Add(new ManifestFile
            {
                Source = $"bin\\Release\\{options.ProjectName}.dll",
                Target = $"lib\\net45\\{options.ProjectName}.dll"
            });
        }
    }
}