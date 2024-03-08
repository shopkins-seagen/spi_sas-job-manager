using Microsoft.Extensions.Configuration;
using SasJobManager.Domain;
using SasJobManager.ServerLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class WorkspaceManager
    {
        private Args _args;

        internal static SASObjectManager.ObjectKeeper _objectKeeper;

        private SAS.Workspace _workspace = null;
        private Array carriage = default(Array);
        private Array lineTypes = default(Array);
        private Array lines = default(Array);
        private string _resolveFolder = "resolved";

        public WorkspaceManager(Args args, IConfigurationRoot _cfg)
        {
            _args = args;
            _objectKeeper = new SASObjectManager.ObjectKeeper(); ;
            Logger = new List<LogEntry>();
            SasObjects = new List<SasObject>();
            SasLogService = new SasLogService(_cfg, _args.DoesCheckLog);
            Connect();
        }

        public SasLogService SasLogService;
        public List<LogEntry> Logger;
        public List<SasObject> SasObjects;

        public SAS.Workspace Workspace
        {
            get
            {
                if (_workspace == null)
                    Connect();

                if (_workspace != null)
                    return _workspace;
                else
                {
                    Logger.Add(new LogEntry { IssueType = IssueType.Fatal, Msg = "Could not connect to SAS Workspace", Source = "WorkspaceManager.Workspace" });
                    return null;
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                return _workspace != null;
            }
        }

        public void Close()
        {
            if (IsConnected) _workspace.Close();
            _objectKeeper.RemoveAllObjects();
            _workspace = null;
        }

        public void Connect()
        {
            if (_workspace != null)
            {
                try
                {
                    Close();
                }
                catch { }
                finally
                {
                    _workspace = null;
                }
            }
            try
            {
                SASObjectManager.IObjectFactory2 obObjectFactory = new SASObjectManager.ObjectFactoryMulti2();
                SASObjectManager.ServerDef obServer = new SASObjectManager.ServerDef();
                obServer.MachineDNSName = _args.Server;
                obServer.Protocol = SASObjectManager.Protocols.ProtocolBridge;
                obServer.Port = Convert.ToInt32(_args.Port);
                obServer.ClassIdentifier = "440196d4-90f0-11d0-9f41-00a024bb830c";
                obServer.BridgeSecurityPackage = "Negotiate";
                obServer.BridgeSecurityPackageList = "Kerberos";
                _workspace = (SAS.Workspace)obObjectFactory.CreateObjectByServer(_args.Context, true, obServer,null, null);
                _objectKeeper.AddObject(1, _args.Context, _workspace);
            }
            catch (Exception ex)
            {
                Logger.Add(new LogEntry { IssueType = IssueType.Fatal, Msg = ex.Message, Source = "SasServer.Connect" });
            }
        }

        public List<SasLogFinding> Submit(SasProgram pgm)
        {

            var response = new List<SasLogFinding>();
            var ls = _workspace.LanguageService;
            pgm.Started = DateTime.Now;

            if (IsConnected)
            {
                try
                {
                
                    ls.Submit(ProgramContent(pgm));

                    ls.FlushLogLines(5000000, out carriage, out lineTypes, out lines);
                    response.AddRange(SasLogService.ManageLog(lines, lineTypes, pgm));

                    WriteLst(ls, pgm);

                    Logger.Add(new LogEntry { IssueType = IssueType.Info, Msg = $"{pgm.Name()} submitted successfully", Source = "SasServer.Submit" });
                    pgm.Completed = DateTime.Now;
                    if (_args.DoesMacroLogging)
                    {
                        SasObjects = GetSasObjects(ls, pgm);
                    }
                    if (_args.DoesResolveProgramCode)
                    {
                        var resolvedPgm = Path.Combine(pgm.Dir(), _resolveFolder, Path.GetFileName(pgm.PgmFn));
                        var formatter = new CodeFormatter(resolvedPgm);
                        formatter.FormatCode();
                    }
                }
                catch(Exception ex)
                {
                    Logger.Add(new LogEntry { IssueType = IssueType.Fatal, Msg = $"{ex.Message}. The session became disconnected during code execution. Terminating a connection to a workspace using 'Endsas' is not supported.", Source = "WorkspaceManager.Submit" });
                }
            }
            else
            {
                Logger.Add(new LogEntry { IssueType = IssueType.Fatal, Msg = "Unable to connect to SAS Workspace", Source = "WorkspaceManager.Submit" });
            }

            this.Close();

            return response;
        }

        private void WriteLst(SAS.LanguageService ls, SasProgram pgm)
        {
            Array c = default(Array);
            Array lt = default(Array);
            Array l = default(Array);

            ls.FlushListLines(1000000, out c, out lt, out l);
            if (l.Length > 0)
            {
                using (var sw = new StreamWriter(Path.Combine(pgm.Dir(), $"{pgm.NameWithoutExtension()}.lst")))
                {
                    for (int i = 0; i < l.GetLength(0); i++)
                    {
                        sw.WriteLine(l.GetValue(i) as string);
                    }
                }
            }
        }

        private List<SasObject> GetSasObjects(SAS.LanguageService ls, SasProgram pgm)
        {
            Array cr = default(Array);
            Array lt = default(Array);
            Array output = default(Array);

            var response = new List<SasObject>();

            StringBuilder metrics = new StringBuilder();
            metrics.AppendLine("ods listing;");
            metrics.AppendLine("proc catalog cat = work.sasmac1 entrytype = macro;" +
                    "contents out=spi_macros(keep = name desc where =(strip(upcase(name))=:'MCR'));" +
                    "run;quit;");
            metrics.AppendLine("options ls=max ps=max;title;");
            metrics.AppendLine("data _null_;set spi_macros;prefix=\"Macro\";file print dlm = ','; put prefix name desc;run;");

            metrics.AppendLine("data spi_env(keep=name value);");
            metrics.AppendLine("length product protocol analysis release $100 isLocalized $2 level $10 name $32  value $100;");
            metrics.AppendLine("array vars[*] product--level;");
            metrics.AppendLine("array env[6] $100 _temporary_('PRODUCT' 'PROJECT' 'ANALYSIS' 'DRAFT' 'ISLOCALIZED' 'FOLDERLEVEL');");
            metrics.AppendLine("do i = 1 to dim(vars);");
            metrics.AppendLine("name = env[i];");
            metrics.AppendLine("if symexist(name) then do;");
            metrics.AppendLine("value = resolve('&' || env[i]);");
            metrics.AppendLine("end;");
            metrics.AppendLine("else value = '';");
            metrics.AppendLine("output;");
            metrics.AppendLine("end;");
            metrics.AppendLine("run;");
            metrics.AppendLine("data _null_;set spi_env;prefix=\"Mvar\";file print dlm = ','; put prefix name value;run;");

            ls.Submit(metrics.ToString());
            ls.FlushListLines(100, out cr, out lt, out output);
            for (int i = 0; i < output.Length; i++)
            {
                var line = output.GetValue(i) as string;
                if (!string.IsNullOrEmpty(line))
                {
                    if (line.Contains(','))
                    {
                        var vals = line.Split(',');
                        if (Enum.TryParse<SasObjectType>(vals[0], out SasObjectType type))
                        {
                            response.Add(new SasObject()
                            {
                                Type = type,
                                Name = vals[1],
                                Value = vals[2]
                            });
                        }                       
                    }
                }
            }
            foreach(var r in response.Where(x=>x.Type == SasObjectType.Mvar))
            {
                if (string.IsNullOrWhiteSpace(r.Value))
                {
                    r.Value = GetValueFromPgm(r.Name, pgm.PgmFn);
                }
            }
            var level = response.Where(x => x.Name == "FOLDERLEVEL").FirstOrDefault();
            if (level != null)
            {
                if (string.IsNullOrWhiteSpace(level.Value))
                {
                    level.Value = GetLevel(response.Where(x => x.Type == SasObjectType.Mvar).ToList());
                }
                if (!string.IsNullOrEmpty(level.Value))
                {
                    if (Enum.TryParse<FolderLevel>(level.Value, out FolderLevel fLevel))
                    {
                        if (fLevel < FolderLevel.ANALYSIS)
                        {
                            string[] folders = { "PRODUCT", "PROJECT", "ANALYSIS", "DRAFT" };
                            for (int i = 0; i < folders.Length; i++)
                            {
                                

                                if ((int)fLevel <= i)
                                {
                                    var mvar = response.Where(x => x.Name == folders[i]).FirstOrDefault();
                                    if (mvar != null)
                                    {
                                        response.Where(x => x.Name == folders[i]).FirstOrDefault().Value = String.Empty;
                                    }
                                }
                            }
                        }
                    }
                }                    
            }

            return response;
        }
        private string GetLevel(List<SasObject> sasObjects)
        {
            var elements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { {"PRODUCT", "PRODUCT" }, { "PROJECT","PROTOCOL" }, { "ANALYSIS", "ANALYSIS" } };
            var currLevel = "SPI";
            foreach (var level in elements.Keys)
            {                
                if (sasObjects.Where(x => x.Name == level && !string.IsNullOrEmpty(x.Value)).Any())
                {
                    currLevel = elements[level];
                }               
            }

            return currLevel;
        }

        private string GetValueFromPgm(string name, string pgmFn)
        {
            var elements = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase) { { "PRODUCT",0 }, { "PROJECT",1 }, { "ANALYSIS",2 }, { "DRAFT",3 }};
            var folders = pgmFn.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Skip(2);
            var idx = folders.ToList().IndexOf("utilities");

            if (idx>=0)
            {
                folders = folders.Take(idx);
            }

            try
            {
                return folders.ToArray()[elements[name]];
            }
            catch
            {
                return string.Empty;
            }                       
        }

        private string ProgramContent(SasProgram pgm)
        {

            var cmd = new StringBuilder();
            cmd.AppendLine($"%let _apparg={pgm.Dir()};\n%let sasprogramfile={pgm.PgmFn};\n");

            if (_args.Mvars.Count > 0)
            {
                cmd.AppendLine($"%global {string.Join(' ',_args.Mvars.Keys)};");
                foreach(var d in _args.Mvars)
                {
                    cmd.AppendLine($"%let {d.Key}={d.Value};");
                }
            }

            var lines = File.ReadAllLines(pgm.PgmFn,Encoding.UTF8).ToList();
            lines.Insert(0, cmd.ToString());

            if (_args.DoesResolveProgramCode)
            {
                AddCodeResolver(pgm, lines);
            }

            return string.Join('\n', lines);
        }

        private List<string> AddCodeResolver(SasProgram pgm, List<string> lines)
        {
            var resolved = Path.Combine(pgm.Dir(), _resolveFolder);
            Directory.CreateDirectory(resolved);
            var fn = Path.Combine(resolved, $"{pgm.NameWithoutExtension()}.sas");
            var cmd = $"options mfile mprint source2;\nfilename mprint \"{fn}\";\n%macro spi_code_wrapper;";
            lines.Insert(1, cmd);
            lines.Add("\n%mend spi_code_wrapper;\n%spi_code_wrapper;");
            return lines;
        }
    }
}
