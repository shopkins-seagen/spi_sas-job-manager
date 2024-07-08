
using CommandLine;
using Microsoft.Extensions.Configuration;
using SasJobManager.Cli.Models;
using SasJobManager.Domain;
using SasJobManager.Lib;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using System.Text.RegularExpressions;

using System.Security.Cryptography;
using System.Text;

namespace SasJobManager.Cli
{

    internal class Program
    {
        private static IConfigurationRoot _cfg;
        private static CmdArgs _args;
        private static Regex _mvarRgx;
     

        static void Main(string[] args)
        {

            AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;
            LoadConfig();

            bool isInteractive=true;

            Console.WriteLine("Starting the SAS Workspace...");
            var options = Parser.Default.ParseArguments<CmdArgs>(args).WithParsed(cla =>
            {
                _args = cla;

                _args.SetPrograms();
                CheckLog(_args.Logger);


                Args sasArgs = BuildArg();
                CheckLog(_args.Logger);
                isInteractive = sasArgs.IsInteractive;

                if (sasArgs.Programs.Count > 0)
                {
                    if (!sasArgs.OnlyReviewLogs)
                    {

                        var sas = new SasManager(sasArgs, new SasContext());
                        CheckLog(sas.Logger);

                        try
                        {
                            var progress = new Progress<string>(value =>
                            {
                                Console.WriteLine(value);
                            });

                        //try
                        //{
                            sas.Run(progress).GetAwaiter().GetResult();
                            sas.WriteSummary(sasArgs.IsInteractive);
                        }
                        catch (Exception ex)
                        {
                            if (sasArgs.IsInteractive)
                            {
                                Console.WriteLine($"Message: {ex.Message}");
                                Console.WriteLine($"stack: {ex.StackTrace}");
                                Console.ReadKey();
                            }
                        }
                    }

                    // only check the logs
                    else
                    {
                        var sas = new SasManager(sasArgs, new SasContext());
                        sas.Started = DateTime.Now;

                        foreach (var pgm in sasArgs.Programs)
                        {
                            if (File.Exists(pgm.LogFn))
                            {
                                Console.WriteLine($"Reviewing {Path.GetFileName(pgm.LogFn)}...");
                                var logService = new SasLogService(sas.Cfg, true,sasArgs.DoesIncludeAutoQcReview);
                                pgm.PgmFn = Path.Combine(Path.GetDirectoryName(pgm.PgmFn), $"{Path.GetFileNameWithoutExtension(pgm.PgmFn)}.sas");
                                var log = logService.CheckLogFile(pgm);
                                sas.Logs.Add(log);
                            }
                            else
                            {
                                CheckLog(new List<LogEntry> { new LogEntry() { IssueType = IssueType.Fatal, Msg = $"No valid log files were specified", Source = "Main" } });

                            }
                        }
                        sas.Finished = DateTime.Now;
                        if (sas.Logs.Count > 0)
                        {
                            sas.WriteSummary(sasArgs.IsInteractive,"Only log review performed. No programs were executed");
                        }
                    }
                }
                else
                {
                    if (sasArgs.IsInteractive)
                    {
                        Console.WriteLine($"No valid programs selected\n\nPress any key to continue");
                        Console.ReadKey();
                    }
                }
            }).WithNotParsed(errors =>
            {
                if (isInteractive)
                {
                    foreach (var e in errors)
                    {
                        try
                        {
                            Console.WriteLine($"Error parsing command line flags.'{((CommandLine.TokenError)e).Token}' is not a valid flag. See user guide for details: {_cfg["user_guide"]}");
                        }
                        catch { } // let it go, not important
                    }
                    Console.WriteLine("\nPress any key to continue");
                    Console.ReadKey();
                }
           
            });
        }



        private static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (_args.IsInteractive.GetValueOrDefault())
            {
                Console.WriteLine($"ERROR: Unexpected error caused termination of the application. If the condition persists contact SPI: {e.ExceptionObject.ToString()}");
                Console.WriteLine("Press Enter to continue");
                Console.ReadLine();
            }
            Environment.Exit(1);
        }

        private static Args BuildArg()
        {
            var args = new Args()
            {
                Server = SetServer(_args.Server),
                Context = _args.Context ?? _cfg["context"],
                BaseMetricsUrl = _cfg["cpu_usage_url_base"],
                MetricsUrl = _cfg["cpu_usage_url"],
                Programs = _args.Programs,
                IsAsync = string.IsNullOrEmpty(_args.IsAsyncStr) ? bool.Parse(_cfg["is_async"]) : _args.IsAsync.GetValueOrDefault(),
                DoesCheckLog = string.IsNullOrEmpty(_args.DoesCheckLogStr) ? bool.Parse(_cfg["does_check_log"]) : _args.DoesCheckLog.GetValueOrDefault(),
                DoesMacroLogging = string.IsNullOrEmpty(_args.DoesMacroLoggingStr) ? bool.Parse(_cfg["does_macro_logging"]) : _args.DoesMacroLogging.GetValueOrDefault(),
                DoesNotifyOnCompletion = string.IsNullOrEmpty(_args.DoesNotifyOnCompletionStr) ? bool.Parse(_cfg["does_notify_on_completion"]) : _args.DoesNotifyOnCompletion.GetValueOrDefault(),
                DoesResolveProgramCode = string.IsNullOrEmpty(_args.DoesResolveCodeStr) ? bool.Parse(_cfg["does_resolve_code"]) : _args.DoesResolveCode.GetValueOrDefault(),
                OnlyReviewLogs = _args.ReviewLogsOnly.GetValueOrDefault(),
                DoesIncludeAutoQcReview = _args.IncludeAutoQcReview.GetValueOrDefault(),
                IsInteractive = _args.IsInteractive.GetValueOrDefault(),
                SummaryFn = ValidHtmlSummary(_args.HtmlSummaryFn),
                Ai = _args.Ai,
                UseDeriveOrder=_args.UseDeriveOrder.GetValueOrDefault(),
                DoesDisplayProgrammers=_args.DisplayProgrammers.GetValueOrDefault(),
                DoesQuitOnError = _args.DoesQuitOnError.GetValueOrDefault(),
                DoesExportLogSummary = _args.DoesExportSummary.GetValueOrDefault(),
            };
            if (_args.Logger.Count>0)
                args.Logger.AddRange(_args.Logger);

            CheckAi(ref args);

            args.UseBestServer = args.Server.Equals("best", StringComparison.OrdinalIgnoreCase) ? true : false;

            args.Port = GetPort(args.Context);

            if (args.DoesNotifyOnCompletion)
            {
                if (_args.Recipients.Count() == 0)
                {
                    args.Recipients.Add(Environment.UserName);
                }
                else
                {
                    args.Recipients.AddRange(_args.Recipients);
                }
            }

            if (!string.IsNullOrEmpty(_args.Mvars))
            {
                foreach(var m in _args.Mvars.Split('!',StringSplitOptions.RemoveEmptyEntries))
                {
                    var dlm = m.IndexOf(':');
                    if (dlm>=0)
                    {
                        if (_mvarRgx.IsMatch(m.Substring(0,dlm)))
                        {
                            args.Mvars[m.Substring(0,dlm)]= m.Substring(dlm+1);
                        }
                        else
                        {
                            _args.Logger.Add(new LogEntry($"Invalid name for macro variable in expression '{m}'", IssueType.Fatal, "BuildArgs"));
                        }           
                    }
                    else
                    {
                        _args.Logger.Add(new LogEntry($"Invalid specification for macro variables. See user guide", IssueType.Fatal, "BuildArgs"));
                    }
                }
            }

            return args;
        }

        private static string SetServer(string? s)
        {
            if (string.IsNullOrEmpty(s))
                return _cfg["server"];
                       
            var serverAlias = _cfg.GetSection("aliases").GetChildren().OrderBy(x => x.Key).Select(x => x.Value).ToList();
            if (serverAlias.IndexOf(s.ToLower())>=0)
            {
                var servers = _cfg.GetSection("servers").GetChildren().OrderBy(x => x.Key).Select(x => x.Value).ToList();
                return servers[serverAlias.IndexOf(s.ToLower())];
            }
            return s;            
        }

        private static void CheckAi(ref Args args)
        {
            if (!string.IsNullOrEmpty(args.Ai))
            {
                if (!File.Exists(args.Ai))
                {
                    _args.Logger.Add(new LogEntry()
                    {
                        IssueType = IssueType.Fatal,
                        Msg = $"Cannot locate the specified AI file: '{args.Ai}'",
                        Source = "Program.CheckAI"
                    });
                    return;
                }
            }
            if(args.UseDeriveOrder && !_args.HasDeriveOrder)
            {
                
                _args.Logger.Add(new LogEntry()
                {
                    IssueType = IssueType.Fatal,
                    Msg = $"CSV file must include a third column that defines sequence and run mode to use the derive order option (-d true)",
                    Source = "Program.CheckAI"
                });
                return;
            }
        }

        public static string ValidHtmlSummary(string fn)
        {
            if (!string.IsNullOrEmpty(_args.HtmlSummaryFn))
            {
                try
                {
                    var loc = Path.GetDirectoryName(_args.HtmlSummaryFn);

                    if (!Path.GetExtension(fn).Contains(".html",StringComparison.OrdinalIgnoreCase))
                        throw new Exception($"Invalid HTML summary file name. See user guide for naming conventions");
                    if (!Directory.Exists(loc))
                        throw new Exception($"Invalid location for HTML summary file name. Path does not exist");

                    return fn;
                }
                catch (Exception ex)
                {
                    _args.Logger.Add(new LogEntry()
                    {
                        IssueType = IssueType.Fatal,
                        Msg = ex.Message,
                        Source = "Program.IsValidHtmlSummary"
                    });
                }
            }
            else
            {
                var loc = Path.Combine(_args.Programs[0].Dir(), "run_summaries");
                Directory.CreateDirectory(loc);
                var htmlFn = Path.Combine(loc, $"summary-{Environment.UserName}-{(DateTime.Now.ToString("yyyyMMddHHmmss"))}.html");
                return htmlFn;
            }
            _args.Logger.Add(new LogEntry()
            {
                IssueType = IssueType.Fatal,
                Msg = "Invalid specification for HTML summary file (-h)",
                Source = "Program.IsValidHtmlSummary"
            });
            return String.Empty;
        }

        private static void LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_cli.json", true, true);
            _cfg = builder.Build();
            SetMvarRgx();
        }

        private static void SetMvarRgx()
        {
            _mvarRgx = new Regex(_cfg["mvar_rgx"], RegexOptions.IgnoreCase);
        }

        private static void CheckLog(List<LogEntry> logger)
        {
            if (logger.Where(x => x.IssueType == IssueType.Fatal).Count() > 0)
            {
                if (_args.IsInteractive.GetValueOrDefault())
                {
                    var oc = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Fatal Errors Detected!\n==============================\n");

                    foreach (var v in logger.Where(x => x.IssueType == IssueType.Fatal))
                    {
                        Console.WriteLine($"Fatal Error: {v.Msg}");
                    }
                    Console.ForegroundColor = oc;
                    Console.WriteLine("\n\nPress any key to continue");
                    Console.ReadKey();
                    
                }
                Environment.Exit(0);
            }
        }

        private static string GetPort(string context)
        {
            return context.ToUpper() == "SASAPP94" ? _cfg["port"] : (int.Parse(_cfg["port"]) + 1).ToString();
        }
    }
}