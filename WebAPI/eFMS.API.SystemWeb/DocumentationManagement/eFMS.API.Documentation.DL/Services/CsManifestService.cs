using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsManifestService : RepositoryBase<CsManifest, CsManifestModel>, ICsManifestService
    {
        public CsManifestService(IContextBase<CsManifest> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public HandleState AddOrUpdateManifest(CsManifestEditModel model)
        {
            try
            {
                var manifest = mapper.Map<CsManifest>(model);
                manifest.CreatedDate = DateTime.Now;
                var hs = new HandleState();
                if (DataContext.Any(x => x.JobId == model.JobId))
                {
                    hs = DataContext.Update(manifest, x => x.JobId == model.JobId);
                }
                else
                {
                    hs = DataContext.Add(manifest);
                }
                if (hs.Success)
                {
                    foreach(var item in model.CsTransactionDetails)
                    {
                        if (item.IsRemoved)
                        {
                            item.ManifestRefNo = null;
                        }
                        else
                        {
                            item.ManifestRefNo = manifest.RefNo;
                        }
                        item.DatetimeModified = DateTime.Now;
                        item.UserModified = manifest.UserCreated;
                        ((eFMSDataContext)DataContext.DC).CsTransactionDetail.Update(item);
                    }
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }

                return hs;
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }

        public CsManifestModel GetById(Guid jobId)
        {
            var query = (from manifest in ((eFMSDataContext)DataContext.DC).CsManifest
                         where manifest.JobId == jobId
                         join pol in ((eFMSDataContext)DataContext.DC).CatPlace on manifest.Pol equals pol.Id into polManifest
                         from pl in polManifest.DefaultIfEmpty()
                         join pod in ((eFMSDataContext)DataContext.DC).CatPlace on manifest.Pod equals pod.Id into podManifest
                         from pd in polManifest.DefaultIfEmpty()
                         select new { manifest, pl, pd }).FirstOrDefault();
            if (query == null) return null;
            var result = mapper.Map<CsManifestModel>(query.manifest);
            result.PodName = query.pd?.NameEn;
            result.PolName = query.pl?.NameEn;
            return result;
        }       

        public Crystal Preview(ManifestReportModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            string packageQuantity = "";
            int containerType = 0;
            if (model.CsMawbcontainers != null)
            {
                string sbContNo = "";
                string sbContType = "";
                foreach (var container in model.CsMawbcontainers)
                {
                    if (!string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                    {
                        sbContNo += container.ContainerNo + "/ " + container.SealNo + ", ";
                    }
                    else if (!string.IsNullOrEmpty(container.ContainerNo) && string.IsNullOrEmpty(container.SealNo))
                    {
                        sbContNo += container.ContainerNo + ", ";
                    }
                    else if (string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                    {
                        sbContNo += container.SealNo + ", ";
                    }
                    sbContType += container.Quantity + "X" + container.ContainerTypeName + ", ";
                    if (!string.IsNullOrEmpty(container.PackageTypeName) && container.PackageQuantity != null)
                    {
                        packageQuantity += container.PackageQuantity + "X" + container.PackageTypeName + ", ";
                    }
                    if(container.ContainerTypeId != null)
                    {
                        containerType = containerType + 1;
                    }
                }
                if (sbContNo.Length > 2)
                {
                    model.SealNoContainerNames = sbContNo.Substring(0, sbContNo.Length - 2);
                }
                if (sbContType.Length > 2)
                {
                    model.NumberContainerTypes = sbContType.Substring(0, sbContType.Length - 2);
                }
            }
            var parameter = new SeaCargoManifestParameter
            {
                ManifestNo = model.RefNo,
                Owner = model.ManifestIssuer ?? string.Empty,
                Marks = model.MasksOfRegistration ?? string.Empty,
                Flight = model.VoyNo ?? string.Empty,
                PortLading = model.PolName ?? string.Empty,
                PortUnlading = model.PodName ?? string.Empty,
                FlightDate = model.InvoiceDate?.ToString("dd/MM/yyyy"),
                Eta = "Eta test",
                Consolidater = model.Consolidator ?? string.Empty,
                DeConsolidater = model.DeConsolidator ?? string.Empty,
                Forwarder = "Forwarder",
                OMB = "OMB",
                ContainerNo = model.SealNoContainerNames ?? "",
                Agent = "Agent",
                QtyPacks = containerType.ToString(),
                TotalShipments = "1",
                CompanyName = "CompanyName",
                CompanyDescription = "CompanyDescription",
                CompanyAddress1 = "CompanyAddress1",
                CompanyAddress2 = "CompanyAddress2",
                Website = "Website",
                Contact = "Contact"
            };
            var manifests = new List<SeaCargoManifest>();
            var freightManifests = new List<FreightManifest>();
            if (model.CsTransactionDetails.Count > 0)
            {

                foreach (var item in model.CsTransactionDetails)
                {
                    var manifest = new SeaCargoManifest
                    {
                        TransID = item.JobNo,
                        HBL = item.Hwbno,
                        Marks = item.ShippingMark,
                        Nofpiece = item.PackageContainer,
                        GrossWeight = item.GW ?? (decimal)item.GW,
                        SeaCBM = item.CBM ?? (decimal)item.CBM,
                        NoOfAWB = 0,
                        Destination = item.FinalDestinationPlace ?? string.Empty,
                        Shipper = item.ShipperDescription ?? string.Empty,
                        Consignee = item.ConsigneeDescription ?? string.Empty,
                        Descriptions = item.DesOfGoods ?? string.Empty,
                        FreightCharge = item.FreightPayment ?? string.Empty,
                        Notify = item.NotifyParty ?? string.Empty,
                        OnboardNote = item.OnBoardStatus ?? string.Empty,
                        MaskNos = string.Empty,
                        TranShipmentTo = item.PlaceFreightPay ?? string.Empty,
                        BillType = item.ServiceType ?? string.Empty
                    };
                    manifests.Add(manifest);
                }
                //freightManifests = new List<FreightManifest> {
                //};
            }
            else
            {
                return null;
            }
            result = new Crystal
            {
                ReportName = "SeaCargoManifest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(manifests);
            result.FormatType = ExportFormatType.PortableDocFormat;
            //result.AddSubReport("FreightManifest", freightManifests);
            result.SetParameter(parameter);
            return result;
        }
    }
}
