using Microsoft.Extensions.Configuration;
using SasJobManager.Domain;
using SasJobManager.Domain.Models;
using SasJobManager.Lib.Models;
using SasJobManager.Lib.Service;
using SasJobManager.ServerLib.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace SasJobManager.Lib
{
    public class SasManager
    {



        private SasContext _context;

        public SasManager(Args args, SasContext context, bool isLogsOnly = false)
        {
            Args = args;
            LoadConfig();
            _context = context;
            Logs = new List<SasLog>();
            Logger = new List<LogEntry>();
            Workspaces = new List<WorkspaceManager>();
            Summary = new RunSummary(Cfg, args);
            ServerMetrics = new ServerMetrics(args.BaseMetricsUrl, args.MetricsUrl);
            FolderService = new FolderService(args.BaseMetricsUrl, Cfg["url_locked"]);
            if (args.Logger.Count > 0)
            {
                Logger.AddRange(args.Logger);
            }
        }
        public Args Args;
        public ServerMetrics ServerMetrics { get; set; }
        public FolderService FolderService { get; set; }
        public List<SasLog> Logs { get; set; }
        public List<LogEntry> Logger;
        public List<WorkspaceManager> Workspaces { get; set; }
        public RunSummary Summary { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public IConfigurationRoot Cfg;


        public async Task ValidateArgs()
        {
            int nPgms = Args.Programs.Count;

            var badPgms = Args.Programs.Where(x => !x.IsValid).ToList();

            if (badPgms.Count > 0)
            {
                var msg = string.Join(',', badPgms.Select(x => x.Name()).ToArray());
                if (Args.IsAsync)
                {                   
                    Logger.Add(new LogEntry()
                    {
                        IssueType = IssueType.Error,
                        Msg = $"The program{(badPgms.Count>1?"s":string.Empty)} '{msg}' could not be located and ha{(badPgms.Count > 1 ? "ve" : "s")} been omitted from processing",
                        Source = "SasManager.ValidateArgs"
                    });

                    Args.Programs.RemoveAll(x => !x.IsValid);
                }
                else
                {
                    Logger.Add(new LogEntry()
                    {
                        IssueType = IssueType.Fatal,
                        Msg = $"The program{(badPgms.Count > 1 ? "s" : string.Empty)} '{msg}' could not be located. Since programming mode is sequential, the run has been aborted",
                        Source = "SasManager.ValidateArgs"
                    });
                }
            }


            if (Args.Programs.Count == 0)
            {
                Logger.Add(new LogEntry()
                {
                    IssueType = IssueType.Fatal,
                    Msg = $"No valid SAS program files were specified",
                    Source = "SasManager.ValidateArgs"
                });
            }
            if (Args.Programs.Count==1 && !Args.UseBestServer)
            {
                Args.IsAsync = false;
            }

            // All locations are not locked per CS-096
            //remmove this try catch
            try
            {
                foreach (var d in Args.Programs.Select(x => x.Dir()).Distinct())
                {
                    Console.WriteLine($"Checking {d}");
                    Tuple<bool, string> isLocked = await FolderService.IsLocked(d);

                    if (isLocked.Item1)
                    {
                        Logger.Add(new LogEntry()
                        {
                            IssueType = IssueType.Fatal,
                            Msg = $"The folder '{d}' is locked per CS-096",
                            Source = "SasManager.ValidateArgs",
                        });
                    }
                    if (!string.IsNullOrEmpty(isLocked.Item2))
                    {
                        Logger.Add(new LogEntry()
                        {
                            IssueType = IssueType.Fatal,
                            Msg = isLocked.Item2,
                            Source = "SasManager.ValidateArgs",
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }

            foreach (var p in Args.Programs)
            {
                if (File.Exists(p.LogFn))
                {
                    if (FolderService.IsFileProtected(p.LogFn))
                    {
                        Logger.Add(new LogEntry()
                        {
                            IssueType = IssueType.Fatal,
                            Msg = $"The SAS log file '{p.LogFn}' is designated as Read-Only",
                            Source = "SasManager.ValidateArgs",
                        });
                    }
                }
            }

            var validServers = Cfg.GetSection($"valid_servers:servers").GetChildren().Select(x => x.Value).ToList();
            var validContexts = Cfg.GetSection($"valid_servers:contexts").GetChildren().Select(x => x.Value).ToList();
            if (!validServers.Contains(Args.Server, StringComparer.OrdinalIgnoreCase))
            {
                Logger.Add(new LogEntry()
                {
                    IssueType = IssueType.Fatal,
                    Msg = $"Invalid SAS application server '{Args.Server}' was specified. See user guide for valid values",
                    Source = "SasManager.ValidateArgs"
                });
            }
            if (!validContexts.Contains(Args.Context, StringComparer.OrdinalIgnoreCase))
            {
                Logger.Add(new LogEntry()
                {
                    IssueType = IssueType.Fatal,
                    Msg = $"Invalid SAS application server context '{Args.Context}' was specified. See user guide for valid values",
                    Source = "SasManager.ValidateArgs",

                });
            }
        }

        public async Task<Dictionary<string, int>> GetServerStats()
        {
            var dict = new Dictionary<string, int>();

            var prodCpu = await ServerMetrics.GetCpuUtilPct(Cfg["production_server"]);
            var stgCpu = await ServerMetrics.GetCpuUtilPct(Cfg["stage_server"]);

            dict[Cfg["stage_server"]] = stgCpu;
            dict[Cfg["production_server"]] = prodCpu;

            return dict;
        }

        public string SelectBestServer(Dictionary<string, int> dict, int bias)
        {
            if ((dict[Cfg["production_server"]] - dict[Cfg["stage_server"]]) < bias)
                return Cfg["production_server"];
            else
                return Cfg["stage_server"];
        }

        public async Task Run(IProgress<string> progress)
        {
            progress.Report($"*** SJM v{Cfg["app_ver"]} ***\n");
            Started = DateTime.Now;
            await ValidateArgs();
            if (Logger.Any(x => x.IssueType == IssueType.Fatal))
            {
                progress.Report($"Fatal Error:{Logger.Where(x => x.IssueType == IssueType.Fatal).FirstOrDefault().Msg}");
                Finished = DateTime.Now;
                return;
            }
            if (Args.IsAsync)
            {
                if (Args.UseDeriveOrder)
                {
                    await RunMixedAsync(progress);
                }
                else
                {
                    await RunParallelAsync(progress);
                }
                
            }
            else
            {
                await Task.Run(() => RunSequentialAsync(progress));
            }
            if (Workspaces.Count > 0)
            {

                if (Args.DoesCheckLog)
                {
                    progress.Report("Summarizing SAS logs...");
                    CompileLogs();
                }
                if (Args.DoesMacroLogging)
                {
                    progress.Report("Recording macros used by programs...");
                    PersistSasObjects();
                }
                Finished = DateTime.Now;
                if (Args.DoesNotifyOnCompletion)
                {
                    progress.Report("Sending notifications...");
                    var details = GetDetails();
                    var msg = new Message(details, Cfg["url_base_msg"], Cfg["url_msg"]);
                    var test = Newtonsoft.Json.JsonConvert.SerializeObject(details);
                    var response = await msg.Send();
                    if (response.IssueType != IssueType.Info)
                    {
                        Logger.Add(new LogEntry()
                        {
                            IssueType = IssueType.Warning,
                            Msg = $"Issue encountered sending notifications: {response.Msg}"
                        });
                    }
                }
            }
            progress.Report("Completed!");
            Finished = DateTime.Now;
        }

      

        private MessageDetails GetDetails()
        {
            var details = new MessageDetails(Args)
            {
                Recipients = Args.Recipients,
                Sender = Environment.UserName,
                Program = Args.Programs.Count == 1 ? $"{Args.Programs[0].NameWithoutExtension()}.sas" : $"{Args.Programs.Count} programs",
            };

            if (Args.DoesCheckLog)
            {
                details.SummaryFn = Args.SummaryFn;
                details.Status = Logs.Max(x => x.WorstFinding());
                details.AddLogSummary(Logs, Started, Finished);
            }

            return details;
        }


        public async Task RunParallelAsync(IProgress<string> progress)
        {
            var queue = new List<Models.SasProgram>();
            var allPgms = Args.Programs;


            do
            {
                var server = string.Empty;
                int pct = 0;

                if (Args.UseBestServer)
                {
                    var resp = await GetServerStats();
                    server = SelectBestServer(resp, int.Parse(Cfg["server_bias"]));
                    pct = resp[server];
                    Args.Server = server;
                }
                else
                {
                    server = Args.Server;
                    pct = await GetCpuUsage(server);
                }

                queue.Clear();
                var batch = GetBatchSize(pct, allPgms.Count);

                progress.Report($"[CPU {pct}%] - Submitting {batch} program{(batch > 1 ? "s" : string.Empty)} in parallel mode on server: {server}.");

                var selected = allPgms.Take(batch);
                allPgms = allPgms.Except(selected).ToList();
                queue.AddRange(selected);

                Parallel.ForEach(queue, async pgm => 
                {
                    var sas = new WorkspaceManager(Args, Cfg);
                    progress.Report($">{pgm.NameWithoutExtension()} submitted at {DateTime.Now.ToString("HH:mm:ss")}");
                    await sas.Submit(pgm);

                    Logger.AddRange(sas.Logger);

                    if (sas.Logger.Where(x => x.IssueType == IssueType.Fatal).Count() > 0)
                    {
                        foreach (var v in sas.Logger.Where(x => x.IssueType == IssueType.Fatal))
                        {
                            progress.Report($"Fatal Error: {v.Msg}");
                        }
                        return;
                    }

                    Workspaces.Add(sas);
                });

                if (Args.DoesCheckLog)
                {
                    if (queue != null)
                    {
                        foreach (var p in queue)
                        {
                            var ws = Workspaces.Where(x => x.SasLogService.Program == p).FirstOrDefault();
                            if (ws != null)
                            {
                                var status = ws.SasLogService.SasLogFindings.Count() > 0 ? ws.SasLogService.SasLogFindings.Max(x => x.Type) : SasFindingType.Clean;
                                progress.Report($">{ws.SasLogService.Program.NameWithoutExtension()} completed at {ws.SasLogService.Program.Completed.ToString("HH:mm:ss")} with overall status '{status.ToString()}'");
                            }
                        }
                    }
                }

                progress.Report($"{Args.Programs.Count - allPgms.Count} of {Args.Programs.Count} completed");


            } while (allPgms.Count > 0);

        }

        public async Task RunSequentialAsync(IProgress<string> progress)
        {
            var server = string.Empty;
            int pct = 0;

            foreach (var pgm in Args.Programs)
            {
                if (Args.UseBestServer)
                {
                    var resp = await GetServerStats();
                    server = SelectBestServer(resp, int.Parse(Cfg["server_bias"]));
                    pct = resp[server];
                    Args.Server = server;
                }
                else
                {
                    server = Args.Server;
                }
                var pctMsg = Args.UseBestServer ? $"[{pct} CPU%]" : String.Empty;
                progress.Report($">{pgm.NameWithoutExtension()} submitted on {server} {pctMsg} at {DateTime.Now.ToString("HH:mm:ss")}");
                var sas = new WorkspaceManager(Args, Cfg);
                await sas.Submit(pgm);
                Logger.AddRange(sas.Logger);
                if (sas.Logger.Where(x => x.IssueType == IssueType.Fatal).Count() > 0)
                {
                    foreach (var v in sas.Logger.Where(x => x.IssueType == IssueType.Fatal))
                    {
                        progress.Report($"Fatal Error: {v.Msg}");
                    }
                    return;
                }
                Workspaces.Add(sas);

                if (Args.DoesCheckLog)
                {
                    if (sas != null)
                    {
                        var status = sas.SasLogService.SasLogFindings.Count() > 0 ? sas.SasLogService.SasLogFindings.Max(x => x.Type) : SasFindingType.Clean;
                        progress.Report($">{sas.SasLogService.Program.NameWithoutExtension()} completed at {sas.SasLogService.Program.Completed.ToString("HH:mm:ss")} with overall status '{status.ToString()}'");

                        if(Args.DoesQuitOnError && status == SasFindingType.Error && !sas.SasLogService.Program.IsQc)
                        {
                            progress.Report($">Terminating execution as -q is true and '{sas.SasLogService.Program.NameWithoutExtension()}' has an ERROR detected");
                            Logger.Add(new LogEntry()
                            {
                                IssueType = IssueType.Fatal,
                                Source = "SasManager.RunSequentialAsync",
                                Msg = $"User requested termination on ERROR. Execution halted at '{sas.SasLogService.Program.NameWithoutExtension()}'"
               
                            });

                            return;
                        }
                    }
                }
                else
                {
                    progress.Report($">{pgm.NameWithoutExtension()} completed at {DateTime.Now.ToString("HH:mm:ss")}");
                }
            }
        }

        public async Task RunMixedAsync(IProgress<string> progress)
        {
            var groups = Args.Programs.GroupBy(x => x.DeriveOrder).OrderBy(x=>x.Key.Value).ToList();

            int completed = 0;
            foreach (var group in groups)
            {
                
                var queue = new List<Models.SasProgram>();
                var allPgms = group.ToList();

                do
                {
                    var server = string.Empty;
                    int pct = 0;

                    if (Args.UseBestServer)
                    {
                        var resp = await GetServerStats();
                        server = SelectBestServer(resp, int.Parse(Cfg["server_bias"]));
                        pct = resp[server];
                        Args.Server = server;
                    }
                    else
                    {
                        server = Args.Server;
                        if (allPgms.Count == 1)
                        {
                            pct = 0;
                        }
                        else
                        {
                            pct = await GetCpuUsage(server);
                        }
                    }

                    queue.Clear();

                    var batch = GetBatchSize(pct, allPgms.Count);

                    if(allPgms.Count==1)
                    {
                        progress.Report($"Submitting {allPgms[0].Name()} sequentially on server: {server}.");
                    }
                    else
                    {
                        progress.Report($"[CPU {pct}%] - Submitting {batch} program{(batch > 1 ? "s" : string.Empty)} in parallel mode on server: {server}.");
                    }
                 

                    var selected = allPgms.Take(batch);
                    allPgms = allPgms.Except(selected).ToList();
                    queue.AddRange(selected);

                    Parallel.ForEach(queue, pgm =>
                    {
                        var sas = new WorkspaceManager(Args, Cfg);
                        

                        progress.Report($">{pgm.NameWithoutExtension()} submitted at {DateTime.Now.ToString("HH:mm:ss")}");
                        sas.Submit(pgm);

                        Logger.AddRange(sas.Logger);
                        

                        if (sas.Logger.Where(x => x.IssueType == IssueType.Fatal).Count() > 0)
                        {
                            foreach (var v in sas.Logger.Where(x => x.IssueType == IssueType.Fatal))
                            {
                                progress.Report($"Fatal Error: {v.Msg}");
                            }
                            return;
                        }

                        Workspaces.Add(sas);
                    });

                    var status = SasFindingType.Clean;
                    string? termPgm = string.Empty ;
                    bool isTermQc = false;
                    if (Args.DoesCheckLog)
                    {
                        if (queue != null)
                        {
                            foreach (var p in queue)
                            {
                                var ws = Workspaces.Where(x => x.SasLogService.Program == p).FirstOrDefault();
                                if (ws != null)
                                {
                                    status = ws.SasLogService.SasLogFindings.Count() > 0 ? ws.SasLogService.SasLogFindings.Max(x => x.Type) : SasFindingType.Clean;
                                    progress.Report($">{ws.SasLogService.Program.NameWithoutExtension()} completed at {ws.SasLogService.Program.Completed.ToString("HH:mm:ss")} with overall status '{status.ToString()}'");
                                    termPgm = status == SasFindingType.Error ? ws.SasLogService.Program.NameWithoutExtension() : string.Empty;
                                    isTermQc = ws.SasLogService.Program.IsQc;
                                }
                            }
                        }
                    }
                    completed += selected.Count();


                    progress.Report($"{completed} of {Args.Programs.Count} completed");

                    if (Args.DoesQuitOnError && status == SasFindingType.Error && !isTermQc )
                    {
                        progress.Report($">Terminating execution as -q is true and '{termPgm}' has an ERROR detected");
                        Logger.Add(new LogEntry()
                        {
                            IssueType = IssueType.Fatal,
                            Source="SasManager.RunMixedAsync",
                            Msg = $"User requested termination on ERROR. Execution halted at '{termPgm}' "
                        });

                        return;
                    }


                } while (allPgms.Count > 0);
            }

        }
        public void WriteSummary(bool isInteractive,string? message=null)
        {
            Summary.WriteSummary(Logs, Logger, Started, Finished,message);
            if (isInteractive)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = Args.SummaryFn,
                    UseShellExecute = true,
                };
                Process.Start(psi);
            }
        }

        private async Task<int> GetCpuUsage(string server)
        {
            return await ServerMetrics.GetCpuUtilPct(server);
        }

        public int GetBatchSize(int pct, int nPgms)
        {
            var batch = 1;

            //if (pct >= 0)
            //{
            //    if (nPgms >=int.Parse(Cfg["max_parallel_pgms"]))
            //    {
            //        batch = (int)Math.Max(Math.Round(((100 - pct) * .01) * int.Parse(Cfg["max_parallel_pgms"]), 0), 1);
            //    }
            //    else
            //    {
            //        batch = (int)Math.Min(Math.Ceiling((double)(100 - pct) / int.Parse(Cfg["pct_per_pgm"])),nPgms);
            //    }
            //}

            batch = (int)Math.Min(Math.Min(Math.Ceiling((double)(100 - pct) / int.Parse(Cfg["pct_per_pgm"])), nPgms), int.Parse(Cfg["max_parallel_pgms"]));



            return batch < 1 ? 1 : batch;
        }

        private void LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_lib.json", true, true);
            Cfg = builder.Build();
        }

        private void CompileLogs()
        {
            foreach (var w in Workspaces)
            {
                var log = new SasLog(w.SasLogService.Program);
                log.Findings.AddRange(w.SasLogService.SasLogFindings);
                if (w.SasLogService.Program.IsQc)
                {
                    log.PassCount = w.SasLogService.PassCount;
                    log.FailCount = w.SasLogService.FailCount;
                    log.IsQcPgm = true;
                }

                Logs.Add(log);
                Logger.AddRange(w.Logger.Where(x => x.IssueType > IssueType.Info).ToList());
            }
        }
        public void PersistSasObjects()
        {
            foreach (var w in Workspaces)
            {

                var pgm = w.SasLogService.Program;
                try
                {
                    var worst = w.SasLogService.SasLogFindings.Count() > 0 ?
                            w.SasLogService.SasLogFindings.Max(x => x.Type).ToString() : SasFindingType.Clean.ToString();
                    var entity = _context.Programs.FirstOrDefault(x => x.Program.ToLower() == pgm.PgmFn.ToLower());
                    if (entity != null)
                    {
                        var product = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "PRODUCT").Select(x => x.Value).FirstOrDefault();
                        var protocol = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "PROJECT").Select(x => x.Value).FirstOrDefault();
                        var analysis = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "ANALYSIS").Select(x => x.Value).FirstOrDefault();
                        var release = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "DRAFT").Select(x => x.Value).FirstOrDefault();
                        var folderLevel = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault();
                        var isLocal = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "ISLOCALIZED").Select(x => x.Value).FirstOrDefault();

                        entity.Program = pgm.PgmFn;
                        entity.Product = product;
                        entity.Protocol = protocol;
                        entity.Analysis = analysis;
                        entity.Release = release;
                        entity.FolderLevel = folderLevel;
                        entity.IsLocal = isLocal == "1" ? true : false;

                        entity.RunDt = DateTime.Now;
                        
                        entity.WorstLogFinding = worst;
                        entity.Client = Environment.MachineName;
                        entity.Server = Args.Server;
                        entity.Context = Args.Context;
                        entity.UserId = Environment.UserName;
                        _context.RemoveRange(_context.Macros.Where(x => x.ProgramId == entity.Id));
                    }
                    else
                    {
                        entity = new Domain.Models.SasProgram();

                        var product = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "PRODUCT").Select(x => x.Value).FirstOrDefault();
                        var protocol = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "PROJECT").Select(x => x.Value).FirstOrDefault();
                        var analysis = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "ANALYSIS").Select(x => x.Value).FirstOrDefault();
                        var release = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "DRAFT").Select(x => x.Value).FirstOrDefault();
                        var folderLevel = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault();
                        var isLocal = w.SasObjects.Where(x => x.Type == SasObjectType.Mvar && x.Name.ToUpper() == "ISLOCAL").Select(x => x.Value).FirstOrDefault();

                        entity.Program = pgm.PgmFn;
                        entity.Product = product;
                        entity.Protocol = protocol;
                        entity.Analysis = analysis;
                        entity.Release = release;
                        entity.FolderLevel = folderLevel;
                        entity.IsLocal = isLocal == "1" ? true : false;
                        entity.RunDt = DateTime.Now;
                        entity.WorstLogFinding = worst;
                        entity.Client = Environment.MachineName;
                        entity.Server = Args.Server;
                        entity.Context = Args.Context;
                        entity.UserId = Environment.UserName;
                        _context.Programs.Add(entity);
                    }

                    foreach (var m in w.SasObjects.Where(x => x.Type == SasObjectType.Macro))
                    {
                        entity.Macros.Add(new Domain.Models.Macro()
                        {
                            Name = m.Name,
                            Version = m.Value
                        });
                    }
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.Add(new LogEntry()
                    {
                        IssueType = IssueType.Warning,
                        Msg = $"{ex.Message}",
                        Source = "WorkspaceManger.PersistSasObjects"
                    });
                }
            }
        }
    }
}