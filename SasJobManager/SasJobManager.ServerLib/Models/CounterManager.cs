using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.ServerLib.Models
{
    public class CounterManager
    {

        private PerformanceCounter _counter;
        private DateTime _readyTime;
        private string _server;
        private string _category;
        private string _name;
        private string _instance;

        public CounterManager(string server)
        {
            _server = server;
            Log = new List<LogEntry>();
            _category = "Processor";
            _name = "% Processor Time";
            _instance = "_Total";
            _counter = new PerformanceCounter(_category,_name,_instance, _server);
            float initialValue = _counter.NextValue();
            _readyTime = DateTime.Now.AddSeconds(1);
        }
        public List<LogEntry> Log { get; set; }
        public bool CounterReady()
        {
            return DateTime.Now > _readyTime;
        }

        public float GetCounter()
        {
            float currentValue = _counter.NextValue();
            _readyTime = DateTime.Now.AddSeconds(1);
            return currentValue;
        }
    }
}
