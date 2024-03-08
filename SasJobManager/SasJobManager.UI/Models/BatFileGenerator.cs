using SasJobManager.Lib.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.Map;

namespace SasJobManager.UI.Models
{
    public class BatFileGenerator
    {
        private Args _args;
        private string _folder;
        private string _exe;
        private string _help;
        private List<SasProgramFile> _programs;

        public BatFileGenerator(Args args,string folder,string exe,string help,List<SasProgramFile> programs )
        {
            _args = args;
            _folder = folder;
            _exe = exe;
            _help = help;
            _programs = programs;
        }

        public void WriteFiles(string batFileName)
        {
            var bn = Path.GetFileNameWithoutExtension(batFileName);

            using (var sw = new StreamWriter(Path.Combine(_folder, $"{bn}.bat")))
            {
                sw.WriteLine($"::See the Quick Reference for available options: {_help}\n");
                sw.WriteLine("@echo off");
                sw.WriteLine($"\"{_exe}\" ^");
                sw.WriteLine($"   -c \"%cd%\\{Path.GetFileNameWithoutExtension(bn)}.csv\" ^");
                
                if (_args.DoesNotifyOnCompletion)
                {
                    sw.WriteLine($"   -n true ^");
                }

                // Only write the params if they are not set to the default
                if (!_args.IsAsync)
                {
                    sw.WriteLine($"   -a {_args.IsAsync} ^");           
                }

                if (!_args.DoesCheckLog)
                    sw.WriteLine($"   -l {_args.DoesCheckLog} ^");
                if (!_args.DoesMacroLogging)
                    sw.WriteLine($"   -m {_args.DoesMacroLogging} ^");
                if (_args.DoesResolveProgramCode)
                    sw.WriteLine($"   -r {_args.DoesResolveProgramCode} ^");
                if (!string.IsNullOrEmpty(_args.Ai))
                {
                    sw.WriteLine($"   -f \"{_args.Ai}\" ^");
                    sw.WriteLine($"   -d {_args.UseDeriveOrder} ^");
                    sw.WriteLine($"   -t {_args.DoesDisplayProgrammers} ^");
                }
                if (Enum.TryParse<ServerContext>(_args.Context,out ServerContext c))
                {
                    if (c != ServerContext.SASApp94)
                    {
                        sw.WriteLine($"   -x {_args.Context} ^");
                    }
                }
                
                if (_args.DoesQuitOnError)
                {
                    sw.WriteLine($"   -q {_args.DoesQuitOnError} ^");
                }
                var server = _args.UseBestServer ? "best" : _args.Server;

                sw.WriteLine($"   -s {server} ^");                
                sw.WriteLine($"   -h \"%cd%\\{Path.GetFileNameWithoutExtension(bn)}.html\" ");
            }

            if (_programs.Count > 0)
            {
                using (var sw = new StreamWriter(Path.Combine(_folder, $"{bn}.csv")))
                {
                    foreach (var p in _programs.OrderBy(x=>x.Seq))
                    {
                        sw.WriteLine($"{p.ToString()},{(p.IsQc ? "2" : "1")}{(_args.UseDeriveOrder?$",{p.Seq}":string.Empty)}");
                    }
                }
            }
        }
    }
}
