namespace SasJobManager.Lib.Models
{
    public class CodeBlock
    {
        public CodeBlock(CodeType type,int num)
        {
            Lines=new List<CodeLine>();
            Type=type;
            BlockNum = num;
        }
        public CodeBlock(CodeType type,List<CodeLine> lines,int num)
        {
            Lines = lines;
            Type = type;
            BlockNum=num;
        }
        public CodeType Type { get; set; }
        public int BlockNum { get; set; }
        public List<CodeLine> Lines { get; set; }

        public void Add(CodeLine line)
        {
            Lines.Add(line);
        }
    }
}
