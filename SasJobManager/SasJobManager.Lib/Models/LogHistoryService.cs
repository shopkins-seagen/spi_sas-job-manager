using RestSharp;
using SasJobManager.ServerLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SasJobManager.Lib.Models
{
    public class LogHistoryService
    {
        private string _base;
        private string _url;
        public LogHistoryService(string baseUrl, string url)
        {
            _base = baseUrl;
            _url = url;
        }

        public async Task<LogEntry> Unlock(string logFile)
        {
            var args = new Dictionary<string, object>();
            args["filename"] = logFile;
            args["islock"] = false;

            var response = new LogEntry();
            try
            {
                var client = new RestClient(_base);
                var request = new RestRequest(_url, Method.Post);
                request.AddJsonBody(args);
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

        public async Task<LogEntry> Lock(string logFile,string user)
        {
            var args = new Dictionary<string, object>();
            args["filename"] = logFile;
            args["islock"] = true;
            args["user"] = Environment.UserName;

            var response = new LogEntry();
            try
            {
                var client = new RestClient(_base);
                var request = new RestRequest(_url, Method.Post);
                request.AddJsonBody(args);
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
