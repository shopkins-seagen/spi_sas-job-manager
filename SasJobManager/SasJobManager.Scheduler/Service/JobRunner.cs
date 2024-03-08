using Microsoft.Extensions.Configuration;
using RestSharp;
using SasJobManager.Cli.Models;
using SasJobManager.Domain;
using SasJobManager.Lib.Models;
using SasJobManager.Scheduler.ViewModels;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telerik.Windows.Documents.Spreadsheet.Expressions.Functions;

namespace SasJobManager.Scheduler.Service
{
    public class JobRunner
    {
        private JobView _job;
        private JobArgs _args;
        private string _defaultSecGroup;
        private bool _isValid;
        private Regex _findNotifyFlag;
        private Regex _findRecipientsFlag;

        private IConfigurationRoot _cfg;
        private static Dictionary<string, string> _apiMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public JobRunner(JobView job, IConfigurationRoot cfg)
        {
            _job = job;
            _cfg = cfg;
            Log = new List<LogEntry>();
            _findNotifyFlag = new Regex("-n\\s*(true|false)", RegexOptions.IgnoreCase);
            _findRecipientsFlag = new Regex("-u\\s*(.*?)(\\^|-|\\n)", RegexOptions.IgnoreCase);

            ParseConfig();
            ParseArgs();
        }
        public List<LogEntry> Log { get; set; }

        private void ParseArgs()
        {
            try
            {
                _args = new JobArgs()
                {
                    Name = _job.Description,
                    BatFile = _job.Driver,
                    Id = _job.Id,
                    IsRecurring = _job.IsRecurring,
                    IsDefaultSecurity = !_job.IsCustomSecurity,
                    User = _job.CreatedBy,
                    SecurityGroup = _job.IsCustomSecurity ? _job.CustomSecurityGroup.Name : _defaultSecGroup,
                    IsValid = true
                };
                if (File.Exists(_job.Driver))
                {
                    _isValid = true;
                }
                else
                {
                    Log.Add(new LogEntry($"The .BAT file '{_job.Driver}' could not be located", IssueType.Fatal, "JobRunner.CheckBat"));
                }

            }
            catch (Exception ex)
            {
                _isValid = false;
            }

        }

        public void CheckUserIsNotified()
        {
            bool hasNotify = false;
            var v = File.ReadAllText(_job.Driver);

            var match = _findNotifyFlag.Match(v);
            if (match.Success)
            {
                if (match.Groups.Count >= 2)
                {
                    if (bool.TryParse(match.Groups[1].Value, out bool notify))
                    {
                        hasNotify = notify;
                    }
                }

            }
            if (!hasNotify)
            {
                Log.Add(new LogEntry("Notifications are not enabled in the .BAT file. To enable on-demand execution,  " +
                                      " set -n true and specify" +
                                      " recipients allowed to run this program in -u", IssueType.Fatal, "JobRunner"));
            }
            if (hasNotify)
            {
                var hasRecipients = false;

                var matchUsers = _findRecipientsFlag.Match(v);
                if (matchUsers.Success)
                {
                    if (matchUsers.Groups.Count >= 2)
                    {
                        var recipients = matchUsers.Groups[1].Value.Split(new char[] { ((Char)32) }, StringSplitOptions.RemoveEmptyEntries);
                        if (recipients.Length > 0)
                        {
                            hasRecipients = true;
                            if (!recipients.Any(x => x.Trim().ToLower() == Environment.UserName.ToLower()))
                            {
                                Log.Add(new LogEntry($"Only the following users are able to run this program interactively: " +
                                                     $"{string.Join(", ",recipients)}\n\n" +
                                                     $"'{Environment.UserName}' must be added to -u in the .bat file to run this job on-demand.", 
                                                     IssueType.Fatal, "JobRunner"));
                            }
                        }
                    }
                }

                if (!hasRecipients)
                {
                    Log.Add(new LogEntry("The -u parameter is not specified or has no arguments in the .bat file. To enable on-demand execution, " +
                                          "specify recipients allowed to run this program in -u", IssueType.Fatal, "JobRunner"));
                }
            }
        }

        public async Task<string> Submit()
        {
            var client = new RestClient(_apiMap.GetValueOrDefault(_args.SecurityGroup));
            var request = new RestRequest("api/runbat", Method.Post);
            request.AddJsonBody(_args);
            request.RequestFormat = DataFormat.Json;
            var webresponse = await client.ExecutePostAsync(request);

            if (webresponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return $"Job '{_job.Description}' successfully submitted.";
            }
            else
            {
                var err = string.IsNullOrWhiteSpace(webresponse.Content ) ? webresponse.ErrorException?.Message ?? "Issue encountered submitting job. Contact SPI" 
                    : webresponse.Content;
                return $"ERROR: {err}";
            }
        }

        private void ParseConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_scheduler.json",false,true);
            _cfg = builder.Build();
            SetSecGroups();
        }

        private void SetSecGroups()
        {
            using (var c = new SasContext())
            {
                foreach (var v in c.UrlMap)
                {
                    _apiMap[v.SecurityGroup] = v.Url;
                }
                _defaultSecGroup = c.UrlMap.Where(x => x.IsDefault ?? false == true).FirstOrDefault().SecurityGroup;
            }
        }
    }
}
