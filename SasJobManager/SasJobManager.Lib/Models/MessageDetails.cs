using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class MessageDetails
    {
        private StringBuilder _html { get; set; }
        private Args _args;
        public MessageDetails(Args args)
        {
            Recipients = new List<string>();
            _html = new StringBuilder();
            _args = args;
            SetDefaultStyle();
        }

        public string? Program { get; set; }
        public string Sender { get; set; }
        
        public String? Content { get; set; }
        public string? SummaryFn { get; set; }
        public SasFindingType Status { get; set; }
        public List<string> Recipients { get; set; }

        public void AddContent(string content)
        {
            _html.AppendLine(content);
     
        }
        public void AddLogSummary(List<SasLog> logs,DateTime started,DateTime finished)
        {
            AddContent("<h3>SAS Job Manager Log Summary Notification</h3>");
            AddContent("<table class=\"logTable\" style=\"width:50%;\">");
            AddContent($"<tr><th>Worst Finding</th><td><i style=\"font-style:normal;\">{Status}</i></td></tr>");
            AddContent($"<tr><th>Started</th><td><i style=\"font-style:normal;\">{started:dd-MMM-yyyy HH:mm:ss}</i></td></tr>");
            AddContent($"<tr><th>Finished</th><td><i style=\"font-style:normal;\">{finished:dd-MMM-yyyy HH:mm:ss}</i></td></tr>");
            AddContent($"</table><hr />");
            AddContent($"<span>See the <a href=\"{SummaryFn}\">summary file</a> for details</span><hr />");            

            if (logs.Count > 0)
            {
                AddContent("<table class=\"logTable\" style=\"border-collapse;collapsed;\">");
                AddContent("<thead><tr><th>Program</th><th>Link to Log</th><th>Worst Finding</><th>First Finding</th></tr></thead><tbody>");
                foreach (SasLog log in logs.OrderBy(x=>x.Program.PgmFn))
                {
                    AddContent($"<tr><td>{log.Program.NameWithoutExtension()}.sas</td>");
                    AddContent($"<td><a target=\"_blank\" href=\"{log.Program.LogFn}\">{Path.GetFileName(log.Program.LogFn)}</a></td>");
                    var worst = log.WorstFinding();
                    AddContent($"<td>{worst}</td>");
                    var finding = log.Findings.OrderBy(x => x.LineNumber).FirstOrDefault();
                    if (finding != null)
                    {
                        AddContent($"<td>(#{finding.LineNumber}:{finding.Type}) {finding.Text}</td>");
                    }
                    else
                    {
                        AddContent($"<td>No Findings</td>");
                    }
                    AddContent("</tr>");
                }
                AddContent("</tbody></table>");
                AddContent($"<br /><span style=\"color:slategray;\">Run on {_args.Server} ({_args.Context}) in {(_args.IsAsync ? "parallel" : "sequential")} mode</span>");
            }
            Content += _html.ToString();
        }


        public void SetDefaultStyle()
        {
            AddContent("<style>table.blueTable {   font-family: Arial, Helvetica, sans-serif;   border: 1px solid #1C6EA4;   background-color: #EEEEEE;   width: 100%;   text-align: left;   border-collapse: collapse; } table.blueTable td, table.blueTable th {   border: 1px solid #AAAAAA;   padding: 3px 2px; } table.blueTable tbody td {   font-size: 11px; } table.blueTable tr:nth-child(even) {   background: #D0E4F5; } table.blueTable thead {   background: #1C6EA4;   background: -moz-linear-gradient(top, #5592bb 0%, #327cad 66%, #1C6EA4 100%);   background: -webkit-linear-gradient(top, #5592bb 0%, #327cad 66%, #1C6EA4 100%);   background: linear-gradient(to bottom, #5592bb 0%, #327cad 66%, #1C6EA4 100%);   border-bottom: 2px solid #444444; } table.blueTable thead th {   font-size: 12px;   font-weight: bold;   color: #FFFFFF;   border-left: 2px solid #D0E4F5; } table.blueTable thead th:first-child {   border-left: none; }  table.blueTable tfoot td {   font-size: 14px; } table.blueTable tfoot .links {   text-align: right; } table.blueTable tfoot .links a{   display: inline-block;   background: #1C6EA4;   color: #FFFFFF;   padding: 2px 8px;   border-radius: 5px; }");
            AddContent("table.logTable {table.logTable tr:nth-child(even) {   background: #87D3FF; }   width: 100%;   background-color: #FFFFFF;   border-collapse: collapse;   border-width: 1px;   border-color: #7ea8f8;   border-style: solid;   color: #000000; }  table.logTable td, table.logTable th {   border-width: 1px;   border-color: #7ea8f8;   border-style: solid;   padding: 5px; }  table.logTable thead {   background-color: #7ea8f8; }");
            AddContent("table.paleBlueRows {   font-family: \"Times New Roman\", Times, serif;   border: 1px solid #FFFFFF;   width: 350px;   height: 200px;   text-align: center;   border-collapse: collapse; } table.paleBlueRows td, table.paleBlueRows th {   border: 1px solid #FFFFFF;   padding: 3px 2px; } table.paleBlueRows tbody td {   font-size: 13px; } table.paleBlueRows tr:nth-child(even) {   background: #D0E4F5; } table.paleBlueRows thead {   background: #0B6FA4;   border-bottom: 5px solid #FFFFFF; } table.paleBlueRows thead th {   font-size: 17px;   font-weight: bold;   color: #FFFFFF;   text-align: center;   border-left: 2px solid #FFFFFF; } table.paleBlueRows thead th:first-child {   border-left: none; }  table.paleBlueRows tfoot {   font-size: 14px;   font-weight: bold;   color: #333333;   background: #D0E4F5;   border-top: 3px solid #444444; } table.paleBlueRows tfoot td {   font-size: 14px; }");
            AddContent("hr {color:blue;");
            AddContent("</style>");
            Content += _html.ToString();
        }
    }
}
