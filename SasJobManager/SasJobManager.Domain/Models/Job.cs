using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class Job
    {
        public Job()
        {
            JobRuns = new List<JobRun>();
        }
        public int Id { get; set; }
        public string Description { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsRecurring { get; set; }
        public string Driver { get; set; }
        public int Hour { get; set; }
        public bool IsMonday { get; set; }
        public bool IsTuesday { get; set; }
        public bool IsWednesday { get; set; }
        public bool IsThursday { get; set; }
        public bool IsFriday { get; set; }
        public bool IsSaturday { get; set; }
        public bool IsSunday { get; set; }
        public bool IsCustomSecurity { get; set; }
        public string? CustomSecurityGroup { get; set; }

        public virtual ICollection<JobRun> JobRuns { get; set; }
    }
}
