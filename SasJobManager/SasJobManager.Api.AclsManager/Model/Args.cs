namespace SasJobManager.Api.AclsManager.Model
{
    public class Args
    {
        public Args(string fileName,bool isLock)
        {
            FileName = fileName;
            IsLock = isLock;
        }
        public string FileName { get; set; }
        public bool IsLock { get; set; }
        public string? User { get; set; }
    }
}
