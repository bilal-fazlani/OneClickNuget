using System;
using Xunit;

namespace OneClickNuget
{
    public class Tests
    {

        /// retreive nuspec file                --
        /// de serialize nuspec file            -- 
        /// get version number from nuspec      --
        /// ask user for version number and other properties of package XX
        /// update nuspec object via UI         XX
        /// serialize nuspec file again         --
        /// update assembly info for version number --
        /// msbuild entire solution             --
        /// run unit tests                      XX
        /// create nuget package using nuspec file with updated dll files --
        /// publish nuget package               XX
        [Fact]
        public void Process()
        {
            #region hardcodings

            //hardcodings
            string csprojPath =
                @"C:\Users\bilalmf\Source\Repos\tracker-enabled-dbcontext\TrackerEnabledDbContext\TrackerEnabledDbContext.csproj";
            string targetPackageVersion = "3.9.0.0";
            string releaseNotes = "v 3.8" + Environment.NewLine +
                "added more functionality" + Environment.NewLine;
            //hardcodings

            #endregion

            var publisher = new NugetPackagePublisher(csprojPath);
            //publisher.Publish(targetPackageVersion, releaseNotes);
        }

        
    }
}
