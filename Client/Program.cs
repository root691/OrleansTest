using System;
using System.Threading.Tasks;
using Grains;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;

namespace Client
{
    internal static class Program
    {
        private static IGrainWithStore _grain;

        private static async Task Main()
        {
            Console.Write("Waiting to start...");
            Console.ReadLine();

            using var client = await CreateClient();
            Console.WriteLine("Client successfully connected to cluster");

            string input;

            do
            {
                await client.DoWork();

                Console.Write("Work is done");
                input = Console.ReadLine();
            } while (input != "exit");
        }

        private static async Task<IClusterClient> CreateClient()
        {
            var client = new ClientBuilder()
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
                .ConfigureApplicationParts(manager => manager.AddApplicationPart(typeof(ILongRunningTask).Assembly))
                .Build();

            await client.Connect();
            _grain = client.GetGrain<IGrainWithStore>("test-grain");
            return client;
        }

        private static async Task DoWork(this IClusterClient client)
        {
            //var grain = client.GetGrain<ILongRunningTask>(Guid.NewGuid());
            //var response = await grain.Execute("Test");

            var response = await _grain.IncrementAndReturn();

            Console.WriteLine($"Get response from server in {DateTime.Now:HH:mm:ss.fff}: {response}");
        }
    }
}
