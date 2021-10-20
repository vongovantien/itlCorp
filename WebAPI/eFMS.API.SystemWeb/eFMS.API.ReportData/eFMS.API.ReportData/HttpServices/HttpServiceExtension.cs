using eFMS.API.ReportData.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.ReportData.HttpServices
{
    public static class HttpServiceExtension
    {
        public async static Task<HttpResponseMessage> GetDataFromApi(Object obj, string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(url, content);
                return response;
            }
            catch (Exception e)
            {

            }
            return null;
        }

        public async static Task<HttpResponseMessage> GetDataFromApi(Object obj, string url, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split(" ")[1];
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

        public async static Task<HttpResponseMessage> PostAPI(Object obj, string url, string token)
        {

            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
                var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split(" ")[1];
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

        public async static Task<HttpResponseMessage> GetApi(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                HttpResponseMessage response = await client.GetAsync(url);
                return response;
            }
            catch (Exception e)
            {

            }
            return null;
        }
        public async static Task<HttpResponseMessage> GetApi(string url, string token)
        {
            try
            {
                HttpClient client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(5);
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                if (!string.IsNullOrEmpty(token))
                {
                    token = token.Split(" ")[1];
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                HttpResponseMessage response = await client.GetAsync(url);
                return response;
            }
            catch (Exception e)
            {

            }
            return null;
        }
    }
}
