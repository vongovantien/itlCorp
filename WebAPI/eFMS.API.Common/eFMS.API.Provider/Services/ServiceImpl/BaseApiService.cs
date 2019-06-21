using eFMS.API.Common.Helpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static eFMS.API.Provider.Infrasture.Settings;

namespace eFMS.API.Provider.Services.ServiceImpl
{
    public class BaseApiService
    {
        protected readonly HttpClient apiClient;
        protected readonly string baseUrl;
        protected readonly IOptions<APIUrls> settings;
        private readonly IMemoryCache memoryCache;

        public BaseApiService(HttpClient httpClient, IOptions<APIUrls> settings, int versionApi, string nameUrlBase, IMemoryCache cache)
        {
            this.settings = settings;
            apiClient = httpClient;
            memoryCache = cache;
            baseUrl = $"{ObjectHelper.GetValueBy(settings.Value, nameUrlBase)}/api/v{versionApi}/{CultureInfo.CurrentCulture.Name}";
        }

        protected async Task<TResponse> GetApi<TResponse>(string strUri)
        {
            try
            {
                string strResponse = await apiClient.GetStringAsync(strUri);
                if (!String.IsNullOrEmpty(strResponse))
                    return JsonConvert.DeserializeObject<TResponse>(strResponse);
            }
            catch {
            }
            return default(TResponse);
        }
        protected async Task<TResponse> GetApi<TResponse>(string strUri, bool? isAllowCaching = false, double? dSeconds = 300)
        {
            try
            {
                if (isAllowCaching == true)
                {
                    bool isExists = memoryCache.TryGetValue(strUri, out TResponse response);
                    if (isExists)
                    {
                        return response;
                    }
                    else
                    {
                        string strResponse = await apiClient.GetStringAsync(strUri);
                        if (!String.IsNullOrEmpty(strResponse))
                        {
                            response = JsonConvert.DeserializeObject<TResponse>(strResponse);
                            MemoryCacheEntryOptions cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromSeconds(dSeconds ?? 0));
                            memoryCache.Set(strUri, response, cacheEntryOptions);
                            return response;
                        }
                    }
                }
                else
                {
                    string strResponse = await apiClient.GetStringAsync(strUri);
                    if (!String.IsNullOrEmpty(strResponse))
                        return JsonConvert.DeserializeObject<TResponse>(strResponse);
                }
            }
            catch
            {
            }
            return default(TResponse);
        }
        protected async Task<TResponse> PostApi<TResponse, TDataModel>(string strUri, TDataModel data) where TDataModel : new()
        {
            TResponse response = default(TResponse);
            try
            {
                var payload = JsonConvert.SerializeObject(data == null ? new TDataModel() : data);
                HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
                HttpResponseMessage httpResponse = null;
                httpResponse = await apiClient.PostAsync(strUri, httpContent);
                string strResponse = await httpResponse.Content.ReadAsStringAsync();
                if (httpResponse.IsSuccessStatusCode && !string.IsNullOrEmpty(strResponse))
                {
                    response = JsonConvert.DeserializeObject<TResponse>(strResponse);
                }
            }
            catch { }
            return response;
        }
        //protected async Task<TResponse> PutApiAsync<TResponse, TDataModel>(string strUri, TDataModel data)
        //{
        //    TResponse response = default(TResponse);
        //    try
        //    {
        //        var payload = JsonConvert.SerializeObject(data);
        //        HttpContent httpContent = new StringContent(payload, Encoding.UTF8, "application/json");
        //        HttpResponseMessage httpResponse = await apiClient.PutAsync(strUri, httpContent);
        //        string strResponse = await httpResponse.Content.ReadAsStringAsync();
        //        if (httpResponse.IsSuccessStatusCode && !string.IsNullOrEmpty(strResponse))
        //        {
        //            response = JsonConvert.DeserializeObject<TResponse>(strResponse);
        //        }
        //    }
        //    catch { }
        //    return response;
        //}
        //protected async Task<TResponse> DeleteAsyncApi<TResponse>(string strUri)
        //{
        //    TResponse response = default(TResponse);
        //    HttpResponseMessage httpResponse = await apiClient.DeleteAsync(strUri);
        //    string strResponse = await httpResponse.Content.ReadAsStringAsync();
        //    if (httpResponse.IsSuccessStatusCode && !string.IsNullOrEmpty(strResponse))
        //    {
        //        response = JsonConvert.DeserializeObject<TResponse>(strResponse);
        //    }
        //    return response;
        //}
    }
}
