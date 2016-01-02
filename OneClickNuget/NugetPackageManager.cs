using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet;

namespace OneClickNuget
{
    /*
    todo: run unit tests,
    todo: publish
    */
    public class NugetPackageManager
    {
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
            PublishOptions options,
            IProgress<PublishProgress> progress,
            CancellationToken cancellationToken
            )
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(progress, 0, "Starting up...");

            await PrepareBinaries(options, progress, cancellationToken);

            await PublishPackage(options, progress, cancellationToken);
        }

        public async Task<Manifest> GetPackageInformation(PackageRetrieveOptions options)
        {
            var nuspecProvider = new NuspecProvider();
            await nuspecProvider.DownloadLatestNuspec(options);
            return await nuspecProvider.ReadNuspectFile(options);
        }

        private async Task PrepareBinaries(PublishOptions options,
            IProgress<PublishProgress> progress, 
            CancellationToken cancellationToken)
        {
            AssemblyInfoPatcher assemblyInfoPatcher = new AssemblyInfoPatcher();
            await assemblyInfoPatcher.PatchVersion(options);
            ReportProgress(progress, 20, "AssemblyInfo patched");
            cancellationToken.ThrowIfCancellationRequested();

            BuildProvider buildProvider = new BuildProvider();
            await buildProvider.Build(options);
            ReportProgress(progress, 50, "Build complete");
            cancellationToken.ThrowIfCancellationRequested();

            await buildProvider.RunUnitTests();
            ReportProgress(progress, 70, "Unit tests skipped");
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task PublishPackage(
            PublishOptions options,
            IProgress<PublishProgress> progress,
            CancellationToken cancellationToken)
        {
            NuspecProvider nuspecProvider = new NuspecProvider();

            await nuspecProvider.UpdateNuspecFile(options);
            ReportProgress(progress, 10, "Nuspec updated");
            cancellationToken.ThrowIfCancellationRequested();

            await nuspecProvider.Pack(options);
            ReportProgress(progress, 90, "Nuget package created");
            cancellationToken.ThrowIfCancellationRequested();

            await nuspecProvider.Publish();
            ReportProgress(progress, 100, "Publish task skipped");
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
