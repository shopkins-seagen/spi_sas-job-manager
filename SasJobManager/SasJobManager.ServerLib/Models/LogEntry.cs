using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.ServerLib.Models
{
    public class LogEntry
    {
        public LogEntry()
        {

        }
        public LogEntry(string msg,IssueType issueType,string source)
        {
            Msg = msg;
            IssueType = issueType;
            Source = source;
        }
        public string Source { get; set; }
        public string Msg { get; set; }
        public IssueType IssueType { get; set; }

    }
}
