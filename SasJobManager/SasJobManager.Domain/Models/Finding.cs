using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class Finding
    {
        public int Id { get; set; }
        public string IssueType { get; set; }
        public string Msg { get; set; }
        public SasProgram Program { get; set; }
        public int ProgramId { get; set; }
    }
}
