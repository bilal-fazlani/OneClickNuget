using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NuGet;
using NuGet.Packaging;
using OneClickNuget.Data;

namespace OneClickNuget
{
    public class NuspecManager
    {
        public async Task RefreshNuspecFile(PackageRetrieveOptions options)
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

        public async Task<Manifest> GetNuspecManifest(PackageRetrieveOptions options)
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

        public async Task PatchNuspecFile(PublishOptions options)
        {
            await Task.Run(async () =>
            {
                if (!File.Exists(options.NuspecFilePath))
                {
                    await RefreshNuspecFile(options);
                }
                
                var nuspec = await GetNuspecManifest(options);

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
                throw new Exception("Cound not write nuspec file to disk. " + ex.Message);
            }
        }
    }
}
