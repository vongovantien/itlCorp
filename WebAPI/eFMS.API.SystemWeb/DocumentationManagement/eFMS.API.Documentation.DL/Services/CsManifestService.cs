﻿using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsManifestService : RepositoryBase<CsManifest, CsManifestModel>, ICsManifestService
    {
        readonly IContextBase<CsTransactionDetail> transactionDetailRepository;
        readonly IContextBase<CatPlace> placeRepository;
        readonly ICsMawbcontainerService containerService;
        readonly ICsTransactionService csTransactionService;
        readonly IContextBase<CsTransaction> transactionRepository;
        readonly IContextBase<CatUnit> unitRepository;
        readonly ICurrentUser currentUser;
        public CsManifestService(IContextBase<CsManifest> repository, 
            IMapper mapper,
            IContextBase<CsTransactionDetail> transactionDetailRepo,
            IContextBase<CatPlace> placeRepo,
            ICsMawbcontainerService contService,
            ICsTransactionService transactionService,
            IContextBase<CsTransaction> transactionRepo,
            IContextBase<CatUnit> unitRepo,
            ICurrentUser currUser) : base(repository, mapper)
        {
            transactionDetailRepository = transactionDetailRepo;
            placeRepository = placeRepo;
            containerService = contService;
            csTransactionService = transactionService;
            transactionRepository = transactionRepo;
            unitRepository = unitRepo;
            currentUser = currUser;
        }

        public HandleState AddOrUpdateManifest(CsManifestEditModel model)
        {
            try
            {
                var manifest = mapper.Map<CsManifest>(model);
                manifest.CreatedDate = DateTime.Now;
                var hs = new HandleState();
                manifest.RefNo = GetManifestNo(model.JobId);
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
                        var s = transactionDetailRepository.Update(tranDetail, x => x.Id == tranDetail.Id);
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

        private string GetManifestNo(Guid jobId)
        {
            string manifestNo = string.Empty;
            var shipment = transactionRepository.Get(x => x.Id == jobId).FirstOrDefault();
            //var prefixJob = shipment.JobNo.Substring(0, 3);
            int length = shipment.JobNo.Length - 1;
            switch (shipment.TransactionType)
            {
                case "SFI":
                    manifestNo = "MSI" + shipment.JobNo.Substring(3);
                    break;
                case "SFE":
                    manifestNo = "MSE" + shipment.JobNo.Substring(3);
                    break;
                case "SLE":
                    manifestNo = "MSE" + shipment.JobNo.Substring(3);
                    break;
                case "AE":
                    manifestNo = "MAE" + shipment.JobNo.Substring(2);
                    break;
            }
            return manifestNo;
        }

        public CsManifestModel GetById(Guid jobId)
        {
            var manifests = DataContext.Get(x => x.JobId == jobId);
            if (manifests.Count() == 0 ) return null;
            var manifest = manifests.First();
            var places = placeRepository.Get(x => x.PlaceTypeId.Contains("port"));
            var result = mapper.Map<CsManifestModel>(manifest);
            result.PolName = places.FirstOrDefault(x => x.Id == manifest.Pol)?.NameEn;
            result.PodName = places.FirstOrDefault(x => x.Id == manifest.Pod)?.NameEn;
            return result;
        }       

        public Crystal PreviewSeaExportManifest(ManifestReportModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = new Crystal();
            string noPieces = string.Empty;
            string totalPackages = string.Empty;
            var ports = placeRepository.Get(x => x.PlaceTypeId.Contains("Port")).ToList();
            model.PolName = model.Pol != null ? ports.Where(x => x.Id == model.Pol)?.FirstOrDefault()?.NameEn : null;
            model.PodName = model.Pod != null ? ports.Where(x => x.Id == model.Pod)?.FirstOrDefault()?.NameEn : null;
            var parameter = new SeaCargoManifestParameter
            {
                ManifestNo = model.RefNo?.ToUpper(),
                Owner = model.ManifestIssuer?.ToUpper() ?? string.Empty,
                Marks = model.MasksOfRegistration ?? string.Empty,
                Flight = model.VoyNo?.ToUpper() ?? string.Empty,
                PortLading = model.PolName?.ToUpper(),
                PortUnlading = model.PodName?.ToUpper(),
                FlightDate = model.InvoiceDate?.ToString("dd/MM/yyyy"),
                Eta = "Eta test",
                Consolidater = model.Consolidator?.ToUpper() ?? string.Empty,
                DeConsolidater = model.DeConsolidator?.ToUpper() ?? string.Empty,
                Forwarder = "Forwarder",
                OMB = "OMB",
                ContainerNo = model.SealNoContainerNames ?? "",
                Agent = "Agent",
                QtyPacks = string.Empty,
                TotalShipments = "1",
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyDescription = "CompanyDescription",
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_ADDRESS2,
                Website = DocumentConstants.COMPANY_WEBSITE,
                Contact = currentUser.UserName
            };
            var manifests = new List<SeaFCLExportCargoManifest>();
            var freightManifests = new List<FreightManifest>();
            if (model.CsTransactionDetails.Count > 0)
            {

                foreach (var item in model.CsTransactionDetails)
                {
                    var manifest = new SeaFCLExportCargoManifest
                    {
                        //TransID = item.JobNo,
                        HBL = item.Hwbno,
                        //ArrivalNo = item.ArrivalNo,
                        //ReferrenceNo = item.ReferenceNo,
                        Marks = item.ShippingMark,
                        Nofpiece = item.PackageContainer,
                        GrossWeight = item.GW ??0,
                        SeaCBM = item.CBM ?? 0,
                        NoOfAWB = 0,
                        Destination = item.FinalDestinationPlace?.ToUpper() ?? string.Empty,
                        Shipper = item.ShipperDescription?.ToUpper() ?? string.Empty,
                        Consignee = item.ConsigneeDescription?.ToUpper() ?? string.Empty,
                        Descriptions = item.DesOfGoods ?? string.Empty,
                        FreightCharge = item.FreightPayment != null? "Frieght: " + item.FreightPayment: string.Empty,
                        Notify = item.NotifyParty ?? string.Empty,
                        OnboardNote = item.OnBoardStatus ?? string.Empty,
                        MaskNos = string.Empty,
                        TranShipmentTo = item.PlaceFreightPay ?? string.Empty,
                        BillType = item.ServiceType ?? string.Empty
                    };
                    manifests.Add(manifest);
                }
            }
            else
            {
                return result;
            }
            result = new Crystal
            {
                ReportName = "SeaCargoManifest.rpt",
                AllowPrint = true,
                AllowExport = true,
                IsLandscape = true
            };
            result.AddDataSource(manifests);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewSeaImportManifest(ManifestReportModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = new Crystal();
            var manifests = new List<ManifestFCLImportReport>();

            var containers = new List<SeaImportCargoManifestContainer>();
            var transaction = csTransactionService.GetDetails(model.JobId);//csTransactionService.GetById(model.JobId);
            var units = unitRepository.Get().ToList();
            var ports = placeRepository.Get(x => x.PlaceTypeId.Contains("Port")).ToList();
            model.PolName = model.Pol != null? ports.Where(x => x.Id == model.Pol)?.FirstOrDefault()?.NameEn: null;

            var shipmentContainers = containerService.Get(x => x.Mblid == model.JobId);
            if (shipmentContainers.Count() == 0) return null;
            foreach (var container in shipmentContainers)
            {
                container.ContainerTypeName = units.Where(x => x.Id == container.ContainerTypeId)?.FirstOrDefault()?.UnitNameEn;
                var containerTemp = new SeaImportCargoManifestContainer
                {
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
            if (model.CsTransactionDetails.Count > 0)
            {
                foreach (var item in model.CsTransactionDetails)
                {
                    string totalPackages = item.PackageContainer + "\n" + item.ContSealNo;
                    var packageType = item.PackageType != null ? units.Where(x => x.Id == item.PackageType)?.FirstOrDefault()?.UnitNameEn: null;
                    string noPieces = item.PackageQty + packageType;
                    var manifest = new ManifestFCLImportReport
                    {
                        DateConfirm = model.InvoiceDate,
                        LoadingDate = transaction.Etd,
                        LocalVessel = transaction.FlightVesselName?.ToUpper(),
                        ContSealNo = transaction.VoyNo?.ToUpper(),
                        PortofDischarge = model.PodName?.ToUpper(),
                        PlaceDelivery = transaction.PlaceDeliveryName?.ToUpper(),
                        HWBNO = item.Hwbno,
                        ATTN = item.ShipperDescription?.ToUpper(),
                        Consignee = item.ConsigneeDescription?.ToUpper(),
                        Notify = item.NotifyPartyDescription?.ToUpper(),
                        TotalPackages = totalPackages,
                        ShippingMarkImport = item.ShippingMark,
                        Description = item.DesOfGoods,
                        NoPieces = noPieces,
                        GrossWeight = item.GrossWeight ?? 0,
                        CBM = item.CBM,
                        Liner = item.ColoaderId,
                        OverseasAgent = transaction.AgentName?.ToUpper()
                    };
                    manifests.Add(manifest);
                    
                   
                }
            }
            if (manifests.Count == 0)
                return result;
            var parameter = new ManifestFCLImportReportParameter
            {
                SumCarton = string.Empty,
                MBL = transaction.Mawb,
                LCL = "FCL",
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_ADDRESS2,
                Website = DocumentConstants.COMPANY_WEBSITE,
                Contact = currentUser.UserName,
            };
            result = new Crystal
            {
                ReportName = "SeaImportCargoManifest.rpt",
                AllowPrint = true,
                AllowExport = true,
                IsLandscape = true
            };
            result.AddDataSource(manifests);
            result.AddSubReport("ContainerDetail", containers);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewAirExportManifest(ManifestReportModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = new Crystal();
            var transaction = csTransactionService.GetDetails(model.JobId);//csTransactionService.GetById(model.JobId);
            var ports = placeRepository.Get(x => x.PlaceTypeId.Contains("Port")).ToList();
            model.PolName = model.Pol != null ? ports.Where(x => x.Id == model.Pol)?.FirstOrDefault()?.NameEn : null;
            model.PodName = model.Pol != null ? ports.Where(x => x.Id == model.Pod)?.FirstOrDefault()?.NameEn : null;
            var manifests = new List<AirCargoManifestReport>();
            if (model.CsTransactionDetails.Count > 0)
            {
                foreach(var item in model.CsTransactionDetails)
                {
                    var manifest = new AirCargoManifestReport {
                        Billype = "H",
                        HWBNO = item.Hwbno?.ToUpper(),
                        Pieces = item.PackageQty?.ToString(),
                        GrossWeight = item.GrossWeight ?? 0,
                        ShipperName = item.ShipperDescription?.ToUpper(),
                        Consignees = item.ConsigneeDescription?.ToUpper(),
                        Description = item.DesOfGoods,
                        FirstDest = item.FirstCarrierBy?.ToUpper(),
                        SecondDest = item.TransitPlaceTo1?.ToUpper(),
                        ThirdDest = item.TransitPlaceTo2?.ToUpper(),
                        Notify = item.NotifyPartyDescription?.ToUpper()
                    };
                    manifests.Add(manifest);
                }
            }
            if (manifests.Count == 0)
                return result;

            var parameter = new AirCargoManifestReportParameter
            {
                AWB = transaction.Mawb ?? string.Empty,
                Marks = model.MasksOfRegistration?? string.Empty,
                Flight = transaction.FlightVesselName?.ToUpper() ?? string.Empty,
                PortLading = model.PolName?.ToUpper() ?? string.Empty,
                PortUnlading = model.PodName?.ToUpper() ?? string.Empty,
                FlightDate = transaction.FlightDate == null?string.Empty: transaction.FlightDate.Value.ToString("MMM dd, yyyy"),
                Shipper = DocumentConstants.COMPANY_NAME + "\n" + DocumentConstants.COMPANY_ADDRESS1,
                Consignee = transaction.AgentName?.ToUpper() ?? string.Empty,
                Contact = currentUser.UserName
            };
            result = new Crystal
            {
                ReportName = "Aircargomanifest.rpt",
                AllowPrint = true,
                AllowExport = true,
                IsLandscape = true
            };
            result.AddDataSource(manifests);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }
    }
}
