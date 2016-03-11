using System;
using System.Collections.Generic;
using System.Linq;
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

            await PrepareBinaries(options, progress, cancellationToken);

            await PublishPackage(options, progress, cancellationToken);
        }

        public async Task<Manifest> GetPackageInformation(PackageRetrieveOptions options)
        {
            var nuspecProvider = new NuspecManager();
            await nuspecProvider.RefreshNuspecFile(options);
            return await nuspecProvider.GetNuspecManifest(options);
        }

        public List<ManifestDependency> GetProjectDependencies(PackageRetrieveOptions options)
        {
            var configReader = new PackagesConfigReader(options);
            var dependenties = configReader.Read().Select(d => new ManifestDependency
            {
                Id = d.PackageIdentity.Id,
                Version = d.PackageIdentity.Version.ToString()
            }).ToList();

            return dependenties;
        } 

        private async Task PrepareBinaries(PublishOptions options,
            IProgress<PackageProgress> progress, 
            CancellationToken cancellationToken)
        {
            ReportProgress(progress, 0, "Patching AssemblyInfo");

            AssemblyInfoPatcher assemblyInfoPatcher = new AssemblyInfoPatcher();
            await assemblyInfoPatcher.PatchVersion(options);
            cancellationToken.ThrowIfCancellationRequested();
            
            ReportProgress(progress, 10, "Building project");

            ProjectBuilder projectBuilder = new ProjectBuilder();
            await projectBuilder.Build(options);
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(progress, 30, "Skipping unit tests");

            await projectBuilder.RunUnitTests();
            cancellationToken.ThrowIfCancellationRequested();
        }

        private async Task PublishPackage(
            PublishOptions options,
            IProgress<PackageProgress> progress,
            CancellationToken cancellationToken)
        {
            ReportProgress(progress, 40, "Updating nuspec file");

            NuspecManager nuspecManager = new NuspecManager();
            await nuspecManager.PatchNuspecFile(options);
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(progress, 60, "Creating nuget package");

            PackageMaker packageMaker = new PackageMaker();
            await packageMaker.CreateNugetPackage(options);
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(progress, 80, "Publishing package");

            await packageMaker.PublishNugetPackage(options);
            cancellationToken.ThrowIfCancellationRequested();

            ReportProgress(progress, 100, "Package published!");
        }
    }
}
