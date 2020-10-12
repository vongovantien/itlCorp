using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Infrastructure.Http
{
    public static class HttpService
    {
        public async static Task<HttpResponseMessage> PostAPI(string url, object obj, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(token))
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                HttpResponseMessage response = await client.PostAsync(url, content);
                return response;
            }
            catch (Exception e)
            {
            }
            return null;
        }
    }
}
