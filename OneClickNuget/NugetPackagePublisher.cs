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

            ReportProgress(progress, 0, "Starting up...");

            await UpdateNuspecFile(nuspecPath, releaseNotes, targetPackageVersion);

            ReportProgress(progress, 10, "Nuspec updated");
            cancellationToken.ThrowIfCancellationRequested();

            await PatchAssemblyInfo(targetPackageVersion, projectDirectory);

            ReportProgress(progress, 20, "AssemblyInfo patched");
            cancellationToken.ThrowIfCancellationRequested();

            await BuildProject(_csprojFilePath);
            ReportProgress(progress, 50, "Build complete");
            cancellationToken.ThrowIfCancellationRequested();

            await RunUnitTests(projectDirectory);

            ReportProgress(progress, 70, "Unit tests skipped");
            cancellationToken.ThrowIfCancellationRequested();

            await CreateNugetPackage(projectDirectory, projectName);

            ReportProgress(progress, 90, "Nuget package created");
            cancellationToken.ThrowIfCancellationRequested();

            await PublishNugetPackage(nuspecPath);

            ReportProgress(progress, 100, "Publish task skipped");
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
                nuspec.Metadata.ReleaseNotes = releaseNotes + nuspec.Metadata.ReleaseNotes;
                nuspec.Metadata.Version = targetPackageVersion;
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
    }
}
