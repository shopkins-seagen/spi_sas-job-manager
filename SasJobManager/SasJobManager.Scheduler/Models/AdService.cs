using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace SasJobManager.Scheduler.Models
{
    public class AdService
    {
        private string _domain;
        private PrincipalContext _pc;
        public AdService(string domain)
        {
            _domain = domain;
            _pc = new PrincipalContext(ContextType.Domain, domain);
        }

        public async Task<IEnumerable<Principal>> GetRegisteredGroups(List<string> registeredGroups)
        {
            var groups = new List<Principal>();
            await Task.Run(() =>
            {
                var gp = new GroupPrincipal(_pc);

                foreach(var g in registeredGroups)
                {
                    var group = GroupPrincipal.FindByIdentity(_pc, g);
                    if (group != null)
                        groups.Add(group);
                }              
            });
            return groups;
        }

        public bool IsUserInGroup(string user,string group)
        {
            var gp = GroupPrincipal.FindByIdentity(_pc, group);
            return gp.GetMembers(true).Any(x=>x.SamAccountName.ToLower() == user.ToLower());
        }

        public bool IsDriverInGroup(string driver,string group)
        {
            var gp = GroupPrincipal.FindByIdentity(_pc, group);
            var acls = GetGroupsByFolder(Path.GetDirectoryName(driver));

            return acls.Any(x => x == group);
        }

        private List<string> GetGroupsByFolder(string? v)
        {
            var groups = new List<string>();
            if (Directory.Exists(v))
            {
                foreach(FileSystemAccessRule rule in new FileSecurity(v, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount)))
                {
                    var g = GroupPrincipal.FindByIdentity(_pc, rule.IdentityReference.Value);
                    if (g != null)
                    {
                        if (g.Name.StartsWith("SAS_"))
                        {
                            groups.Add(g.Name);
                        }
                    }
                }
            }

            return groups;
        }

        public Principal? ConvertToPrincipal(string? group)
        {
            if (group == null)
                return null;
            try
            {
                return GroupPrincipal.FindByIdentity(_pc,group); 
            }
            catch
            {
                return null;
            }
        }
    }
}
