using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class Schedule
    {
        public int Id { get; set;  }
        public int Day { get; set; }
        public Job Job { get; set; }
        public int JobId { get; set; }
    }
}
