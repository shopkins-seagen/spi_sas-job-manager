using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SasJobManager.ServerLib.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using ClosedXML.Excel;

namespace SasJobManager.Lib.Models
{
    public class RunSummary
    {
        private IConfigurationRoot _cfg;
        private Args _args;
        private string _css;
        private string _js;
        private StringBuilder html;
        private StringBuilder _msg;
        private DateTime _started;
        private DateTime _finished;

        public RunSummary(IConfigurationRoot cfg, Args args)
        {
            _cfg = cfg;
            SetJsCss();

            html = new StringBuilder();
            _msg = new StringBuilder();
            _args = args;
        }

        public void WriteSummary(List<SasLog> logs, List<LogEntry> runLog, DateTime started, DateTime finished, string? message)
        {
            var isAnyQc = logs.Any(x => x.IsQcPgm);
            // Open the HTML document
            html.AppendLine("<!doctype html><html><head><meta charset=\"UTF-8\"/>");
            html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, maximum-scale=2.0, user-scalable=yes\"/>");
            html.AppendLine("<title>SAS Log Summary</title>");
            html.AppendLine("<link rel=\"stylesheet\" href=\"https://stackpath.bootstrapcdn.com/bootstrap/4.2.1/css/bootstrap.min.css\" integrity=\"sha384-GJzZqFGwb1QTTN6wy59ffF1BuGJpLSa9DkKMp0DgiMDm4iYMj70gZWKYbI706tWS\" crossorigin=\"anonymous\">");
            html.AppendLine($"<style>{_css}</style>");
            html.AppendLine($"<style></style>");

            var qcClass = isAnyQc ? "visibility:visible" : "visibility:collapse";
            var spanCols = isAnyQc ? 3 : 2;
            spanCols = _args.DoesDisplayProgrammers ? spanCols + 2 : spanCols;

            html.AppendLine($"<style>.isQc {{{qcClass};}}</style>");

            html.AppendLine("</head><body><div class='wrapper'>");
            html.AppendLine("<div class='container'><div style='margin-top:10px;margin-bottom:10px;'>");

            // Runtime summary
            html.AppendLine("<div class='alert alert-secondary' style='margin-top:10px;margin-bottom:10px;'>");
            html.AppendLine("<table style='table-layout:fixed;'>");
            html.AppendLine("<div style='float:right;'><button class='btn btn-primary'  id='exit' role='button'>Close & Discard</button></div>");
            html.AppendLine($"<tr><th>Run by:</th><td>{Environment.UserName}</td></tr>");
            html.AppendLine($"<tr><th>Server:</th><td>{(_args.UseBestServer ? "Best" : _args.Server)} ({_args.Context})</td></tr>");
            html.AppendLine($"<tr><th>Started:</th><td>{started.ToString("ddMMMyyyy HH:mm")}</td></tr>");
            html.AppendLine($"<tr><th>Finished:</th><td>{finished.ToString("ddMMMyyyy HH:mm")} ({Elapsed(started, finished)})</td></tr>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            // wrap summaries
            html.AppendLine("<div class='wrapper'>");

            // SAS Log finding summary
            html.AppendLine($"<div style='float:left;border: 1px solid lightslategray;padding:5px;border-radius: 5px;'>");
            html.AppendLine("<p style='font-weight:bold;color:midnightblue;'>Counts of Programs by Worst Finding</p>");
            html.AppendLine("<table>");
            html.AppendLine("<tr>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-secondary filterButton'>All</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-danger filterButton'>Error</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-warning filterButton'>Warning</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-info filterButton'>Note</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-primary filterButton'>Notice</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-success filterButton'>Clean</button>");
            html.AppendLine("</td>");
            html.AppendLine("</tr>");
            html.AppendLine("<tr>");
            html.AppendLine($"<td class='tdCentered'>{logs.Count()}</td>");
            html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.WorstFinding() == SasFindingType.Error).Count()}</td>");
            html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.WorstFinding() == SasFindingType.Warning).Count()}</td>");
            html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.WorstFinding() == SasFindingType.Note).Count()}</td>");
            html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.WorstFinding() == SasFindingType.Notice).Count()}</td>");
            html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.WorstFinding() == SasFindingType.Clean).Count()}</td>");
            html.AppendLine($"</tr>");
            html.AppendLine($"</table>");
            html.AppendLine($"</div>");

            // Automated QC findings
            var totalTests = logs.Select(x => x.FailCount + x.PassCount).Sum();
            var passes = logs.Sum(x => x.PassCount);
            html.AppendLine("<div class='isQc' style='float:right;border: 1px solid lightslategray;padding:5px;border-radius: 5px;'>");
            html.AppendLine($"<p style='font-weight:bold;color:midnightblue;'>Automated QC Tests: ({passes} passed/{totalTests} total) </p>");
            html.AppendLine("<div>");
            html.AppendLine("<p style=\"text-align: center;color:midnightblue;font-variant: small-caps;\">Summary by Program</p>");
            html.AppendLine("<table>");
            html.AppendLine("<tr>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-secondary filterQcButton'>All</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-success filterQcButton'>Passed</button>");
            html.AppendLine("</td>");
            html.AppendLine("<td>");
            html.AppendLine("<button class='btn btn-sm btn-outline-danger filterQcButton'>Failed</button>");
            html.AppendLine("</td>");
            html.AppendLine("</tr>");
            html.AppendLine("<tr>");
            if (isAnyQc)
            {
                var total = logs.Where(x => x.IsQcPgm).Count();
                html.AppendLine($"<td class='tdCentered'>{total}</td>");
                html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.IsQcPgm && x.IsPass()).Count()}</td>");
                html.AppendLine($"<td class='tdCentered'>{logs.Where(x => x.IsQcPgm && !x.IsPass()).Count()}</td>");
            }
            else
            {
                html.AppendLine($"<td class='tdCentered'></td>");
                html.AppendLine($"<td class='tdCentered'></td>");
                html.AppendLine($"<td class='tdCentered'></td>");
            }
            html.AppendLine($"</tr>");
            html.AppendLine($"</table>");
            html.AppendLine($"</div>");
            html.AppendLine($"</div>");

            html.AppendLine("</div>");
            html.AppendLine("<div style='clear: both;'></div>");
            html.AppendLine("</div>");


            if (runLog != null)
            {
                if (runLog.Any(x => x.IssueType > IssueType.Info))
                {
                    // Runlog toggle button

                    html.AppendLine($"<button class=\"collapsible\">{runLog.Where(x => x.IssueType > IssueType.Info).Count()} Runtime Issues Encountered</button>");
                    html.AppendLine("<div class=\"content\">");
                    html.AppendLine("<ul style='list-style-type: circle;font-size:smaller;'>");

                    foreach (var v in runLog.Where(x => x.IssueType > IssueType.Info))
                    {
                        html.AppendLine($"<li>[{v.IssueType}] ({v.Source}): {v.Msg}</li>");
                    }
                    html.AppendLine("</ul></div>");
                }
            }

            // Write sas log findings summary table
            html.AppendLine("<div class='container' style='margin-top:20px;'>");
            html.AppendLine("<div style='text-align:center;border-top: 1px solid lightgray;padding:5px;'>");
            html.AppendLine("<button style='float:left;' id='btn' class='btn btn-sm btn-dark expandChildTable'></button>");
            html.AppendLine("<h5 style='color:midnightblue'>Summary of SAS logs</h5>");
            html.AppendLine("</div>");
            html.AppendLine("<table class='table table-bordered table-sm' style='table-layout:fixed;' id='t1'>");
            html.AppendLine("<thead>");

            // Parent table Headers
            // Status heading
            html.AppendLine("<th style='width:25%;background-color:lightgray;'>");
            html.AppendLine("<div style='text-align:center;display: flex;'>");
            html.AppendLine("<div class='input-group-prepend'>");
            html.AppendLine("<label class='input-group-text' style='font-weight: bold;' for='selIssueLevel'>Status</label>");
            html.AppendLine("</div>");
            html.AppendLine("<select class='custom-select' id='selIssueLevel' onchange='filterTable()' style='width:100px;margin:0px;'>");
            html.AppendLine("<option class='btn btn-sm btn-secondary'>All</option>");
            html.AppendLine("<option class='btn btn-sm btn-danger'>Error</option>");
            html.AppendLine("<option class='btn btn-sm btn-warning'>Warning</option>");
            html.AppendLine("<option class='btn btn-sm btn-info'>Note</option>");
            html.AppendLine("<option class='btn btn-sm btn-primary'>Notice</option>");
            html.AppendLine("<option class='btn btn-sm btn-success'>Clean</option>");
            html.AppendLine("</select>");
            html.AppendLine("</div>");
            html.AppendLine("</th>");

            // Log
            html.AppendLine("<th style='width:30%;background-color: lightgray;'>SAS Log <span style='float:right'>(elapsed time)</span></th>");

            // Optionally display developer and tester
            if (_args.DoesDisplayProgrammers)
            {
                html.AppendLine("<th style='width:10%;background-color: lightgray;'>Developer</th>");
                html.AppendLine("<th style='width:10%;background-color: lightgray;'>Tester</th>");
            }

            // Qc
            if (isAnyQc)
            {
                html.AppendLine("<th style='background-color: lightgray;' class='isQc'>");
                html.AppendLine("<div style='text-align:center;display: flex;'>");
                html.AppendLine("<div class='input-group-prepend'>");
                html.AppendLine("<label class='input-group-text' style='font-weight: bold;' for='selQCLevel'>Auto QC</label>");
                html.AppendLine("</div>");
                html.AppendLine("<select class='custom-select' id='selQCLevel' onchange='filterTableQc()' style='width:100px;margin:0px;'>");
                html.AppendLine("<option class='btn btn-sm btn-secondary'>All</option>");
                html.AppendLine("<option class='btn btn-sm btn-success'>Passed</option>");
                html.AppendLine("<option class='btn btn-sm btn-danger'>Failed</option>");
                html.AppendLine("</select>");
                html.AppendLine("</div>");
                html.AppendLine("</th>");
            }

            html.AppendLine("</tr></thead>");


            var paths = logs.DistinctBy(x => x.Program.Dir()).Select(x => x.Program.Dir()).OrderBy(x=>x).ToList();
            int detailTableCount = 0;

            foreach (var p in paths)
            {
                // Location line
                html.AppendLine($"<tr><td colspan='{spanCols}'>" +
                    $"<span style='font-weight:bold;color:midnightblue;'>{p}</span>" +
                    $"</td></tr>");

                foreach (var v in logs.Where(x => x.Program.Dir() == p).OrderBy(x=>x.Program.Name()))
                {
                    detailTableCount++;
                    // Add the classes used for presentation and filtering
                    var status = v.WorstFinding();
                    var parentClass = $"{status.ToString().ToLower()}";
                    if (v.IsQcPgm)
                    {
                        parentClass += $" {(v.IsPass() ? "passed" : "failed")}";
                    }

                    // Parent row
                    html.AppendLine($"<tr class='parentRow {parentClass}'>");

                    // Status
                    html.AppendLine($"<td><span class='btn btn-sm expandDetails tableBtn btn-{GetBootstrapClass(status, true)}'><span style='margin-left: 5px;'>{status.ToString()}</span></span></td>");

                    // Log
                    html.AppendLine($"<td><span class='btn btn-sm' style='color:white;border:1px solid #337ab7;'><a target=\"_blank\" href=\"{v.Program.LogFn}\">{Path.GetFileName(v.Program.LogFn)}</a></span>");

                    // add link to LST file if one exists

                    var lst = $@"{Path.GetDirectoryName(v.Program.LogFn)}\{Path.GetFileNameWithoutExtension(v.Program.PgmFn)}.lst";
                    if (File.Exists(lst))
                    {
                        html.AppendLine($"<span class='btn btn-sm' style='color:white;border:1px solid #337ab7;'><a target=\"_blank\" href=\"{lst}\">{Path.GetFileName(lst)}</a></span>");
                    }



                    if (v.IsQcPgm)
                    {
                        html.AppendLine("<sup class='sup'>QC</sup>");
                    }
                    html.AppendLine($"<i style='float: right;'>({Elapsed(v.Program.Started, v.Program.Completed)})</i>");

                    if (_args.DoesDisplayProgrammers)
                    {
                        html.AppendLine($"<td>{v.Program.Developer}</td>");
                        html.AppendLine($"<td>{v.Program.Tester}</td>");
                    }

                    // QC
                    if (isAnyQc)
                    {
                        if (v.IsQcPgm)
                        {
                            html.AppendLine($"<td><a  class='{(v.IsPass() ? "passQc" : "failQc")}' href=\"{(Path.Combine(v.Program.Dir(), $"{v.Program.NameWithoutExtension()}.lst"))}\"" +
                                $"target='_blank'>({v.PassCount}/{v.PassCount + v.FailCount}) Tests passed</a></td>");
                        }
                        else
                        {
                            html.AppendLine("<td></td>");
                        }
                    }
                    html.AppendLine("</tr>");

                    // Child rows
                    html.AppendLine($"<tr class='childTableRow {parentClass}'><td colspan='{spanCols}'><h5>Findings</h5>");
                    html.AppendLine($"<table id='Findings{detailTableCount}' style='table-layout:fixed;width:100%;' class='table'><thead><tr>");
                    html.AppendLine("<th style='width:20%'><span>Type (Line#)</span>");
                    html.AppendLine($"<select  id='selDetailLevel{detailTableCount}' onchange=\"filterDetails({detailTableCount})\" style='width:75px;margin:0px;text-align: left;font-size: smaller;'>");

                    html.AppendLine("<option class='btn btn-sm btn-secondary'>All</option>");
                    html.AppendLine("<option class='btn btn-sm btn-danger'>Error</option>");
                    html.AppendLine("<option class='btn btn-sm btn-warning'>Warning</option>");
                    html.AppendLine("<option class='btn btn-sm btn-info'>Note</option>");
                    html.AppendLine("<option class='btn btn-sm btn-primary'>Notice</option>");
                    html.AppendLine("</select></th>");
                    html.AppendLine("<th style='width:80%;overflow-wrap: normal;'>Message</th></tr></thead><tbody>");

                    if (v.Findings.Count > 0)
                    {
                        foreach (var f in v.Findings.OrderBy(x => x.LineNumber))
                        {
                            html.AppendLine($"<tr class='detailRow {f.Type.ToString().ToLower()}'>");
                            html.AppendLine($"<td class='btn-sm btn-{GetBootstrapClass(f.Type)}'>{f.Type} ({f.LineNumber})</td>");
                            html.AppendLine($"<td class='btn-sm btn-{GetBootstrapClass(f.Type)}'>{f.Text}</td></tr>");
                        }
                    }
                    else
                    {
                        html.AppendLine($"<tr><td>Clean</td><td>No log findings</td></tr>");
                    }
                    html.AppendLine("</tbody></table></td></tr>");

                }
            }
            html.AppendLine("</table></div></div>");

            // Add javascript libraries
            html.AppendLine("<script src=\"http://ajax.googleapis.com/ajax/libs/jquery/1.9.1/jquery.min.js\" type=\"text/javascript\"></script>");
            html.AppendLine("<script src='https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.14.6/umd/popper.min.js' integrity='sha384-wHAiFfRlMFy6i5SRaxvfOCifBUQy1xHdJ/yoi7FRNXMRBu5WHdZYu1hA6ZOblgut' crossorigin='anonymous'></script>");
            html.AppendLine("<script src='https://stackpath.bootstrapcdn.com/bootstrap/4.2.1/js/bootstrap.min.js' integrity='sha384-B0UglyR+jN6CkvvICOB2joaf5I4l3gm9GU6Hc1og6Ls7i6U/mkkaduKaBhlAXv9k' crossorigin='anonymous'></script>");
            html.AppendLine($"<script>{_js}</script>");
            html.AppendLine($"<script>{BuildDynamicJs()}</script>");

            html.AppendLine("</body>");

            if (message != null)
            {
                html.AppendLine($"<footer style='text-align:left;font-size:smaller;margin-left:20px;'><span>{message}</span></footer>");
            }
            html.AppendLine($"<footer  style = 'text-align:center;font-size:smaller;'><span>SASJobManager v{_cfg["app_ver"]}</span></footer>");
            html.AppendLine("</html>");
            Write();
            if (_args.DoesExportLogSummary)
            {
                ExportLogSummary(logs, isAnyQc, _args.DoesDisplayProgrammers);
            }

        }

        private string BuildDynamicJs()
        {
            var fn = _args.SummaryFn.Replace(@"\", @"\\");
            var sb = new StringBuilder();
            sb.AppendLine("$(document).ready(function(){");
            //   sb.AppendLine("$(window).on('beforeunload', function(e) {");
            sb.AppendLine("$('#exit').click(async function(e) {");
            sb.AppendLine("e.stopPropagation();");
            sb.AppendLine("e.preventDefault();");

            sb.AppendLine("if (confirm(\"Are you sure you want to delete the SJM Run Summary?\")==true){");
            sb.AppendLine($"fetch(\"{_cfg["url_delete"]}?fn={fn}\",");
            sb.AppendLine("{method: \"GET\",");
            sb.AppendLine("mode: 'no-cors',");
            sb.AppendLine("headers:{'Accept': 'text/plain','Content-Type': 'text/plain'},}).then(x=>{window.close()});");


            sb.AppendLine("}});});");
            return sb.ToString();
        }

        private string GetBootstrapClass(SasFindingType fType, bool isBody = false)
        {
            var bs = new List<string>() { "danger", "warning", "primary", "info", "success" };
            var ft = new List<SasFindingType>() { SasFindingType.Error, SasFindingType.Warning, SasFindingType.Notice, SasFindingType.Note, SasFindingType.Clean };
            var pos = ft.IndexOf(fType);

            if (pos < 0)
                return "secondary";

            return $"{(isBody ? "outline-" : string.Empty)}{bs[pos]}";
        }
        private string GetBootstrapClassForIssueType(IssueType iType)
        {
            var bs = new List<string>() { "danger", "danger", "warning", "primary", "info" };
            var ft = new List<IssueType>() { IssueType.Fatal, IssueType.Error, IssueType.Warning, IssueType.Note, IssueType.Info };
            var pos = ft.IndexOf(iType);

            if (pos < 0)
                return "secondary";

            return bs[pos];
        }

        private void Write()
        {
            using (var w = new StreamWriter(_args.SummaryFn))
            {
                w.Write(html);
            }
        }


        private void SetJsCss()
        {
            _js = string.Join('\n', _cfg.GetSection($"js").GetChildren().Select(x => x.Value));
            _css = string.Join('\n', _cfg.GetSection($"css").GetChildren().Select(x => x.Value));
        }
        private string UseWindowsPath(string path, string root)
        {
            var folders = path.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries).Skip(2).ToList().Prepend($"file://{root}//");
            return string.Join(" /", folders);
        }

        private string Elapsed(DateTime started, DateTime finsished)
        {
            var interval = Math.Round(System.Math.Abs((started - finsished).TotalSeconds), 1);
            return $"{(interval >= 60 ? Math.Round(interval / 60, 1) : interval)} {(interval >= 60 ? "min" : "sec")}";
        }

        public void ExportLogSummary(List<SasLog> logs, bool isQc, bool isProgrammer)
        {
            try
            {
                var fn = Path.Combine(Path.GetDirectoryName(_args.SummaryFn),
                    $"{(Path.GetFileNameWithoutExtension(_args.SummaryFn))}.xlsx");

                using (var wb = new XLWorkbook())
                {
                    var ws = wb.Worksheets.Add("logs");

                    int rowNum = 1;

                    ws.Cell(rowNum, 1).Value = "Log";
                    ws.Cell(rowNum, 2).Value = "Worst Finding";
                    ws.Cell(rowNum, 3).Value = "Run_Duration";
                    int cell = 3;
                    if (isProgrammer)
                    {
                        ws.Cell(rowNum, cell + 1).Value = "Programmer";
                        ws.Cell(rowNum, cell + 2).Value = "Tester";
                        cell = cell+2;
                    }
                    if (isQc)
                    {
                        ws.Cell(rowNum, cell + 1).Value = "Tests_Passed";
                        ws.Cell(rowNum, cell + 2).Value = "Tests_Failed";
                        cell = cell+2;
                    }
                    ws.Cell(rowNum, cell + 1).Value = "Category";
                    ws.Cell(rowNum, cell + 2).Value = "Log_Line";



                    foreach (var v in logs.OrderBy(x=>x.Program.PgmFn))
                    {
                        rowNum++;
                        ToggleHyperlink(ws.Cell(rowNum, 1), v.Program.LogFn);
                        ws.Cell(rowNum, 2).Value = v.WorstFinding().ToString();
                        ws.Cell(rowNum, 3).Value = Elapsed(v.Program.Started, v.Program.Completed);

                        cell = 3;
                        if (isProgrammer)
                        {
                            ws.Cell(rowNum, cell + 1).Value = v.Program.Developer;
                            ws.Cell(rowNum, cell + 2).Value = v.Program.Tester;
                            cell = cell+2;
                        }
                        if (isQc)
                        {
                            
                            if (v.PassCount + v.PassCount > 0)
                            {
                                ws.Cell(rowNum, cell + 1).Value = v.PassCount;
                                ws.Cell(rowNum, cell + 2).Value = v.FailCount;                  
                            }
                            cell = cell+2;
                        }
 
                        foreach (var f in v.Findings)
                        {
                            if (f == v.Findings.FirstOrDefault())
                            {
                                ws.Cell(rowNum, cell + 1).Value = f.Type.ToString();
                                ws.Cell(rowNum, cell + 2).Value = f.Text;
                            }
                            else
                            {
                                rowNum++;
                                ToggleHyperlink(ws.Cell(rowNum, 1), v.Program.LogFn);
                                ws.Cell(rowNum, 2).Value = v.WorstFinding().ToString();
                                ws.Cell(rowNum, 3).Value = Elapsed(v.Program.Started, v.Program.Completed);

                                cell = 3;
                                if (isProgrammer)
                                {
                                    ws.Cell(rowNum, cell + 1).Value = v.Program.Developer;
                                    ws.Cell(rowNum, cell + 2).Value = v.Program.Tester;
                                    cell = cell+2;
                                }
                                if (isQc)
                                {
                                    if (v.PassCount + v.PassCount > 0)
                                    {
                                        ws.Cell(rowNum, cell + 1).Value = v.PassCount;
                                        ws.Cell(rowNum, cell + 2).Value = v.FailCount;                                        
                                    }
                                    cell = cell+2;
                                }
                                ws.Cell(rowNum, cell + 1).Value = f.Type.ToString();
                                ws.Cell(rowNum, cell + 2).Value = f.Text;  
                            }
                        }
                    }
                    var header = ws.Range($"A1:{GetColumnLetter(cell+2)}1");
                    header.Style.Font.Bold = true;
                    header.Style.Fill.BackgroundColor = XLColor.LightSlateGray;
                    header.SetAutoFilter();

                    ws.Columns().AdjustToContents();

                    wb.SaveAs(fn);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to export the SAS log results to Excel. {ex.Message}");
            }
        }


        private void ToggleHyperlink(IXLCell c, string fn)
        {
            c.Value = Path.GetFileName(fn);
            if (File.Exists(fn))
            {
                var h = new XLHyperlink(fn);
                c.SetHyperlink(h);
                c.Style.Font.FontColor = XLColor.Blue;
            }
        }
        private string GetColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }
    }
}
