namespace SasJobManager.Api.AclsManager.Model
{
    public class SasLogHistory
    {
        public int Id { get; set; }
        public string User { get; set; }
        public DateTime LastModified { get; set; }
        public int SasLogId { get; set; }
        public SasLog SasLog { get; set; }
    }
}
