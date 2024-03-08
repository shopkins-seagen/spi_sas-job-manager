using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class JobRun
    {
        public JobRun()
        {
            Messages = new List<JobRunMsg>();
        }
        public int Id { get; set; }
        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }
        public string? WorstFinding { get; set; }
        public Job Job { get; set; }    
        public int JobId { get; set; }
        public string AppPoolIdentity { get; set; }
        public virtual ICollection<JobRunMsg> Messages { get; set; }

    }
}
