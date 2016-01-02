using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Logging;
using OneClickNuget.Data;

namespace OneClickNuget
{
    public class ProjectBuilder
    {
        public async Task Build(PublishOptions options)
        {
            await Task.Run(() =>
            {
                var project = 
                ProjectCollection.GlobalProjectCollection.LoadedProjects.FirstOrDefault(
                    x=>x.FullPath.ToLower() == options.ProjectFilePath.ToLower()) ?? 
                new Project(options.ProjectFilePath);

                project.SetProperty("Configuration", "Release");
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
