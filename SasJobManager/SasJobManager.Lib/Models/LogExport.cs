using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class LogExport
    {
        public string Log { get; set; }
        public XLHyperlink Hyperlink { get; set; }
        public string Programmer { get; set; }
        public string Tester { get; set; }
        public string WorstFinding { get; set; }
        public string Elapsed { get; set; }
        public int? TestsPassed { get; set; }
        public int? TestsFailed { get; set; }
        public string Category { get; set; }
        public string Line { get; set; }
    }
}
