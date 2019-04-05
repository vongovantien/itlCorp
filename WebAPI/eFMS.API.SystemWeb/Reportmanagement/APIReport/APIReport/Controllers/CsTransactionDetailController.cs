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
using System.Text;

namespace APIReport.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class CsTransactionDetailController : ApiController
    {
        [HttpPost]
        [Route("api/CsTransactionDetail/PreviewFCLManifest")]
        public HttpResponseMessage PreviewFCLManifest(ManifestModel model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {
                //var data = await APIProvider.Get<CsTransactionDetailReport>("http://localhost:44366/api/v1/1/CsTransactionDetail/GetReport?jobId=bce3392f-ece6-4740-a657-803fcef46687");
                
                var reports = new List<HouseBillManifestModel>();
                if(model.CsMawbcontainers != null)
                {
                    string sbContNo = "";
                    string sbContType = "";
                    foreach (var container in model.CsMawbcontainers)
                    {
                        if (!string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                        {
                            sbContNo += container.ContainerNo + "/ " + container.SealNo + ", ";
                        }
                        else if (!string.IsNullOrEmpty(container.ContainerNo)  && string.IsNullOrEmpty(container.SealNo))
                        {
                            sbContNo += container.ContainerNo + ", ";
                        }
                        else if (string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                        {
                            sbContNo += container.SealNo + ", ";
                        }
                        sbContType += container.Quantity + "X" + container.ContainerTypeName + ", ";
                    }
                    if(sbContNo.Length > 2)
                    {
                        model.SealNoContainerNames = sbContNo.Substring(0, sbContNo.Length - 2);
                    }
                    if(sbContType.Length > 2)
                    {
                        model.NumberContainerTypes = sbContType.Substring(0, sbContType.Length - 2);
                    }
                }
                var result = new CsTransactionDetailReport
                {
                    POD = model.PodName,
                    POL = model.PolName,
                    VesselNo = model.VoyNo,
                    ETD = model.InvoiceDate?.ToString("dd MMMM, yyyy"),
                    SealNoContainerNames = model.SealNoContainerNames,
                    NumberContainerTypes = model.NumberContainerTypes,
                };
                if (model.CsTransactionDetails != null)
                {
                    foreach (var item in model.CsTransactionDetails)
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
                            Description = item.DesOfGoods,
                            FreightPayment = item.FreightPayment
                        };
                        reports.Add(houseBill);
                    }
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
                response.Content = new StreamContent(memoryStream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                if (ContentDispositionHeaderValue.TryParse("inline; filename=" + string.Format("Waybill-{0}.pdf", "aa"), out ContentDispositionHeaderValue contentDisposition))
                {
                    response.Content.Headers.ContentDisposition = contentDisposition;
                }
            }
            catch (Exception)
            {
                var statuscode = HttpStatusCode.NotFound;
                response = Request.CreateResponse(statuscode);
            }
            return response;
        }

        [HttpPost]
        [Route("api/CsTransactionDetail/PreviewFCLShippingInstruction")]
        public HttpResponseMessage PreviewFCLShippingInstruction(CsShippingInstructionModel model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {
                var contHouseBills = new List<ShippingInstructionContHouse>();
                string sumContainerType = "";
                string sumPackageType = "";
                if (model.CsTransactionDetails != null)
                {
                    int quantity = 0;
                    int quantityPackage = 0;
                    foreach (var container in model.CsTransactionDetails)
                    {
                        string sbContNo = "";
                        string sbNumberPackage = "";
                        decimal grossWeight = 0;
                        decimal cbm = 0;
                        var item = new ShippingInstructionContHouse();
                        //if(sumContainerType.Length == 0)
                        //{
                        //    sumContainerType = container.Quantity + "X" + container.ContainerTypeName;
                        //}
                        //else
                        //{
                        //    if (!sumContainerType.Contains(container.ContainerTypeName))
                        //    {
                        //        quantity += (int)container?.Quantity;
                        //        sumContainerType = quantity + "X" + container.ContainerTypeName;
                        //    }
                        //}
                        //sbContNo += container.Quantity + "X" + container.ContainerTypeName + "\n";
                        //if (!string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                        //{
                        //    sbContNo += container.ContainerNo + "/ " + container.SealNo + ", ";
                        //}
                        //else if (!string.IsNullOrEmpty(container.ContainerNo) && string.IsNullOrEmpty(container.SealNo))
                        //{
                        //    sbContNo += container.ContainerNo + ", ";
                        //}
                        //else if (string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                        //{
                        //    sbContNo += container.SealNo + ", ";
                        //}
                        //if(container.PackageQuantity != null && !string.IsNullOrEmpty(container.PackageTypeName))
                        //{
                        //    sbNumberPackage += container.PackageQuantity + " " + container.PackageTypeName + ", ";
                        //    if (sumPackageType.Length == 0)
                        //    {
                        //        sumPackageType = container.PackageQuantity + "X" + container.PackageTypeName;
                        //    }
                        //    else
                        //    {
                        //        if (!sumPackageType.Contains(container.PackageTypeName))
                        //        {
                        //            quantityPackage += (int)container?.PackageQuantity;
                        //            sumPackageType = quantityPackage + "X" + container.PackageTypeName;
                        //        }
                        //    }
                        //}

                        //if (sbContNo.Length > 2)
                        //{
                        //    item.ContainerSealNo = sbContNo.Substring(0, sbContNo.Length - 2);
                        //}
                        //if(sbNumberPackage.Length > 2)
                        //{
                        //    item.PackagesNote = sbNumberPackage.Substring(0, sbNumberPackage.Length - 2);
                        //}
                        //grossWeight += (decimal)container?.Gw;
                        //cbm += (decimal)container?.Cbm;
                        //if(item.PackagesNote.Length > 0 || item.ContainerSealNo.Length > 0)
                        //{
                        //    contHouseBills.Add(item);
                        //}
                    }
                }
                var shippingIns = new List<ShippingInstructionReport>();
                var si = new ShippingInstructionReport
                {
                    IssuedUserName = model.IssuedUserName,
                    IssuedUserTel = string.Empty,
                    SupplierName = model.SupplierName,
                    InvoiceNoticeRecevier = model.InvoiceNoticeRecevier,
                    BookingNo = model.BookingNo,
                    InvoiceDate = model.InvoiceDate?.ToString("dd MMMM, yyyy"),
                    LoadingDate = model.LoadingDate?.ToString("dd MMMM, yyyy"),
                    Shipper = model.Shipper,
                    ConsigneeDescription = model.ConsigneeDescription,
                    CargoNoticeRecevier = model.CargoNoticeRecevier,
                    PodName = model.PodName,
                    PolName = model.PolName,
                    PoDelivery = model.PoDelivery,
                    VesselNo = model.VoyNo,
                    Remark = model.Remark,
                    PaymenType = model.PaymenType,
                    SumGrossWeight = contHouseBills.Sum(x => x.GrossWeight),
                    SumVolume = contHouseBills.Sum(x => x.Volume),
                    SumContainerSealNo = sumContainerType,
                    SumPackagesNote = sumPackageType
                };
                var rd = new ReportDocument();
                rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/FCLExport"), "ShippingInstructionReport.rpt"));
                rd.SetDataSource(shippingIns);
                rd.Subreports["ShippingInstructionContHouseBill"].SetDataSource(contHouseBills);
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                MemoryStream memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                stream.Close();
                memoryStream.Position = 0;
                //200
                //successful
                var statuscode = HttpStatusCode.OK;
                response = Request.CreateResponse(statuscode);
                response.Content = new StreamContent(memoryStream);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
                if (ContentDispositionHeaderValue.TryParse("inline; filename=" + string.Format("Waybill-{0}.pdf", "aa"), out ContentDispositionHeaderValue contentDisposition))
                {
                    response.Content.Headers.ContentDisposition = contentDisposition;
                }
            }
            catch (Exception)
            {
                var statuscode = HttpStatusCode.NotFound;
                response = Request.CreateResponse(statuscode);
            }
            return response;
        }
        //[HttpGet]
        //public HttpResponseMessage PreviewFCLManifest()
        //{
        //    HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
        //    try
        //    {
        //        //var data = await APIProvider.Get<CsTransactionDetailReport>("http://localhost:44366/api/v1/1/CsTransactionDetail/GetReport?jobId=bce3392f-ece6-4740-a657-803fcef46687");
        //        var result = new List<CsTransactionDetailReport> {
        //            new CsTransactionDetailReport{ POL = "POL", POD = "POD"}
        //        };
        //        var reports = new List<HouseBillManifestModel>() {
        //            //new HouseBillManifestModel{ Hwbno = "Hwbno" }
        //        };
        //        var rd = new ReportDocument();
        //        rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/FCLExport"), "ManifestReport.rpt"));
        //        rd.SetDataSource(result);
        //        rd.Subreports["ManifestHouseBillDetail"].SetDataSource(reports);
        //        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
        //        MemoryStream memoryStream = new MemoryStream();
        //        stream.CopyTo(memoryStream);
        //        stream.Close();
        //        memoryStream.Position = 0;
        //        //200
        //        //successful
        //        var statuscode = HttpStatusCode.OK;
        //        response = Request.CreateResponse(statuscode);
        //        //response.Content = new StreamContent(new MemoryStream(byteInfo));
        //        response.Content = new StreamContent(memoryStream);
        //        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        //        //response.Content.Headers.ContentLength = byteInfo.Length;
        //        if (ContentDispositionHeaderValue.TryParse("inline; filename=" + string.Format("Waybill-{0}.pdf", "aa"), out ContentDispositionHeaderValue contentDisposition))
        //        {
        //            response.Content.Headers.ContentDisposition = contentDisposition;
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        var statuscode = HttpStatusCode.NotFound;
        //        //var message = String.Format("Unable to find resource. Waybill \"{0}\" may not exist.", waybill);
        //        response = Request.CreateResponse(statuscode);
        //    }
        //    return response;
        //}
    }
}
