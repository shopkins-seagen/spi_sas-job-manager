using Microsoft.AspNetCore.Mvc;
using SasJobManager.Lib;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using ServerManager.Api.Models;
using SasJobManager.Domain;
using ServerManager.Api.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RunController : ControllerBase
    {
        private Args _args;
        private SasManager _sas;
        public RunController()
        {
                
        }

        // POST api/<RunController>
        [HttpPost]
        public void  Post([FromBody] List<string> drivers)
        {
            var parser = new DriverParser(drivers);
            parser.ParseBats();
            //_args = args;
            //_sas = new SasManager(args, new SasContext());
            //SetPrograms();

            //await _sas.ValidateArgs();
            //if (_sas.Logger.Where(x=>x.IssueType==IssueType.Error).Count()>0)
            //{
            //    return _sas.Logger;
            //}

            //Task.Run(async () =>
            //{
            //    var msgs = new List<string>();
            //    var progress = new Progress<string>(value =>
            //    {
            //        using (var sw = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "test.txt"), true))
            //        {
            //            sw.WriteLine(value);
            //        }
            //        //msgs.Add(value);
            //    });

            //    try
            //    {
            //        await _sas.Run(progress);
            //        if (_args.DoesCheckLog)
            //        {
            //            _sas.WriteSummary(true);
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _sas.Logger.Add(new LogEntry()
            //        {
            //            IssueType = IssueType.Fatal,
            //            Msg = ex.Message,
            //            Source = "Post"
            //        });    
            //    }
            //});

            //return new List<LogEntry>() { new LogEntry()
            //{
            //    IssueType=IssueType.Info,
            //    Msg="ok i guess",
            //    Source="Post"
            //}};      
        }

        private void SetPrograms()
        {
            //if (!string.IsNullOrEmpty(_args.Csv))
            //{
            //    if (System.IO.File.Exists(_args.Csv))
            //    {
            //        _args.Programs.AddRange(ReadCsv());
            //    }
            //    else
            //    {
            //        _sas.Logger.Add(new LogEntry()
            //        {
            //            Msg = $"The CSV file {_args.Csv} could not be located",
            //            IssueType = IssueType.Error,
            //            Source = "RunSAS.SetPrograms"
            //        });
            //    }
            //}
        }



        //private IEnumerable<SasProgram> ReadCsv()
        //{
        //    string line = string.Empty;
        //    using (var fs = new FileStream(_args.Csv, FileMode.Open, FileAccess.Read))
        //    {
        //        using (var r = new StreamReader(fs))
        //        {
        //            int counter = 0;
        //            while ((line = r.ReadLine()) != null)
        //            {
        //                if (line.IndexOf(',') > 0)
        //                {
        //                    var vals = line.Split(',');

        //                    counter++;
        //                    var isQc = vals.Length > 1 ? vals[1].Trim() == "2" ? true : false : false;
        //                    if (!string.IsNullOrEmpty(vals[0]))
        //                    {
        //                        if (System.IO.File.Exists(vals[0]))
        //                        {
        //                            var pgm = new SasProgram(vals[0], isQc, counter);
        //                            yield return pgm;
        //                        }
        //                        else
        //                        {
        //                            _sas.Logger.Add(new LogEntry()
        //                            {
        //                                Msg = $"The program {vals[0]} could not be located",
        //                                IssueType = IssueType.Warning,
        //                                Source = "RunSAS.ReadCSV"
        //                            });
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
