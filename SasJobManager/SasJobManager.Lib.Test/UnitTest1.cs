using Microsoft.VisualStudio.TestTools.UnitTesting;
using SasJobManager.Lib.Models;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using System;
using SasJobManager.Domain;
using System.Text.RegularExpressions;
using SasJobManager.ServerLib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Xml.XPath;
using SasJobManager.Domain.Migrations;

namespace SasJobManager.Lib.Test
{
    [TestClass]
    public class UnitTest1
    {
        private static readonly string _server = "sgsasv1.sg.seagen.com";
        private static readonly string _context = "SASApp94";
        private static readonly string _port = "8591";
        private static Args _args;
        private static SasManager _sas;
        private static string _root = @"O:\stat_prog_infra\testing\runsas\dotnet\unit_test\data\adam\pgms";
        private static string _rootv2 = @"O:\stat_prog_infra\testing\sjm\dotnet\use_ai\data\sdtm\pgms";
        private static string _ai = @"O:\stat_prog_infra\testing\sjm\dotnet\use_ai\utilities\ai\ai.xlsx";
        private static string _cli = @"C:\github\sp-sas-job-manager\SasJobManager\SasJobManager.Cli\bin\Debug\net6.0\SasJobManager.Cli.exe";


        [AssemblyInitialize]
        public static void AssemblyInit(TestContext tc)
        {
            _args = new Args(_server, _context, _port);
            _args.IsAsync = false;
            _args.UseBestServer = false;
            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;
            _args.BaseMetricsUrl = "http://spi2:8030";
            _args.MetricsUrl = "api/usage";
            _args.DoesCheckLog = true;

        }


        #region Simuate actual run
        public async Task _Simulate()
        {
            _root = @"O:\Projects\iSAFE\iSAFE_Dev\safety_analysis_1014\v_work\data\adhoc\_DIT_Sandbox\pgms\sudha\runlists";


            var pgm = new List<string> { "v-1-1-cpr-raw" };
            var msgs = new List<string>();
            _args.Csv = @"O:\Projects\iSAFE\iSAFE_Dev\safety_analysis_1014\v_work\data\adhoc\_DIT_Sandbox\pgms\sudha\runlists\test.csv";
            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = false;
            _args.UseBestServer = false;
            _args.Server = "sgsasv1.sg.seagen.com";

            SetPgms(pgm, ref _args, true);
            _args.SummaryFn = @$"{Path.Combine(_root, "run_summaries")}\summary-{Environment.UserName}-{(DateTime.Now.ToString("yyyyMMddHHmmss"))}.html";
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);
            sas.WriteSummary(true);
        }
        #endregion
        #region Version 1.0 tests

        /// <TestDescription>Execute SAS code</TestDescription>            
        /// <TestId>UT.SAS01.01</TestId> 
        /// <ReqId>SAS01.01,SAS02.01,SAS03.01,SAS04.01,SAS05.01,SAS07.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task RunSASCode()
        {
            var pgm = "test1";
            var msgs = new List<string>();
            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;

            SetPgms(Path.Combine(_root, pgm), ref _args);
            _args.SummaryFn = @$"{Path.Combine(_root, "run_summaries")}\summary-{Environment.UserName}-{(DateTime.Now.ToString("yyyyMMddHHmmss"))}.html";
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);
            sas.WriteSummary(false);

            var dset = new FileInfo(Path.Combine(Directory.GetParent(_root).FullName, $"{pgm}.sas7bdat"));


            // Expected outcomes
            Assert.IsTrue(dset.LastWriteTime > sas.Started);
            Assert.AreEqual(sas.Logs[0].WorstFinding(), SasFindingType.Clean);
            Assert.IsTrue(File.Exists(Path.Combine(_root, $"{pgm}.log")));
            Assert.IsTrue(File.Exists(Path.Combine(_root, $"{pgm}.lst")));
            Assert.IsTrue(File.Exists(sas.Args.SummaryFn));
        }

        /// <TestDescription>Detect each regex pattern in the SAS log file</TestDescription>            
        /// <TestId>UT.SAS02.01</TestId> 
        /// <ReqId>SAS03.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task FindAllLogIssues()
        {
            var pgm = "create_all_msgs";
            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;
            var msgs = new List<string>();

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes

            Assert.AreEqual(sas.Logs[0].WorstFinding(), SasFindingType.Error);

            Assert.AreEqual(sas.Logs[0].Findings.Where(x => x.Type == SasFindingType.Note).Count(), sas.Cfg.GetSection($"cfg_lists:notes").GetChildren().Count());
            Assert.AreEqual(sas.Logs[0].Findings.Where(x => x.Type == SasFindingType.Error).Count(), 2);
            Assert.AreEqual(sas.Logs[0].Findings.Where(x => x.Type == SasFindingType.Warning).Count(), 2);
            Assert.AreEqual(sas.Logs[0].Findings.Where(x => x.Type == SasFindingType.Notice).Count(), 1);
        }




        /// <TestDescription>Record department macros compiled during program execution</TestDescription>            
        /// <TestId>UT.PP01.01</TestId> 
        /// <ReqId>PP01.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task RecordMacros()
        {
            var folders = _root.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Skip(2).ToArray();
            var ctx = new SasContext();
            var pgm = "record_macros";

            var msgs = new List<string>();
            _args.DoesMacroLogging = true;
            _args.DoesResolveProgramCode = false;
            _args.DoesNotifyOnCompletion = false;

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, ctx);


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);


            var program = ctx.Programs.Where(x => x.Program.Contains("record_macros.sas")).FirstOrDefault();
            var macros = ctx.Programs.Where(x => x.Program.Contains("record_macros.sas")).Select(x => x.Macros).ToList();

            // Expected outcomes
            Assert.AreEqual(6, macros[0].Count());
            Assert.AreEqual(ctx.Macros.Where(x => x.Name == "MCR_SPI_TEST").Select(x => x.Version).FirstOrDefault(), "Version 99.4");
            Assert.AreEqual(program.Product, folders[0]);
            Assert.AreEqual(program.Protocol, folders[1]);
            Assert.AreEqual(program.Analysis, folders[2]);
            Assert.AreEqual(program.Release, folders[3]);
            Assert.AreEqual(program.Program, Path.Combine(_root, $"{pgm}.sas"));
            Assert.AreEqual(program.WorstLogFinding, "Warning");
            Assert.AreEqual(program.Server, "sgsasv1-stg.sg.seagen.com");
            Assert.AreEqual(program.Context, "SASApp94");
            Assert.AreEqual(program.IsLocal, true);
            Assert.AreEqual(program.UserId, Environment.UserName);

        }

        /// <TestDescription>Confirm email notification was sent without an issue</TestDescription>            
        /// <TestId>UT.PP02.01</TestId> 
        /// <ReqId>PP02.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task ConfirmNotificationByEmail()
        {
            var pgm = "empty";
            _args.DoesNotifyOnCompletion = true;
            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;

            _args.Recipients.Add("shopkins");

            var msgs = new List<string>();

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            Assert.IsTrue(msgs.Select(x => x.Contains("notification")).Count() > 0);               // did send a message
            Assert.IsTrue(sas.Logger.Where(x => x.Msg.Contains("notification")).Count() == 0); // did not encounter an issue
        }

        /// <TestDescription>Confirm resolved code file is created and has no occurences of macro keyword</TestDescription>            
        /// <TestId>UT.PP03.01</TestId> 
        /// <ReqId>PP03.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task ResolvedCodeFile()
        {
            var pgm = "test_resolved_code";

            _args.DoesResolveProgramCode = true;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;

            var msgs = new List<string>();

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes           
            var resolved = Path.Combine(_root, "resolved", $"{pgm}.sas");
            Assert.IsTrue(File.Exists(resolved));
            Assert.IsTrue(!Regex.IsMatch(File.ReadAllText(resolved), "%macro", RegexOptions.IgnoreCase));
        }




        /// <TestDescription>Submit programs in parallel</TestDescription>            
        /// <TestId>UT.CFG.03.01</TestId> 
        /// <ReqId>CFG03.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task RunInParallel()
        {
            // p1 = 10s, p2=20s, p3 = 0
            var pgm = new List<string> { "parallel2", "parallel3", "parallel1" };

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = true;


            var msgs = new List<string>();

            SetPgms(pgm, ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            var p1 = sas.Workspaces.Where(x => x.SasLogService.Program.NameWithoutExtension() == "parallel1")
                .Select(x => x.SasLogService.Program).FirstOrDefault();
            var p2 = sas.Workspaces.Where(x => x.SasLogService.Program.NameWithoutExtension() == "parallel2")
                .Select(x => x.SasLogService.Program).FirstOrDefault();
            var p3 = sas.Workspaces.Where(x => x.SasLogService.Program.NameWithoutExtension() == "parallel3")
                .Select(x => x.SasLogService.Program).FirstOrDefault();

            var dict = new Dictionary<string, Tuple<DateTime, DateTime>>();

            dict.Add(p1.NameWithoutExtension(), new Tuple<DateTime, DateTime>(p1.Started, p1.Completed));
            dict.Add(p2.NameWithoutExtension(), new Tuple<DateTime, DateTime>(p2.Started, p2.Completed));
            dict.Add(p3.NameWithoutExtension(), new Tuple<DateTime, DateTime>(p3.Started, p3.Completed));

            // Expected outcomes
            Assert.IsTrue(Math.Abs((p1.Started - p2.Started).TotalSeconds) <= 1);
            Assert.IsTrue(Math.Abs((p2.Started - p3.Started).TotalSeconds) <= 1);

            Assert.IsTrue(p3.Completed < p1.Completed);
            Assert.IsTrue(p1.Completed < p2.Completed);
        }

        /// <TestDescription>Submit programs sequentially</TestDescription>            
        /// <TestId>UT.CFG03.02</TestId> 
        /// <ReqId>CFG03.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task RunInSequence()
        {
            // p1 = 10s, p2=20s, p3 = 0
            var pgm = new List<string> { "parallel2", "parallel3", "parallel1" };

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = false;


            var msgs = new List<string>();

            SetPgms(pgm, ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            var p1 = sas.Workspaces.Where(x => x.SasLogService.Program.NameWithoutExtension() == "parallel1")
                .Select(x => x.SasLogService.Program).FirstOrDefault();
            var p2 = sas.Workspaces.Where(x => x.SasLogService.Program.NameWithoutExtension() == "parallel2")
                .Select(x => x.SasLogService.Program).FirstOrDefault();
            var p3 = sas.Workspaces.Where(x => x.SasLogService.Program.NameWithoutExtension() == "parallel3")
                .Select(x => x.SasLogService.Program).FirstOrDefault();

            var dict = new Dictionary<string, Tuple<DateTime, DateTime>>();

            dict.Add(p1.NameWithoutExtension(), new Tuple<DateTime, DateTime>(p1.Started, p1.Completed));
            dict.Add(p2.NameWithoutExtension(), new Tuple<DateTime, DateTime>(p2.Started, p2.Completed));
            dict.Add(p3.NameWithoutExtension(), new Tuple<DateTime, DateTime>(p3.Started, p3.Completed));

            // Expected outcomes

            Assert.IsTrue(p2.Completed <= p3.Started);
            Assert.IsTrue(p3.Completed < p1.Started);
        }

        /// <TestDescription>Calculate batch size for parallel processing by CPU availability</TestDescription>            
        /// <TestId>UT.CFG04.01</TestId> 
        /// <ReqId>CFG04.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public void BatchSizeByCpu()
        {
            var data = new List<string>();
            _sas = new SasManager(_args, new SasContext());
            var max_pgms = int.Parse(_sas.Cfg["max_parallel_pgms"]);
            var per_pgm = int.Parse(_sas.Cfg["pct_per_pgm"]);

            int low = 10;
            int mid = 50;
            int high = 90;

            var cal_low = _sas.GetBatchSize(low, max_pgms);
            var cal_mid = _sas.GetBatchSize(mid, max_pgms);
            var cal_high = _sas.GetBatchSize(high, max_pgms);

            var comp_low = (int)Math.Min(Math.Min(Math.Ceiling((double)(100 - low) / per_pgm), max_pgms), max_pgms);
            var comp_mid = (int)Math.Min(Math.Min(Math.Ceiling((double)(100 - mid) / per_pgm), max_pgms), max_pgms);
            var comp_high = (int)Math.Min(Math.Min(Math.Ceiling((double)(100 - high) / per_pgm), max_pgms), max_pgms);

            // expected outcomes
            Assert.AreEqual(cal_low, comp_low);
            Assert.AreEqual(comp_mid, cal_mid); ;
            Assert.AreEqual(comp_high, cal_high);

        }

        /// <TestDescription>Identify the best server by CPU availability</TestDescription>            
        /// <TestId>UT.CFG05.01</TestId> 
        /// <ReqId>CFG05.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task BestServerByCpuAvailability()
        {
            var data = new List<string>();
            _sas = new SasManager(_args, new SasContext());


            var stats = await _sas.GetServerStats();
            var stg = stats[_sas.Cfg["stage_server"]];
            var prd = stats[_sas.Cfg["production_server"]];
            int bias = 0;

            // expected outcomes
            var best = _sas.SelectBestServer(stats, bias);
            Assert.IsTrue(best == _sas.Cfg["stage_server"] ? prd - stg >= bias : prd - stg < bias);

            bias = 25;


            best = _sas.SelectBestServer(stats, bias);
            Assert.IsTrue(best == _sas.Cfg["stage_server"] ? prd - stg >= bias : prd - stg < bias);

            bias = 50;


            best = _sas.SelectBestServer(stats, bias);
            Assert.IsTrue(best == _sas.Cfg["stage_server"] ? prd - stg >= bias : prd - stg < bias);

            bias = 99;

            best = _sas.SelectBestServer(stats, bias);
            Assert.IsTrue(best == _sas.Cfg["stage_server"] ? prd - stg >= bias : prd - stg < bias);


            // mock values
            var myStats = new Dictionary<string, int>();
            myStats[_sas.Cfg["stage_server"]] = 20;
            myStats[_sas.Cfg["production_server"]] = 40;
            bias = 20;

            best = _sas.SelectBestServer(myStats, bias);
            Assert.AreEqual(best, _sas.Cfg["stage_server"]);

            myStats[_sas.Cfg["production_server"]] = 39;
            best = _sas.SelectBestServer(myStats, bias);
            Assert.AreEqual(best, _sas.Cfg["production_server"]);

        }

        /// <TestDescription>Identify progrograms as QC and capture pass/fails for each test</TestDescription>            
        /// <TestId>UT.QC.01.01</TestId> 
        /// <ReqId>QC.01.01,QC.01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task QcResults()
        {
            // p1 = 10s, p2=20s, p3 = 0
            var pgm = new List<string> { "qc1", "qc2" };

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = true;
            _args.Server = "sgsasv1-stg.sg.seagen.com";


            var msgs = new List<string>();

            SetPgms(pgm, ref _args, true);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);
            var qc1 = sas.Logs.Where(x => x.Program.NameWithoutExtension() == "qc1").FirstOrDefault();
            var qc2 = sas.Logs.Where(x => x.Program.NameWithoutExtension() == "qc2").FirstOrDefault();

            // Expected outcomes
            Assert.AreEqual(qc1.PassCount, 3);
            Assert.AreEqual(qc1.FailCount, 0);
            Assert.AreEqual(qc2.PassCount, 1);
            Assert.AreEqual(qc2.FailCount, 2);

        }


        /// <TestDescription>Handle unexpected disconnections from server due to user code</TestDescription>            
        /// <TestId>UT.EX01.01</TestId> 
        /// <ReqId>EX01.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task HandleEndSAS()
        {
            var pgm = "endsas";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            Assert.IsTrue(Regex.IsMatch(sas.Logger[0].Msg, "became disconnected during code execution", RegexOptions.IgnoreCase));
        }
        /// <TestDescription>Terminate execution if folder is locked per CS-096</TestDescription>            
        /// <TestId>UT.EX02.01</TestId> 
        /// <ReqId>EX02.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task FolderLocked()
        {
            var folder = @"O:\stat_prog_infra\testing\runsas\dotnet\unit_test\data\profile_analysis\pgms";
            var pgm = "locked-cs096";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = false;
            _args.DoesNotifyOnCompletion = false;
            _args.DoesCheckLog = true;

            SetPgms(Path.Combine(folder, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes

            Assert.AreEqual(sas.Logger[0].IssueType, IssueType.Fatal);
            Assert.IsTrue(Regex.IsMatch(sas.Logger[0].Msg, "locked", RegexOptions.IgnoreCase));
        }
        

        /// <TestDescription>Invalid program is specified in parallel mode</TestDescription>            
        /// <TestId>UT.EX04.01</TestId> 
        /// <ReqId>EX04.01</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task InvalidProgramAsync()
        {
            // p1 = 10s, p2=20s, p3 = 0
            var pgm = new List<string> { "clean1", "clean4", "clean2", "clean3" };

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = true;
            _args.DoesCheckLog = true;
            _args.Server = "sgsasv1-stg.sg.seagen.com";


            var msgs = new List<string>();

            SetPgms(pgm, ref _args, true);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);
            var err = sas.Logger.Where(x => x.IssueType == IssueType.Error).FirstOrDefault();

            // Expected outcomes
            Assert.IsTrue(err != null);
            Assert.IsTrue(err.Msg.Contains("omitted"));
            Assert.IsTrue(sas.Workspaces.Count() == 2);


        }
        /// <TestDescription>Invalid program is specified in sequential mode</TestDescription>            
        /// <TestId>UT.EX04.02</TestId> 
        /// <ReqId>EX04.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task InvalidProgramSequential()
        {
            // p1 = 10s, p2=20s, p3 = 0
            var pgm = new List<string> { "clean1", "clean4", "clean2", "clean3" };

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = false;
            _args.DoesCheckLog = true;
            _args.Server = "sgsasv1-stg.sg.seagen.com";


            var msgs = new List<string>();

            SetPgms(pgm, ref _args, true);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            var err = sas.Logger.Where(x => x.IssueType == IssueType.Fatal).FirstOrDefault();
            // Expected outcomes
            Assert.IsTrue(err != null);
            Assert.IsTrue(err.Msg.Contains("aborted"));
            Assert.IsTrue(sas.Workspaces.Count() == 0);

        }

        /// <TestDescription>No valid programs</TestDescription>            
        /// <TestId>UT.EX04.03</TestId> 
        /// <ReqId>EX04.03</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task NoValidPrograms()
        {
            // p1 = 10s, p2=20s, p3 = 0
            var pgm = new List<string> { "clean3", "clean4" };

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.IsAsync = true;
            _args.DoesCheckLog = true;
            _args.Server = "sgsasv1-stg.sg.seagen.com";


            var msgs = new List<string>();

            SetPgms(pgm, ref _args, true);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            var err = sas.Logger.Where(x => x.IssueType == IssueType.Fatal).FirstOrDefault();
            // Expected outcomes
            Assert.IsTrue(err.Msg.Contains("no valid SAS program", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(sas.Workspaces.Count(), 0);

        }


        /// <TestDescription>Capture the macro variables values from the environment if the listing destination is turned off</TestDescription>            
        /// <TestId>UT.PP01.02</TestId> 
        /// <ReqId>PP01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task GetMvarsFromEnv()
        {
            var pgm = "clean1";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.UseBestServer = false;
            _args.IsInteractive = true;

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            var mvars = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Mvar).ToList();
            var macros = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Macro).ToList();

            using (var c = new SasContext())
            {
                // check macros against db
                var db = c.Programs.Include(x => x.Macros).Where(x => x.Program.Contains(pgm)).FirstOrDefault();
                Assert.IsNotNull(db);

                foreach (var m in macros)
                {
                    var entity = db.Macros.Where(x => x.Name == m.Name).FirstOrDefault();

                    Assert.IsNotNull(entity);
                    Assert.AreEqual(entity.Version, m.Value);
                }

                // check macro variables against db              
                Assert.AreEqual(db.Product, mvars.Where(x => x.Name == "PRODUCT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Protocol, mvars.Where(x => x.Name == "PROJECT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Analysis, mvars.Where(x => x.Name == "ANALYSIS").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Release, mvars.Where(x => x.Name == "DRAFT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.FolderLevel, mvars.Where(x => x.Name == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.IsLocal, mvars.Where(x => x.Name == "ISLOCALIZED").Select(x => x.Value).FirstOrDefault() == "1" ? true : false);

            }
        }

        /// <TestDescription>Capture the macro variables values from the environment when the program doesnt use init.sas</TestDescription>            
        /// <TestId>UT.PP01.03</TestId> 
        /// <ReqId>PP01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task GetMvarsFromEnvNoInit()
        {
            var pgm = "no-init";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.UseBestServer = false;
            _args.IsInteractive = true;

            SetPgms(Path.Combine(_root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            var mvars = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Mvar).ToList();
            var macros = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Macro).ToList();

            using (var c = new SasContext())
            {
                // check macros against db
                var db = c.Programs.Include(x => x.Macros).Where(x => x.Program.Contains(pgm)).FirstOrDefault();
                Assert.IsNotNull(db);

                foreach (var m in macros)
                {
                    var entity = db.Macros.Where(x => x.Name == m.Name).FirstOrDefault();

                    Assert.IsNotNull(entity);
                    Assert.AreEqual(entity.Version, m.Value);
                }

                // check macro variables against db              
                Assert.AreEqual(db.Product, mvars.Where(x => x.Name == "PRODUCT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Protocol, mvars.Where(x => x.Name == "PROJECT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Analysis, mvars.Where(x => x.Name == "ANALYSIS").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Release, mvars.Where(x => x.Name == "DRAFT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.FolderLevel, mvars.Where(x => x.Name == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault());

            }
        }


        /// <TestDescription>Capture the macro variables values from the environment if the listing destination is turned off above the analysis level</TestDescription>            
        /// <TestId>UT.PP01.04</TestId> 
        /// <ReqId>PP01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task GetMvarsFromEnvAboveAnalysis()
        {
            var root = @"O:\stat_prog_infra\testing\runsas\utilities\formats\pgms";
            var pgm = "test-protocol";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.UseBestServer = false;
            _args.IsInteractive = true;

            SetPgms(Path.Combine(root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            var mvars = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Mvar).ToList();
            var macros = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Macro).ToList();

            using (var c = new SasContext())
            {
                // check macros against db
                var db = c.Programs.Include(x => x.Macros).Where(x => x.Program.Contains(pgm)).FirstOrDefault();
                Assert.IsNotNull(db);

                foreach (var m in macros)
                {
                    var entity = db.Macros.Where(x => x.Name == m.Name).FirstOrDefault();

                    Assert.IsNotNull(entity);
                    Assert.AreEqual(entity.Version, m.Value);
                }

                // check macro variables against db              
                Assert.AreEqual(db.Product, mvars.Where(x => x.Name == "PRODUCT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Protocol, mvars.Where(x => x.Name == "PROJECT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Analysis, mvars.Where(x => x.Name == "ANALYSIS").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Release, mvars.Where(x => x.Name == "DRAFT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.FolderLevel, mvars.Where(x => x.Name == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.IsLocal, mvars.Where(x => x.Name == "ISLOCALIZED").Select(x => x.Value).FirstOrDefault() == "1" ? true : false);
            }
        }


        /// <TestDescription>Capture the macro variables values from the environment if the listing destination is turned off above the protocol level</TestDescription>            
        /// <TestId>UT.PP01.05</TestId> 
        /// <ReqId>PP01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task GetMvarsFromEnvAboveProtocol()
        {
            var root = @"O:\stat_prog_infra\testing\utilities\formats\pgms";
            var pgm = "test-product";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.UseBestServer = false;
            _args.IsInteractive = true;
            _args.Server = "sgsasv1-stg.sg.seagen.com";
            

            SetPgms(Path.Combine(root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            var mvars = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Mvar).ToList();
            var macros = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Macro).ToList();

            using (var c = new SasContext())
            {
                // check macros against db
                var db = c.Programs.Include(x => x.Macros).Where(x => x.Program.Contains(pgm)).FirstOrDefault();
                Assert.IsNotNull(db);

                foreach (var m in macros)
                {
                    var entity = db.Macros.Where(x => x.Name == m.Name).FirstOrDefault();

                    Assert.IsNotNull(entity);
                    Assert.AreEqual(entity.Version, m.Value);
                }

                // check macro variables against db              
                Assert.AreEqual(db.Product, mvars.Where(x => x.Name == "PRODUCT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Protocol, mvars.Where(x => x.Name == "PROJECT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Analysis, mvars.Where(x => x.Name == "ANALYSIS").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Release, mvars.Where(x => x.Name == "DRAFT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.FolderLevel, mvars.Where(x => x.Name == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.IsLocal, mvars.Where(x => x.Name == "ISLOCALIZED").Select(x => x.Value).FirstOrDefault() == "1" ? true : false);
            }
        }

        /// <TestDescription>Capture the macro variables values from the environment if the listing destination is turned off above the protocol level</TestDescription>            
        /// <TestId>UT.PP01.05</TestId> 
        /// <ReqId>PP01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task GetMvarsFromEnvAboveProduct()
        {
            var root = @"O:\stat_prog_infra\utilities\formats\pgms";
            var pgm = "test-dept";

            var msgs = new List<string>();

            _args.DoesResolveProgramCode = false;
            _args.DoesMacroLogging = true;
            _args.DoesNotifyOnCompletion = false;
            _args.UseBestServer = false;
            _args.IsInteractive = true;

            SetPgms(Path.Combine(root, pgm), ref _args);
            var sas = new SasManager(_args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);

            // Expected outcomes
            var mvars = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Mvar).ToList();
            var macros = sas.Workspaces[0].SasObjects.Where(x => x.Type == SasObjectType.Macro).ToList();

            using (var c = new SasContext())
            {
                // check macros against db
                var db = c.Programs.Include(x => x.Macros).Where(x => x.Program.Contains(pgm)).FirstOrDefault();
                Assert.IsNotNull(db);

                foreach (var m in macros)
                {
                    var entity = db.Macros.Where(x => x.Name == m.Name).FirstOrDefault();

                    Assert.IsNotNull(entity);
                    Assert.AreEqual(entity.Version, m.Value);
                }

                // check macro variables against db              
                Assert.AreEqual(db.Product, mvars.Where(x => x.Name == "PRODUCT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Protocol, mvars.Where(x => x.Name == "PROJECT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Analysis, mvars.Where(x => x.Name == "ANALYSIS").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.Release, mvars.Where(x => x.Name == "DRAFT").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.FolderLevel, mvars.Where(x => x.Name == "FOLDERLEVEL").Select(x => x.Value).FirstOrDefault());
                Assert.AreEqual(db.IsLocal, mvars.Where(x => x.Name == "ISLOCALIZED").Select(x => x.Value).FirstOrDefault() == "1" ? true : false);
            }
        }
        #endregion

        #region Version 2 tests
        /// <TestDescription>Programs run in the sequence defined in the 3rd pos of CSV and programs with same seq run in parallel. If quit is not
        /// specified, programs all execute even if an error is encountered</TestDescription>            
        /// <TestId>UT.MIX02.01</TestId> 
        /// <ReqId>MIX02.01,MIX03.01,BAT01.01</ReqId>
        /// <Version>2.0</Version>
        [TestMethod]
        public void DeriveOrderAsyncNoQuit()
        {
            var currDt = DateTime.Now;

            var result = new List<string>();
            string[] args ={"-c \"O:\\stat_prog_infra\\testing\\sjm\\dotnet\\use_ai\\data\\sdtm\\pgms\\bat01.csv\"",
                                "-s sgsasv1.sg.seagen.com",
                                @"-f O:\stat_prog_infra\testing\sjm\dotnet\use_ai\utilities\ai\ai.xlsx",
                                @"-m false",
                                @"-i false",
                                @"-d true",
                                @"-t true",
                                @"-h O:\stat_prog_infra\testing\sjm\dotnet\use_ai\data\sdtm\pgms\bat01.html",
                                "-q false",
                                "-a true"
                                };


            var psi = new ProcessStartInfo(_cli)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = string.Join(' ', args)
            };

            Process proc = new Process() { StartInfo = psi };
            proc.Start();

            proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    result.Add(e.Data);
            };
            proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    result.Add($"ERROR: {e.Data}");
            };

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();

            // Programs run in groups defined in correct sequence
            var mh = result.FirstOrDefault(x => x.Contains("submitting dm seq", StringComparison.OrdinalIgnoreCase));
            var mhPos = mh != null ? result.IndexOf(mh) : 0;
            Assert.IsTrue(mhPos > 0);

            var aeCm = result.FirstOrDefault(x => x.Contains("submitting 2 programs in parallel", StringComparison.OrdinalIgnoreCase));
            var aeCmPost = aeCm != null ? result.IndexOf(aeCm) : 0;
            Assert.IsTrue(aeCmPost > 0);

            Assert.IsTrue(aeCmPost > mhPos);

            // All programs complete
            Assert.IsNotNull(result.FirstOrDefault(x => x.Contains("8 of 8 completed")));


        }


        /// <TestDescription>Application terminates execution if -q=true and a SAS error is encountered in Mixed Mode (-d=true -a=true)</TestDescription>            
        /// <TestId>UT.BAT02.01</TestId> 
        /// <ReqId>BAT02.01</ReqId>
        /// <Version>2.0</Version>
        [TestMethod]
        public void QuitMixed()
        {
            var currDt = DateTime.Now;

            var result = new List<string>();
            string[] args ={"-c \"O:\\stat_prog_infra\\testing\\sjm\\dotnet\\use_ai\\data\\sdtm\\pgms\\bat01.csv\"",
                                "-s sgsasv1.sg.seagen.com",
                                @"-f O:\stat_prog_infra\testing\sjm\dotnet\use_ai\utilities\ai\ai.xlsx",
                                @"-m false",
                                @"-i false",
                                @"-d true",
                                @"-t true",
                                @"-h O:\stat_prog_infra\testing\sjm\dotnet\use_ai\data\sdtm\pgms\bat01.html",
                                "-q true",
                                "-a true"
                                };


            var psi = new ProcessStartInfo(_cli)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = string.Join(' ', args)
            };

            Process proc = new Process() { StartInfo = psi };
            proc.Start();

            proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    result.Add(e.Data);
            };
            proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    result.Add($"ERROR: {e.Data}");
            };

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();


            for (int i = 0; i < result.Count; i++)
            {
                if (Regex.IsMatch(result[i], "terminating execution.*mh.*ERROR", RegexOptions.IgnoreCase))
                {
                    // No other programs ran after MH as each program generates at least 3 lines
                    Assert.IsTrue(result.Count - (i + 1) <= 3);
                }
            }


        }
        /// <TestDescription>Application terminates execution if -q=true and a SAS error is encountered in Sequential Mode (-d=false -a=false)</TestDescription>            
        /// <TestId>UT.BAT02.01</TestId> 
        /// <ReqId>BAT02.01</ReqId>
        /// <Version>2.0</Version>
        [TestMethod]
        public void QuitSequential()
        {
            var currDt = DateTime.Now;

            var result = new List<string>();
            string[] args ={"-c \"O:\\stat_prog_infra\\testing\\sjm\\dotnet\\use_ai\\data\\sdtm\\pgms\\bat01.csv\"",
                                "-s sgsasv1.sg.seagen.com",
                                @"-f O:\stat_prog_infra\testing\sjm\dotnet\use_ai\utilities\ai\ai.xlsx",
                                @"-m false",
                                @"-i false",
                                @"-d false",
                                @"-t true",
                                @"-h O:\stat_prog_infra\testing\sjm\dotnet\use_ai\data\sdtm\pgms\bat01.html",
                                "-q true",
                                "-a false"
                                };


            var psi = new ProcessStartInfo(_cli)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                Arguments = string.Join(' ', args)
            };

            Process proc = new Process() { StartInfo = psi };
            proc.Start();

            proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    result.Add(e.Data);
            };
            proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
            {
                if (e.Data != null)
                    result.Add($"ERROR: {e.Data}");
            };

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();


            for (int i = 0; i < result.Count; i++)
            {
                if (Regex.IsMatch(result[i], "terminating execution.*mh.*ERROR", RegexOptions.IgnoreCase))
                {
                    // No other programs ran after MH as each program generates at least 3 lines
                    Assert.IsTrue(result.Count - (i + 1) <= 2);
                }
            }
        }
        /// <TestDescription>Confirm tester and developer are written in the summary table of the HTML summary</TestDescription>            
        /// <TestId>UT.SUM01.01</TestId> 
        /// <ReqId>SUM01.01</ReqId>
        /// <Version>2.0</Version>
        [TestMethod]
        public async Task DisplayProgrammers()
        {

            var msgs = new List<string>();
            var args = new Args(_server, _context, _port);
            args.IsAsync = true;
            args.UseBestServer = false;
            args.DoesResolveProgramCode = false;
            args.DoesMacroLogging = false;
            args.DoesNotifyOnCompletion = false;
            args.BaseMetricsUrl = "http://spi2:8030";
            args.MetricsUrl = "api/usage";
            args.DoesCheckLog = true;
            args.DoesResolveProgramCode = false;
            args.DoesMacroLogging = false;
            args.DoesNotifyOnCompletion = false;
            args.UseBestServer = false;
            args.IsInteractive = false;


            args.DoesDisplayProgrammers = true;
            args.Ai = _ai;

            args.SummaryFn = "O:\\stat_prog_infra\\testing\\sjm\\dotnet\\use_ai\\data\\sdtm\\pgms\\bat01-02.html";

            var path = @"O:\stat_prog_infra\testing\sjm\dotnet\use_ai\data\sdtm";
            string[] pgms = { "ae", "cm", "mh", "dm",
                              "v-ae", "v-cm", "v-mh", "v-dm" };
            for (int i = 0; i < pgms.Length; i++)
            {
                var pgm = Path.Combine(path, i > 3 ? "testing" : "pgms", pgms[i]);
                SetPgms(pgm, ref args, i + 1, i > 3 ? true : false);
            }

            // add programmers and testers
            foreach (var p in args.Programs)
            {
                p.Developer = "mness";
                p.Tester = "atella";
            }

            var sas = new SasManager(args, new SasContext());


            var progress = new Progress<string>(value =>
            {
                msgs.Add(value);
            });

            await sas.Run(progress);
            sas.WriteSummary(false);

            string line;
            bool isFoundQcSummary = false;
            using (StreamReader sr = new StreamReader(args.SummaryFn))
            {
                // Count the HTML tags with dev and tester
                int counter=0;
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.Contains("<td>mness</td>"))
                        counter++;
                    if (line.Contains("<td>atella</td>"))
                        counter++;
                    // find the QC summary
                    if (Regex.IsMatch(line, "automated qc tests:\\s+\\(\\d+\\s+passed\\/\\d+\\s+total\\)",RegexOptions.IgnoreCase))
                    {
                        isFoundQcSummary = true;
                    }
                }
                Assert.AreEqual(counter, 16);
                Assert.IsTrue(isFoundQcSummary);
            }
        }




        #endregion

        #region Scheduler

        [TestMethod]
        public void RunAsyncSuccess()
        {

        }

        #endregion

        #region Version 3
        /// <TestDescription>Run the same programs 2x and ensure the logs are read-only after each completion</TestDescription>            
        /// <TestId>UT.LOG.01.01</TestId> 
        /// <ReqId>LOG.01.01,LOG.01.02</ReqId>
        /// <Version>1.0</Version>
        [TestMethod]
        public async Task SetLogsToReadOnly()
        {
            for (int i = 0; i == 1; i++)
            {
                var path = @"O:\stat_prog_infra\testing\sjm\secure_log\v3_0\data\adam\pgms";
                var pgm = new List<string> { "test", "test1" };
                var msgs = new List<string>();

                _args.DoesResolveProgramCode = false;
                _args.DoesMacroLogging = false;
                _args.DoesNotifyOnCompletion = false;

                _args.Mvars.Add("myvar1", "this is only a test");
                _args.Mvars.Add("myvar2", "O:\\this\\those");
                _args.Mvars.Add("myvar3", "&sysdate9");

                SetPgms(path, pgm, ref _args);
                _args.SummaryFn = @$"{Path.Combine(path, "run_summaries")}\summary-{Environment.UserName}-{(DateTime.Now.ToString("yyyyMMddHHmmss"))}.html";
                var sas = new SasManager(_args, new SasContext());


                var progress = new Progress<string>(value =>
                {
                    msgs.Add(value);
                });

                await sas.Run(progress);
                sas.WriteSummary(false);

                // Expected outcomes
                foreach (var f in _args.Programs)
                {
                    var fi = new FileInfo(f.LogFn);
                    Assert.IsTrue(fi.Attributes.HasFlag(FileAttributes.ReadOnly));
                }
            }
        }

        #endregion
        #region utilities
        public void SetPgms(string pgm, ref Args args, bool isQc = false)
        {
            args.Programs.Clear();
            var pgms = new List<SasProgram>() { new SasProgram($"{pgm}.sas", isQc, 1) };
            args.Programs = pgms;
        }
        public void SetPgms(string pgm, ref Args args, int seq, bool isQc = false)
        {
            var saspgm = new SasProgram($"{pgm}.sas", isQc, seq);
            args.Programs.Add(saspgm);
        }
        public void SetPgms(List<string> pgm, ref Args args, bool isQc = false)
        {
            args.Programs.Clear();
            var pgms = new List<SasProgram>();


            foreach (var (c, i) in pgm.Select((c, i) => (c, i)))
            {
                pgms.Add(new SasProgram(Path.Combine(_root, $"{c}.sas"), isQc, i));
            }

            args.Programs = pgms;
        }
        public void SetPgms(string path,List<string> pgm, ref Args args, bool isQc = false)
        {
            args.Programs.Clear();
            var pgms = new List<SasProgram>();


            foreach (var (c, i) in pgm.Select((c, i) => (c, i)))
            {
                pgms.Add(new SasProgram(Path.Combine(path, $"{c}.sas"), isQc, i));
            }

            args.Programs = pgms;
        }


        #endregion
    }
}

