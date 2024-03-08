using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SasJobManager.Domain.Models;

namespace SasJobManager.Domain
{
    public class SasContext : DbContext
    {
        private IConfigurationRoot _cfg;
        public SasContext()
        {
            SetDb();
        }
        public SasContext(DbContextOptions<SasContext> options) : base(options)
        {

            SetDb();
        }
        public DbSet<SasProgram> Programs { get; set; }
        public DbSet<Macro> Macros { get; set; }
        public DbSet<Finding> Findings { get; set; }
        public DbSet<Job> Jobs { get; set; }
        public DbSet<JobRun> JobRuns { get; set; }
        public DbSet<JobRunMsg> JobRunMsgs { get; set; }
        public DbSet<SchedulerRun> SchedulerRun { get; set; }      
        public DbSet<UrlMap> UrlMap { get; set; }

        private void SetDb()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_domain.json", true, true);
            _cfg = builder.Build();

        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var conn = _cfg.GetConnectionString("db");
            optionsBuilder.UseSqlServer(conn);
        }
    }
}
