using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.ServerLib.Models
{
    public class GroupInfo
    {
        public string Name { get; set; }
        public bool HasModify { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsGroup { get; set; }
    }
}
