using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class JobRunMsg
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public JobRun JobRun { get; set; }  
        public int JobRunId { get; set; }
    }
}
