using Newtonsoft.Json;
using System;
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
                    token = token.Split()[1]; // remove bearer
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
    }
}
