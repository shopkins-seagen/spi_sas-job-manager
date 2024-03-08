using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Cli.Models
{
    public class Programmers
    {
        public Programmers(string dev,string tester)
        {
            Developer = dev;
            Tester= tester;
        }
        public string? Developer { get; set; }
        public string? Tester { get; set; }
    }
}
