using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SasJobManager.UI.Models
{
    public partial class SasProgramFile : INotifyPropertyChanged
    {
        private bool _isSelected;
        private int _seq;
        private bool _isQc;

        public SasProgramFile(string pgm, string dir,int seq)
        {
            Name = pgm;
            Path = dir;
            _seq = seq;
        }
        public string Name { get; set; }
        public string Path { get; set; }
        public ProgramCategory? Category { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        public int Seq
        {
            get { return _seq; }
            set
            {
                _seq = value;
                OnPropertyChanged();
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                OnPropertyChanged();
            }
        }
       

        public bool IsQc
        {
            get { return _isQc; }
            set
            {
                _isQc = value;
                OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string property = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }

        public override string? ToString()
        {
            try
            {
                return System.IO.Path.Combine(this.Path, this.Name);
            }
            catch
            {
                return null;
            }
        }
    }
}
