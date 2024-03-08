using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ServerManager.Api.Service;
using System.Management.Automation;


namespace ServerManager.Api.Controllers
{
    [EnableCors]
    [Route("api/[controller]")]
    [ApiController]
    public class DeleteController : ControllerBase
    {
        public DeleteController(IConfiguration cfg)
        {
            FileSystemManager.MapDrive(cfg);

        }
        // GET: api/<DeleteController>

        [HttpGet]
        public void Get(string fn)
        {
            try
            {
                System.IO.File.Delete(fn);
                Console.WriteLine($"Deleted '{fn}'");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


    }
}
