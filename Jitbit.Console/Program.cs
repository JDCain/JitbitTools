using System;
using Microsoft.Extensions.Configuration;
using JitBit.Core;

namespace JitBit.Console
{
    public class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("environment");

            var builder = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory);

            if (environment == "production")
            {
                builder.AddJsonFile(@"appsettings.json", optional: false, reloadOnChange: true);
            }
            else
            {
                builder.AddUserSecrets<Program>();
            }
            Configuration = builder.Build();
            Application.MainAsync(Configuration["Jitbit_Host"], Configuration["Jitbit_Cred"], Configuration["Url0"], Configuration["Url1"]).Wait();
        }

       
    }
}
