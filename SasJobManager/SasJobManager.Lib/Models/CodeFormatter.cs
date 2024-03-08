using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class CodeFormatter
    {
        private List<string> _formatted;
        private List<string> _raw;
        private string _fn;
        private int _currentPos;
        private int _currentLine;
        private List<Regex> _startPatterns;
        private List<Regex> _endPatterns;
        private List<CodeBlock> _blocks;
        private string _outLoc;
        private int _tabSpaces;
        private Regex _localDrive;


        public CodeFormatter(string fn)
        {
            _fn = fn;
            _outLoc = Path.Combine(Path.GetDirectoryName(fn), "formatted");
            _formatted = new List<string>();
            _blocks = new List<CodeBlock>();
            _raw = new List<string>();
            _currentPos = 0;
            _currentLine = 1;
            SetPatterns();
            Directory.CreateDirectory(_outLoc);
            _tabSpaces = 2;
            _localDrive = new Regex(@"(F|C|E):\\");

        }


        private void SetPatterns()
        {
            _startPatterns = new List<Regex>();
            _endPatterns = new List<Regex>();
            // Sequence of CodeType is Data,Proc,Sql,Comment,Libname,Options,Other - ensure CodeType enum aligns with regex if updated

            Regex[] _str = { new Regex("^data", RegexOptions.IgnoreCase),
                             new Regex(@"^proc\s+(?!sql)",RegexOptions.IgnoreCase),
                             new Regex(@"^proc\s+sql",RegexOptions.IgnoreCase),
                             new Regex(@"((\/\*|%\*)|^\*)"),
                             new Regex(@"^libname\s+",RegexOptions.IgnoreCase),
                             new Regex(@"^option[s]*\s+",RegexOptions.IgnoreCase)
            };
            Regex[] _end = { new Regex(@"run\s*;$", RegexOptions.IgnoreCase),
                             new Regex(@"(quit|run)\s*;",RegexOptions.IgnoreCase),
                             new Regex(@"quit\s*;",RegexOptions.IgnoreCase),
                             new Regex(@"(\*\/|;)"),
                             new Regex(";"),
                             new Regex(";")
            };
            _startPatterns.AddRange(_str);
            _endPatterns.AddRange(_end);

        }

        public void FormatCode()
        {
            _raw = File.ReadAllLines(_fn).ToList();



            for (int i = 0; i < _raw.Count; i++)
            {
                if (string.IsNullOrEmpty(_raw[i]))
                    continue;

                var type = CodeType.Other;
                var lines = new List<CodeLine>();

                bool isMatched = false;
                foreach (var (rgx, j) in _startPatterns.Select((rgx, j) => (rgx, j)))
                {
                    if (rgx.IsMatch(_raw[i].Trim()))
                    {
                        isMatched = true;
                        type = (CodeType)j;
                        bool isEndOfCodeBlock = false;
                        int firstBlockLine = i;
                        lines.Add(new CodeLine(_raw[i], i));

                        do
                        {
                            if (i >= _raw.Count)
                            {
                                break;
                            }

                            isEndOfCodeBlock = _endPatterns[j].IsMatch(_raw[i].Trim());

                            if (!isEndOfCodeBlock)
                            {
                                i++;
                                lines.Add(new CodeLine(_raw[i], i));
                            }

                        } while (!isEndOfCodeBlock);
                    }
                }
                if (!isMatched)
                {
                    var block = new CodeBlock(CodeType.Other, i);
                    block.Add(new CodeLine(_raw[i], i));
                    _blocks.Add(block);
                }
                else
                {
                    _blocks.Add(new CodeBlock(type, lines, i));
                }

            }
            foreach (var cb in _blocks)
            {
                switch (cb.Type)
                {
                    case CodeType.Data:
                        cb.Lines = FmtDataStep(cb);
                        break;
                    case CodeType.Proc:
                        cb.Lines = FmtProc(cb);
                        break;
                    case CodeType.Sql:
                        cb.Lines = FmtSql(cb);
                        break;
                    case CodeType.Options:
                        cb.Lines = FmtOptions(cb);
                        break;
                    case CodeType.Libname:
                        cb.Lines = FmtLibname(cb);
                        break;
                    case CodeType.Comment:
                        cb.Lines = FmtComment(cb);
                        break;
                    default:
                        cb.Lines = FmtMisc(cb);
                        break;
                }
            }
            WriteFormattedCode();
        }



        private List<CodeLine> FmtProc(CodeBlock cb)
        {
            var code = new List<CodeLine>();
            foreach (var (line, i) in cb.Lines.Select((line, i) => (line, i)))
            {
                var indent = line == cb.Lines.FirstOrDefault() || line == cb.Lines.LastOrDefault() ? 0 : 1;
                code.Add(new CodeLine(line.Text, i, indent));
            }

            return code;
        }

        private List<CodeLine> FmtOptions(CodeBlock cb)
        {
            var code = new List<CodeLine>();
            var lines = SplitOptionLines(cb.Lines);
            foreach (var (line, i) in lines.Select((line, i) => (line, i)))
            {
                code.Add(new CodeLine(line.Text, i, i == 0 ? 0 : 4));
            }

            return code;
        }

        private List<CodeLine> SplitOptionLines(List<CodeLine> lines)
        {
            var code = new List<CodeLine>();
            foreach (var (line, i) in lines.Select((line, i) => (line, i)))
            {
                var newLine = string.Empty;
                if (line.Text.Length > 150)
                {
                    var words = line.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (var word in words)
                    {
                        newLine += $"{word} ";
                        if (newLine.Length > 100 || word == words.LastOrDefault())
                        {
                            code.Add(new CodeLine(newLine, i));
                            newLine = String.Empty;
                        }
                    }
                }
                else
                {
                    code.Add(new CodeLine(line.Text.Trim(), i));
                }
            }
            return code;
        }

        private List<CodeLine> FmtLibname(CodeBlock cb)
        {
            var lines = SeparateLibLines(cb.Lines);
            return lines;
        }
        private List<CodeLine> FmtMisc(CodeBlock cb)
        {
            var code = new List<CodeLine>();
            foreach (var (line, i) in cb.Lines.Select((line, i) => (line, i)))
            {
                if (_localDrive.IsMatch(line.Text))
                {
                    code.AddRange(HandleLocalPaths(line));
                }
                else
                {
                    code.Add(line);
                }
            }

            return code;
        }

        private List<CodeLine> HandleLocalPaths(CodeLine line)
        {
            var codes = new List<CodeLine>();
            var dl = "%sysfunc(pathname(work))";

            try
            {
                var stms = line.Text.Split(new char[] { '"' });

                var folders = stms[1].Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (Path.GetExtension(stms[1]) == String.Empty)
                {
                    if (folders.LastOrDefault() != "Prc2")
                    {
                        var newFolders = folders.Skip(folders.IndexOf("Prc2")+1);
                        codes.Add(new CodeLine($"%sysexec mkdir \"{dl}\\{string.Join('\\', newFolders)}\";",line.LineNo-1,line.Indent));
                        codes.Add(new CodeLine($"{stms[0]} \"{dl}\\{string.Join('\\', newFolders)}\";", line.LineNo, line.Indent));
                    }
                    else
                    {
                        codes.Add(new CodeLine($"{stms[0]} \"{dl}\";", line.LineNo, line.Indent));
                    }
                }
                else
                {
                    codes.Add(new CodeLine($"{stms[0]} \"{dl}\\{Path.GetFileName(stms[1])}\";", line.LineNo, line.Indent));
                }
            }
            catch
            {
                // not in a recognizable format, so just carry on
                codes.Add(line);
            }

            return codes;
        }

        private List<CodeLine> SeparateLibLines(List<CodeLine> liness)
        {           
            var code = new List<CodeLine>();
            foreach (var (line, i) in liness.Select((line, i) => (line, i)))
            {
                if (line.Text.Contains("("))
                {
                    var stm = line.Text.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    if (stm.Count() > 1)
                    {
                        int nLibs = 0;
                        code.Add(new CodeLine($"{stm[0]}(", i, 0));
                        var libs = stm[1].Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                        foreach (var (v, j) in libs.Select((v, j) => (v, j)))
                        {
                            code.Add(new CodeLine(v, j, 4));
                            nLibs++;
                        }
                        if (stm.Count() > 2)
                        {
                            nLibs++;
                            for (int j = 2; j < stm.Length; j++)
                            {
                                var opts = j == 2 ? $"){stm[j]}" : $"{stm[j]}";
                                code.Add(new CodeLine(opts, nLibs, 4));
                            }
                        }
                    }
                }
                else if (_localDrive.IsMatch(line.Text))
                {
                    code.AddRange(HandleLocalPaths(line));
                }
                else
                {
                    code.Add(new CodeLine(line.Text, i, 0));
                }
            }
            return code;
        }

        private List<CodeLine> FmtComment(CodeBlock cb)
        {
            var code = new List<CodeLine>();
            foreach (var (line, i) in cb.Lines.Select((line, i) => (line, i)))
            {
                var newLine = line.Text.Trim().Replace("-* ", "-*\n")
                                              .Replace(" *-", "\n*-");
                code.Add(new CodeLine(newLine, i));
            }
            return code;
        }

        private List<CodeLine> FmtDataStep(CodeBlock cb)
        {
            var lines = SeparateLines(cb.Lines);

            Regex strIndent = new Regex(@"(^if.*then\s+do.*;|^do.*;|^else\s+do\s*;)", RegexOptions.IgnoreCase);
            Regex endIndent = new Regex(@"(end\s*;)", RegexOptions.IgnoreCase);

            var formatted = new List<CodeLine>();
            int indent = 0;
            foreach (var (line, i) in lines.Select((line, i) => (line, i)))
            {
                if (line == lines.LastOrDefault())
                {
                    if (indent > 0)
                    {
                        indent--;
                    }
                }

                if (endIndent.IsMatch(line.Text.Trim()) && indent > 0)
                {
                    if (indent > 0)
                    {
                        indent--;
                    }
                }

                formatted.Add(new CodeLine(line.Text, i, indent));

                if (strIndent.IsMatch(line.Text.Trim()) || i == 0)
                    indent++;
            }

            return formatted;
        }
        private List<CodeLine> FmtSql(CodeBlock cb)
        {

            var lines = SeparateSqlLines(cb.Lines);

            Regex strIndent = new Regex(@"^(create|select|from|where|having|order)", RegexOptions.IgnoreCase);
            Regex endIndent = new Regex(@"(quit)", RegexOptions.IgnoreCase);

            var formatted = new List<CodeLine>();
            int indent = 0;

            foreach (var (line, i) in lines.Select((line, i) => (line, i)))
            {
                if (endIndent.IsMatch(line.Text.Trim()) || i == 0)
                {
                    indent = 0;
                }
                else
                {
                    if (strIndent.IsMatch(line.Text.Trim()))
                        indent = 1;
                    else
                        indent = 4;
                }

                formatted.Add(new CodeLine(line.Text, i, indent));
            }

            return formatted;
        }

        private List<CodeLine> SeparateSqlLines(List<CodeLine> lines)
        {
            Regex kw = new Regex(@"(select|from|where|having|order|quit)", RegexOptions.IgnoreCase);
            var code = new List<CodeLine>();

            foreach (var (line, i) in lines.Select((line, i) => (line, i)))
            {
                if (kw.IsMatch(line.Text.Trim()))
                {
                    var words = Regex.Split(line.Text, @"(?<=[\s;])");
                    string newLine = string.Empty;
                    foreach (var word in words)
                    {
                        if (kw.IsMatch(word))
                        {
                            if (!string.IsNullOrEmpty(newLine))
                                code.Add(new CodeLine(newLine.Trim(), i));

                            newLine = string.Empty;
                        }
                        newLine += $" {word}";
                        if (word == words.LastOrDefault())
                            code.Add(new CodeLine(newLine.Trim(), i));
                    }
                }
                else
                {
                    code.Add(new CodeLine(line.Text.Trim(), i));
                }
            }
            return code;
        }

        private List<CodeLine> SeparateLines(List<CodeLine> lines)
        {
            var code = new List<CodeLine>();
            var joined = string.Join(String.Empty, lines.Select(x => x.Text));

            foreach (var (line, i) in Regex.Split(joined, @"(?<=[;])").Select((line, i) => (line, i)))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    code.Add(new CodeLine(line, i));
            }
            return code;
        }

        private void WriteFormattedCode()
        { 
            // temp hack
            using (var w = new StreamWriter(Path.Combine(_outLoc, Path.GetFileName(_fn))))
            {
                foreach (var block in _blocks.OrderBy(x => x.BlockNum))
                {
                    foreach (var line in block.Lines.OrderBy(x => x.LineNo))
                    {
                        w.WriteLine($"{string.Concat(Enumerable.Repeat(" ", line.Indent * _tabSpaces))}{string.Join(" ", line.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))}");
                    }
                    w.Write("\n");
                }
            }
        }
    }
}
