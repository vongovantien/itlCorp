using eFMS.ReportData.Extension;
using eFMS.ReportData.Models;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;

namespace eFMS.ReportData.Controllers
{
    [RoutePrefix("v{version}/{lang}/ReportData")]
    public class ReportDataController : ApiController
    {
        #region catalogue
        #region export for country
        [Route("catalogue/exportcountry")]
        [HttpPost]
        public HttpResponseMessage ExportCountry([FromBody] CatCountryCriteria catCountryCriteria)
        {
            HttpClient client = new HttpClient();
            Helper helper = new Helper();
            //client.BaseAddress = new Uri();
            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonConvert.SerializeObject(catCountryCriteria, Formatting.Indented);
            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            // List data response.
            HttpResponseMessage response = client.PostAsync(Host.Url + "/Catalogue" + "/api/v1/en-US/CatCountry/query", content).Result; ;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            var dataObjects = response.Content.ReadAsAsync<List<CatCountry>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateCountryExcelFile(dataObjects);
            var buffer = stream as MemoryStream;
            var res = Request.CreateResponse(HttpStatusCode.OK);
            HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
            Response.Content = new ByteArrayContent(buffer.ToArray());
            Response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            Response.Content.Headers.ContentDisposition.FileName = "Countries List.xlsx";
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return Response;
        }
        #endregion
        #region export for catplace
        [Route("catalogue/exportcatplace")]
        [HttpPost]
        public HttpResponseMessage ExportCatPlace([FromBody] CatPlaceCriteria catPlaceCriteria)
        {
            HttpClient client = new HttpClient();
            Helper helper = new Helper();
            //client.BaseAddress = new Uri();
            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonConvert.SerializeObject(catPlaceCriteria, Formatting.Indented);
            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            // List data response.
            HttpResponseMessage response = client.PostAsync(Host.Url + "/Catalogue" + "/api/v1/en-US/CatPlace/query", content).Result; ;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            var dataObjects = response.Content.ReadAsAsync<List<WareHouse>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
            // export warehourse
            if(catPlaceCriteria.PlaceType == CatPlaceTypeEnum.Warehouse)
            {
                var stream = helper.CreateWareHourseExcelFile(dataObjects);
                var buffer = stream as MemoryStream;
                var res = Request.CreateResponse(HttpStatusCode.OK);
                HttpResponseMessage Response = new HttpResponseMessage(HttpStatusCode.OK);
                Response.Content = new ByteArrayContent(buffer.ToArray());
                Response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
                Response.Content.Headers.ContentDisposition.FileName = "Warehouse List.xlsx";
                Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                return Response;
            }
            return null;
        }
        #endregion
        #region export for commodity list 
        [Route("catalogue/exportcommoditylist")]
        [HttpPost]
        public HttpResponseMessage ExportCommodityList([FromBody] CatCommodityCriteria catCommodityCriteria)
        {
            HttpClient client = new HttpClient();
            Helper helper = new Helper();
            //client.BaseAddress = new Uri();
            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

            string json = JsonConvert.SerializeObject(catCommodityCriteria, Formatting.Indented);
            var content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            // List data response.
            HttpResponseMessage response = client.PostAsync(Host.Url + "/Catalogue" + "/api/v1/en-US/CatCommonity/query", content).Result; ;  // Blocking call! Program will wait here until a response is received or a timeout occurs.
            var dataObjects = response.Content.ReadAsAsync<List<CatCommodityModel>>().Result;  //Make sure to add a reference to System.Net.Http.Formatting.dll
            var stream = helper.CreateCommoditylistExcelFile(dataObjects);
            var buffer = stream as MemoryStream;
            var res = Request.CreateResponse(HttpStatusCode.OK);
            var Response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(buffer.ToArray())
            };
            Response.Content.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("attachment");
            Response.Content.Headers.ContentDisposition.FileName = "Commodity List.xlsx";
            Response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return Response;
        }
        #endregion

  

        #endregion
    }
}
