using ClosedXML.Excel;
using SasJobManager.Domain.Models;
using SasJobManager.LaunchScheduledJobs.Service;
using SasJobManager.Scheduler.Service;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using Syroot.Windows.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Telerik.Windows.Controls;

namespace SasJobManager.Scheduler.ViewModels
{
    public partial class MainViewModel
    {
        public ICommand AddJobCommand { get; }
        public ICommand SaveNewJobCommand { get; }
        public ICommand CancelNewJobCommand { get; }
        public ICommand SelectDriverCommand { get; }
        public ICommand SaveJobDetailsCommand { get; }
        public ICommand CancelJobDetailsCommand { get; }
        public ICommand DeleteJobCommand { get; }
        public ICommand ViewAllJobsCommand { get; }
        public ICommand RefreshJobRunCommand { get; }
        public ICommand HelpCommand { get; }
        public ICommand RunAsyncCommand { get; }

        private bool CanRunAsync()
        {
            return SelectedJob != null;
        }

        private async void OnRunAsync()
        {

            if (SelectedJob != null)
            {
                IsBusy = true;
                Message = string.Empty;
                var runner = new JobRunner(SelectedJob, _cfg);
                if (runner.Log.Any(x => x.IssueType == IssueType.Fatal))
                {
                    _dialogService.ShowMessageBox(runner.Log.Where(x => x.IssueType == IssueType.Fatal).FirstOrDefault().Msg, "Fatal Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                runner.CheckUserIsNotified();

                if (runner.Log.Any(x => x.IssueType == IssueType.Fatal))
                {
                    IsBusy = false;

                    _dialogService.ShowMessageBox($"{runner.Log.FirstOrDefault(x => x.IssueType == IssueType.Fatal).Msg}",
                        "Insufficient Priviledges",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var result = await runner.Submit();
                IsBusy = false;
                _dialogService.ShowMessageBox(result, "Server Response", MessageBoxButton.OK, MessageBoxImage.Information);
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
        private bool CanRefreshJobRun()
        {
            return SelectedJob != null;
        }

        private void OnRefreshJobRun()
        {
            SetJobRuns();
        }
        private bool CanAddNewJob()
        {
            return !IsBusy;
        }
        public void OnAddNewJob()
        {
            IsNewJobVisible = true;
            SelectedJob = null;
        }
        public bool CanSaveNewJob()
        {
            return !string.IsNullOrEmpty(NewJobDescription) && !HasErrors && !string.IsNullOrEmpty(DriverFile);
        }

        public void OnSaveNewJob()
        {
            var job = new JobView(Environment.UserName, DriverFile, NewJobDescription);

            job.IsRecurring = true;
            job.IsEnabled = true;

            var response = _repo.SaveNewJob(NewJvToJob(job));
            if (!string.IsNullOrEmpty(response.Msg))
                Message = response.Msg;

            if (response.IsOk)
            {

                job.Id = response.Id;
                Jobs.Add(job);
                _allJobs.Add(job);
                job.IsPendingChange = false;
                SelectedJob = job;
                OnCancelNewJob();
            }
        }

        public void OnCancelNewJob()
        {
            NewJobDescription = string.Empty;
            DriverFile = string.Empty;
            IsNewJobVisible = false;

        }

        private async void OnSelectDriver(string isSelected)
        {
            await SelectDriver(bool.Parse(isSelected));
            IsBusy = false;
        }

        private async Task SelectDriver(bool isSelected)
        {

            await Task.Run(() =>
          {
              var ofd = new Microsoft.Win32.OpenFileDialog()
              {
                  Filter = "Batch files (*.bat)|*.bat",
                  Multiselect = false,
                  Title = "Select .BAT file",
                  InitialDirectory = _cfg["default"],
                  FileName = DriverFile
              };

              IsBusy = true;
              if (ofd.ShowDialog().GetValueOrDefault())
              {
                  if (isSelected)
                      SelectedJob.Driver = ofd.FileName;
                  else
                      DriverFile = ofd.FileName;
              }

          });
        }

        private bool CanSaveJobDetails()
        {
            return true;
        }

        private void OnSaveJobDetails()
        {
            if (SelectedJob.IsCustomSecurity)
            {
                if (!CheckMembership())
                    return;
            }

            var job = GetSelectedJobDetails();
            if (job != null)
            {

                var response = _repo.UpdateJob(job);
                if (response.IsOk)
                {
                    SyncJob();
                    Message = response.Msg;

                    SelectedJob.IsPendingChange = false;


                }
                else
                {
                    Message = $"ERROR: {response.Msg}";
                }
            }
        }

        private bool CheckMembership()
        {
            if (SelectedJob == null)
                return false;

            if (SelectedJob.CustomSecurityGroup == null)
            {
                Message = "A security group must be selected for custom security studies";
                return false;
            }
            else
            {
                if (Environment.UserName.ToLower() != "shopkins")
                {
                    if (!_adService.IsUserInGroup(Environment.UserName, SelectedJob.CustomSecurityGroup.Name))
                    {
                        Message = $"ERROR: User '{Environment.UserName}' is not a member of AD group: '{SelectedJob.CustomSecurityGroup.Name}'.";
                        return false;
                    }
                    if (!_adService.IsDriverInGroup(SelectedJob.Driver, SelectedJob.CustomSecurityGroup.Name))
                    {
                        Message = $"ERROR: batch file '{SelectedJob.Driver}' does not inherit the ACLs from AD group: '{SelectedJob.CustomSecurityGroup.Name}'.";
                        return false;
                    }
                }
            }

            return true;
        }

        private bool CanCancelJobDetails()
        {
            return true;
        }

        private void OnCancelJobDetails()
        {
            if (SelectedJob != null)
            {
                if (!SelectedJob.IsPendingChange)
                {
                    Message = $"No changes are pending for job {SelectedJob.Description}";
                    return;
                }

                SyncJob();
                SelectedJob.IsPendingChange = false;
                Message = $"Canceled pending changes for job {SelectedJob.Description}";
            }
        }


        private bool CanDeleteJob()
        {
            return SelectedJob != null;
        }

        private void OnDeleteJob()
        {
            var diag = _dialogService.ShowMessageBox($"Are you sure you want to delete '{SelectedJob.Description}'",
                "Confirm Deletion", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (diag == MessageBoxResult.Yes)
            {
                var response = _repo.DeleteJob(SelectedJob.Id);
                if (response.IsOk)
                {
                    Jobs.Remove(SelectedJob);
                    SelectedJob = null;

                }
            }
        }

        private void SyncJob()
        {
            var entity = _repo.GetJob(SelectedJob.Id);
            if (entity != null)
            {
                var jv = JobToJv(entity);
                var job = Jobs.FirstOrDefault(x => x.Id == entity.Id);
                if (job != null)
                {
                    var results = from srcProp in typeof(JobView).GetProperties()
                                  let targetProperty = typeof(JobView).GetProperty(srcProp.Name)
                                  where srcProp.CanRead
                                  && targetProperty != null
                                  && (targetProperty.GetSetMethod(true) != null && !targetProperty.GetSetMethod(true).IsPrivate)
                                  && targetProperty.PropertyType.IsAssignableFrom(srcProp.PropertyType)
                                  select new { sourceProperty = srcProp, targetProperty = targetProperty };

                    foreach (var props in results)
                    {
                        props.targetProperty.SetValue(job, props.sourceProperty.GetValue(jv));
                    }
                }
            }
        }

        private bool CanViewAllJobs()
        {
            return !IsBusy;
        }
        private async void OnViewAllJobs()
        {
            IsBusy = true;

            await Task.Run(() =>
            {
                using (var xls = new XLWorkbook())
                {
                    var ws = xls.AddWorksheet("Scheduled Tasks");

                    var title = ws.Range("A1:H1");
                    title.Merge();
                    title.Value = $"Scheduled Jobs Count by Hour x Day as of: {DateTime.Now.ToString("ddMMMyyyy HH:mm")}";
                    title.Style.Font.FontSize = 13;
                    title.Style.Font.Bold = true;

                    int cell = 2;
                    int row = 2;
                    ws.Row(2).Cell(1).Value = "Hour";
                    foreach (DayOfWeek day in (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek)))
                    {
                        ws.Row(row).Cell(cell).Value = day.ToString();
                        cell++;
                    }
                    var jobs = _repo.GetJobs();
                    cell = 2;
                    foreach (DayOfWeek day in (DayOfWeek[])Enum.GetValues(typeof(DayOfWeek)))
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            ws.Row(i + 3).Cell(1).Value = $"{i:0}";
                            var count = GetJobs(jobs, i, day);
                            var cellId = ws.Row(i + 3).Cell(cell);
                            cellId.Value = count;


                            if (count > 0)
                            {
                                cellId.Style.Font.Bold = true;
                                cellId.Style.Font.FontColor = XLColor.Blue;
                                cellId.Style.Fill.BackgroundColor = XLColor.CadmiumYellow;
                            }
                            else
                            {
                                cellId.Style.Font.FontColor = XLColor.Gray;
                            }
                            cellId.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                        }
                        cell++;
                    }
                    var yAxis = ws.Range("A2:A25");
                    var xAxis = ws.Range("B2:H2");
                    yAxis.Style.Font.Bold = true;
                    xAxis.Style.Font.Bold = true;
                    yAxis.Style.Fill.BackgroundColor = XLColor.Green;
                    xAxis.Style.Fill.BackgroundColor = XLColor.Green;
                    xAxis.Style.Font.FontColor = XLColor.White;
                    yAxis.Style.Font.FontColor = XLColor.White;

                    var dl = KnownFolders.Downloads.Path;
                    ws.Columns().AdjustToContents();


                    xls.SaveAs(Path.Combine(dl, "scheduled-jobs.xlsx"));

                    Message = $"Saved to {Path.Combine(dl, "scheduled-jobs.xlsx")}";
                }
                IsBusy = false;
            });
        }

        public int GetJobs(List<Job> jobs, int hour, DayOfWeek day)
        {
            int counter = 0;
            var rgx = new Regex("^is[a-z]{3,6}day$", RegexOptions.IgnoreCase);
            var jobsByHour = jobs.Where(x => x.Hour == hour).ToList();
            foreach (var j in jobsByHour)
            {
                var days = DaysJobRuns(j, rgx);
                if (days.Any(x => x == day))
                {
                    counter++;
                }
            }

            return counter;
        }

        private List<DayOfWeek> DaysJobRuns(Job job, Regex dayRgx)
        {
            var days = new List<DayOfWeek>();

            foreach (PropertyInfo p in job.GetType().GetProperties().Where(x => dayRgx.IsMatch(x.Name)))
            {
                if (p.GetValue(job) is true)
                {
                    var day = $"{char.ToUpper(p.Name.Substring(2)[0])}{p.Name.Substring(3).ToLower()}";
                    if (Enum.TryParse<DayOfWeek>(day, out DayOfWeek result))
                    {
                        days.Add(result);
                    }
                }
            }

            return days;
        }
        private Job NewJvToJob(JobView jv)
        {
            var job = new Job
            {
                Description = jv.Description,
                Driver = jv.Driver,
                CreatedBy = jv.CreatedBy,
                CreatedOn = jv.CreatedOn
            };

            return job;
        }

        private JobView JobToJv(Job job)
        {
            return new JobView()
            {
                Id = job.Id,
                Description = job.Description,
                CreatedBy = job.CreatedBy,
                CreatedOn = job.CreatedOn,
                Driver = job.Driver,
                Hour = job.Hour,
                IsEnabled = job.IsEnabled,
                IsRecurring = job.IsRecurring,
                IsMonday = job.IsMonday,
                IsTuesday = job.IsTuesday,
                IsWednesday = job.IsWednesday,
                IsThursday = job.IsThursday,
                IsFriday = job.IsFriday,
                IsSaturday = job.IsSaturday,
                IsSunday = job.IsSunday,
                IsCustomSecurity = job.IsCustomSecurity,
                CustomSecurityGroup = _adService.ConvertToPrincipal(job.CustomSecurityGroup),
                IsPendingChange = false
            };
        }

        private Job? GetSelectedJobDetails()
        {
            if (SelectedJob != null)
            {
                var job = new Job
                {
                    Id = SelectedJob.Id,
                    Description = SelectedJob.Description,
                    CreatedBy = SelectedJob.CreatedBy,
                    CreatedOn = SelectedJob.CreatedOn,
                    Driver = SelectedJob.Driver,
                    Hour = SelectedJob.Hour,
                    IsEnabled = SelectedJob.IsEnabled,
                    IsRecurring = SelectedJob.IsRecurring,
                    IsMonday = SelectedJob.IsMonday,
                    IsTuesday = SelectedJob.IsTuesday,
                    IsWednesday = SelectedJob.IsWednesday,
                    IsThursday = SelectedJob.IsThursday,
                    IsFriday = SelectedJob.IsFriday,
                    IsSaturday = SelectedJob.IsSaturday,
                    IsSunday = SelectedJob.IsSunday,
                    IsCustomSecurity = SelectedJob.IsCustomSecurity,
                    CustomSecurityGroup = !SelectedJob.IsCustomSecurity ? null : SelectedJob.CustomSecurityGroup != null ? SelectedJob.CustomSecurityGroup.Name : null

                };

                return job;

            }
            else
                return null;
        }
        private void FilterResults()
        {
            var temp = new ObservableCollection<JobView>();
            if (!string.IsNullOrEmpty(Filter))
            {
                foreach (var p in _allJobs)
                {
                    if (p.Description.StartsWith(Filter, StringComparison.OrdinalIgnoreCase))
                    {
                        temp.Add(p);
                    }
                }
            }
            else
            {
                temp.AddRange(_allJobs);
            }

            Jobs.Clear();
            Jobs.AddRange(temp);
        }
    }
}
