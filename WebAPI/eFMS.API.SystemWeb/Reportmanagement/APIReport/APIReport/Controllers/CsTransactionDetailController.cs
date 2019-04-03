using APIReport.Helpers;
using System;
using System.Collections.Generic;
using eFMS.Domain.Report;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using CrystalDecisions.CrystalReports.Engine;
using System.IO;
using System.Net.Http.Headers;
using APIReport.Models;
using System.Web.Http.Cors;

namespace APIReport.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CsTransactionDetailController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage PreviewFCLManifest(ManifestModel model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {
                //var data = await APIProvider.Get<CsTransactionDetailReport>("http://localhost:44366/api/v1/1/CsTransactionDetail/GetReport?jobId=bce3392f-ece6-4740-a657-803fcef46687");
                var result = new CsTransactionDetailReport();
                result.POD = "POD";
                var reports = new List<HouseBillManifestModel>();
                if (model.csTransactionDetails == null) return response;
                foreach (var item in model.csTransactionDetails)
                {
                    var houseBill = new HouseBillManifestModel
                    {
                        Hwbno = item.Hwbno,
                        Packages = item.PackageContainer,
                        Weight = (decimal)item.GW,
                        Volumn = (decimal)item.CBM,
                        Shipper = item.ShipperDescription,
                        NotifyParty = item.NotifyPartyDescription,
                        ShippingMark = item.ShippingMark,
                        Description = item.DesOfGoods
                    };
                    reports.Add(houseBill);
                }
                var rd = new ReportDocument();
                rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/FCLExport"), "ManifestReport.rpt"));
                rd.SetDataSource(new List<CsTransactionDetailReport>() { result });
                rd.Subreports["ManifestHouseBillDetail"].SetDataSource(reports);
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


        [HttpGet]
        public HttpResponseMessage PreviewFCLManifest()
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {
                //var data = await APIProvider.Get<CsTransactionDetailReport>("http://localhost:44366/api/v1/1/CsTransactionDetail/GetReport?jobId=bce3392f-ece6-4740-a657-803fcef46687");
                var result = new List<CsTransactionDetailReport> {
                    new CsTransactionDetailReport{ POL = "POL", POD = "POD"}
                };
                var reports = new List<HouseBillManifestModel>() {
                    //new HouseBillManifestModel{ Hwbno = "Hwbno" }
                };
                var rd = new ReportDocument();
                rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/FCLExport"), "ManifestReport.rpt"));
                rd.SetDataSource(result);
                rd.Subreports["ManifestHouseBillDetail"].SetDataSource(reports);
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
    }
}
