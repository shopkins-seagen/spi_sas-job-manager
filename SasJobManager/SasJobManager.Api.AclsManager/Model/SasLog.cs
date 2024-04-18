

namespace SasJobManager.Api.AclsManager.Model
{
    public class SasLog
    {

        public int Id { get; set; } 
        public string LogFile { get; set; }
        public virtual ICollection<SasLogHistory> History { get; set; } = new List<SasLogHistory>();
    }
 
}
