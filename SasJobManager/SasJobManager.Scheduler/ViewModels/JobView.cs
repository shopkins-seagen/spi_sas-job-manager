using Prism.Commands;
using Prism.Events;
using SasJobManager.Domain.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Scheduler.ViewModels
{
    public class JobView : INotifyPropertyChanged
    {
        private int id;
        private string description;
        private string createdBy;
        private DateTime createdOn;
        private bool isEnabled;
        private bool isRecurring;
        private string driver;
        private int hour;
        private bool isMonday;
        private bool isTuesday;
        private bool isWednesday;
        private bool isThursday;
        private bool isFriday;
        private bool isSaturday;
        private bool isSunday;
        private bool isCustomSecurity;
        private Principal? customSecurityGroup;

        private bool _isPendingChange;

        public JobView()
        {
 
        }
        public JobView(string _createdby,string _driver,string _description)
        {
            CreatedBy=_createdby; 
            CreatedOn = DateTime.Now;
            Driver=_driver;
            Description=_description;

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        public bool IsPendingChange
        {
            get { return _isPendingChange; }
            set
            {
                _isPendingChange = value;
                OnPropertyChanged();
            }
        }
        public bool IsCustomSecurity
        {
            get { return isCustomSecurity; }
            set
            {
    
                IsPendingChange = true;
                isCustomSecurity = value;
                OnPropertyChanged();
            }
        }
        public Principal? CustomSecurityGroup
        {
            get { return customSecurityGroup; }
            set
            {
              
                IsPendingChange = true;
                customSecurityGroup = value;
                OnPropertyChanged();
            }
        }


        public int Id { get => id; set => id = value; }
        public string Description
        {
            get => description; set
            {
                if (!string.IsNullOrEmpty(Description))
                    IsPendingChange = true;

                description = value;
                OnPropertyChanged();
            }
        }
        public string CreatedBy
        {
            get => createdBy; set
            {
                createdBy = value;
                OnPropertyChanged();
            }
        }
        public DateTime CreatedOn
        {
            get => createdOn; set
            {
                createdOn = value;
                OnPropertyChanged();
            }
        }
        public bool IsEnabled
        {
            get => isEnabled; set
            {
                isEnabled = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsRecurring
        {
            get => isRecurring; set
            {
                isRecurring = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public string Driver
        {
            get => driver; set
            {
                if (!string.IsNullOrEmpty(driver))
                    IsPendingChange = true;

                driver = value;
                OnPropertyChanged();
            }
        }

        public int Hour
        {
            get => hour; set
            {
                hour = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsMonday
        {
            get => isMonday; set
            {
                isMonday = value;
                IsPendingChange = true;                
                OnPropertyChanged();
               
            }
        }
        public bool IsTuesday
        {
            get => isTuesday; set
            {
                isTuesday = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsWednesday
        {
            get => isWednesday; set
            {
                isWednesday = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsThursday
        {
            get => isThursday; set
            {
                isThursday = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsFriday
        {
            get => isFriday; set
            {
                isFriday = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsSaturday
        {
            get => isSaturday; set
            {
                isSaturday = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }
        public bool IsSunday
        {
            get => isSunday; set
            {
                isSunday = value;
                IsPendingChange = true;
                OnPropertyChanged();
            }
        }


    }
}
