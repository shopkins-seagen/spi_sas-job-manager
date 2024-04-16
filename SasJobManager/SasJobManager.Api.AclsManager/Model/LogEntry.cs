namespace SasJobManager.Api.AclsManager.Model
{
    public partial class LogEntry
    {
        public LogEntry(string msg,MsgType msgType)
        {
            Msg= msg;
            MsgType = msgType;
        }
        public string Msg{ get; set; }
        public MsgType MsgType { get; set; }
    }
}
