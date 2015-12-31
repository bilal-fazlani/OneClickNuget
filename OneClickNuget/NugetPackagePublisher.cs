using System;
using System.IO;
using NuGet;

namespace OneClickNuget
{
    public class NugetPackagePublisher
    {
        private readonly string _csprojFilePath;

        public NugetPackagePublisher(string csprojFilePath)
        {
            _csprojFilePath = csprojFilePath;
        }

        public void Publish(string targetPackageVersion,
            string releaseNotes)
        {
            string projectName = Path.GetFileNameWithoutExtension(_csprojFilePath);
            string projectDirectory = Path.GetDirectoryName(_csprojFilePath);
            string nuspecPath = Path.Combine(projectDirectory, projectName + ".nuspec");

            UpdateNuspecFile(nuspecPath, releaseNotes, targetPackageVersion);

            PatchAssemblyInfo(targetPackageVersion, projectDirectory);

            BuildProject(_csprojFilePath);

            //RunUnitTests(csprojPath);

            CreateNugetPackage(projectDirectory, projectName);

            //PublishNugetPackage(nuspecPath);
        }

        private static void PublishNugetPackage(string nuspecPath)
        {
            throw new NotImplementedException();
        }

        private static void UpdateNuspecFile(string nuspecPath, string releaseNotes, string targetPackageVersion)
        {
            NuspecProvider nuspecProvider = new NuspecProvider(nuspecPath);
            var nuspec = nuspecProvider.ReadNuspectFile();
            UpdateNuspec(nuspec, releaseNotes, targetPackageVersion);
            nuspecProvider.WriteNuspecFile(nuspec);
        }

        private static void CreateNugetPackage(string projectDirectory, string projectName)
        {
            string nuspecPath = Path.Combine(projectDirectory, projectName + ".nuspec");
            new NuspecProvider(nuspecPath).Pack(projectDirectory, projectName);
        }

        private static void RunUnitTests(string csprojPath)
        {
            throw new NotImplementedException();
        }

        private static void BuildProject(string csprojPath)
        {
            new BuildProvider(csprojPath).Build();
        }

        private static void PatchAssemblyInfo(string version, string projectDirectory)
        {
            AssemblyInfoPatcher patcher = new AssemblyInfoPatcher(projectDirectory);
            patcher.PatchVersion(version);
        }

        private static void UpdateNuspec(
            Manifest nuspec, 
            string releaseNotes,
            string targetPackageVersion)
        {
            nuspec.Metadata.ReleaseNotes = releaseNotes + nuspec.Metadata.ReleaseNotes;
            nuspec.Metadata.Version = targetPackageVersion;
        }
    }
}
