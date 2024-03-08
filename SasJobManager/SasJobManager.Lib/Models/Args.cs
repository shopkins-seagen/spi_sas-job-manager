
using SasJobManager.ServerLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class Args
    {
        private DateTime _started;
        public Args() 
        { 
            Programs = new List<SasProgram>(); 
            Recipients = new List<string>();
            _started = DateTime.Now;
            Logger = new List<LogEntry>();
            Mvars = new Dictionary<string, string>();
        }

        public Args(string server, string context, string port)
        {
            Server = server;
            Context = context;
            Port = port;
            Programs = new List<SasProgram>();
            Recipients = new List<string>();
            _started = DateTime.Now;
            Logger = new List<LogEntry>();
            Mvars = new Dictionary<string, string>();
        }
        public List<LogEntry> Logger;
        public Dictionary<string,string> Mvars { get; set; }
        public string SummaryFn { get; set; }
        public string Server { get; set; }
        public string Context { get; set; }
        public string Port { get; set; }
        public List<SasProgram> Programs { get; set; }
        public bool IsAsync { get; set; }
        public string BaseMetricsUrl { get; set; }
        public string MetricsUrl { get; set; }
        public bool DoesMacroLogging { get; set; }
        public bool DoesResolveProgramCode { get; set; }
        public bool DoesCheckLog { get; set; }
       
        public bool UseBestServer { get; set; }
        public bool DoesNotifyOnCompletion { get; set; }
        public bool OnlyReviewLogs { get; set; }
        public bool DoesIncludeAutoQcReview { get; set; }
        public bool IsInteractive { get; set; }
        public string? Csv { get; set; }
        public string? Ai { get; set; }
        public bool UseDeriveOrder { get; set;}
        public bool DoesDisplayProgrammers { get; set; }
        public bool UseAi { get; set; }
        public bool DoesQuitOnError { get; set; }
        public bool DoesExportLogSummary { get; set; }
      
        public List<string> Recipients { get; set; }

    }
}
