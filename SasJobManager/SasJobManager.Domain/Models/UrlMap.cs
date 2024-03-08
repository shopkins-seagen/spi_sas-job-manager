using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class UrlMap
    {
        public int Id { get; set; }
        public string Url { get; set; } 
        public string SecurityGroup { get; set; }
        public bool? IsDefault { get; set; }
    }
}
