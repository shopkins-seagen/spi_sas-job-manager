using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.LaunchScheduledJobs.Service
{
    public class RunLogger
    {
        private string _fn;
        public RunLogger(string fn)
        {
            _fn = fn;
        }
        public void Record(int jobs,string? msg="")
        {
            try
            {
                using (var sw = new StreamWriter(_fn, true))
                {
                    sw.WriteLine($"{(DateTime.Now.ToString("yyyy-MM-dd HH:mm"))},{jobs},{msg}");
                }
            }
            catch 
            {
                // local
            }
        }
    }
}
