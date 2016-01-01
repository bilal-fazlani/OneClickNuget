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
        public async Task Build(PublishOptions options)
        {
            await Task.Run(() =>
            {
                var project = new Project(options.ProjectFilePath);
                project.SetGlobalProperty("Configuration", "Release");
                var success = project.Build(new FileLogger());

                if (!success)
                {
                    throw new Exception("build failed. see msbuild.log for more details");
                }
            });
        }

        public async Task RunUnitTests()
        {
            await Task.Delay(new TimeSpan(0, 0, 3));
        }
    }
}
