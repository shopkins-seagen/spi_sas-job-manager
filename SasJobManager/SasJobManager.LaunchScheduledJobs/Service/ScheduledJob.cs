using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasJobManager.Domain;
using SasJobManager.Cli;
using SasJobManager.Domain.Models;
using System.Reflection;
using System.Text.RegularExpressions;

namespace SasJobManager.LaunchScheduledJobs.Service
{
    public class ScheduledJob
    {
        private SasContext _context;
        private DayOfWeek _day;
        private int _hour;
        private Regex _jobDay;

        public ScheduledJob(DayOfWeek day, int hour)
        {
            _context = new SasContext();
            Jobs = new List<Job>();
            _day = day;
            _hour = hour;
            _jobDay = new Regex("^is[a-z]{3,6}day$", RegexOptions.IgnoreCase);
        }

        public List<Job> Jobs { get; set; }

        public void GetJobs()
        {
            var allJobs = _context.Jobs.Where(x => x.IsEnabled && x.Hour==_hour).ToList();
            foreach(var j in allJobs)
            {
                var days = DaysJobRuns(j);
                if (days.Any(x=>x==_day))
                {
                    Jobs.Add(j);
                }
            }
        }

        private List<DayOfWeek> DaysJobRuns(Job job)
        {
            var days = new List<DayOfWeek>();
            foreach(PropertyInfo p in job.GetType().GetProperties().Where(x=>_jobDay.IsMatch(x.Name)))
            {
                if( p.GetValue(job) is true)
                {
                    var day = $"{char.ToUpper(p.Name.Substring(2)[0])}{p.Name.Substring(3).ToLower()}";
                    if (Enum.TryParse<DayOfWeek>(day,out DayOfWeek result))
                    {            
                       days.Add(result);
                    }
                }
            }

            return days;
        }
    }

    
}
