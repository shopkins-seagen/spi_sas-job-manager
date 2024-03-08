using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.Security.Principal;

namespace SasJobManager.ServerLib.Models
{
    public class ActiveDirectory
    {
        private string _domain;
        private PrincipalContext _pc;
        public ActiveDirectory(string domain)
        {
            _domain = domain;
            _pc = new PrincipalContext(ContextType.Domain, domain);
            Logger = new List<LogEntry>();
        }

        public List<LogEntry> Logger { get; set; }
        

        public bool IsFolderLocked(string folder)
        {

            try
            {
                if (Directory.Exists(folder))
                {
                    var groups = GetAdGroups(folder, false, true, false).Where(x => !x.IsAdmin);

                    var hasModifyGroups = groups.Where(x => x.HasModify).Select(x => x.Name).ToList();
                    if (hasModifyGroups.Count() > 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    Logger.Add(new LogEntry()
                    {
                        Msg = "Folder not found",
                        IssueType = IssueType.Error,
                        Source = "ActiveDirectory.IsFolderLocked"
                    });
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Add(new LogEntry()
                {
                    Msg = ex.Message,
                    IssueType = IssueType.Error,
                    Source = "ActiveDirectory.IsFolderLocked"
                });

                return false;
            }
        }

        public IEnumerable<GroupInfo> GetAdGroups(string folder, bool includeUsers, bool isSasOnly, bool includeMembers)
        {
            if (Directory.Exists(folder))
            {
                var di = new DirectoryInfo(folder);
                var rules = new FileSecurity(di.FullName, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount));


                foreach (FileSystemAccessRule rule in rules)
                {
                    var grp = new GroupInfo();
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(_pc, rule.IdentityReference.Value);


                    if (group != null)
                    {
                        grp.IsGroup = true;

                        if (group.Name.ToUpper().StartsWith("SAS"))
                        {
                            grp.Name = group.Name;
                            grp.HasModify = IsModify(rule);
                            grp.IsAdmin = IsAdmin(rule);

                            yield return grp;
                        }

                    }
                }
            }
        }

        private bool IsAdmin(FileSystemAccessRule ar)
        {
            return ar.FileSystemRights.HasFlag(FileSystemRights.FullControl);
        }
        private bool IsModify(FileSystemAccessRule ar)
        {
            return ar.FileSystemRights.HasFlag(FileSystemRights.Modify);
        }
    }
}
