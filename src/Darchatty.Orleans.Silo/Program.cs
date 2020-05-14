using System;
using System.Threading.Tasks;
using Darchatty.Orleans.Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrgnalR.Silo;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Darchatty.Orleans.Silo
{
    public class Program
    {
        public static Task Main(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((host, builder) =>
                {
                    builder
                        .SetBasePath(Environment.CurrentDirectory)
                        .AddJsonFile("appsettings.Development.json", optional: true)
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddEnvironmentVariables();
                })
                .UseOrleans((context, siloBuilder) =>
                {
                    siloBuilder
                        .Configure<ProcessExitHandlingOptions>(options => options.FastKillOnProcessExit = false)
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "darchatty-cluster";
                            options.ServiceId = "darchatty";
                        })
                        .AddOrgnalRWithMemoryGrainStorage()
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChatGrain).Assembly).WithReferences())
                        .ConfigureLogging(logging => { logging.AddConsole().SetMinimumLevel(LogLevel.Information); });
                    ConfigureClustering(siloBuilder, context.Configuration);
                    ConfigureStorage(siloBuilder, context.Configuration);
                })
                .RunConsoleAsync();
        }

        private static void ConfigureStorage(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            if (configuration.GetValue("StorageMode", "memory") == "memory")
            {
                siloBuilder.AddMemoryGrainStorageAsDefault();
            }
        }

        private static void ConfigureClustering(ISiloBuilder siloBuilder, IConfiguration configuration)
        {
            if (configuration.GetValue("ClusterMode", "local") == "local")
            {
                siloBuilder.UseLocalhostClustering();
            }
            else
            {
                siloBuilder.UseKubeMembership();
            }
        }
    }
}
