using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace SasJobManager.Lib.Models
{
    public class ServerMetrics
    {
        private string _base;
        private string _url;
        public ServerMetrics(string baseUrl, string url)
        {
            _base = baseUrl; 
            _url = url;
        }

        public async Task<int> GetCpuUtilPct(string server)
        {
            try
            {
                var client = new RestClient(_base);
                var request = new RestRequest(_url, Method.Get);
                request.AddParameter("server", server);
                var webresponse = await client.ExecuteAsync(request);
                if (int.TryParse(webresponse.Content, out int result))
                {
                    return result;
                }

            }
            catch (Exception ex)
            {
                return -1;
            }
            return -1;
        }
    }
}
