using SasJobManager.Lib.Models;

namespace ServerManager.Api.Service
{
    public class SasRunnerService
    {
        private Args _args;
        public SasRunnerService(Args args )
        {
            _args= args; 
        }

        public void Run()
        {
            //var pgms = GetSasPrograms();
            //if (pgms.Count() > 0)
            //{
            //    var args = GetArgs(pgms);

            //    var sas = new SasManager(args, new Domain.SasContext());
            //    var progress = new Progress<string>(value =>
            //    {
            //        Message = value;
            //    });
            //    await sas.Run(progress);
            //    if (args.DoesCheckLog)
            //    {
            //        sas.WriteSummary(true);
            //    }

            //}
        }


        public void SetPrograms()
        {
            //if (!string.IsNullOrEmpty(_args.))
            //{
            //    if (File.Exists(this.Csv))
            //    {
            //        Programs.AddRange(ReadCsv());
            //    }
            //    else
            //    {
            //        Logger.Add(new LogEntry()
            //        {
            //            Msg = $"The CSV file {Csv} could not be located",
            //            IssueType = IssueType.Error,
            //            Source = "CmdArgs.SetPrograms"
            //        });
            //    }
            //}
            //else
            //{
            //    int counter = 0;
            //    foreach (var p in Pgms)
            //    {
            //        counter++;
            //        var pgm = new SasProgram(p, false, counter);
            //        Programs.Add(pgm);
            //    }
            //}
        }

        //private IEnumerable<SasProgram> ReadCsv()
        //{
            //string line = string.Empty;
            //using (var fs = new FileStream(Csv, FileMode.Open, FileAccess.Read))
            //{
            //    using (var r = new StreamReader(fs))
            //    {
            //        int counter = 0;
            //        while ((line = r.ReadLine()) != null)
            //        {
            //            if (line.IndexOf(',') > 0)
            //            {
            //                var vals = line.Split(',');

            //                counter++;
            //                var isQc = vals.Length > 1 ? vals[1].Trim() == "2" ? true : false : false;
            //                var pgm = new SasProgram(vals[0], isQc, counter);

            //                yield return pgm;
            //            }
            //        }
            //    }
            //}
        //}
    }
}
