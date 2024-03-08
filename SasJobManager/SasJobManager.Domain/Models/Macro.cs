using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class Macro
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public SasProgram Program { get; set; }    
        public int ProgramId { get; set; }
        
    }
}
