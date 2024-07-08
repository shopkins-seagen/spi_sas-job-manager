using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Office.CustomUI;
using Markdig.Extensions.SmartyPants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using Newtonsoft.Json;
using SasJobManager.Domain;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using ServerManager.Api.Service;
using System;
using System.Net.Mail;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LaunchController : ControllerBase
    {
        private IConfiguration _cfg;
        private SasContext _context;
        private List<LogEntry> _issues;
        private ClientArgs _args;
        public LaunchController(IConfiguration cfg)
        {
            _cfg = cfg;
            _issues = new List<LogEntry>();
            FileSystemManager.MapDrive(cfg);
            _issues = new List<LogEntry>();

        }
        [HttpPost]
        public async Task Post(object raw)
        {

            try
            {
                var args = JsonConvert.DeserializeObject<ClientArgs>(raw?.ToString());

                if (args != null)
                {
                    args.Smtp = _cfg["smtp_server"];
                    _args = args;
                    var launcher = new Launcher(args);
                    if (!launcher.ValidateArgs())
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine("<em>An error was encountered during argument validation.</em><ul>");
                        foreach (var e in launcher.Log.Where(x => x.IssueType >= IssueType.Warning))
                        {
                            msg.AppendLine($"<li>{e.Msg}</li>");
                        }
                        msg.AppendLine($"</ul>");
                        try
                        {
                            await SendNotification(true, msg.ToString());
                        }
                        catch (Exception ex)
                        {
                            // just fail
                        }
                        return;
                    }
                    await launcher.RunBatCmd();

                }
            }
            catch (Exception ex)
            {
                await SendNotification(true, $"An error was encountered during execution: {ex.Message}");
            }
        }

        private async Task SendNotification(bool isComplete, string msg)
        {

            var smtp = new SmtpClient(_cfg["smtp_server"]);
            var mail = new MailMessage()
            {
                IsBodyHtml = true,
                Subject = $"SAS Job Manager Bat Launcher  '{Path.GetFileName(_args.Name)}' {(isComplete ? "completed" : "started")}",
                Body = $"{msg}",
                From = new MailAddress($"{_args.User}@seagen.com")
            };
            mail.To.Add($"{_args.User}@seagen.com");
            await smtp.SendMailAsync(mail);

        }
    }
}

