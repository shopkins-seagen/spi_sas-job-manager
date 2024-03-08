using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.Security.Principal;
using System.DirectoryServices.ActiveDirectory;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ServerManager.Api.Service
{
    public static class ActiveDirectoryService
    {
        private static readonly string _domain = "SG.SEAGEN.COM";
        

        public static bool DoesFolderHaveGroup(string f,string g)
        {
            var pc  = new PrincipalContext(ContextType.Domain, _domain);

            if (Directory.Exists(f))
            {
                foreach (FileSystemAccessRule rule in new FileSecurity(f, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount)))
                {

                    var group = GroupPrincipal.FindByIdentity(pc, rule.IdentityReference.Value);
                    if (group != null)
                    {
                        if (group.Name.Equals(g,StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
