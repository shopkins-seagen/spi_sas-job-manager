using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using System.Diagnostics;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;


namespace ServerManager.Api.Service
{
    public class Launcher
    {
        private ClientArgs _args;
        private Regex _statusRgx;

        public Launcher(ClientArgs args)
        {
            Log = new List<LogEntry>();
            _args = args;
            _statusRgx = new Regex(@"overall\s*status\s*'(Clean|Notice|Note|Warning|Error)'$", RegexOptions.IgnoreCase);
        }
        public List<LogEntry> Log { get; set; }
        public async Task RunBatCmd()
        {

            var result = new List<string>();
            await Task.Run(() =>
            {

                try
                {
                    var psi = new ProcessStartInfo("cmd", $"/c \"{_args.Name}\"")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_args.Name),
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true
                    };


                    Process proc = new Process() { StartInfo = psi };
                    proc.Start();

                    SendNotification(false, $"'{Path.GetFileName(_args.Name)}' started at {DateTime.Now.ToString("HH:mm")} on SEASASAP01T as PID {proc.Id}.\nContact SPI if you need support with this job prior to successful completion.");

                    proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            result.Add(e.Data);
                            Log.Add(new LogEntry(e.Data, IssueType.Info, "Launch"));
                        }
                    };
                    proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                        {
                            result.Add(e.Data);
                            Log.Add(new LogEntry(e.Data, IssueType.Error, "Launch"));
                        }
                    };

                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Add(new LogEntry(ex.Message, IssueType.Error, "Launch"));
                    result.Add(ex.Message);
                    SendNotification(true, $"ERROR: {Path.GetFileName(_args.Name)} failed",result);

                    return;
                }

                var status = GetOverallStatus(result).ToString() ?? string.Empty;
                SendNotification(true, $"{Path.GetFileName(_args.Name)} completed with overall status of {status}", result);
            });

        }

        private async Task SendNotification(bool isComplete, string msg,List<string> response=null)
        {
            var content = new StringBuilder();
            content.AppendLine("<style>ul {list-style: none; margin-left: 0; padding-left: 1em; text-indent: -1em;}</style>");

            content.AppendLine($"{msg}<hr/ >");
            if (response!= null)
            {
                content.AppendLine("<h4>Runtime Log</h4>");
                content.AppendLine("<ul>");
                foreach(var v in response)
                {
                    content.AppendLine($"<li style=\"color:slateblue;font-size:smaller;\">{v}</li>");
                }
                content.AppendLine("</ul>");
            }


            var smtp = new SmtpClient(_args.Smtp);
            var mail = new MailMessage()
            {
                IsBodyHtml = true,
                Subject = $"SAS Job Manager Bat Launcher '{Path.GetFileName(_args.Name)}' {(isComplete ? "completed" : "started")} at {DateTime.Now.ToString("HH:mm")}",
                Body = $"{content.ToString()}",
                From = new MailAddress($"{_args.User}@seagen.com")
            };
            mail.To.Add($"{_args.User}@seagen.com");
            await smtp.SendMailAsync(mail);

        }

        private SasFindingType? GetOverallStatus(List<string> runMsgs)
        {
            var statuses = new List<SasFindingType>();
            foreach (var m in runMsgs)
            {
                if (_statusRgx.IsMatch(m))
                {
                    var match = _statusRgx.Match(m);
                    if (match.Groups.Count >= 2)
                    {
                        if (Enum.TryParse<SasFindingType>(match.Groups[1].Value, out SasFindingType status))
                        {
                            statuses.Add(status);
                        }
                    }
                }
            }
            return statuses.Count > 0 ? statuses.Max() : null;
        }

        public bool ValidateArgs()
        {
            if (!File.Exists(_args.Name))
            {
                Log.Add(new LogEntry($"The Bat file '{_args.Name}' could not be found",IssueType.Fatal,"Launcher.ValidateArgs"));
                return false;
            }
            if (!ActiveDirectoryService.DoesUserHaveWrite(_args.User, Path.GetDirectoryName(_args.Name)))
            {
                Log.Add(new LogEntry($"The user '{_args.User}' does not have write on the folder containing the Batch file", IssueType.Fatal, "Launcher.ValidateArgs"));
                return false;
            }
            return true;

        }
    }
}
