using ServerManager.Api.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.LaunchScheduledJobs.Service
{
    public class ActiveDirectoryService
    {
        private string _domain;
        private PrincipalContext _pc;
        public ActiveDirectoryService(string domain)
        {
            _domain = domain;
            _pc = new PrincipalContext(ContextType.Domain, domain);
        }

        public async Task<IEnumerable<Principal>> GetAllGroups()
        {
            var groups = new List<Principal>();
            await Task.Run(() =>
            {
                var gp = new GroupPrincipal(_pc);
                PrincipalSearcher searcher = new PrincipalSearcher(gp);
                foreach (var g in searcher.FindAll().Where(x => x.Name.StartsWith("SAS_")).OrderBy(x => x.Name))
                {
                    groups.Add(g);
                }
            });
            return groups;
        }

        public bool IsUserInGroup(string user, string group)
        {
            var gp = GroupPrincipal.FindByIdentity(_pc, group);
            return gp.GetMembers(true).Any(x => x.SamAccountName.ToLower() == user.ToLower());
        }

        public bool IsDriverInGroup(string driver, string group)
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
                foreach (FileSystemAccessRule rule in new FileSecurity(v, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount)))
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
                return GroupPrincipal.FindByIdentity(_pc, group);
            }
            catch
            {
                return null;
            }
        }

        internal bool IsMember(JobArgs job, string securityGroup, out string msg)
        {
            msg = string.Empty;

            if (!IsDriverInGroup(job.BatFile, securityGroup))
            {
                msg = $"The bat file '{job.BatFile}' for job '{job.Name}' is outside of default security. Specify the correct security group in the scheduler.";
                return false;
            }

            return true;
        }

        public bool CheckMembership(JobArgs job,out string msg)
        {
            msg = string.Empty;


            if (job.SecurityGroup == null)
            {               
                msg = $"Security group is missing for job {job.Name} but special security is checked. Job {job.Name} will not be processed.";
                return false;
            }
            else
            {
                if (!IsUserInGroup(job.User, job.SecurityGroup))
                {
  
                    msg = $"The user '{job.User}' is not a member of security group {job.SecurityGroup}. Job {job.Name} will not be processed.";
                    return false;
                }
                if (!IsDriverInGroup(job.BatFile, job.SecurityGroup))
                {
                    msg =  $"The bat file '{job.BatFile}' does not inherit security group {job.SecurityGroup}. Job {job.Name} will not be processed.";
                    return false;
                }
            }
            

            return true;
        }
    }
}
