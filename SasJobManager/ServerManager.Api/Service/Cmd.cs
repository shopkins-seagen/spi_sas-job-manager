using SasJobManager.Domain;
using SasJobManager.Domain.Models;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServerManager.Api.Service
{
    public class Cmd
    {
        private JobArgs _job;
        private Repository _repo;
        private Regex _statusRgx;


        public Cmd(JobArgs job, SasContext context)
        {
            _job = job;
            _repo = new Repository(new SasContext());
            _statusRgx = new Regex(@"overall\s*status\s*'(Clean|Notice|Note|Warning|Error)'$", RegexOptions.IgnoreCase);
            Log = new List<LogEntry>();
        }

        public List<LogEntry> Log { get; set; }

        public async void RunBatCmd()
        {
            await Task.Run(() =>
            {

                _job.Started = DateTime.Now;
                Log.Add(new LogEntry($"Started {_job.BatFile} @{_job.Started}", IssueType.Info, "RunBatCmd"));

                var result = new List<string>();
                try
                {
                    var psi = new ProcessStartInfo("cmd", $"/c \"{_job.BatFile}\"")
                    {
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(_job.BatFile),
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true
                    };

                    Process proc = new Process() { StartInfo = psi };
                    proc.Start();

                    proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
                   {
                            if (e.Data != null)
                            {
                                result.Add(e.Data);
                                Log.Add(new LogEntry(e.Data, IssueType.Info, "RunBatCmd"));
                            }
                        };
                    proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
                   {
                            if (e.Data != null)
                            {
                                result.Add(e.Data);
                                Log.Add(new LogEntry(e.Data, IssueType.Error, "RunBatCmd"));
                            }
                        };

                    proc.BeginOutputReadLine();
                    proc.BeginErrorReadLine();
                    proc.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Add(new LogEntry(ex.Message, IssueType.Error, "RunBatCmd"));
                    result.Add(ex.Message);
                }

                if (result.Count > 0)
                {
                    _job.RunMsgs.AddRange(result.Where(x => !string.IsNullOrEmpty(x)));
                }

                _job.Finished = DateTime.Now;
                var status = GetOverallStatus(_job.RunMsgs).ToString() ?? string.Empty;
                var response = _repo.RecordRunDetails(_job.Id, _job.RunMsgs, _job.Started, _job.Finished, status);
                Log.Add(new LogEntry(response.Msg, response.IsOk ? IssueType.Info : IssueType.Error, "RunBatCmd"));

                //using (var s = new StreamWriter(@"C:\inetpub\wwwroot\logs\logger.txt"))
                //{
                //    foreach (var v in Log)
                //    {
                //        s.WriteLine($"{v.IssueType}({v.Source}): {v.Msg}");
                //    }
                //}
            });

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
    }
}
