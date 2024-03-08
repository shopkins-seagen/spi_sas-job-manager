    using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using SasJobManager.Domain;
using SasJobManager.Domain.Models;
using SasJobManager.LaunchScheduledJobs.Service;
using SasJobManager.Lib.Models;
using ServerManager.Api.Models;
using System.Net.Http.Json;

namespace SasJobManager.LaunchScheduledJobs
{
    public class Program
    {
        private static IConfigurationRoot _cfg;
        private static Repository _repo;
        private static ActiveDirectoryService _adService = new ActiveDirectoryService("sg.seagen.com");
        private static RunLogger _logger;
        private static Dictionary<string, string> _apiMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static string _defaultAccessGroup;

        static void Main(string[] args)
        {
            LoadConfig();

            _repo = new Repository(new Domain.SasContext());

            var jobs = GetScheduledJobs(DateTime.Now.DayOfWeek, DateTime.Now.Hour);

            _logger = new RunLogger(_cfg["run_log"]);
            _logger.Record(jobs.Count);

            if (jobs.Count > 0)
            {
                var jobArgs = jobs.Select(x => new JobArgs()
                {
                    Name = x.Description,
                    BatFile = x.Driver,
                    Id = x.Id,
                    IsRecurring = x.IsRecurring,
                    IsDefaultSecurity = !x.IsCustomSecurity,
                    User = x.CreatedBy,
                    SecurityGroup = x.IsCustomSecurity ? x.CustomSecurityGroup : _cfg["default_group"],
                    IsValid=true
                }).ToList();

                // disable non-recurring jobs
                foreach (var ja in jobArgs)
                {
                    if (!ja.IsRecurring)
                    {
                        var job = jobs.FirstOrDefault(x => x.Id == ja.Id);
                        if (job != null)
                        {
                            ja.SchedulerMsg = $"'{ja.Name}' will be disabled after this run as Recurring was not checked in the scheduler";
                            job.IsEnabled = false;
                            var dbMsg = _repo.UpdateJob(job);
                        }
                    }
                }

                try
                {
                    foreach(var j in jobArgs)
                    {
                        var client = new RestClient(_apiMap.GetValueOrDefault(j.SecurityGroup));
                        var request = new RestRequest("api/runbat", Method.Post);
                        request.AddJsonBody(j);
                        request.RequestFormat = DataFormat.Json;
                        var webresponse = client.ExecutePostAsync(request).GetAwaiter().GetResult();

                        var run = CreateRunData(j,DateTime.Now, webresponse);
                        _repo.RecordSchedulerDetails(run);
                    }
                }
                catch(Exception ex)
                {
                    _logger.Record(jobs.Count, $"ERROR: {ex.StackTrace}");
                }
            }
        }

        private static SchedulerRun CreateRunData(JobArgs job, DateTime now, RestResponse response)
        {
            var run = new SchedulerRun()
            {
                LaunchedAt = now,
                IsOk = response.IsSuccessful,
                SecurityGroup = job.SecurityGroup,
                Message = response.Content ?? string.Empty,
                Name = job.Name,
                JobId = job.Id,
                DriverLoc = job.BatFile,
                Owner = job.User,
            };

            return run;
        }

        public static List<Job> GetScheduledJobs(DayOfWeek day, int hour)
        {
            var s = new ScheduledJob(day, hour);
            s.GetJobs();
            return s.Jobs;
        }

        public static void LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_launcher.json", true, true);
            _cfg = builder.Build();
            using (var c = new SasContext())
            {
                foreach (var v in c.UrlMap)
                {
                    _apiMap[v.SecurityGroup] = v.Url;
                }
                _defaultAccessGroup = c.UrlMap.Where(x => x.IsDefault ?? false == true).FirstOrDefault().SecurityGroup;
            }
        }
    }
}



