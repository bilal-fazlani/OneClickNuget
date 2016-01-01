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
                var project = new Project(_csprojPath);
                project.SetGlobalProperty("Configuration", "Release");
                var success = project.Build(new FileLogger());

                if (!success)
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
