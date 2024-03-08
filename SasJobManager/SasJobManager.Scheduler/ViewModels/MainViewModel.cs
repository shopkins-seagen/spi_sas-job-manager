using SasJobManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.IO;
using Prism.Events;
using SasJobManager.Domain;
using SasJobManager.Scheduler.Models;
using System.DirectoryServices.AccountManagement;
using Microsoft.Extensions.Configuration;
using DocumentFormat.OpenXml.Spreadsheet;

namespace SasJobManager.Scheduler.ViewModels
{
    public partial class MainViewModel : NotifyDataErrorInfoBase, INotifyPropertyChanged
    {

        private Repository _repo;
        private JobView _selectedJob;
        private bool _isNewJobVisble;
        private string _driverFile;
        private string _driverFileName;
        private string _newJobDescription;
        private bool _isBusy;
        private string _message;
        private IDialogService _dialogService;
        private string _filter;
        private List<JobView> _allJobs;

        private IEventAggregator _eventAggregator;
        private JobRun _selectedJobRun;
        private bool _isBlinded;
        private AdService _adService;
        private IConfigurationRoot _cfg;

        public MainViewModel(IEventAggregator eventAggregator, SasContext sasContext, IDialogService dialogService)
        {
            LoadConfig();
            _repo = new Repository(sasContext);
            Jobs = new ObservableCollection<JobView>();
            JobRuns = new ObservableCollection<JobRun>();
            SelectedJobRunMsgs = new ObservableCollection<JobRunMsg>();
            SasGroups = new ObservableCollection<Principal>();

            _allJobs = new List<JobView>();
            AddJobCommand = new DelegateCommand(OnAddNewJob, CanAddNewJob);
            SaveNewJobCommand = new DelegateCommand(OnSaveNewJob, CanSaveNewJob);
            CancelNewJobCommand = new DelegateCommand(OnCancelNewJob);
            SelectDriverCommand = new DelegateCommand<string>(OnSelectDriver);
            SaveJobDetailsCommand = new DelegateCommand(OnSaveJobDetails, CanSaveJobDetails);
            CancelJobDetailsCommand = new DelegateCommand(OnCancelJobDetails, CanCancelJobDetails);
            DeleteJobCommand = new DelegateCommand(OnDeleteJob, CanDeleteJob);
            ViewAllJobsCommand = new DelegateCommand(OnViewAllJobs, CanViewAllJobs);
            RefreshJobRunCommand = new DelegateCommand(OnRefreshJobRun, CanRefreshJobRun);
            HelpCommand = new DelegateCommand(OnHelp);
            RunAsyncCommand = new DelegateCommand(OnRunAsync, CanRunAsync);

            IsNewJobVisible = false;
            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _adService = new AdService("sg.seagen.com");

        }


        public DialogService DialogSerice { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<JobView> Jobs { get; set; }
        public ObservableCollection<Principal> SasGroups { get; set; }
        public ObservableCollection<JobRun> JobRuns { get; set; }
        public ObservableCollection<JobRunMsg> SelectedJobRunMsgs { get; set; }

        private async void LoadSasGroups()
        {
            IsBusy = true;

            foreach (var g in await _adService.GetRegisteredGroups(_cfg.GetSection("apis:groups").GetChildren().Select(x => x.Value).ToList()))
            {
                SasGroups.Add(g);
            }
            IsBusy = false;
        }

        public JobView SelectedJob
        {

            get { return _selectedJob; }
            set
            {
                _selectedJob = value;
                OnPropertyChanged();
                ((DelegateCommand)DeleteJobCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RefreshJobRunCommand).RaiseCanExecuteChanged();
                ((DelegateCommand)RunAsyncCommand).RaiseCanExecuteChanged();

                SetJobRuns();
                if (value != null)
                    OnCancelNewJob();
            }
        }
        public JobRun SelectedJobRun
        {
            get { return _selectedJobRun; }
            set
            {
                _selectedJobRun = value;
                OnPropertyChanged();
                SetJobRunMsgs();
            }
        }

        private void SetJobRuns()
        {

            JobRuns.Clear();
            if (SelectedJob != null)
            {
                foreach (JobRun j in _repo.GetJobRuns(SelectedJob.Id))
                {
                    if (j != null)
                        JobRuns.Add(j);
                }
            }
        }
        private void SetJobRunMsgs()
        {
            SelectedJobRunMsgs.Clear();
            if (SelectedJobRun != null)
            {
                foreach (JobRunMsg j in _repo.GetJobRunMsgs(SelectedJobRun.Id))
                {
                    if (!string.IsNullOrEmpty(j.Message))
                        SelectedJobRunMsgs.Add(j);
                }
            }
        }

        public bool IsNewJobVisible
        {
            get { return _isNewJobVisble; }
            set
            {
                _isNewJobVisble = value;
                OnPropertyChanged();
            }
        }
        public string DriverFile
        {
            get { return _driverFile; }
            set
            {
                _driverFile = value;
                if (!string.IsNullOrEmpty(value))
                {
                    DriverFileName = Path.GetFileName(value);
                }
                else
                {
                    DriverFileName = string.Empty;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(DriverFileName));
                ((DelegateCommand)SaveNewJobCommand).RaiseCanExecuteChanged();
            }
        }
        public string DriverFileName
        {
            get { return _driverFileName; }
            set
            {
                _driverFileName = value;
            }
        }
        public string NewJobDescription
        {
            get { return _newJobDescription; }
            set
            {
                _newJobDescription = value;
                OnPropertyChanged();
                if (NewJobDescription != null)
                {
                    ValidateProperty(nameof(NewJobDescription));
                    ((DelegateCommand)SaveNewJobCommand).RaiseCanExecuteChanged();
                }
            }
        }
        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                ((DelegateCommand)ViewAllJobsCommand).RaiseCanExecuteChanged();
            }
        }
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                OnPropertyChanged();
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


        public async Task LoadAsync()
        {
            IsBusy = true;
            Jobs.Clear();
            _allJobs.Clear();
            await Task.Run(() =>
            {
                foreach (var j in _repo.GetJobs())
                {
                    var jv = JobToView(j);
                    if (jv != null)
                        _allJobs.Add(jv);
                }
            });
            Jobs.AddRange(_allJobs);
            LoadSasGroups();
            IsBusy = false;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        private void ValidateProperty(string propertyName)
        {
            ClearErrors(propertyName);

            switch (propertyName)
            {
                case nameof(NewJobDescription):

                    if (string.IsNullOrWhiteSpace(NewJobDescription))
                    {
                        AddError(propertyName, "Description cannot be missing");
                        return;
                    }
                    if (!IsUnique(NewJobDescription))
                    {
                        AddError(propertyName, "Description must be unique");
                        return;
                    }
                    break;

            }
        }

        private bool IsUnique(string newJobDescription)
        {
            return true;
        }
        private JobView? JobToView(Job job)
        {
            if (job != null)
            {
                var jv = new JobView();
                jv.Driver = job.Driver;
                jv.Description = job.Description;
                jv.Id = job.Id;
                jv.IsEnabled = job.IsEnabled;
                jv.CreatedBy = job.CreatedBy;
                jv.CreatedOn = job.CreatedOn;
                jv.IsRecurring = job.IsRecurring;
                jv.Hour = job.Hour;
                jv.IsMonday = job.IsMonday;
                jv.IsTuesday = job.IsTuesday;
                jv.IsWednesday = job.IsWednesday;
                jv.IsThursday = job.IsThursday;
                jv.IsFriday = job.IsFriday;
                jv.IsSaturday = job.IsSaturday;
                jv.IsSunday = job.IsSunday;
                jv.IsCustomSecurity = job.IsCustomSecurity;
                jv.CustomSecurityGroup = _adService.ConvertToPrincipal(job.CustomSecurityGroup);

                jv.IsPendingChange = false;

                return jv;
            }
            else
                return null;
        }

        private void LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_scheduler.json", true, true);
            _cfg = builder.Build();
        }
    }
}
