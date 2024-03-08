using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using Newtonsoft.Json;
using RestSharp;
using SasJobManager.Domain;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using ServerManager.Api.Service;
using System.Text.RegularExpressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RunClientController : ControllerBase
    {
        private SasContext _context;
        private IConfiguration _cfg;
        private string _defaultSecGroup;
        private string _name;
        private Dictionary<string, string> _apiMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);


        public RunClientController(IConfiguration cfg, SasContext context)
        {
            _cfg = cfg;
            FileSystemManager.MapDrive(cfg);
            _context = context;
            LoadUrlMap();
        }

        [HttpPost]
        public string Get(object rawArgs)
        {
            var jsonResponse = new Dictionary<string, string>();
            var args = new ClientArgs();
            try
            {
                args = JsonConvert.DeserializeObject<ClientArgs>(rawArgs.ToString());

            }
            catch(Exception ex)
            {
                jsonResponse["ERROR:"]=ex.Message;
                return JsonConvert.SerializeObject(jsonResponse);
            }

            var job = GetJsonBody(args.Name);
           
            if (job == null)
            {
                jsonResponse["ERROR:"] = $"The job '{args.Name}' could not be found. Confirm Job Name";
                return JsonConvert.SerializeObject(jsonResponse);
            }

            var isValidKvp = CheckUserIsValid(job,args.User);
            if (!isValidKvp.Key)
            {
                jsonResponse["ERROR:"] = isValidKvp.Value;
                return JsonConvert.SerializeObject(jsonResponse);
            }

            RestResponse? response = SubmitRequest(job);
            if (response != null)
            {
                jsonResponse[response.StatusCode == System.Net.HttpStatusCode.OK ? "NOTICE:" : "WARNING"] = response.Content;
            }
            else
            {
                jsonResponse["ERROR:"] = "RunClient service unavailable. Contact SPI for support";
            }
            return JsonConvert.SerializeObject(jsonResponse);
        }


        private RestResponse? SubmitRequest(JobArgs job)
        {
            var client = new RestClient(_apiMap.GetValueOrDefault(job.SecurityGroup));
            var request = new RestRequest("api/runbat", Method.Post);
            request.AddJsonBody(job);
            request.RequestFormat = DataFormat.Json;
            return client.ExecutePostAsync(request).GetAwaiter().GetResult();
        }

        private void LoadUrlMap()
        {
            foreach (var v in _context.UrlMap)
            {
                _apiMap[v.SecurityGroup] = v.Url;
            }
            _defaultSecGroup = _context.UrlMap.Where(x => x.IsDefault ?? false).FirstOrDefault().SecurityGroup;
        }

        private JobArgs? GetJsonBody(string name)
        {

            var job = _context.Jobs.FirstOrDefault(x => x.Description.ToLower() == name.ToLower());

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

        public KeyValuePair<bool,string> CheckUserIsValid(JobArgs job,string user)
        {
            if (string.IsNullOrEmpty(user))
            {
                return new KeyValuePair<bool, string>(false, "Invalid argument to the API. Contact SPI for support");
            }
            var findNotifyFlag = new Regex("(-n)\\s*(\\w+)", RegexOptions.IgnoreCase);
            var findRecipientsFlag = new Regex("-u\\s*(.*?)(\\^|-|\\n)", RegexOptions.IgnoreCase);
      

            bool hasNotify = false;
            var v = System.IO.File.ReadAllText(job.BatFile);

            var match = findNotifyFlag.Match(v);
            if (match.Success)
            {
                if (match.Groups.Count >= 2)
                {
                    if (bool.TryParse(match.Groups[2].Value, out bool notify))
                    {
                        hasNotify = notify;
                    }
                }
            }
            if (!hasNotify)
            {
                return new KeyValuePair<bool, string>(false,"Notifications are not enabled in the .BAT file. To enable on-demand execution, " +
                                      "set -n true and specify the recipients allowed to run this program in -u");
            }
            if (hasNotify)
            {
                var hasRecipients = false;

                var matchUsers = findRecipientsFlag.Match(v);
                if (matchUsers.Success)
                {
                    if (matchUsers.Groups.Count >= 2)
                    {
                        var recipients = matchUsers.Groups[1].Value.Split(new char[] { ((Char)32) }, StringSplitOptions.RemoveEmptyEntries);
                        if (recipients.Length > 0)
                        {
                            hasRecipients = true;
                            if (!recipients.Any(x => x.Trim().ToLower() == user.ToLower()))
                            {
                                return new KeyValuePair<bool,string>(false,$"Only the following users are able to run this program interactively: " +
                                                     $"{string.Join(", ", recipients)}. " +
                                                     $"'{user}' must be added to -u in the .bat file to run this job on-demand.");       
                            }
                        }
                    }
                }

                if (!hasRecipients)
                {
                    return new KeyValuePair<bool, string>(false,"The -u parameter is not specified or has no arguments in the .bat file. To enable on-demand execution, " +
                                          "specify recipients allowed to run this program in -u");
                }
            }

            return new KeyValuePair<bool, string>(true, string.Empty);
        }
    }
}
