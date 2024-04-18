using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SasJobManager.Api.AclsManager.Domain;
using SasJobManager.Api.AclsManager.Model;
using SasJobManager.Api.AclsManager.Services;

namespace SasJobManager.Api.AclsManager.Test
{
    public class UnitTest1
    {
       

        [Fact]
        public void TestLock()
        {
            var cfg = LoadConfig();
            var conn = cfg.GetConnectionString("dev");
            var options = new DbContextOptionsBuilder<SasLogContext>()
                        .UseSqlServer(conn)
                        .Options;

            var args = new Args(@"O:\stat_prog_infra\testing\sjm\secure_log\v01\data\adam\pgms\test.log", true);
            args.User= Environment.UserName;
            var ac = new AclsService(args, cfg,new SasLogContext(options));
            ac.Unlock();
            ac.Lock();
        }
        [Fact]
        public void TestUnLock()
        {
            var cfg = LoadConfig();
            var conn = cfg.GetConnectionString("dev");
            var options = new DbContextOptionsBuilder<SasLogContext>()
                        .UseSqlServer(conn)
                        .Options;
            var args = new Args(@"O:\stat_prog_infra\testing\sjm\secure_log\v01\data\adam\pgms\test.log", true);
            var ac = new AclsService(args, cfg, new SasLogContext(options));
            ac.Unlock();
        }

        private IConfiguration LoadConfig()
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings_test.json", true, true);
        
            return builder.Build();
        }
    }
}