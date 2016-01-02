using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using NuGet.Packaging;

namespace OneClickNuget
{
    public class NuspecProvider
    {
        public async Task DownloadLatestNuspec(PackageRetrieveOptions options)
        {
            await Task.Run(() =>
            {
                try
                {
                    IPackageRepository repo =
                        PackageRepositoryFactory.Default.CreateRepository("https://packages.nuget.org/api/v2");
                    var package = repo.FindPackage(options.ProjectName);

                    using (var pkgStream = package.GetStream())
                    {
                        var pkgReader = new PackageReader(pkgStream);
                        using (Stream nuspecStream = pkgReader.GetNuspec())
                        using (var fileStream = new FileStream(options.NuspecFilePath, FileMode.Create))
                        {
                            nuspecStream.CopyTo(fileStream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Unable to download read from internet. " + ex.Message);
                }
            });
        }

        public async Task<Manifest> ReadNuspectFile(PackageRetrieveOptions options)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (FileStream fileStream = new FileStream(options.NuspecFilePath, FileMode.Open))
                    {
                        return Manifest.ReadFrom(fileStream, true);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Could not read nuspec file. {ex.Message}");
                }
            });
        }

        private void WriteNuspecFile(Manifest nuspec, PublishOptions options)
        {
            try
            {
                using (var filestream = new FileStream(options.NuspecFilePath, FileMode.Create))
                {
                    nuspec.Save(filestream, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cound not write nuspec file to disk. "+ ex.Message);
            }
        }

        public async Task UpdateNuspecFile(PublishOptions options)
        {
            await Task.Run(async () =>
            {
                var nuspec = await ReadNuspectFile(options);

                if (!string.IsNullOrEmpty(options.ReleaseNotes))
                {
                    nuspec.Metadata.ReleaseNotes =
                        $"v {options.TargetPackageVersion}{Environment.NewLine}" +
                        $"{options.ReleaseNotes}{Environment.NewLine}{Environment.NewLine}" +
                        nuspec.Metadata.ReleaseNotes;
                }

                nuspec.Metadata.Version = options.TargetPackageVersion.ToNormalizedString();

                WriteNuspecFile(nuspec, options);
            });
        }

        public async Task Pack(PublishOptions options)
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

        private List<ManifestFile> GetManifestFiles(List<ManifestFile> onlineManifestFileEntries,
            PublishOptions options)
        {
            if (onlineManifestFileEntries == null || !onlineManifestFileEntries.Any())
            {
                List<ManifestFile> files = new List<ManifestFile>();
                AddDefaultManifestFileEntry(files, options);
                return files;
            }

            if(!ContainsDefaultEnty(onlineManifestFileEntries, options))
            {
                AddDefaultManifestFileEntry(onlineManifestFileEntries, options);
                return onlineManifestFileEntries;
            }

            return onlineManifestFileEntries;
        } 

        public async Task Publish()
        {
            await Task.Delay(new TimeSpan(0, 0, 3));
        }
    }
}
