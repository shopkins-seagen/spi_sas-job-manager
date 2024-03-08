using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class SasProgram
    {
        public SasProgram(string pgm, bool isQc, int seq)
        {
            PgmFn = pgm;
            IsQc = isQc;
            Seq = seq;

            if (pgm != null)
            {
                try
                {
                    if (File.Exists(pgm))
                    {
                        LogFn = Path.Combine(Path.GetDirectoryName(pgm), $"{Path.GetFileNameWithoutExtension(pgm)}.log");
                        IsValid = true;
                    }
                    else
                    {
                        IsValid = false;
                    }

                }
                catch
                {
                    IsValid = false;
                }
            }
        }
        public string PgmFn { get; set; }
        public string LogFn { get; set; }
        public bool IsQc { get; set; }
        public int Seq { get; set; }
        public bool IsValid { get; set; }
        public DateTime Started { get; set; }
        public DateTime Completed { get; set; }

        public int? DeriveOrder { get; set; }

        public string? Developer { get; set; }

        public string? Tester { get; set; }



        public string Dir()
        {
            if (IsValid)
            {
                return Path.GetDirectoryName(PgmFn);
            }
            return string.Empty;
        }
        public string NameWithoutExtension()
        {
            if (IsValid)
            {
                return Path.GetFileNameWithoutExtension(PgmFn);
            }
            return string.Empty;
        }

        public string Name()
        {
            return Path.GetFileNameWithoutExtension(PgmFn);
        }

    }
}
