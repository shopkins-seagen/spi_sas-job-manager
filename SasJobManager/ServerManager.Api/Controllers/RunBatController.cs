using Microsoft.AspNetCore.Mvc;
using SasJobManager.Lib;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using SasJobManager.Domain;
using ServerManager.Api.Service;
using SasJobManager.Domain.Models;


namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RunBatController : ControllerBase
    {
        private IConfiguration _cfg;
        private SasContext _context;
        private JobArgs _args;
        private List<LogEntry> _issues;

        public RunBatController(IConfiguration cfg,SasContext context)
        {
            _cfg = cfg; 
            _issues = new List<LogEntry>();
            FileSystemManager.MapDrive(cfg);
            _context = context;
        }

        [HttpPost]
        public async Task<string> Post(JobArgs args)
        {
            _args = args;
            try
            {
                var cmd = new Cmd(args, _context);
                ValidateJob();

                if (_issues.Any(x=>x.IssueType==IssueType.Fatal))
                {
                    return $"ERROR: {_issues.FirstOrDefault().Msg}";
                }

                await Task.Run(async () =>
                {
                    
                    cmd.RunBatCmd();
                   
                });
                
            }
            catch(Exception ex)
            {

                return ex.Message;
            }

            return $"Job started at {DateTime.Now.ToString("HH:mm")} on {DateTime.Now.ToString("ddMMMyyyy")}";
        }

        private void ValidateJob()
        {
            var errors = new List<LogEntry>();
            // Check ID is valid
            if (!_context.Jobs.Any(x=>x.Id == _args.Id))
            {
                _issues.Add(new LogEntry($"The job '{_args.Name}' (ID={_args.Id}) could not be found in the database.Contact SPI", IssueType.Fatal,
                    "ValidateJob"));
                return;
            }
            if (!System.IO.File.Exists(_args.BatFile))
            {
                _issues.Add(new LogEntry($"The .bat file '{_args.BatFile}' for job '{_args.Name}' could not be found. Update the scheduler for this job.", IssueType.Fatal,
                    "ValidateJob"));
                return;
            }
            // SEcurity group is assigned to the folder where the bat file resides
            if (! ActiveDirectoryService.DoesFolderHaveGroup(System.IO.Path.GetDirectoryName(_args.BatFile), _args.SecurityGroup))
            {
                _issues.Add(new LogEntry($"The location of the .bat file '{System.IO.Path.GetDirectoryName(_args.BatFile)}' for job '{_args.Name}' does not have the AD group '{_args.SecurityGroup}' applied.", IssueType.Fatal,
                    "ValidateJob"));
                return;
            }

        }
    }
}
