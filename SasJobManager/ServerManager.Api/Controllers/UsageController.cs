using Microsoft.AspNetCore.Mvc;
using SasJobManager.ServerLib.Models;

namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsageController : ControllerBase
    {


        [HttpGet]
        public int Get(string server)
        {
            var service = new Server(server);
            return service.GetCpuUtilization();
        }
    }
}
