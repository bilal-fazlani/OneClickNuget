using System;
using System.Collections.Generic;
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

        public void Build()
        {
            var globalProperties = new Dictionary<string, string>();
            var buildRequest = new BuildRequestData(_csprojPath, globalProperties, null, new[] { "Build" }, null);
            var pc = new ProjectCollection();
            var bp = new BuildParameters(pc);

            var loggers = new List<ILogger> {new FileLogger()};
            bp.Loggers = loggers;

            var result = BuildManager.DefaultBuildManager
                .Build(bp, buildRequest);

            if (result.OverallResult != BuildResultCode.Success)
            {
                throw new Exception("build failed. see msbuild.log for more details");
            }
        }

        public void RunUnitTests()
        {
            throw new NotImplementedException();
        }
    }
}
