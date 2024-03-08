using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using MediaFoundation.EVR;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Prism.Commands;
using SasJobManager.Lib.Models;
using SasJobManager.UI.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace SasJobManager.UI.ViewModels
{
    public partial class MainViewModel : NotifyDataErrorInfoBase, INotifyPropertyChanged
    {

        private string _filter;
        private List<SasProgramFile> _allPgms;
        private bool _doesNotifyOnCompletion;
        private RunMode _mode;
        private bool _doesCheckLog;
        private bool _doesRecordMacros;
        private ServerContext _serverContext;
        private bool _doesResolveCode;
        private Server _server;
        private string _folder;
        private IConfigurationRoot _cfg;
        private string _message;
        private bool _isBusy;
        private bool _isIncludeAllChecked;
        private bool _isQcAllChecked;
        private bool _isAzSort;
        private string _messages;
        private ServerMetrics _metrics;
        private bool _isExpanded;
        private int _prodCpu;
        private int _stgCpu;
        private string _batFileName;
        private bool _doesCreateBatFile;
        private bool _isProductionServer;
        private bool _isStageServer;
        private bool _isBestServer;
        private string? _ai;

        private bool _addQcPgms;
        private bool _displayProgrammersInSummary;
        private bool _useDeriveOrder;
        private bool _useAiPrograms;
        private bool _doesQuitOnError;








        public MainViewModel()
        {
            Programs = new ObservableCollection<SasProgramFile>();
            BatFilePrograms = new ObservableCollection<SasProgramFile>();

            _allPgms = new List<SasProgramFile>();
            LoadConfig();
            SubmitCommand = new DelegateCommand(OnSubmitExecute, CanSubmitExecute);
            ToggleIncludeCommand = new DelegateCommand(OnToggleInclude);
            ToggleQcCommand = new DelegateCommand(OnToggleQc);
            ToggleSortCommand = new DelegateCommand(OnToggleSort);
            ResizeCommand = new DelegateCommand(OnResize);
            RefreshProgramsCommand = new DelegateCommand(OnRefresh);
            CpuUtilizationCommand = new DelegateCommand(OnCpuUtilization);
            AllCpuUtilizationCommand = new DelegateCommand(OnCpuUtilizationAllServers);
            SelectFolderCommand = new DelegateCommand(OnSelectFolder);
            SelectServerCommand = new DelegateCommand<object>(OnSelectServer);
            ServerHelpCommand = new DelegateCommand(OnServerHelp);
            HelpCommand = new DelegateCommand(OnHelp);
            CreateBatchFileCommand = new DelegateCommand(OnCreateBatchFileExecute, CanCreateBatchFile);
            SelectAiCommand = new DelegateCommand(OnSelectAi);
            ClearAiCommand = new DelegateCommand(OnClearAi, CanClearAi);
            HelpCreateBatchFileCommand = new DelegateCommand(OnHelpCreateBatchFile);
            Message = String.Empty;
            IsAzSort = false;

        }



        public event PropertyChangedEventHandler PropertyChanged;
        public ObservableCollection<SasProgramFile> Programs { get; set; }

        #region Batch file generation properties

        public ObservableCollection<SasProgramFile> BatFilePrograms { get; set; }

        public bool UseAiPrograms
        {
            get { return _useAiPrograms; }
            set
            {
                _useAiPrograms = value;
                OnPropertyChanged();
                if (!value)
                {
                    UseDeriveOrder = false;
                }
            }
        }

        public bool UseDeriveOrder
        {
            get { return _useDeriveOrder; }
            set
            {
                _useDeriveOrder = value;
                OnPropertyChanged();
            }
        }
        public bool DoesQuitOnError
        {
            get { return _doesQuitOnError; }
            set
            {
                _doesQuitOnError = value;
                OnPropertyChanged();
            }
        }
        public bool DisplayProgrammersInSummary
        {
            get { return _displayProgrammersInSummary; }
            set
            {
                _displayProgrammersInSummary = value;
                OnPropertyChanged();
            }
        }

        public bool AddQcPgms
        {
            get { return _addQcPgms; }
            set
            {
                _addQcPgms = value;
                OnPropertyChanged();
            }
        }

        public string? Ai
        {
            get { return _ai; }
            set
            {
                _ai = value;
                OnPropertyChanged();
                ((DelegateCommand)ClearAiCommand).RaiseCanExecuteChanged();
            }
        }

        public string BatFileName
        {
            get { return _batFileName; }
            set
            {
                _batFileName = value;
                OnPropertyChanged();
                ValidateProperty(nameof(BatFileName));

            }
        }

        public bool DoesCreateBatFile
        {
            get { return _doesCreateBatFile; }
            set
            {
                _doesCreateBatFile = value;
                OnPropertyChanged();
                ValidateProperty(nameof(BatFileName));
                ((DelegateCommand)CreateBatchFileCommand).RaiseCanExecuteChanged();
                if (value == false)
                {
                    Ai = null;
                    UseDeriveOrder = false;
                    DisplayProgrammersInSummary = false;
                    UseAiPrograms = false;
                }
            }
        }

        #endregion

        public Server Server
        {
            get { return _server; }
            set
            {
                _server = value;
                IsStageServer = _server == Server.stage;
                IsProductionServer = _server == Server.production;
                IsBestServer = _server == Server.best;

                OnPropertyChanged();
            }
        }

        public bool IsProductionServer
        {
            get { return _isProductionServer; }
            set
            {
                _isProductionServer = value;
                OnPropertyChanged();
            }
        }
        public bool IsStageServer
        {
            get { return _isStageServer; }
            set
            {
                _isStageServer = value;
                OnPropertyChanged();
            }
        }


        public bool IsBestServer
        {
            get { return _isBestServer; }
            set
            {
                _isBestServer = value;
                OnPropertyChanged();
            }
        }



        public int ProdCpu
        {
            get { return _prodCpu; }
            set
            {
                _prodCpu = value;
                OnPropertyChanged();
            }
        }

        public int StgCpu
        {
            get { return _stgCpu; }
            set
            {
                _stgCpu = value;
                OnPropertyChanged();
            }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public string Folder
        {
            get { return _folder; }
            set
            {
                _folder = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                if (SubmitCommand != null)
                {
                    ((DelegateCommand)SubmitCommand).RaiseCanExecuteChanged();
                    ((DelegateCommand)CreateBatchFileCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                OnPropertyChanged();
                FilterResults();
            }
        }


        public ServerContext ServerContext
        {
            get { return _serverContext; }
            set
            {
                _serverContext = value;
                OnPropertyChanged();
            }
        }

        public RunMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                OnPropertyChanged();
            }
        }

        public bool DoesCheckLog
        {
            get { return _doesCheckLog; }
            set
            {
                _doesCheckLog = value;
                OnPropertyChanged();
            }
        }


        public bool DoesRecordMacros
        {
            get { return _doesRecordMacros; }
            set
            {
                _doesRecordMacros = value;
                OnPropertyChanged();
            }
        }
        public bool DoesResolveCode
        {
            get { return _doesResolveCode; }
            set
            {
                _doesResolveCode = value;
                OnPropertyChanged();
            }
        }

        public bool DoesNotifyOnCompletion
        {
            get { return _doesNotifyOnCompletion; }
            set
            {
                _doesNotifyOnCompletion = value;
                OnPropertyChanged();
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                if (!string.IsNullOrEmpty(value))
                    Messages += $"\n{value}";
                OnPropertyChanged();
            }
        }


        public string Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged();
            }
        }


        public bool IsIncludeAllChecked
        {
            get { return _isIncludeAllChecked; }
            set
            {
                _isIncludeAllChecked = value;
                OnPropertyChanged();
            }
        }
        public bool IsQcAllChecked
        {
            get { return _isQcAllChecked; }
            set
            {
                _isQcAllChecked = value;
                OnPropertyChanged();
            }
        }
        public bool IsAzSort
        {
            get { return _isAzSort; }
            set
            {
                _isAzSort = value;
                OnPropertyChanged();
            }
        }

        private string _serverName;

        public string ServerName
        {
            get { return _serverName; }
            set
            {
                _serverName = value;
                OnPropertyChanged();
            }
        }

        public async Task Load(string path, List<string> files)
        {
            Programs.Clear();
            await Task.Run(() =>
            {
                _allPgms.Clear();


                Folder = path;
                var di = new DirectoryInfo(path);
                var rgx = new Regex(_cfg["restrict_sas_pgms_rgx"], RegexOptions.IgnoreCase);

                foreach (var (f, i) in di.GetFiles("*.sas", SearchOption.TopDirectoryOnly).OrderBy(x => x.Name).Select((f, i) => (f, i)))
                {
                    if (!rgx.IsMatch(f.Name))
                    {
                        bool isSelected = files.Contains(f.Name);
                        var pgm = new SasProgramFile(f.Name, path, i);
                        pgm.IsSelected = isSelected;

                        _allPgms.Add(pgm);
                    }
                }
            });

            Programs.AddRange(_allPgms);
        }

        private void LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_ui.json", true, true);
            _cfg = builder.Build();
            SetDefaults();
        }
        private async void SetDefaults()
        {
            Server = Enum.Parse<Server>(_cfg["server"]);

            ServerContext = Enum.Parse<ServerContext>(_cfg["server_context"]);
            Mode = Enum.Parse<RunMode>(_cfg["run_mode"]);
            DoesCheckLog = bool.Parse(_cfg["does_check_log"]);
            DoesRecordMacros = bool.Parse(_cfg["does_record_macros"]);
            DoesResolveCode = bool.Parse(_cfg["does_resolve_code"]);
            DoesNotifyOnCompletion = bool.Parse(_cfg["does_notify_on_completion"]);
            await OnCpuUtilizationAllServersAsync();


        }

        private async Task OnCpuUtilizationAllServersAsync()
        {
            IsBusy = true;
            _metrics = new ServerMetrics(_cfg["cpu_usage_url_base"], _cfg["cpu_usage_url"]);

            var prodPct = await _metrics.GetCpuUtilPct(_cfg["production_server"]);
            ProdCpu = AssignCpuValue(prodPct, _cfg["production_server"]);

            var stgPct = await _metrics.GetCpuUtilPct(_cfg["stage_server"]);
            StgCpu = AssignCpuValue(stgPct, _cfg["stage_server"]);

            IsBusy = false;
        }

        private int AssignCpuValue(int pct, string server)
        {
            if (pct < 0)
            {
                Message = $"Utilization for '{server}' could not be calculated at this time. If problem persists contact SPI";
                return 999;
            }
            else
                return pct;
        }

        private string GetPort()
        {
            return ServerContext == ServerContext.SASApp94 ? _cfg["port"] : (int.Parse(_cfg["port"]) + 1).ToString();
        }
        private Args GetArgs(IEnumerable<SasProgram> pgms)
        {
            var args = new Args(_cfg[$"{Server}_server"], ServerContext.ToString(), GetPort());
            args.Programs = pgms.ToList();
            args.DoesMacroLogging = DoesRecordMacros;
            args.DoesNotifyOnCompletion = DoesNotifyOnCompletion;
            args.DoesResolveProgramCode = DoesResolveCode;
            args.DoesCheckLog = DoesCheckLog;
            args.BaseMetricsUrl = _cfg["cpu_usage_url_base"];
            args.MetricsUrl = _cfg["cpu_usage_url"];
            args.IsAsync = Mode == RunMode.parallel ? true : false;
            args.UseBestServer = Server == Server.best;
            args.DoesQuitOnError = DoesQuitOnError;

            var summaryLoc = @$"{Folder}\run_summaries";
            if (!Directory.Exists(summaryLoc))
                Directory.CreateDirectory(summaryLoc);

            args.SummaryFn = @$"{summaryLoc}\summary-{Environment.UserName}-{(DateTime.Now.ToString("yyyyMMddHHmmss"))}.html";


            if (DoesNotifyOnCompletion)
                args.Recipients.Add(Environment.UserName);

            return args;
        }
        private IEnumerable<SasProgram> GetSasPrograms()
        {
            foreach (var (f, i) in Programs.Select((f, i) => (f, i)))
            {
                if (f.IsSelected)
                {
                    var fn = Path.Combine(f.Path, f.Name);
                    if (File.Exists(fn))
                        yield return new SasProgram(fn, f.IsQc, i);
                    else
                        Message = $"The file '{f.Name}' could not be located";
                }
            }
        }

        private void GetSasProgramsForBatchFile()
        {

            if (UseAiPrograms)
            {
                GetProgramsFromAi();
            }
            else
            {
                foreach (var (f, i) in Programs.Select((f, i) => (f, i)))
                {
                    if (f.IsSelected)
                    {
                        var fn = Path.Combine(f.Path, f.Name);
                        if (File.Exists(fn))
                        {
                            var pgm = new SasProgramFile(fn, Folder, i);
                            pgm.IsQc = f.IsQc;
                            BatFilePrograms.Add(pgm);
                            if (!pgm.IsQc && AddQcPgms)
                            {
                                var testFolder = Path.Combine(Directory.GetParent(Folder).FullName, "testing");
                                if (Directory.Exists(testFolder))
                                {
                                    if (File.Exists(Path.Combine(testFolder, $"v-{f.Name}")))
                                    {
                                        var qcPgm = new SasProgramFile($"v-{f.Name}", testFolder, Programs.Count + 1000);
                                        qcPgm.IsQc = true;
                                        BatFilePrograms.Add(qcPgm);
                                    }
                                    else
                                    {
                                        Message = $"Could not locate the QC program for '{f.Name}'. Ensure QC program is in the testing folder and follows dept naming conventions";
                                    }
                                }
                            }
                        }
                        else
                            Message = $"The file '{f.Name}' could not be located";
                    }
                }
            }
        }

        private void GetProgramsFromAi()
        {
            BatFilePrograms.Clear();
            var root = GetRootFromFile(Ai, new Regex("utilities"));
            if (root == null)
                return;

            string[] sheets = { "TLFs", "Datasets" };
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
                                var typePos = headings.Where(x => x.Value.ToString().ToLower() == "type").FirstOrDefault()?.Address.ColumnLetter;
                                var pgmPos = headings.Where(x => x.Value.ToString().ToLower() == "program").FirstOrDefault()?.Address.ColumnLetter;
                                var orderPos = headings.Where(x => x.Value.ToString().ToLower() == "derive_order").FirstOrDefault()?.Address.ColumnLetter;
                                var filterPos = headings.Where(x => x.Value.ToString().ToLower() == "sb_ignore_record").FirstOrDefault()?.Address.ColumnLetter;


                                if (pgmPos == null || typePos == null)
                                {
                                    Message = $"AI requires fields Type and Progam to exist";
                                    return;
                                }
                                foreach (var row in ws.RowsUsed().Skip(1))
                                {
                                    if (filterPos != null)
                                    {
                                        try
                                        {
                                            var filter = row.Cell(filterPos).Value.ToString();
                                            if (filter.ToLower() == "true")
                                            {
                                                continue;
                                            }
                                        }
                                        catch
                                        {
                                            // couldnt parse filter do to unexpected values
                                        }
                                    }

                                    var pgmName = row.Cell(pgmPos).Value.ToString();
                                    var pgmType = sheet == "Datasets" ? row.Cell(typePos).Value.ToString().ToLower() : "tlfs";
                                    var order = sheet == "Datasets" && orderPos != null ?
                                        row.Cell(orderPos).Value.ToString() : "0";

                                    var pgm = CreateProgram(root,
                                        pgmName,
                                        pgmType,
                                        sheet == "TLFs" ? "outputs" : "data",
                                        order,
                                        false);

                                    if (pgm != null)
                                    {
                                        // Handle multiple outputs for the same program
                                        if (!BatFilePrograms.Any(x => x.Name.ToLower() == pgm.Name.ToLower() &&
                                                                    x.Path == pgm.Path))
                                        {
                                            BatFilePrograms.Add(pgm);
                                            if (AddQcPgms)
                                            {
                                                var qcPgm = CreateProgram(root,
                                                    $"v-{pgmName}",
                                                    pgmType,
                                                    sheet == "TLFs" ? "outputs" : "data",
                                                    order,
                                                    true);
                                                if (qcPgm != null)
                                                {

                                                    BatFilePrograms.Add(qcPgm);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Message = $"Error reading AI file: {ex.Message}";
                            return;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Message = $"Error reading AI file: {ex.Message}";
                return;
            }
            if (BatFilePrograms.Count > 0)
            {
                FixProgramSeq();
            }
        }

        private void FixProgramSeq()
        {
            const int offset = 1001;

            foreach (var pgm in BatFilePrograms.OrderBy(x => x.Seq).ThenBy(x => x.Category))
            {
                if (pgm.Category != null)
                {
                    pgm.Seq = pgm.IsQc ? (int)pgm.Category + offset : (int)pgm.Category + pgm.Seq;
                }
                else
                {
                    Message = $"The category for Program {pgm.Name} could not be determined. This may affect the sequence for this record.";
                }
                
            }
        }

        public SasProgramFile? CreateProgram(string root, string pgmName, string pgmType, string category,string order, bool isQc)
        {
            try
            {
                var path = Path.Combine(root, category, pgmType, isQc ? "testing" : "pgms");
                var pgm = Path.Combine(path, $"{Path.GetFileNameWithoutExtension(pgmName)}.sas");
                if (File.Exists(pgm))
                {
                    int.TryParse(order, out int seq);
                    var sasPgm = new SasProgramFile(Path.GetFileName(pgm), path, seq);
                    sasPgm.IsQc = isQc;
                    if (Enum.TryParse<ProgramCategory>(pgmType, out ProgramCategory cat))
                    {
                        sasPgm.Category = cat;
                    }
                    else
                    {
                        Message = $"Unable to categorize program type of {pgmType}";
                    }

                    return sasPgm;
                }
            }
            catch (Exception ex)
            {
                Message = $"Error parsing AI - contact SPI: {ex.Message}";

            }
            return default;
        }

        public string? GetRootFromFile(string fn, Regex lastFolder)
        {
            var dir = Path.GetExtension(fn) == string.Empty ? fn : Path.GetDirectoryName(fn);

            var folders = dir.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            do
            {
                if (folders.Count == 0)
                {
                    Message = $"The analysis index must be under a '{lastFolder}' folder to use this feature";
                    return null;
                }
                folders = folders.Take(folders.Count() - 1).ToList();

            } while (!lastFolder.IsMatch(folders.LastOrDefault()));

            return Path.GetDirectoryName(string.Join(Path.DirectorySeparatorChar, folders));
        }



        private void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);

            switch (propertyName)
            {

                case nameof(BatFileName):
                    if (DoesCreateBatFile)
                    {
                        if (string.IsNullOrEmpty(BatFileName))
                        {
                            AddError(propertyName, ".bat file name is required");
                        }
                    }
                    break;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}
