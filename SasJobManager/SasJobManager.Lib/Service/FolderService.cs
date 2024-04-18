using SasJobManager.ServerLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json;

namespace SasJobManager.Lib.Service
{
    public class FolderService
    {
        private string _base;
        private string _url;

        public FolderService(string baseUrl, string url)
        {
            _base = baseUrl;
            _url = url;
        }

        public async Task<Tuple<bool,string>> IsLocked(string folder)
        {
            
            try
            {
                var client = new RestClient(_base);
                var request = new RestRequest(_url, Method.Get);
                request.AddParameter("folder", folder);
                var webresponse = await client.ExecuteAsync(request);
                if (webresponse.Content != null)
                {
                    return JsonConvert.DeserializeObject<Tuple<bool, string>>(webresponse.Content);
                }
                else
                {
                    return Tuple.Create(false, $"Unable to evaluate CS-096 Status of folder {folder}");
                }
            }
            catch (Exception ex)
            {
                return Tuple.Create(false, ex.Message);
            }            
        }

        public bool IsFileProtected(string file)
        {
            var fi = File.GetAttributes(file);
            return fi.HasFlag(FileAttributes.ReadOnly);
        }

        public static bool ToggleReadOnly(string file,bool makeRO)
        {
            try
            {
                if (makeRO)
                    File.SetAttributes(file, FileAttributes.ReadOnly);
                else
                    File.SetAttributes(file, FileAttributes.Normal);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
