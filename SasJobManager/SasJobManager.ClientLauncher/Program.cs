using RestSharp;
using SasJobManager.Domain;
using ServerManager.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Management.Automation;
using System.Text.RegularExpressions;

namespace SasJobManager.ClientLauncher
{
    internal class Program
    {
        private static string _defaultSecGroup;
        private static Dictionary<string, string> _apiMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public static void Main(string[] args)
        {
            LoadConfig();

            // Missing argument
            if (args.Length == 0)
            {
                Console.Write($"ERROR: missing required argument: Job Name!");
                return;
            }

            // Incorrect argument
            JobArgs? jobArgs = GetJsonBody(args[0]);
            if (jobArgs == null)
            {
                Console.WriteLine($"The jobs '{args[0]}' could not be found. Confirm Job Name");
                return;
            }

            var client = new RestClient(_apiMap.GetValueOrDefault(jobArgs.SecurityGroup));
            var request = new RestRequest("api/runbat", Method.Post);
            request.AddJsonBody(jobArgs);
            request.RequestFormat = DataFormat.Json;
            var response = client.ExecutePostAsync(request).GetAwaiter().GetResult();
            var msg = string.Empty;
            if (response != null)
            {
                var fixMsg = new Regex("Scheduler",RegexOptions.IgnoreCase);
                msg = fixMsg.Replace(response.Content, "Job");
                    Console.WriteLine($"{(response.StatusCode == System.Net.HttpStatusCode.OK ? "NOTICE:" : "WARNING")}{msg}");
            }
        }

        private static JobArgs? GetJsonBody(string name)
        {
            using (var c = new SasContext())
            {
                var job = c.Jobs.FirstOrDefault(x => x.Description.ToLower() == name.ToLower());

                if (job == null)
                    return null;

                var jobArgs = new JobArgs()
                {
                    Name = job.Description,
                    BatFile = job.Driver,
                    IsDefaultSecurity = !job.IsCustomSecurity,
                    SecurityGroup = job.IsCustomSecurity ? job.CustomSecurityGroup : _defaultSecGroup,
                    IsRecurring = job.IsRecurring,
                    IsValid = true,
                    User = Environment.UserName,
                    Id = job.Id
                };

                return jobArgs;
            }
        }

        private static void LoadConfig()
        {
            using (var c = new SasContext())
            {
                foreach (var v in c.UrlMap)
                {
                    _apiMap[v.SecurityGroup] = v.Url;
                }
                _defaultSecGroup = c.UrlMap.Where(x => x.IsDefault ?? false).FirstOrDefault().SecurityGroup;
            }
        }
    }
}
