using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;
using ServerManager.Api.Models;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace SasJobManager.Api.Tests
{
    public class UnitTest1
    {
        private readonly string _loc = @"O:\Projects\Training\sjm\automation\v1\data\adam\pgms";

        /// <TestDescription>Successfully launch a SAS job on the app server from a client application</TestDescription>            
        /// <TestId>UT.API.01.01</TestId> 
        /// <ReqId>API01.01</ReqId>
        /// <Version>2.1</Version>
        [Fact]
        public async Task Post_RunClient_Success()
        {
            var args = new ClientArgs()
            {
                Name = "test",
                User = "shopkins"
            };
            var json = JsonConvert.SerializeObject(args);

            await using var application = new WebApplicationFactory<Program>();

            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            var client = application.CreateClient();

            client.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


            var response = await client.PostAsync("/api/runclient", content);
            var msg = response.Content.ReadAsStringAsync().Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Matches("started at", msg);
        }

        /// <TestDescription>Fail when user not listed in -u</TestDescription>            
        /// <TestId>UT.API.02.01</TestId> 
        /// <ReqId>API02.01</ReqId>
        /// <Version>2.1</Version>
        [Fact]
        public async Task Post_RunClient_FailUser()
        {
            var args = new ClientArgs()
            {
                Name = "test",
                User = "mness"
            };
            var json = JsonConvert.SerializeObject(args);

            await using var application = new WebApplicationFactory<Program>();

            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            var client = application.CreateClient();

            client.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


            var response = await client.PostAsync("/api/runclient", content);
            var msg = response.Content.ReadAsStringAsync().Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Matches(new Regex(@"ERROR:.*'mness' must",RegexOptions.IgnoreCase), msg);
        }

        /// <TestDescription>Fail when -u is not declared in the .bat file</TestDescription>            
        /// <TestId>UT.API.02.02</TestId> 
        /// <ReqId>API02.01</ReqId>
        /// <Version>2.1</Version>
        [Fact]
        public async Task Post_RunClient_FailU()
        {
            SwapBat("submit-fail-u.bat");

            var args = new ClientArgs()
            {
                Name = "test",
                User = "shopkins"
            };
            var json = JsonConvert.SerializeObject(args);

            await using var application = new WebApplicationFactory<Program>();

            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");


            var client = application.CreateClient();

            client.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


            var response = await client.PostAsync("/api/runclient", content);
            var msg = response.Content.ReadAsStringAsync().Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Matches(new Regex(@"ERROR:.*-u parameter is not specified", RegexOptions.IgnoreCase), msg);
            SwapBat("submit-original.bat");

        }

        /// <TestDescription>Fail when -n is not declared in the .bat file</TestDescription>            
        /// <TestId>UT.API.02.03</TestId> 
        /// <ReqId>API02.01</ReqId>
        /// <Version>2.1</Version>
        [Fact]
        public async Task Post_RunClient_FailN()
        {
            SwapBat("submit-fail-n.bat");

            var args = new ClientArgs()
            {
                Name = "test",
                User = "shopkins"
            };
            var json = JsonConvert.SerializeObject(args);

            await using var application = new WebApplicationFactory<Program>();

            HttpContent content = new StringContent(json);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

            var client = application.CreateClient();

            client.DefaultRequestHeaders
                .Accept
                .Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));


            var response = await client.PostAsync("/api/runclient", content);
            var msg = response.Content.ReadAsStringAsync().Result;

            Assert.True(response.IsSuccessStatusCode);
            Assert.Matches(new Regex(@"ERROR:.*notifications are not enabled", RegexOptions.IgnoreCase), msg);
            SwapBat("submit-original.bat");
        }

        private void SwapBat(string fn)
        {
            File.Copy(Path.Combine(_loc, fn), Path.Combine(_loc, "submit.bat"), true);
        }
    }
}

