using System;
using System.IO;
using NuGet;

namespace OneClickNuget
{
    public class NuspecProvider
    {
        private readonly string _nuspecFilePath;

        public NuspecProvider(string nuspecFilePath)
        {
            _nuspecFilePath = nuspecFilePath;
        }

        public Manifest ReadNuspectFile()
        {
            try
            {
                using (FileStream fileStream = new FileStream(_nuspecFilePath, FileMode.Open))
                {
                        return Manifest.ReadFrom(fileStream, true);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Could not read nuspec file. {ex.Message}");
            }
        }

        public void WriteNuspecFile(Manifest nuspec)
        {
            using (var filestream = new FileStream(_nuspecFilePath, FileMode.Create))
            {
                nuspec.Save(filestream, true);
            }
        }

        public void Pack(string projectDirectory, string projectName)
        {
            using (FileStream fileStream = new FileStream(_nuspecFilePath, FileMode.Open))
            {
                var nuspec = Manifest.ReadFrom(fileStream, true);
                var pacakgeBuilder = new PackageBuilder();
                pacakgeBuilder.Populate(nuspec.Metadata);

                pacakgeBuilder.PopulateFiles(projectDirectory, nuspec.Files);

                string nupkgPath = Path.Combine(projectDirectory, $"{projectName} {nuspec.Metadata.Version}.nupkg");
                FileStream nupkgStream = new FileStream(nupkgPath, FileMode.Create);

                pacakgeBuilder.Save(nupkgStream);
            }
        }

        public void Publish()
        {
            throw new NotImplementedException();
        }
    }
}
