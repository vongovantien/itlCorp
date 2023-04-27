using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Common.Helpers
{
    public static class HttpClientService
    {
        public async static Task<HttpResponseMessage> PostAPI(string url, object obj, string token)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                StringContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split().Length > 1 ? token.Split()[1] : token; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await client.PostAsync(url, content);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<HttpResponseMessage> PostAPI(string url, object obj, string token, List<KeyValuePair<string, string>> headers)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                StringContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split()[1]; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }
                }

                HttpResponseMessage response = await client.PostAsync(url, content);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<HttpResponseMessage> PostAPI(string url, object obj, string username, string password)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                StringContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(username))
                {
                    var authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);
                }

                HttpResponseMessage response = await client.PostAsync(url, content);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<HttpResponseMessage> GetApi(string url, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split()[1]; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                HttpResponseMessage response = await client.GetAsync(url);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<HttpResponseMessage> DeleteApi(string url, string token)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split()[1]; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await client.DeleteAsync(url);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<HttpResponseMessage> PutAPI(string url, object obj, string token)
        {
            try
            {
                HttpClient client = new HttpClient();

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                StringContent content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split()[1]; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                HttpResponseMessage response = await client.PutAsync(url, content);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<byte[]> GetByteArrayFromFile(string url, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split()[1]; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var response = await client.GetByteArrayAsync(url);
                return response;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }

        public async static Task<Stream> GetFileContentResult(string url, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split()[1]; // remove bearer
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                var s = await client.GetStreamAsync(url);
                return s;
            }
            catch (Exception e)
            {
                new LogHelper("eFMS_HttpService_Log", e.ToString());
            }
            return null;
        }
    }
}
