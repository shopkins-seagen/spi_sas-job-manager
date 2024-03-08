using Microsoft.AspNetCore.Mvc;
using ServerManager.Api.Service;
using SasJobManager.ServerLib.Models;
using System.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IsLockedController : ControllerBase
    {
        private string _domain;
        public IsLockedController(IConfiguration cfg)
        {
            FileSystemManager.MapDrive(cfg);
            _domain = cfg["domain"];

        }
        // GET: api/<IsLockedController>
        [HttpGet]
        public Tuple<bool,string> Get(string folder)
        {
            var ad = new ActiveDirectory(_domain);
            var isLocked = ad.IsFolderLocked(folder);
            var resp = new Tuple<bool,string>(isLocked,ad.Logger.Count()==0?string.Empty:ad.Logger.FirstOrDefault().Msg);
            return resp;
        }
    }
}
