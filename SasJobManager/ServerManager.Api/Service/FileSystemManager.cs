using System.Management.Automation;

namespace ServerManager.Api.Service
{
    public static class FileSystemManager
    {

        public static void MapDrive(IConfiguration cfg)
        {
            var aliases = cfg.GetSection($"mappings:aliases").GetChildren().Select(x => x.Value).ToList();
            var servers = cfg.GetSection($"mappings:servers").GetChildren().Select(x => x.Value).ToList();
            try
            {
                for (int i = 0; i < aliases.Count; i++)
                {
                    PowerShell ps = PowerShell.Create()
                        .AddScript($"subst {aliases[i]}: {servers[i]}");
                    ps.Invoke();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
