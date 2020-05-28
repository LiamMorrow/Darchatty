using System;
using System.Threading.Tasks;
using Darchatty.Gateway.Auth;
using Darchatty.Gateway.Controllers;
using Darchatty.Gateway.Hubs;
using Darchatty.Orleans.GrainInterfaces;
using Darchatty.Web.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrgnalR.SignalR;
using Orleans;
using Orleans.Clustering.Kubernetes;
using Orleans.Configuration;

namespace Darchatty.Gateway
{
    public class Startup
    {
        private readonly IConfiguration configuration;

        public Startup(IConfiguration config)
        {
            configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(serviceProvider =>
            {
                var clientBuilder = new ClientBuilder()
                    // Clustering information
                    .Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "darchatty-cluster";
                        options.ServiceId = "darchatty";
                    })
                    .UseOrgnalR()
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(IChatGrain).Assembly))
                    .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(ChatMessageDto).Assembly).WithReferences())
                    .ConfigureLogging(x => x.AddConsole());
                if (configuration.GetValue("OrleansClusteringMode", "local") == "local")
                {
                    clientBuilder.UseLocalhostClustering();
                }
                else
                {
                    clientBuilder.UseKubeGatewayListProvider();
                }

                return clientBuilder.Build();
            });
            services.AddSignalR()
                .UseOrgnalR()
                .AddNewtonsoftJsonProtocol();

            services.AddCors(options => options.AddPolicy("CorsPolicy", builder =>
            {
                builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(s => true)
                    .AllowCredentials();
            }));

            services.AddAuthentication("Basic")
                    .AddScheme<BasicAuthenticationOptions, BasicAuthenticationHandler>("Basic", null);
            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
#pragma warning disable VSTHRD002
            var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
            var lifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
            app.ApplicationServices.GetRequiredService<IClusterClient>().Connect(async x =>
            {
                logger.CreateLogger("Silo connect").LogInformation("Could not connect to orleans silo!  Retrying in 5 seconds.");
                if (lifetime.ApplicationStopping.IsCancellationRequested)
                {
                    return false;
                }

                await Task.Delay(5000);
                return true;
            }).GetAwaiter().GetResult();

            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHub<ChatHub>("/hub/chat");
                endpoints.MapHub<AuthHub>("/hub/auth");
                endpoints.MapControllers();
            });
        }
    }
}
