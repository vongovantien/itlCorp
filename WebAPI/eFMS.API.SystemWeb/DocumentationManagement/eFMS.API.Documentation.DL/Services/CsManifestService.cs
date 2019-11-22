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
        readonly IContextBase<CsTransactionDetail> transactionDetailRepository;
        readonly IContextBase<CatPlace> placeRepository;
        readonly ICsMawbcontainerService containerService;
        public CsManifestService(IContextBase<CsManifest> repository, 
            IMapper mapper,
            IContextBase<CsTransactionDetail> transactionDetailRepo,
            IContextBase<CatPlace> placeRepo,
            ICsMawbcontainerService contService) : base(repository, mapper)
        {
            transactionDetailRepository = transactionDetailRepo;
            placeRepository = placeRepo;
            containerService = contService;
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
                        var tranDetail = mapper.Map<CsTransactionDetail>(item);
                        transactionDetailRepository.Update(tranDetail, x => x.Id == tranDetail.Id);
                    }
                    transactionDetailRepository.SubmitChanges();
                    DataContext.SubmitChanges();
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
            var manifests = DataContext.Get(x => x.JobId == jobId);
            if (manifests == null) return null;
            var manifest = manifests.First();
            var places = placeRepository.Get(x => x.PlaceTypeId.Contains("port"));
            var result = mapper.Map<CsManifestModel>(manifest);
            result.PolName = places.FirstOrDefault(x => x.Id == manifest.Pol)?.DisplayName;
            result.PodName = places.FirstOrDefault(x => x.Id == manifest.Pod)?.DisplayName;
            return result;
        }       

        public Crystal PreviewFCLExportManifest(ManifestReportModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            string noPieces = string.Empty;
            string totalPackages = string.Empty;
            
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
                QtyPacks = string.Empty,
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
                        //TransID = item.JobNo,
                        HBL = item.Hwbno,
                        //ArrivalNo = item.ArrivalNo,
                        //ReferrenceNo = item.ReferenceNo,
                        Marks = item.ShippingMark,
                        Nofpiece = item.PackageContainer,
                        GW = item.GW ?? (decimal)item.GW,
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
                        BillType = item.ServiceType ?? string.Empty,
                        NW = item.NetWeight ?? (decimal)item.NetWeight,
                        PortofDischarge = item.PODName, 
                    };
                    manifests.Add(manifest);
                }
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
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewFCLImportManifest(ManifestReportModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var manifests = new List<ManifestFCLImportReport>();

            string noPieces = string.Empty;
            string totalPackages = string.Empty;
            var containers = new List<SeaImportCargoManifestContainer>();
            if (model.CsTransactionDetails.Count > 0)
            {
                foreach(var item in model.CsTransactionDetails)
                {
                    var houseContainers = containerService.Query(new Models.Criteria.CsMawbcontainerCriteria { Hblid = item.Id });
                    if(houseContainers.Count() > 0)
                    {
                        foreach(var container in houseContainers)
                        {
                            if(container.PackageTypeId != null)
                            {
                                noPieces = noPieces + container.PackageQuantity + " " + container.PackageTypeName + "\n";
                            }
                            totalPackages = totalPackages + container.Quantity + "X" + container.ContainerTypeName + "\n"
                                + container.ContainerNo + container.SealNo + ";\n";
                            var containerTemp = new SeaImportCargoManifestContainer {
                                Qty = container.Quantity,
                                ContType = container.ContainerTypeName,
                                ContainerNo = container.ContainerNo,
                                SealNo = container.SealNo,
                                TotalPackages = container.PackageQuantity,
                                UnitPack = container.PackageTypeName,
                                GrossWeight = container.Gw,
                                CBM = container.Cbm,
                                DecimalNo = 2
                            };
                            containers.Add(containerTemp);
                        }
                    }

                    var manifest = new ManifestFCLImportReport
                    {
                        LoadingDate = item.Etd,
                        LocalVessel = item.LocalVessel,
                        ContSealNo = item.LocalVoyNo,
                        PortofDischarge = model.PodName,
                        PlaceDelivery = model.PolName,
                        HWBNO = item.Hwbno,
                        ATTN = item.ShipperDescription,
                        Consignee = item.ConsigneeDescription,
                        Notify = item.NotifyPartyDescription,
                        TotalPackages = totalPackages,
                        ShippingMarkImport = item.ShippingMark,
                        Description = item.DesOfGoods,
                        NoPieces = noPieces,
                        GrossWeight = item.GrossWeight,
                        CBM = item.CBM,
                        Liner = item.ColoaderId
                    };
                    manifests.Add(manifest);
                }
            }

            var parameter = new ManifestFCLImportReportParameter
            {
                SumCarton = string.Empty,
                MBL = string.Empty,
                LCL = "FCL",
                CompanyName = "Indo Trans",
                CompanyAddress1 = "52, Trường Sơn, Phường 2, Tân Bình",
                CompanyAddress2 = "52, Trường Sơn, Phường 2, Tân Bình",
                Website = "itlvn.com.vn",
                Contact = "admin"
            };
            result = new Crystal
            {
                ReportName = "SeaImportCargoManifest.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(manifests);
            result.AddSubReport("ContainerDetail", containers);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
    }
}
