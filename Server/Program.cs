using System;
using System.Diagnostics;
using Grains;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

namespace Server
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var container = new Container();
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(config => config.AddCommandLine(args))
                .ConfigureAppConfiguration((hostBuilderContext, config) =>
                {
                    var environment = hostBuilderContext.HostingEnvironment.EnvironmentName;

                    config.AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{environment}.json", true, true);

                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostBuilderContext, collection) =>
                    ConfigureServices(hostBuilderContext, collection, container))
                .UseWindowsService()
                .UseOrleans((hostBuilderContext, siloBuilder) =>
                {
                    var configuration = hostBuilderContext.Configuration;
                    siloBuilder.Configure<GrainCollectionOptions>(options =>
                        {
                            options.CollectionAge = TimeSpan.FromMinutes(10);
                        })
                        .Configure<ClusterOptions>(options =>
                        {
                            options.ClusterId = "MyCluster";
                            options.ServiceId = "TestCluster";
                        })
                        .UseAdoNetClustering(options =>
                        {
                            options.Invariant = "Npgsql";
                            options.ConnectionString =
                                "User ID=test_user;Password=test_password;Host=test_db;Port=5432;Database=orleans_test;Pooling=false;Enlist=false;SearchPath=orleans_test";
                        })
                        .AddAdoNetGrainStorageAsDefault(options =>
                        {
                            options.Invariant = "Npgsql";
                            options.ConnectionString =
                                "User ID=test_user;Password=test_password;Host=test_db;Port=5432;Database=orleans_test;Pooling=false;Enlist=false;SearchPath=orleans_test";
                            options.UseJsonFormat = true;
                            options.UseFullAssemblyNames = true;
                        })
                        .ConfigureEndpoints(configuration.GetValue<int>("silo"), configuration.GetValue<int>("gateway"))
                        .ConfigureLogging(builder =>
                        {
                            builder.AddConfiguration(configuration.GetSection("Logging"));
                            builder.AddNLog(configuration);
                            if (configuration.GetValue<bool>("logConsole"))
                            {
                                builder.AddConsole();
                            }
                        })
                        .ConfigureApplicationParts(manager =>
                            manager.AddApplicationPart(typeof(LongRunningTask).Assembly).WithReferences());
                });

            var host = hostBuilder.Build();
            host.UseSimpleInjector(container);
            container.Verify();

            var diagnosticResults = Analyzer.Analyze(container);
            Debug.Assert(diagnosticResults.Length == 0, "diagnosticResults.Length == 0");

            host.Run();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services, Container container)
        {
            services.AddSimpleInjector(container,
                options =>
                {
                    options.AddLogging();
                });

            var configuration = context.Configuration;

            services.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddNLog(configuration);
                if (configuration.GetValue<bool>("logConsole"))
                {
                    builder.AddConsole();
                }
            });
        }
    }
}
