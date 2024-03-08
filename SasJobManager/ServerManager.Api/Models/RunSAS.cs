using SasJobManager.Domain;
using SasJobManager.Lib;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;

namespace ServerManager.Api.Models
{
    public class RunSAS
    {
        private Args _args;
        public RunSAS(Args args)
        {
            _args = args;
            SasMgr = new SasManager(args, new SasContext());
            SetPrograms();

        }

        public async Task<List<LogEntry>> RunPgms()
        {
            if (SasMgr.Logger.Where(x => x.IssueType == IssueType.Error).Count() > 0)
            {
                return SasMgr.Logger;
            }

            await SasMgr.ValidateArgs();
            if (SasMgr.Logger.Where(x => x.IssueType == IssueType.Error).Count() > 0)
            {
                return SasMgr.Logger;
            }

            var progress = new Progress<string>(value =>
            {
                Console.WriteLine(value);
            });

            await SasMgr.Run(progress);

            return SasMgr.Logger;

        }

        public void CreateSummary()
        {
            if (_args.DoesCheckLog)
            {
                SasMgr.WriteSummary(true);
            }
        }

        public SasManager SasMgr { get; set; }

        private void SetPrograms()
        {
            if (!string.IsNullOrEmpty(_args.Csv))
            {
                if (File.Exists(_args.Csv))
                {
                    _args.Programs.AddRange(ReadCsv());
                }
                else
                {
                    SasMgr.Logger.Add(new LogEntry()
                    {
                        Msg = $"The CSV file {_args.Csv} could not be located",
                        IssueType = IssueType.Error,
                        Source = "RunSAS.SetPrograms"
                    });
                }
            }
        }

        private IEnumerable<SasProgram> ReadCsv()
        {
            string line = string.Empty;
            using (var fs = new FileStream(_args.Csv, FileMode.Open, FileAccess.Read))
            {
                using (var r = new StreamReader(fs))
                {
                    int counter = 0;
                    while ((line = r.ReadLine()) != null)
                    {
                        if (line.IndexOf(',') > 0)
                        {
                            var vals = line.Split(',');

                            counter++;
                            var isQc = vals.Length > 1 ? vals[1].Trim() == "2" ? true : false : false;
                            if (!string.IsNullOrEmpty(vals[0]))
                            {
                                if (File.Exists(vals[0]))
                                {
                                    var pgm = new SasProgram(vals[0], isQc, counter);
                                    yield return pgm;
                                }
                                else
                                {
                                    SasMgr.Logger.Add(new LogEntry()
                                    {
                                        Msg = $"The program {vals[0]} could not be located",
                                        IssueType = IssueType.Warning,
                                        Source = "RunSAS.ReadCSV"
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
