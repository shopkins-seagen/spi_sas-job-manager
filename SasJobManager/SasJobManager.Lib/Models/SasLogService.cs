using DocumentFormat.OpenXml.Drawing;
using Microsoft.Extensions.Configuration;
using SasJobManager.Lib.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class SasLogService
    {
        private Array _lines;
        private Array _types;
        private IConfiguration _cfg;
        private Dictionary<Regex, SasFindingType> _patterns;
        private bool _doesCheckLog;
        private bool _includesAutoQc;
        public SasLogService(IConfigurationRoot cfg, bool doesCheckLog, bool includesAutoQc = false)
        {
            _cfg = cfg;
            _doesCheckLog = doesCheckLog;
            _includesAutoQc = includesAutoQc;

            SasLogFindings = new List<SasLogFinding>();
            _patterns = new Dictionary<Regex, SasFindingType>();
            if (doesCheckLog)
                SetLogIssuePatterns();
        }

        public List<SasLogFinding> SasLogFindings;
        public int PassCount { get; set; }
        public int FailCount { get; set; }
        public SasProgram Program { get; set; }


        public async Task<List<SasLogFinding>> ManageLog(Array lines, Array types, SasProgram pgm)
        {

            Program = pgm;
            _lines = lines;
            _types = types;
            WriteLog();

            if (_doesCheckLog)
            {
                CheckLog(pgm);
            }


            return SasLogFindings;
        }

        private void CheckLog(SasProgram pgm)
        {

            for (int i = 0; i < _lines.Length; i++)
            {
                var type = (SAS.LanguageServiceLineType)_types.GetValue(i);
                var line = _lines.GetValue(i) as string;
                if (!string.IsNullOrEmpty(line))
                {
                    switch (type)
                    {
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeError:
                            ReviewLine(SasFindingType.Error, line, i);
                            break;
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeWarning:
                            ReviewLine(SasFindingType.Warning, line, i);
                            break;
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeNote:
                            ReviewLine(SasFindingType.Note, line, i);
                            if (pgm.IsQc)
                            {
                                ReviewLineQc(SasFindingType.QC, line, i);
                            }
                            break;
                        case SAS.LanguageServiceLineType.LanguageServiceLineTypeNormal:
                            ReviewLine(SasFindingType.Notice, line, i);
                            // Errors and warnings with embedded codes
                            ReviewLine(SasFindingType.Error, line, i);
                            ReviewLine(SasFindingType.Warning, line, i);
                            break;
                    }
                }
            }


        }
        public SasLog CheckLogFile(SasProgram pgm)
        {
            var sasLog = new SasLog(pgm);
            sasLog.Started = DateTime.Now;

            _lines = File.ReadLines(pgm.LogFn).ToArray();
            for (int i = 0; i < _lines.Length; i++)
            {

                var line = _lines.GetValue(i) as string;
                if (!string.IsNullOrEmpty(line))
                {
                    bool isMatched = false;
                    if (ReviewLine(SasFindingType.Error, line, i))
                    {
                        isMatched = true;
                    }
                    else if (ReviewLine(SasFindingType.Warning, line, i))
                    {
                        isMatched = true;
                    }
                    else if (ReviewLine(SasFindingType.Note, line, i))
                    {
                        isMatched = true;
                    }
                    else if (ReviewLine(SasFindingType.Notice, line, i))
                    {
                        isMatched = true;
                    }

                    if (_includesAutoQc && !isMatched)
                    {
                        sasLog.IsQcPgm = true;
                        ReviewLineQc(SasFindingType.QC, line, i, ref sasLog);
                    }
                }
            }
            if (SasLogFindings.Count == 0)
            {
                SasLogFindings.Add(new SasLogFinding(SasFindingType.Clean, "No issues detected", 0));
            }
            sasLog.Finsished = DateTime.Now;
            sasLog.Findings.AddRange(SasLogFindings);

            return sasLog;

        }

        private bool ReviewLineQc(SasFindingType type, string line, int i)
        {
            foreach (var r in _patterns.Where(x => x.Value == type))
            {
                if (r.Key.IsMatch(line))
                {
                    if (r.Key.ToString().ToUpper().Contains("PASS"))
                        PassCount++;
                    else
                        FailCount++;
                    return true;
                }
            }
            return false;
        }

        // For log review only functionality
        private bool ReviewLineQc(SasFindingType type, string line, int i, ref SasLog log)
        {
            foreach (var r in _patterns.Where(x => x.Value == type))
            {
                if (r.Key.IsMatch(line))
                {
                    if (r.Key.ToString().ToUpper().Contains("PASS"))
                        log.PassCount++;
                    else
                        log.FailCount++;
                    return true;
                }
            }
            return false;
        }

        private bool ReviewLine(SasFindingType type, string line, int lineNo)
        {
            foreach (var r in _patterns.Where(x => x.Value == type))
            {
                if (r.Key.IsMatch(line))
                {
                    SasLogFindings.Add(new SasLogFinding(type, line, lineNo));
                    return true;
                }
            }
            return false;
        }
        private void WriteLog()
        {
            try
            {
                if (File.Exists(Program.LogFn))
                {
                    FolderService.ToggleReadOnly(Program.LogFn, false);
                }
                using (var s = new StreamWriter(Program.LogFn))
                {
                    foreach (var l in _lines)
                    {
                        s.WriteLine(l);
                    }
                }
                FolderService.ToggleReadOnly(Program.LogFn, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: unable to write Log: {ex.Message}");
            }
        }

        private void SetLogIssuePatterns()
        {
            foreach (var v in _cfg.GetSection($"cfg_lists:notes").GetChildren().Select(x => x.Value).ToList())
            {
                _patterns[new Regex(v, RegexOptions.IgnoreCase)] = SasFindingType.Note;
            }
            string[] _s = { "notice", "errors", "warnings", "qc_pass", "qc_fail" };
            SasFindingType[] _t = { SasFindingType.Notice, SasFindingType.Error, SasFindingType.Warning, SasFindingType.QC, SasFindingType.QC };
            for (int i = 0; i < _t.Length; i++)
            {
                _patterns[new Regex(_cfg[_s[i]])] = _t[i];
            }
        }
    }
}
