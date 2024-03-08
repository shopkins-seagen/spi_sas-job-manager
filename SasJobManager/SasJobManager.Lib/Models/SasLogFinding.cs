namespace SasJobManager.Lib.Models
{
    public class SasLogFinding
    {
        public SasLogFinding(SasFindingType type,string text,int lineNo)
        {
            Type= type;
            Text= text;
            LineNumber= lineNo;
        }
        public SasFindingType Type { get; set; }
        public string Text { get; set; }
        public int LineNumber { get; set; }
    }
}