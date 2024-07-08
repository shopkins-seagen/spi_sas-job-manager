using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.Security.Principal;
using System.DirectoryServices.ActiveDirectory;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Management.Automation.Internal;

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

        public static bool DoesUserHaveWrite(string user,string folder)
        {
            var pc = new PrincipalContext(ContextType.Domain, _domain);
            var acls = new List<IdentityReference>();

            var fi = new FileInfo(folder);
            var fs = fi.GetAccessControl();
            var rules = new FileSecurity(fi.FullName, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount));

            foreach (AuthorizationRule rule in rules)
            {
                FileSystemAccessRule ar = rule as FileSystemAccessRule;
                if (ar != null)
                {
                    var group = GroupPrincipal.FindByIdentity(pc, ar.IdentityReference.Value);
                    var isAdmin = ar.FileSystemRights.HasFlag(FileSystemRights.FullControl);

                    if (group != null)
                    {
                        if (group.Name.StartsWith("SAS", StringComparison.OrdinalIgnoreCase) && !isAdmin)
                        {
                            if (ar.FileSystemRights.HasFlag(FileSystemRights.Modify))
                            {
                                var members = group.GetMembers(true).Select(x => x.SamAccountName.ToLower()).ToList();
                                if (members.Any(x => x.Contains(user.ToLower())))
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


    }
}
