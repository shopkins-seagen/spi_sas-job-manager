using System.Reflection;

namespace SasJobManager.ServerLib.Models
{
    public class Server
    {
        private string _server;
        CounterManager _counter;
        public Server(string server)
        {
            _counter = new CounterManager(server);
            Log = new List<LogEntry>();
        }
        public List<LogEntry> Log { get; set; }
        public int GetCpuUtilization()
        {
            var isReady = _counter.CounterReady();
            int s = 0;

            while (!isReady)
            {
                System.Threading.Thread.Sleep(1000);
                isReady = _counter.CounterReady();
                s++;
                if (s > 20) // if over 20s, something went wrong
                {
                    Log.Add(new LogEntry() { IssueType = IssueType.Warning, Msg = "Unable to calculate server utilization", Source = MethodBase.GetCurrentMethod().Name });
                    return 100;
                }
            }
            return (int)Math.Round(_counter.GetCounter());
        }
    }
}