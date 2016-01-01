using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        private static void ReportProgress(IProgress<PublishProgress> progress, 
            int percent, string message)
        {
            progress.Report(new PublishProgress
            {
                Message = message,
                Percent = percent
            });
        }

        public async Task Publish(
            string targetPackageVersion,
            string releaseNotes,
            IProgress<PublishProgress> progress,
            CancellationToken cancellationToken
            )
        {
            cancellationToken.ThrowIfCancellationRequested();

            string projectName = Path.GetFileNameWithoutExtension(_csprojFilePath);
            string projectDirectory = Path.GetDirectoryName(_csprojFilePath);
            string nuspecPath = Path.Combine(projectDirectory, projectName + ".nuspec");
            int percent = 0;

            ReportProgress(progress, percent, "Publish started");

            await UpdateNuspecFile(nuspecPath, releaseNotes, targetPackageVersion);

            percent = percent + 10;
            ReportProgress(progress, percent, "Nuspec updated");
            cancellationToken.ThrowIfCancellationRequested();

            await PatchAssemblyInfo(targetPackageVersion, projectDirectory);

            percent = percent + 10;
            ReportProgress(progress, percent, "AssemblyInfo patched");
            cancellationToken.ThrowIfCancellationRequested();

            await BuildProject(_csprojFilePath);
            percent = percent + 30;
            ReportProgress(progress, percent, "Build complete");
            cancellationToken.ThrowIfCancellationRequested();

            await RunUnitTests(projectDirectory);

            percent = percent + 20;
            ReportProgress(progress, percent, "Unit tests skipped");
            cancellationToken.ThrowIfCancellationRequested();

            await CreateNugetPackage(projectDirectory, projectName);

            percent = percent + 20;
            ReportProgress(progress, percent, "Nuget package created");
            cancellationToken.ThrowIfCancellationRequested();

            await PublishNugetPackage(nuspecPath);

            percent = percent + 10;
            ReportProgress(progress, percent, "Publish task skipped");
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static async Task PublishNugetPackage(string nuspecPath)
        {
            await Task.Delay(new TimeSpan(0, 0, 3));
        }

        private static async Task UpdateNuspecFile(string nuspecPath, string releaseNotes, string targetPackageVersion)
        {
            await Task.Run(() =>
            {
                NuspecProvider nuspecProvider = new NuspecProvider(nuspecPath);
                var nuspec = nuspecProvider.ReadNuspectFile();
                UpdateNuspec(nuspec, releaseNotes, targetPackageVersion);
                nuspecProvider.WriteNuspecFile(nuspec);
            });
        }

        private static async Task CreateNugetPackage(string projectDirectory, string projectName)
        {
            await Task.Run(() =>
            {
                string nuspecPath = Path.Combine(projectDirectory, projectName + ".nuspec");
                new NuspecProvider(nuspecPath).Pack(projectDirectory, projectName);
            });
        }

        private static async Task RunUnitTests(string projectDiretory)
        {
            await Task.Delay(new TimeSpan(0, 0, 3));
        }

        private static async Task BuildProject(string csprojPath)
        {
            await new BuildProvider(csprojPath).Build();
        }

        private static async Task PatchAssemblyInfo(string version, string projectDirectory)
        {
            await Task.Run(() =>
            {
                AssemblyInfoPatcher patcher = new AssemblyInfoPatcher(projectDirectory);
                patcher.PatchVersion(version);
            });
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
