using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SasJobManager.Api.AclsManager.Domain;
using SasJobManager.Api.AclsManager.Model;
using SasJobManager.Api.AclsManager.Services;


namespace SasJobManager.Api.AclsManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogAccessController : ControllerBase
    {
        private IConfiguration _cfg;
        private IHttpContextAccessor _ca;
        private SasLogContext _context;

        public LogAccessController(IConfiguration cfg, IHttpContextAccessor httpContextAccessor, SasLogContext context)
        {
            _cfg = cfg;
            _context = context;
            _ca = httpContextAccessor;

            NetworkService.MapDrive(cfg);

        }
        // GET: api/<LogAccessController>
        [HttpGet]
        public IActionResult Get(string fn)
        {
            return Ok(true);
        }


        // POST api/<LogAccessController>
        [HttpPost]
        public IActionResult Post(object strArgs)
        {
            try
            {
                var args = JsonConvert.DeserializeObject<Args>(strArgs?.ToString());
                if (args != null)
                { 
                    var svc = new AclsService(args, _cfg, _context);
                    if (args.IsLock)
                    {
                        svc.Lock();
                    }
                    else
                    {
                        svc.Unlock();
                    }

                    if (svc.Log.Any(x => (int)x.MsgType > 1))
                    {
                        return BadRequest(svc.Log);
                    }

                    return Ok(svc.Log);
                }
                return BadRequest("Unable to parse arguments");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string? GetCurrentUser()
        {
            try
            {
                var name = _ca.HttpContext?.User.Identity?.Name?.Split('\\')[1];
                return !string.IsNullOrWhiteSpace(name) ? name : default;
            }
            catch
            {
                return default;
            }
        }
    }
}
