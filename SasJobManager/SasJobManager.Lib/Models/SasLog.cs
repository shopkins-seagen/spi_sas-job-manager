using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class SasLog
    {
        public SasLog(SasProgram pgm)
        {
            Findings = new List<SasLogFinding>();
            Program = pgm;
        }
        public SasProgram Program { get; set; }
        public List<SasLogFinding> Findings { get; set; }
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public bool IsQcPgm { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finsished { get; set; }
        
        public SasFindingType WorstFinding()
        {
            if (Findings.Count > 0)
            {
                return Findings.Max(x => x.Type);
            }
            else
                return SasFindingType.Clean;
        }
        public bool IsPass()
        {
            return (PassCount > 0 && FailCount == 0) ? true : false;
        }
    }
}
