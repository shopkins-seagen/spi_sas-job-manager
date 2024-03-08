using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Domain.Models
{
    public class DbResponse
    {
        public bool IsOk { get; set; }
        public string? Msg { get; set; }
        public int Id { get; set; }

    }
}
