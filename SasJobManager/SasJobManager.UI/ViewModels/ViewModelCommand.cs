using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.IO;
using SasJobManager.UI.Models;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using Prism.Commands;
using SasJobManager.Lib.Models;
using SasJobManager.Lib;
using System.Windows;
using System.Windows.Input;
using System.Diagnostics;
using Microsoft.OData;

namespace SasJobManager.UI.ViewModels
{
    public partial class MainViewModel
    {
        public ICommand SubmitCommand { get; }
        public ICommand ToggleIncludeCommand { get; }
        public ICommand ToggleQcCommand { get; }
        public ICommand ToggleSortCommand { get; }
        public ICommand ResizeCommand { get; }
        public ICommand RefreshProgramsCommand { get; }
        public ICommand CpuUtilizationCommand { get; }
        public ICommand AllCpuUtilizationCommand { get; }
        public ICommand SelectFolderCommand { get; }
        public ICommand SelectServerCommand { get; }
        public ICommand ServerHelpCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand CreateBatchFileCommand { get; }
        public ICommand SelectAiCommand { get; }
        public ICommand ClearAiCommand { get; }
        public ICommand HelpCreateBatchFileCommand { get; }


        private void OnHelpCreateBatchFile()
        {
            var url = _cfg["help_batch"];
            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Message = $"Could not open help: {ex.Message}";
            }
        }
        private bool CanClearAi()
        {
            return !string.IsNullOrEmpty(Ai);
        }

        private void OnClearAi()
        {
            Ai = null;
            UseAiPrograms = false;
            UseDeriveOrder = false;
            DisplayProgrammersInSummary = false;
        }
        private void OnSelectAi()
        {
            var ofd = new Ookii.Dialogs.Wpf.VistaOpenFileDialog()
            {

                Filter = "AI files|*.xlsx",
                Title = "Select AI to add Derive Order to CSV file"
            };
            if (!string.IsNullOrEmpty(Folder))
            {
                try
                {
                    var root = GetRootFromFile(Folder, new Regex("(data|outputs)"));
                    var utils = Path.Combine(root, "utilities", "ai");
                    var util = string.Join(Path.DirectorySeparatorChar, utils);
                    if (Directory.Exists(util))
                    {
                        ofd.InitialDirectory = util;
                        ofd.FileName = util;
                    }
                    else
                    {
                        ofd.InitialDirectory = Folder;
                    }
                }
                catch
                {
                    // do nothing
                }
            }
            else
            {
                ofd.InitialDirectory = @"O:\Projects";
            }

            if (ofd.ShowDialog().GetValueOrDefault())
            {
                Ai = ofd.FileName;
            }
        }

        private async void OnRefresh()
        {
            await Load(Folder, new List<string>());
        }
        private void FilterResults()
        {
            var temp = new ObservableCollection<SasProgramFile>();
            if (!string.IsNullOrEmpty(Filter))
            {
                foreach (var p in _allPgms)
                {
                    if (p.Name.StartsWith(Filter, StringComparison.OrdinalIgnoreCase))
                    {
                        temp.Add(p);
                    }
                }
            }
            else
            {
                temp.AddRange(_allPgms);
            }

            Programs.Clear();
            Programs.AddRange(temp);
        }
        private bool CanSubmitExecute()
        {
            return !IsBusy;
        }

        private async void OnSubmitExecute()
        {
            Messages = string.Empty;
            IsBusy = true;
            await Task.Run(async () =>
            {


                var pgms = GetSasPrograms();
                if (pgms.Count() > 0)
                {
                    var args = GetArgs(pgms);

                    var sas = new SasManager(args, new Domain.SasContext());
                    var progress = new Progress<string>(value =>
                    {
                        Message = value;
                    });
                    await sas.Run(progress);
                    if (args.DoesCheckLog)
                    {
                        sas.WriteSummary(true);
                    }
                }
                else
                {
                    Message = "You must select a least one program";
                }


            });
            IsBusy = false;
        }



        private void OnToggleInclude()
        {
            IsIncludeAllChecked = !IsIncludeAllChecked;
            foreach (var p in Programs)
            {
                p.IsSelected = IsIncludeAllChecked;
            }
        }
        private void OnToggleQc()
        {
            IsQcAllChecked = !IsQcAllChecked;
            foreach (var p in Programs)
            {
                p.IsQc = IsQcAllChecked;
            }
        }
        private void OnToggleSort()
        {

            var temp = new ObservableCollection<SasProgramFile>();
            temp.AddRange(Programs);

            temp = IsAzSort ? new ObservableCollection<SasProgramFile>(temp.OrderBy(x => x.Name)) :
                                          new ObservableCollection<SasProgramFile>(temp.OrderByDescending(x => x.Name));


            Programs.Clear();
            Programs.AddRange(temp);
            IsAzSort = !IsAzSort;
        }
        private void OnResize()
        {
            if (!IsExpanded)
            {
                if (Application.Current.MainWindow.Width == 620)
                {
                    Application.Current.MainWindow.Width = Application.Current.MainWindow.Width + 600;
                }
            }
            else
            {
                Application.Current.MainWindow.Width = 620;
            }
            IsExpanded = !IsExpanded;
        }
        private async void OnCpuUtilization()
        {
            IsBusy = true;
            _metrics = new ServerMetrics(_cfg["cpu_usage_url_base"], _cfg["cpu_usage_url"]);
            var pct = await _metrics.GetCpuUtilPct(_cfg[$"{Server}_server"]);
            Message = $"{Server} is at {pct}% utilization";
            IsBusy = false;
        }
        private async void OnCpuUtilizationAllServers()
        {
            IsBusy = true;
            _metrics = new ServerMetrics(_cfg["cpu_usage_url_base"], _cfg["cpu_usage_url"]);

            var prodPct = await _metrics.GetCpuUtilPct(_cfg["production_server"]);
            ProdCpu = prodPct;
            var stgPct = await _metrics.GetCpuUtilPct(_cfg["stage_server"]);
            StgCpu = stgPct;
            IsBusy = false;
        }

        private bool CanCreateBatchFile()
        {
            return DoesCreateBatFile && !IsBusy;
        }

        private async void OnCreateBatchFileExecute()
        {
            IsBusy = true;
            BatFilePrograms.Clear();
            Message = string.Empty;

            await Task.Run(() =>
            {
                

                if (string.IsNullOrWhiteSpace(BatFileName))
                {
                    Message = "You must provide a name for the .bat file";
                    return;
                }
                GetSasProgramsForBatchFile();

                if (BatFilePrograms.Count == 0)
                    Message = "Creating .BAT file only. To generate the corresponding CSV file, select 1 or more programs or select AI";

                var args = GetBatchArgs();
                var bf = new BatFileGenerator(args, Folder, _cfg["client_exe"], _cfg["help_cli"], BatFilePrograms.ToList());
                bf.WriteFiles(BatFileName);
            });
            IsBusy = false;

        }

        private Args GetBatchArgs()
        {
            var args = new Args(_cfg[$"{Server}_server"], ServerContext.ToString(), GetPort());
            args.DoesMacroLogging = DoesRecordMacros;
            args.DoesNotifyOnCompletion = DoesNotifyOnCompletion;
            args.DoesResolveProgramCode = DoesResolveCode;
            args.DoesCheckLog = DoesCheckLog;
            args.IsAsync = Mode == RunMode.parallel ? true : false;
            args.UseBestServer = Server == Server.best;
            args.DoesDisplayProgrammers = DisplayProgrammersInSummary;
            args.UseDeriveOrder = UseDeriveOrder;
            args.Ai = Ai;
            args.UseAi = UseAiPrograms;
            args.DoesQuitOnError = DoesQuitOnError;

            if (DoesNotifyOnCompletion)
                args.Recipients.Add(Environment.UserName);

            return args;
        }

        private async void OnSelectFolder()
        {

            IsBusy = true;
            var ofd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog
            {
                SelectedPath = Folder,
                ShowNewFolderButton = false,
                Description = "Select folder"
            };

            if (ofd.ShowDialog().GetValueOrDefault())
            {
                Folder = ofd.SelectedPath;
                await Load(Folder, new List<string>());
            }
            IsBusy = false;
        }

        private void OnSelectServer(object obj)
        {
            if (Enum.TryParse<Server>((string)obj, out Server server))
            {
                Server = server;
                IsProductionServer = server == Server.production;

            }
        }

        private void OnServerHelp()
        {
            var url = _cfg["help_server"];
            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Message = $"Could not open help: {ex.Message}";
            }
        }

        private void OnHelp()
        {
            var url = _cfg["help"];
            try
            {
                var psi = new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                Message = $"Could not open help: {ex.Message}";
            }
        }
    }
}
