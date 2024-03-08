using ServerManager.Api.Models;
using System.Text.RegularExpressions;

namespace ServerManager.Api.Service
{
    public class DriverParser
    {
        private List<string> _driverFiles;
        private Regex _flags;
        private Regex _startJob;
        private List<JobConfig> _jobs;
        public DriverParser(List<string> driverFiles)
        {
            _driverFiles = driverFiles;
            _startJob = new Regex("SasJobManager.Cli.exe", RegexOptions.IgnoreCase);
            _flags = new Regex("^\\s*-(a|s|x|m|n|u|c|p|h)\\s*(.*)", RegexOptions.IgnoreCase);
            _jobs = new List<JobConfig>();
        }

        public void ParseBats()
        {
            foreach (var fn in _driverFiles)
            {
                try
                {
                    using (var s = new StreamReader(fn))
                    {
                        int jobCount = 0;
                        string line;
                        while ((line = s.ReadLine()) != null)
                        {
                            if (_startJob.IsMatch(line))
                            {
                                jobCount++;
                                var job = new JobConfig();
                             

                                if (_flags.IsMatch(line))
                                {
                                    var matches = _flags.Match(line);
                                    if (matches.Groups.Count == 3)
                                    {
                                      
                                    }
                                }
                            }
                        }
                    }
                }
                catch(Exception ex)
                {

                }
            }
        }

        private string FixValue(string v,string loc)
        {
           return v.Replace("\"", String.Empty).Replace("^", String.Empty).Replace("%cd%",loc).Trim();
        }
    }
}
