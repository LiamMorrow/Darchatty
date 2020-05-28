using System;
using System.Threading.Tasks;
using Darchatty.Data;
using Darchatty.Data.Model;
using Darchatty.Orleans.Grains;
using Darchatty.Orleans.Silo.Storage;
using Darchatty.Web.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrgnalR.Silo;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Storage;

namespace Darchatty.Orleans.Silo
{
    public class Program
    {
        public static Task Main(string[] args)
        => CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
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
                        .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChatMessageDto).Assembly).WithReferences())
                        .ConfigureLogging(logging => { logging.AddConsole().SetMinimumLevel(LogLevel.Information); });
                    ConfigureClustering(siloBuilder, context.Configuration);
                    ConfigureStorage(siloBuilder);
                })
                .ConfigureServices((host, services) =>
                {
                    services.AddDbContextPool<UserDbContext>(options =>
                        options.UseNpgsql(
                            host.Configuration.GetConnectionString("UserDbContext"),
                            assembly => assembly.MigrationsAssembly(typeof(UserDbContext).Assembly.FullName)));
                    services.Configure<IdentityOptions>(opts =>
                    {
                        opts.Password.RequireDigit = false;
                        opts.Password.RequiredLength = 1;
                        opts.Password.RequireLowercase = false;
                        opts.Password.RequireNonAlphanumeric = false;
                        opts.Password.RequireUppercase = false;
                    });
                    services.AddIdentity<UserDao, IdentityRole>()
                        .AddEntityFrameworkStores<UserDbContext>()
                        .AddUserManager<UserManager<UserDao>>();
                });

        private static void ConfigureStorage(ISiloBuilder siloBuilder)
        {
            siloBuilder.AddMemoryGrainStorageAsDefault();
            siloBuilder.ConfigureServices(services =>
            {
                services.AddSingletonNamedService<IGrainStorage, UserStateGrainStorage>("UserState");
            });
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
