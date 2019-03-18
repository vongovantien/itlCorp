using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace APIReport.Helpers
{
    public static class APIProvider
    {
        /// <summary>
        /// get api
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="urlApi"></param>
        /// <returns></returns>
        public static async Task<T> Get<T>(string urlApi)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    T t = default(T);
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.BaseAddress = new Uri(urlApi);

                    var response = httpClient.GetAsync(urlApi).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        HttpContent content = response.Content;
                        t = await response.Content.ReadAsAsync<T>();
                    }
                    return t;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}