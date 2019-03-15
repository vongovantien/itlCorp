using APIReport.Helpers;
using CrystalDecisions.CrystalReports.Engine;
using eFMS.Domain.Report;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace APIReport.Controllers
{
    public class CsTransactionController : ApiController
    {
        //GET api/values
        public async System.Threading.Tasks.Task<HttpResponseMessage> GetHouseBillReport(string apiUrl)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {
                var data = await APIProvider.Get<CsTransactionDetailReport>("http://localhost:44366/api/v1/1/CsTransactionDetail/GetReport?jobId=6C4B17D4-3607-41BA-B640-7B941449076C");
                var reports = new List<CsTransactionDetailReport>
                {
                    data
                };
                var rd = new ReportDocument();
                rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports"), "HBLReportSample.rpt"));
                rd.SetDataSource(reports);
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                stream.Close();
                memoryStream.Position = 0;
                //200
                //successful
                var statuscode = HttpStatusCode.OK;
                response = Request.CreateResponse(statuscode);
                //response.Content = new StreamContent(new MemoryStream(byteInfo));
                response.Content = new StreamContent(memoryStream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                //response.Content.Headers.ContentLength = byteInfo.Length;
                ContentDispositionHeaderValue contentDisposition = null;
                if (ContentDispositionHeaderValue.TryParse("inline; filename=" + string.Format("Waybill-{0}.pdf", "aa"), out contentDisposition))
                {
                    response.Content.Headers.ContentDisposition = contentDisposition;
                }
            }
            catch (Exception)
            {
                var statuscode = HttpStatusCode.NotFound;
                //var message = String.Format("Unable to find resource. Waybill \"{0}\" may not exist.", waybill);
                response = Request.CreateResponse(statuscode);
            }
            return response;
        }

        public HttpResponseMessage Get(Guid id)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {

                List<object> reports = null;
                var rd = new ReportDocument();
                rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports"), "HBLReportSample.rpt"));
                rd.SetDataSource(reports);
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                stream.Close();
                memoryStream.Position = 0;
                //200
                //successful
                var statuscode = HttpStatusCode.OK;
                response = Request.CreateResponse(statuscode);
                //response.Content = new StreamContent(new MemoryStream(byteInfo));
                response.Content = new StreamContent(memoryStream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                //response.Content.Headers.ContentLength = byteInfo.Length;
                ContentDispositionHeaderValue contentDisposition = null;
                if (ContentDispositionHeaderValue.TryParse("inline; filename=" + string.Format("Waybill-{0}.pdf", "aa"), out contentDisposition))
                {
                    response.Content.Headers.ContentDisposition = contentDisposition;
                }
            }
            catch (Exception ex)
            {
                var statuscode = HttpStatusCode.NotFound;
                //var message = String.Format("Unable to find resource. Waybill \"{0}\" may not exist.", waybill);
                response = Request.CreateResponse(statuscode);
            }
            return response;
        }
    }
}
