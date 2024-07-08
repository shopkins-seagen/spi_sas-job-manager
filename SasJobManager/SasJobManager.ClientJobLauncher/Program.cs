using Microsoft.Extensions.Configuration;
using RestSharp;
using SasJobManager.Domain.Models;
using ServerManager.Api.Models;
using System.Net;

namespace SasJobManager.ClientJobLauncher
{
    internal class Program
    {
        private static IConfigurationRoot _cfg;
        private static Repository _repo;


        static void Main(string[] args)
        {

            try
            {
                LoadConfig();

                var cli = new ClientArgs()
                {
                    Name = args[0],
                    User = Environment.UserName
                };
                Console.WriteLine($"Launching {cli.Name}...");
                Launch(cli);

                Console.WriteLine($"\n'{cli.Name}' successfully launched");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nERROR: {ex.Message}");
            }

            Console.WriteLine("\nPress any key to continue");
            Console.ReadKey();
            Environment.Exit(0);

        }

        private static async void Launch(ClientArgs cli)
        {
            var client = new RestClient(_cfg["url_base"]);
            var request = new RestRequest(_cfg["url_end"], Method.Post);
            request.AddJsonBody(cli);
            request.RequestFormat = DataFormat.Json;
            var response = await client.ExecutePostAsync(request);


        }

        private static void LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_cjl.json", true, true);
            _cfg = builder.Build();
        }
    }
}