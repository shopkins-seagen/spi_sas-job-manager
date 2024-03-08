using SasJobManager.Domain;
using SasJobManager.Lib;
using SasJobManager.Lib.Models;
using SasJobManager.ServerLib.Models;
using System.DirectoryServices.ActiveDirectory;

namespace ServerManager.Api.Models
{
    public class PgmRunner
    {
        private Args _args;
        public PgmRunner(Args args)
        {
            _args = args;
            RunMessages = new List<string>();
        }

        public List<string> RunMessages { get; set; }
        public async Task Submit()
        {
            await Task.Run(async () =>
            {
                var sas = new SasManager(_args, new SasContext());
                var progress = new Progress<string>(value =>
                {
                    RunMessages.Add(value);
                });

                await sas.Run(progress);
                if (_args.DoesCheckLog)
                {
                    sas.WriteSummary(true);
                }
            });
        }


    public SasManager SasMgr { get; set; }




}
}
