using Microsoft.Extensions.Configuration;
using RestSharp;
using SasJobManager.ServerLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class Message
    {
        private string _msgBaseUrl;
        private string _msgUrl;
        private MessageDetails _details;
     

        public Message(MessageDetails details,string msgBaseUrl,string msgUrl)
        {
            _msgBaseUrl = msgBaseUrl;
            _msgUrl = msgUrl;
            _details = details;
   
        }

        public async Task<LogEntry> Send()
        {
            var response = new LogEntry();
            try
            {
                var client = new RestClient(_msgBaseUrl);
                var request = new RestRequest(_msgUrl, Method.Post);
                request.AddJsonBody(_details);
                request.RequestFormat = DataFormat.Json;
                var webresponse = await client.ExecutePostAsync(request);
                response.IssueType = webresponse.StatusCode == System.Net.HttpStatusCode.OK ? IssueType.Info : IssueType.Error;
                if (webresponse.Content != null)
                {
                    response.Msg = webresponse.Content;
                }

            }
            catch (Exception ex)
            {
                response.IssueType = IssueType.Error;
                response.Msg = ex.Message;
            }

            return response;
        }

    }
}
