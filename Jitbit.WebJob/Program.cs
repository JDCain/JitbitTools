using System;
using Microsoft.Azure.WebJobs;

namespace Jitbit.WebJob
{
    class Program
    {
        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("AzureWebJobsDashboard", "DefaultEndpointsProtocol=https;AccountName=ecotstorage00;AccountKey=97DgAnNrcdzEx/c/1vX3TIlVuBSwfWR3V+SwsAb2u7mThu/qhc9SAgcdvSlmYkPom+WFK4Roh45ayNqBjde5nA==;EndpointSuffix=core.windows.net");
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "DefaultEndpointsProtocol=https;AccountName=ecotstorage00;AccountKey=97DgAnNrcdzEx/c/1vX3TIlVuBSwfWR3V+SwsAb2u7mThu/qhc9SAgcdvSlmYkPom+WFK4Roh45ayNqBjde5nA==;EndpointSuffix=core.windows.net");
            var config = new JobHostConfiguration();

            //if (config.IsDevelopment)
            //{
            //    config.UseDevelopmentSettings();
            //}
            config.UseTimers();
            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
