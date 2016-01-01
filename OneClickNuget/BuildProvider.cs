using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;
using Microsoft.Build.Logging;

namespace OneClickNuget
{
    public class BuildProvider
    {
        private readonly string _csprojPath;

        public BuildProvider(string csprojPath)
        {
            _csprojPath = csprojPath;
        }

        public async Task Build()
        {
            await Task.Run(() =>
            {
                var globalProperties = new Dictionary<string, string>();
                var buildRequest = new BuildRequestData(_csprojPath, globalProperties, null, new[] { "Build" }, null);
                var projectColletion = new ProjectCollection();

                var buildParameters = new BuildParameters(projectColletion);

                var loggers = new List<ILogger> { new FileLogger() };
                buildParameters.Loggers = loggers;

                var result = BuildManager.DefaultBuildManager
                    .Build(buildParameters, buildRequest);

                if (result.OverallResult != BuildResultCode.Success)
                {
                    throw new Exception("build failed. see msbuild.log for more details");
                }
            });
        }

        public void RunUnitTests()
        {
            throw new NotImplementedException();
        }
    }
}
