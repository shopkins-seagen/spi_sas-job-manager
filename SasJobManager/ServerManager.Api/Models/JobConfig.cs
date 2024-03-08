namespace ServerManager.Api.Models
{
    public class JobConfig
    {
        public JobConfig()
        {
            Options = new Dictionary<string, string>();
        }
        public int Job { get; set; }
        public Dictionary<string, string> Options { get; set; }
    }
}
