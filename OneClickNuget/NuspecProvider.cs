using System;
using System.IO;
using System.Threading.Tasks;
using NuGet;

namespace OneClickNuget
{
    public class NuspecProvider
    {
        private Manifest ReadNuspectFile(PublishOptions options)
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
        }

        private void WriteNuspecFile(Manifest nuspec, PublishOptions options)
        {
            using (var filestream = new FileStream(options.NuspecFilePath, FileMode.Create))
            {
                nuspec.Save(filestream, true);
            }
        }

        public async Task UpdateNuspecFile(PublishOptions options)
        {
            await Task.Run(() =>
            {
                var nuspec = ReadNuspectFile(options);

                nuspec.Metadata.ReleaseNotes =
                $"v {options.TargetPackageVersion}{Environment.NewLine}" +
                $"{options.ReleaseNotes}{Environment.NewLine}{Environment.NewLine}" +
                nuspec.Metadata.ReleaseNotes;

                nuspec.Metadata.Version = options.TargetPackageVersion;

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

                    pacakgeBuilder.PopulateFiles(options.ProjectDirectory, nuspec.Files);

                    string nupkgPath = Path.Combine(options.ProjectDirectory, $"{options.ProjectName} {nuspec.Metadata.Version}.nupkg");
                    FileStream nupkgStream = new FileStream(nupkgPath, FileMode.Create);

                    pacakgeBuilder.Save(nupkgStream);
                }
            });
        }

        public async Task Publish()
        {
            await Task.Delay(new TimeSpan(0, 0, 3));
        }
    }
}
