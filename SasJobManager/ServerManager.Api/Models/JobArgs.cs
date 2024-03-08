namespace ServerManager.Api.Models
{
    public class JobArgs
    {
        public JobArgs()
        {
            RunMsgs = new List<string>();
            IsValid = true;
        }
        public int Id { get; set; }
        public string? Name { get; set; }
        public bool IsRecurring { get; set; }
        public string BatFile { get; set; }
        public bool IsDefaultSecurity { get; set; }
        public string? SecurityGroup { get; set; }
        public string? SchedulerMsg { get; set; }
        public string User { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public List<string> RunMsgs { get; set; }
        public bool IsValid { get; set; }
    }
}
