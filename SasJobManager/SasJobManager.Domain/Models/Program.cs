using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class SasProgram
    {
        public int Id { get; set; }
        public string Program { get; set; }
        public DateTime RunDt { get; set; }

        public string? Product { get; set; }
        public string? Protocol { get; set; }
        public string? Analysis { get; set; }
        public string? Release { get; set; }
        public bool IsLocal { get; set; }
        public string? FolderLevel { get; set; }
        public string? WorstLogFinding { get; set; }
        public string UserId { get; set; }
        public string Client { get; set; }
        public string Server { get; set; }
        public string Context{get;set;}
        public virtual ICollection<Macro> Macros { get; set; } = new List<Macro>();
        public virtual ICollection<Finding> Findings { get; set; } = new List<Finding>();


    }
}
