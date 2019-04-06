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
                List<ContainerObject> listContainerTypes = new List<ContainerObject>();
                List<ContainerObject> listPackageTypes = new List<ContainerObject>();
                if (model.CsTransactionDetails != null)
                {
                    foreach (var transactionDetail in model.CsTransactionDetails)
                    {
                        var item = new ShippingInstructionContHouse();
                        if(transactionDetail.CsMawbcontainers != null)
                        {
                            item.ContainerSealNo = string.Empty;
                            item.PackagesNote = string.Empty;
                            foreach (var container in transactionDetail.CsMawbcontainers)
                            {
                                listContainerTypes.Add(new ContainerObject { Quantity = (int)container.Quantity, Name = container.ContainerTypeName });
                                item.ContainerSealNo += container.Quantity + "X" + container.ContainerTypeName + " ";
                                if(!string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                                {
                                    item.ContainerSealNo += container.ContainerNo + "/" + item.ContainerSealNo + ", ";
                                }
                                if(container.PackageQuantity != null && container.PackageTypeId != null)
                                {
                                    item.PackagesNote += container.PackageQuantity + " " + container.PackageTypeName;
                                    listPackageTypes.Add(new ContainerObject { Quantity = (int)container.PackageQuantity, Name = container.PackageTypeName });
                                }
                                item.GoodsDescription = string.Join(",", transactionDetail.CsMawbcontainers.Select(x => x.Description));
                                item.GrossWeight = (decimal)transactionDetail.CsMawbcontainers.Sum(x => x.Gw);
                                item.Volume = (decimal)transactionDetail.CsMawbcontainers.Sum(x => x.Cbm);

                            }
                        }
                        contHouseBills.Add(item);
                    }
                }
                var s = listContainerTypes.GroupBy(x => new { x.Quantity, x.Name });
                var t = listPackageTypes.GroupBy(x => new { x.Quantity, x.Name });
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
                    SumContainerSealNo = "",
                    SumPackagesNote = ""
                };
                shippingIns.Add(si);
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
            catch (Exception ex)
            {
                var statuscode = HttpStatusCode.NotFound;
                response = Request.CreateResponse(statuscode);
            }
            return response;
        }
        [HttpPost]
        [Route("api/CsTransactionDetail/PreviewOLC")]
        public HttpResponseMessage PreviewOCL(CsShippingInstructionModel model)
        {
            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.BadRequest);
            try
            {
                var oclList = new List<OCLModel>
                {
                    new OCLModel {
                        POD = model.PodName,
                        POL = model.PolName,
                        VoyNo = model.VoyNo,
                        FDestination = model.PoDelivery
                    }
                };
                var oclContainerList = new List<OCLContainerModel>();
                if(model.CsMawbcontainers != null)
                {
                    int i = 0;
                    foreach(var container in model.CsMawbcontainers)
                    {
                        i = i + 1;
                        decimal? cbm = container.Cbm;
                        decimal? gw = container.Gw;
                        var item = new OCLContainerModel
                        {
                            STT = i,
                            CBM = cbm != null?(decimal)cbm : 0,
                            GW = gw != null? (decimal)gw:0,
                            ContainerNo = container.ContainerNo,
                            SealNo = container.SealNo
                        };
                        oclContainerList.Add(item);
                    }
                }
                var rd = new ReportDocument();
                rd.Load(Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/Reports/FCLExport"), "OCL.rpt"));
                rd.SetDataSource(oclList);
                rd.Subreports["OCLContainer"].SetDataSource(oclContainerList);
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
            catch (Exception ex)
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
