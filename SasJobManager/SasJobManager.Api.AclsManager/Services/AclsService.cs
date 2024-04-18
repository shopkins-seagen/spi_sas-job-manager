using Microsoft.AspNetCore.Authentication;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SasJobManager.Api.AclsManager.Domain;
using SasJobManager.Api.AclsManager.Model;
using System.DirectoryServices.AccountManagement;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.RegularExpressions;

namespace SasJobManager.Api.AclsManager.Services
{
    public class AclsService
    {
        private Args _args;
        private IConfiguration _cfg;
        private List<GroupDetails> _groups;
        private SasLogContext _context;

        public AclsService(Args args, IConfiguration cfg,SasLogContext context)
        {
            _args = args;
            _cfg = cfg;
            _context = context;
            Log = new List<LogEntry>();
        }
        public List<LogEntry> Log { get; set; }

        public void Unlock()
        {
            if (ToggleInheritance(false))
            {
                Log.Add(new LogEntry($"Inheritance restored for {Path.GetFileName(_args.FileName)}", MsgType.Notice));
            }
        }
        public void Lock()
        {
            var filter = new Regex(_cfg["filter"], RegexOptions.IgnoreCase);
            var pc = new PrincipalContext(ContextType.Domain, _cfg["domain"]);

            try
            {
                if (ToggleInheritance(true))
                {
                    var fi = new FileInfo(_args.FileName);
                    var rules = new FileSecurity(fi.FullName, AccessControlSections.Access).GetAccessRules(true, true, typeof(NTAccount));

                    foreach (AuthorizationRule rule in rules)
                    {
                        FileSystemAccessRule ar = rule as FileSystemAccessRule;
                        if (ar != null)
                        {
                            var group = GroupPrincipal.FindByIdentity(pc, ar.IdentityReference.Value);

                            if (group != null)
                            {
                                if (filter.IsMatch(group.Name))
                                {
                                    if (!HasFlag(ar, FileSystemRights.FullControl))
                                    {
                                        if (HasFlag(ar, FileSystemRights.Modify))
                                        {
                                            var removeModifyRule = new FileSystemAccessRule(ar.IdentityReference, FileSystemRights.Modify, AccessControlType.Allow);
                                            var addReadExecuteRule = new FileSystemAccessRule(ar.IdentityReference, FileSystemRights.ReadAndExecute, AccessControlType.Allow);
                                            var fs = fi.GetAccessControl();
                                            fs.ModifyAccessRule(AccessControlModification.RemoveSpecific, removeModifyRule, out bool isUpdated);
                                            fs.AddAccessRule(addReadExecuteRule);
                                            fi.SetAccessControl(fs);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    LogEntry response = RecordLog();
                    if (response.MsgType != MsgType.Notice)
                    {
                        Log.Add(new LogEntry($"Failed to record history in the database: {response.Msg}", MsgType.Warning));
                        return;
                    }
                    Log.Add(new LogEntry($"Write access removed for: {_args.FileName}",MsgType.Notice));
                }
            }
            catch (Exception ex)
            {
                Log.Add(new LogEntry(ex.Message, MsgType.Error));
            }
        }

        private LogEntry RecordLog()
        {
            try
            {
                var entity = _context.SasLogs.FirstOrDefault(x=>x.LogFile.ToLower()==_args.FileName.ToLower());

                if (entity != null)
                {
                    var log = new SasLog();
                    log.LogFile = _args.FileName;
                    var history = new SasLogHistory();
                    history.User = _args.User;
                    history.LastModified = DateTime.Now;
                    log.History.Add(history);
                    _context.SasLogs.Add(log);
                }
                else
                {
                    var log = new SasLog();
                    log.LogFile = _args.FileName;
                    var history = new SasLogHistory();
                    history.User = _args.User;
                    history.LastModified = DateTime.Now;
                    log.History.Add(history);
                    _context.SasLogs.Add(log);
                }
                _context.SaveChanges();
                return new LogEntry($"{Path.GetFileName(_args.FileName)} is set to RO", MsgType.Notice) ;

            }
            catch(Exception ex)
            {
                return new LogEntry(ex.Message,MsgType.Error) ;
            }
        }

        public bool ToggleInheritance(bool isBreakInheritance)
        {
            try
            {
                var fi = new FileInfo(_args.FileName);
                var fs = fi.GetAccessControl();
                fs.SetAccessRuleProtection(isBreakInheritance, true); /* True not inherited, false is inherited */
                fi.SetAccessControl(fs);
                return true;
            }
            catch(Exception ex) 
            {
                Log.Add(new LogEntry(ex.Message, MsgType.Error));
                return false;
            }
        }

        private bool HasAccess()
        {
            return false;
        }

        private bool HasFlag(FileSystemAccessRule ar, FileSystemRights rights)
        {
            return ar.FileSystemRights.HasFlag(rights);
        }
    }
}
