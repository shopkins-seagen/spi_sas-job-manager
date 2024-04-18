using CommandLine;
using SasJobManager.ServerLib.Models;
using SasJobManager.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClosedXML.Excel;
using System.Text.RegularExpressions;

namespace SasJobManager.Cli.Models
{
    public class CmdArgs
    {
        private IEnumerable<string> _pgms;
        private IEnumerable<string> _recipients;
        private string? _csv;
        private string? _server;
        private string? _context;
        private string? _isAsyncStr;
        private string? _doesMacroLoggingStr;
        private string? _doesResolveCodeStr;
        private string? _doesCheckLogStr;
        private string? _doesNotifyOnCompletionStr;
        //private string? _useBestServerStr;
        private string? _reviewLogsOnlyStr;
        private string? _isInteractiveStr;
        private string _htmlSummaryFn;
        private string? _ai;
        private string? _useDeriveOrder;
        private string? _displayProgrammers;
        private string? _doesQuitOnError;
        private string? _includeAutoQcReviewStr;
        private string? _doesExportSummaryStr;
        private string? _mvars;





        public CmdArgs()
        {
            Programs = new List<SasProgram>();
            Logger = new List<LogEntry>();

        }
        public List<LogEntry> Logger;
        public bool HasDeriveOrder { get; set; }

        [Option('f', HelpText = "[Optional]\nName of the AI file.")]
        public string? Ai
        {
            get { return _ai; }
            set
            {
                _ai = value;
            }
        }
        [Option('g', HelpText = "[Optional]\nKey:value pairs of macro varialbe to assign in the program")]
        public string? Mvars
        {
            get { return _mvars; }
            set
            {
                _mvars = value;
            }
        }


        [Option('h', HelpText = "[Required]\nPath and name of the HTML summary file.")]
        public string HtmlSummaryFn
        {
            get { return _htmlSummaryFn; }
            set
            {
                _htmlSummaryFn = value;
            }
        }

        [Option('p', Separator = ',', HelpText = "[Required(if -c is not specified)]\nSpace-delimited list of SAS programs including path")]
        public IEnumerable<string> Pgms
        {
            get => _pgms;
            set
            {
                _pgms = value;
            }
        }
        [Option('u', Separator = ',', HelpText = "[Optional]\nSpace-delimited list of user accounts to notify on completion of job." +
            "\nValid values:\n\t[blank] (default) - current user\n\tspace-delimited list of network accounts (e.g. -u shopkins atella")]
        public IEnumerable<string> Recipients
        {
            get => _recipients;
            set
            {
                _recipients = value;
            }
        }

        [Option('c', HelpText = "[Required(if -p is not specified)]\nCSV file that identifies the list of SAS programs to execute")]
        public string? Csv
        {
            get => _csv;
            set
            {
                _csv = value;
            }
        }
        [Option('s', HelpText = "[Required]\nName of server hosting the SAS application\nValid values: \n\tsgsasv1.sg.seagen.com\n\tsgsasv1-stg.sg.seagen.com\n\tbest")]
        public string? Server
        {
            get => _server;
            set
            {                
                _server = value;
            }
        }

        [Option('x', HelpText = "[Optional]\nServer context\nValid values:\n\tSASApp94 (default)\n\tSasAppUTF8 (UTF-8 MBCS support)")]
        public string? Context
        {
            get => _context;
            set
            {
                _context = value;
            }
        }

        [Option('a', HelpText = "[Optional]\nBoolean that indicates if the programs are submitted " +
            "sequentially(e.g. program completes before next program starts), or in parallel (e.g. all programs submitted asynchronously)" +
            "\nValid values:\n\ttrue (default) - submit in parallel\n\tfalse - submit synchronously")]
        public string? IsAsyncStr
        {
            get => _isAsyncStr;
            set
            {
                _isAsyncStr = value;
                if (bool.TryParse(value, out bool isSeq))
                {
                    IsAsync = isSeq;
                }
            }
        }
        
        [Option('e', HelpText = "[Optional]\nBoolean that indicates is the data for QC summary is export to an Excel file" +
            "\nValid values:\n\ttrue - Export the SAS log summary data to Excel\n\tfalse (default) - do not export SAS log summary to Excel")]
        public string? DoesExportSummaryStr
        {
            get => _doesExportSummaryStr;
            set
            {
                _doesExportSummaryStr = value;
                if (bool.TryParse(value, out bool doesExportSummary))
                {
                    DoesExportSummary = doesExportSummary;
                }
            }
        }

        [Option('q', HelpText = "[Optional]\nBoolean that indicates if batch execution terminates if a SAS error is encountered " +
            "\nValid values:\n\ttrue - processing terminates is an error in a primary program is encountered\n\tfalse (default) - execution is not terminated if an error is encountered")]
        public string? DoesQuitOnErrStr
        {
            get { return _doesQuitOnError; }
            set
            {
                _doesQuitOnError = value;
                if (bool.TryParse(value, out bool doesTerminateOnError))
                {
                    DoesQuitOnError = doesTerminateOnError;
                }
            }
        }

        [Option('l', HelpText = "[Optional]\nBoolean that indicates if post-processing (e.g. log review) is performed after program are executed." +
            "\nValid values:\n\ttrue (default) - review the log and generate summary\n\tfalse - do not review the log")]
        public string? DoesCheckLogStr
        {
            get => _doesCheckLogStr;
            set
            {
                _doesCheckLogStr = value;
                if (bool.TryParse(value, out bool doesCheckLog))
                {
                    DoesCheckLog = doesCheckLog;
                }
            }
        }
        [Option('d', HelpText = "[Optional]\nGroup and order the programs by the int in the 3rd pos of csv")]
        public string UseDeriveOrderStr
        {
            get { return _useDeriveOrder; }
            set
            {
                _useDeriveOrder = value;
                if (bool.TryParse(value, out bool useDeriveOrder))
                {
                    UseDeriveOrder = useDeriveOrder;
                }
            }
        }
        [Option('m', HelpText = "[Optional]\nBoolean that indicates macros are logged for AIM.\nValid values:\n\ttrue (default) - record macros" +
            "\n\tfalse - do not record macros")]
        public string? DoesMacroLoggingStr
        {
            get => _doesMacroLoggingStr;
            set
            {
                _doesMacroLoggingStr = value;
                if (bool.TryParse(value, out bool doesMacroLogging))
                {
                    DoesMacroLogging = doesMacroLogging;
                }
            }
        }
        [Option('r', HelpText = "[Optional]\nBoolean that indicates if SAS program code is resolved (e.g. no external dependencies or macros) in a " +
            "corresponding program in 'resolved' subdirectory. Used to deliver runnable programs to an external recipient without providing infrastructure" +
            "\nValid values:\n\ttrue - creates a corresponding program with resolved code\n\tfalse (default) - do not create resolved program files")]
        public string? DoesResolveCodeStr
        {
            get => _doesResolveCodeStr;
            set
            {
                _doesResolveCodeStr = value;
                if (bool.TryParse(value, out bool doesResolveCode))
                {
                    DoesResolveCode = doesResolveCode;
                }
            }
        }

        [Option('n', HelpText = "[Optional]\nBoolean that indicates if user is notified via email at completion.\nValid values" +
            "\n\ttrue - send a email message to [recipients] or if recienpts is not provided, send message to current user" +
            "\n\tfalse (default) - do not send email message")]
        public string? DoesNotifyOnCompletionStr
        {
            get => _doesNotifyOnCompletionStr;
            set
            {
                _doesNotifyOnCompletionStr = value;
                if (bool.TryParse(value, out bool doesNotifyOnCompletion))
                {
                    DoesNotifyOnCompletion = doesNotifyOnCompletion;
                }
            }
        }

        [Option('o', HelpText = "[Optional]\nBoolean that indicates app just performs log review and summary for the logs specified in -p\nValid values" +
                "\n\ttrue - reviews and summarized the log files specified in -p\n\tfalse (default) - processesof SAS file as specfied", Default = "false")]
        public string? ReviewLogsOnlyStr
        {
            get { return _reviewLogsOnlyStr; }
            set
            {
                _reviewLogsOnlyStr = value;
                if (bool.TryParse(value, out bool reviewLogsOnly))
                {
                    ReviewLogsOnly = reviewLogsOnly;
                }
            }
        }

        [Option('b', HelpText = "[Optional]\nBoolean that indicates app includes QC summary. Used with -o true to add QC review\nValid values" +
        "\n\ttrue - Add QC review to the Only review log action\n\tfalse (default) - processesof SAS file as specfied", Default = "false")]
        public string? IncludeAutoQcReviewStr
        {
            get { return _includeAutoQcReviewStr; }
            set
            {
                _includeAutoQcReviewStr = value;
                if (bool.TryParse(value, out bool includeAutoQc))
                {
                    IncludeAutoQcReview = includeAutoQc;
                }
            }
        }


        [Option('i', HelpText = "[Optional]\nBoolean that indicates app opens the HTML log summary interactively\nValid values" +
        "\n\ttrue (default) - opens the log summary\n\tfalse - does not open the log file. Use for automation or unmonitored runs", Default = "true")]
        public string? IsInteractiveStr
        {
            get { return _isInteractiveStr; }
            set
            {
                _isInteractiveStr = value;
                if (bool.TryParse(value, out bool isInteractive))
                {
                    IsInteractive = isInteractive;
                }
            }
        }

        [Option('t', HelpText = "[Optional]\nBoolean that indicates if programmer and tester are displayed in HTML summary (requires AI)\nValid values" +
        "\n\ttrue - Displays programmer and tester in summary if they are recorded in AI\n\tfalse (default)- does not display programmer and tester", Default = "false")]
        public string? DisplayProgrammersStr
        {
            get { return _displayProgrammers; }
            set
            {
                _displayProgrammers = value;
                if (bool.TryParse(value, out bool displayProgrammers))
                {
                    DisplayProgrammers = displayProgrammers;
                }
            }
        }


        public List<SasProgram> Programs { get; set; }
        public bool? IsAsync { get; set; }
        public bool? DoesCheckLog { get; set; }
        public bool? DoesMacroLogging { get; set; }
        public bool? DoesResolveCode { get; set; }
        public bool? DoesNotifyOnCompletion { get; set; }
        public bool? ReviewLogsOnly { get; set; }
        public bool? IsInteractive { get; set; }
        public bool? UseDeriveOrder { get; set; }
        public bool? DisplayProgrammers { get; set; }
        public bool? DoesQuitOnError { get; set; }
        public bool? IncludeAutoQcReview { get; set; }
        public bool? DoesExportSummary { get; set; }

        public void SetPrograms()
        {

            if (!string.IsNullOrEmpty(this.Csv))
            {
                if (File.Exists(this.Csv))
                {
                    Programs.AddRange(ReadCsv());
                }
                else
                {
                    Logger.Add(new LogEntry()
                    {
                        Msg = $"The CSV file {Csv} could not be located",
                        IssueType = IssueType.Fatal,
                        Source = "CmdArgs.SetPrograms"
                    });
                }
            }
            else
            {
                int counter = 0;
                foreach (var p in Pgms)
                {
                    if (!Regex.IsMatch(Path.GetExtension(p) ?? string.Empty, "(sas|log)", RegexOptions.IgnoreCase))
                    {
                        Logger.Add(new LogEntry()
                        {
                            Msg = $"A CSV file was specfied using the -p flag. -p Accepts programs, -c accepts .csv files",
                            IssueType = IssueType.Fatal,
                            Source = "CmdArgs.SetPrograms"
                        });
                        return;
                    }
                    counter++;
                    var pgm = new SasProgram(p, false, counter);
                    Programs.Add(pgm);
                }
            }
            if (DisplayProgrammers.GetValueOrDefault())
            {
                if (!string.IsNullOrEmpty(Ai))
                {
                    AddProgrammers();
                }
                else
                {
                    Logger.Add(new LogEntry()
                    {
                        Msg = $"Display programmer and tester is true (-t), but the AI file {Ai} was not specified (-f)",
                        IssueType = IssueType.Warning,
                        Source = "CmdArgs.SetPrograms"
                    });
                    DisplayProgrammers = false;
                }
            }
        }

        private void AddProgrammers()
        {

            if (!File.Exists(Ai))
            {
                Logger.Add(new LogEntry()
                {
                    Msg = $"The AI file {Ai} could not be found. Developer and Tester will not appear in summary",
                    IssueType = IssueType.Warning,
                    Source = "CmdArgs.AddProgrammers"
                });
                return;
            }
            var lookUp = ReadAi();
            if (lookUp.Count > 0)
            {
                foreach (var p in Programs)
                {
                    if (lookUp.TryGetValue(p.Name(), out Programmers programmers))
                    {                        
                        p.Developer = programmers.Developer;
                        p.Tester = programmers.Tester;
                        if (!p.IsQc)
                            NotifyMissingProgrammer(programmers,p.Name());
                    }
                    else
                    {
                        Logger.Add(new LogEntry()
                        {
                            Msg = $"Could not locate program {p.Name()} in AI to get programmer and tester details",
                            IssueType = IssueType.Note,
                            Source = "CmdArgs.AddProgrammers"
                        });
                    }
                }
            }
            else
            {
                Logger.Add(new LogEntry()
                {
                    Msg = $"No matching programmers were found for the selected programs. Dev and tester will not be displayed in summary",
                    IssueType = IssueType.Note,
                    Source = "CmdArgs.SetPrograms"
                });
                DisplayProgrammers = false;
            }

        }

        private void NotifyMissingProgrammer(Programmers programmers, string v)
        {
            var vars = new List<string>();

            if (string.IsNullOrEmpty(programmers.Developer))
                vars.Add("developer");
            if (string.IsNullOrEmpty(programmers.Tester))
                vars.Add("tester");
  
            if (vars.Count > 0)
            {
                var msg = $"{string.Join(" and ", vars)} could not be located for program '{v}.sas'";
                Logger.Add(new LogEntry()
                {
                    Msg = msg,
                    IssueType = IssueType.Note,
                    Source = "CmdArgs.AddProgrammers"
                });
            }           
        }

        private Dictionary<string, Programmers> ReadAi()
        {
            string[] sheets = { "TLFs", "Datasets" };
            var dict = new Dictionary<string, Programmers>();
            try
            {
                using (var wb = new XLWorkbook(Ai))
                {
                    foreach (var sheet in sheets)
                    {
                        try
                        {
                            var ws = wb.Worksheet(sheet);
                            if (ws != null)
                            {

                                var headings = ws.Row(1).CellsUsed();
                                var pgmPos = headings.Where(x => x.Value.ToString().ToLower() == "program").FirstOrDefault()?.Address.ColumnLetter;
                                var devPos = headings.Where(x => x.Value.ToString().ToLower() == "developer").FirstOrDefault()?.Address.ColumnLetter;
                                var testPos = headings.Where(x => x.Value.ToString().ToLower() == "tester").FirstOrDefault()?.Address.ColumnLetter;
                                if (devPos == null || testPos == null || pgmPos == null)
                                {
                                    Logger.Add(new LogEntry()
                                    {
                                        Msg = $"Could not locate program name, Developer, or Tester in the {sheet} worksheet. Developer and tester will not be displayed in the summary",
                                        IssueType = IssueType.Warning,
                                        Source = "CmdArgs.ReadAi"
                                    });
                                    return new Dictionary<string, Programmers>();
                                }
                                foreach (var row in ws.RowsUsed().Skip(1))
                                {
                                    var pgm = row.Cell(pgmPos).Value.ToString();
                                    if (!string.IsNullOrEmpty(pgm))
                                    {
                                        pgm = Path.GetFileNameWithoutExtension(pgm);

                                        if (!dict.ContainsKey(pgm))
                                        {
                                            dict.TryAdd(pgm, new Programmers(row.Cell(devPos).Value.ToString(),
                                                                         row.Cell(testPos).Value.ToString()));
                                            dict.TryAdd($"v-{pgm}", new Programmers(row.Cell(devPos).Value.ToString(),
                                                                         row.Cell(testPos).Value.ToString()));
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Add(new LogEntry()
                            {
                                Msg = $"ERROR encountered trying to read AI to get developer and tester: {ex.Message}",
                                IssueType = IssueType.Warning,
                                Source = "CmdArgs.ReadAi"
                            });
                            return new Dictionary<string, Programmers>();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.Add(new LogEntry()
                {
                    Msg = $"ERROR encountered trying to read AI to get developer and tester: {ex.Message}",
                    IssueType = IssueType.Warning,
                    Source = "CmdArgs.ReadAi"
                });

                return new Dictionary<string, Programmers>();
            }

            return dict;
        }



        private IEnumerable<SasProgram> ReadCsv()
        {
            string line = string.Empty;
            using (var fs = new FileStream(Csv, FileMode.Open, FileAccess.Read))
            {
                using (var r = new StreamReader(fs))
                {
                    int counter = 0;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line.IndexOf(',') > 0)
                        {
                            var vals = line.Split(',');

                            counter++;
                            var isQc = vals.Length > 1 ? vals[1].Trim() == "2" ? true : false : false;
                            var pgm = new SasProgram(vals[0], isQc, counter);
                            if (UseDeriveOrder.GetValueOrDefault())
                            {
                                try
                                {
                                    if (int.TryParse(vals[2], out int ord))
                                    {
                                        pgm.DeriveOrder = ord;
                                        HasDeriveOrder = true;
                                    }
                                    else
                                    {
                                        Logger.Add(new LogEntry()
                                        {
                                            Msg = $"The program {pgm.Name} has an invalid value for order. Correct order or do not set the '-d true' option",
                                            IssueType = IssueType.Fatal,
                                            Source = "CmdArgs.ReadCSV"
                                        });
                                        UseDeriveOrder = false;

                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.Add(new LogEntry()
                                    {
                                        Msg = $"If -d=true the 3rd position in the CSV must be an integer.",
                                        IssueType = IssueType.Fatal,
                                        Source = "CmdArgs.ReadCSV"
                                    });
                                    UseDeriveOrder = false;
                                }
                            }

                            yield return pgm;
                        }
                    }
                }
            }
        }
    }
}

