using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class SchedulerRun
    {
        public SchedulerRun()
        {
        }

        public int Id { get; set; }
        public DateTime LaunchedAt { get; set; }
        public bool IsOk { get; set; }
        public string Name { get; set; }
        public int JobId { get; set; }
        public string DriverLoc { get; set; }
        public string Owner { get; set; }
        public string? SecurityGroup { get; set; }
        public string? Msg { get; set; }
        public string? Message { get; set; }

    }
}
