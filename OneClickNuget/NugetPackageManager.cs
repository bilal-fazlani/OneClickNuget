using System;
using System.Threading;
using System.Threading.Tasks;
using NuGet;
using OneClickNuget.Data;

namespace OneClickNuget
{
    /*
    todo: run unit tests,
    todo: show dependency update options
    todo: use msbuild package
    */
    public class NugetPackageManager
    {
        private static void ReportProgress(IProgress<PackageProgress> progress, 
            int percent, string message)
        {
            progress.Report(new PackageProgress
            {
                Message = message,
                Percent = percent
            });
        }

        public async Task Publish(
            PublishOptions options,
            IProgress<PackageProgress> progress,
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
            var nuspecProvider = new NuspecManager();
            await nuspecProvider.RefreshNuspecFile(options);
            return await nuspecProvider.GetNuspecManifest(options);
        }

        private async Task PrepareBinaries(PublishOptions options,
            IProgress<PackageProgress> progress, 
            CancellationToken cancellationToken)
        {
            AssemblyInfoPatcher assemblyInfoPatcher = new AssemblyInfoPatcher();
            await assemblyInfoPatcher.PatchVersion(options);
            ReportProgress(progress, 20, "AssemblyInfo patched");
            cancellationToken.ThrowIfCancellationRequested();

            ProjectBuilder projectBuilder = new ProjectBuilder();
            await projectBuilder.Build(options);
            ReportProgress(progress, 50, "Build complete");
            cancellationToken.ThrowIfCancellationRequested();

            await projectBuilder.RunUnitTests();
            ReportProgress(progress, 70, "Unit tests skipped");
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task PublishPackage(
            PublishOptions options,
            IProgress<PackageProgress> progress,
            CancellationToken cancellationToken)
        {
            NuspecManager nuspecManager = new NuspecManager();
            await nuspecManager.PatchNuspecFile(options);
            ReportProgress(progress, 10, "Nuspec updated");
            cancellationToken.ThrowIfCancellationRequested();

            PackageMaker packageMaker = new PackageMaker();
            await packageMaker.CreateNugetPackage(options);
            ReportProgress(progress, 90, "Nuget package created");
            cancellationToken.ThrowIfCancellationRequested();

            await packageMaker.PublishNugetPackage(options);
            ReportProgress(progress, 100, "Publish task completed!");
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
