using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SasJobManager.Lib.Models;
using ServerManager.Api.Models;
using ServerManager.Api.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerManager.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotifyController : ControllerBase
    {
        private IConfiguration _cfg;
        public NotifyController(IConfiguration cfg)
        {
            _cfg = cfg; ;
            FileSystemManager.MapDrive(cfg);

        }


        [HttpPost]
        public async Task<bool> Post(object msg)
        {           
            try
            {
                if (msg != null)
                {
                    MessageDetails messageDetails = JsonConvert.DeserializeObject<MessageDetails>(msg.ToString());
                    var mail = new Notification(_cfg["smtp_server"], messageDetails);
                    await mail.Send();
                    return true;
                }
                else 
                    return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
