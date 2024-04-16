using Microsoft.EntityFrameworkCore;
using SasJobManager.Api.AclsManager.Model;

namespace SasJobManager.Api.AclsManager.Domain
{
    public class SasLogContext:DbContext
    {
        public SasLogContext(DbContextOptions options) : base(options) 
        {
                
        }
        public DbSet<SasLog> SasLogs { get; set; }
        public DbSet<SasLogHistory> SasLogsHistory { get; set;}
    }
}
