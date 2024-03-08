using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class CodeLine
    {
        public CodeLine(string text,int lineNo)
        {
            Text = text;
            LineNo = lineNo;
        }
        public CodeLine(string text, int lineNo,int indent)
        {
            Text = text;
            LineNo = lineNo;
            Indent = indent;
        }
        public string Text { get; set; }
        public int Indent { get; set; }
        public int LineNo { get; set; }

    }
}
