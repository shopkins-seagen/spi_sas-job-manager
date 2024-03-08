using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class SasObject
    {
        public SasObjectType Type { get; set; }
        public string Name { get; set; }    
        public string Value { get; set; }
    }
}
