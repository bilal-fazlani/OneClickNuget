using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using OneClickNuget.Data;

namespace OneClickNuget
{
    public class PackageMaker
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

                    using (FileStream nupkgStream = new FileStream(nupkgPath, FileMode.Create))
                    {
                        pacakgeBuilder.Save(nupkgStream);
                    }
                }
            });
        }

        public async Task PublishNugetPackage(PublishOptions options)
        {
            try
            {
                await Task.Run(() =>
                {
                    IPackage localPackage = GetLocalPackage(options);

                    FileInfo packageFileInfo = new FileInfo(options.PackagePath);

                    PackageServer packageServer = new PackageServer("https://www.nuget.org/api/v2/package",
                        "OneClickNuget");

                    packageServer.PushPackage(options.ApiKey, localPackage,
                        packageFileInfo.Length, 180000, false);
                });
            }
            catch (Exception ex)
            {
                throw new Exception("Could not publish package. "+ ex.Message);
            }
        }

        private static IPackage GetLocalPackage(PublishOptions options)
        {
            IPackageRepository localRepo = PackageRepositoryFactory.Default
                .CreateRepository(options.ProjectDirectory);

            IPackage localPackage = localRepo
                .FindPackagesById(options.ProjectName)
                .SingleOrDefault(x => x.Version == options.TargetPackageVersion);

            if (localPackage == null) throw new Exception($"could not find nuget package of version {options.TargetPackageVersion} on local disk");

            return localPackage;
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