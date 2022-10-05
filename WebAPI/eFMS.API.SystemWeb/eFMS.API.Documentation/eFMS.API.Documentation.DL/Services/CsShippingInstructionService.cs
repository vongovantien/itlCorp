﻿using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShippingInstructionService : RepositoryBase<CsShippingInstruction, CsShippingInstructionModel>, ICsShippingInstructionService
    {
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<CatPlace> placeRepository;
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<CsMawbcontainer> containerRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        private readonly IContextBase<CsTransaction> cstransRepository;
        private readonly IContextBase<CatUnit> catUnitRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<SysEmployee> employeeRepository;
        private readonly IOptions<ApiUrl> apiUrl;
        readonly ICsTransactionDetailService transactionDetailService;
        readonly ICsMawbcontainerService csMawbcontainerService;

        public CsShippingInstructionService(IContextBase<CsShippingInstruction> repository,
            IMapper mapper,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<SysUser> userRepo,
            IContextBase<CsMawbcontainer> containerRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CsTransaction> cstransRepo,
            IContextBase<CatUnit> catUnitRepo,
            IContextBase<SysOffice> officeRepo,
            IContextBase<SysEmployee> employeeRepo,
            ICsTransactionDetailService transDetailService,
            ICsMawbcontainerService mawbcontainerService,
        IOptions<ApiUrl> url) : base(repository, mapper)
        {
            partnerRepository = partnerRepo;
            placeRepository = placeRepo;
            userRepository = userRepo;
            containerRepository = containerRepo;
            unitRepository = unitRepo;
            cstransRepository = cstransRepo;
            catUnitRepository = catUnitRepo;
            officeRepository = officeRepo;
            transactionDetailService = transDetailService;
            employeeRepository = employeeRepo;
            apiUrl = url;
            csMawbcontainerService = mawbcontainerService;
        }

        public HandleState AddOrUpdate(CsShippingInstructionModel model)
        {
            var result = new HandleState();
            var modelUpdate = mapper.Map<CsShippingInstruction>(model);
            if (DataContext.Any(x => x.JobId == model.JobId))
            {
                result = DataContext.Update(modelUpdate, x => x.JobId == model.JobId);
            }
            else
            {
                result = DataContext.Add(modelUpdate);
            }
            return result;
        }

        public CsShippingInstructionModel GetById(Guid jobId)
        {
            var transDetail = transactionDetailService.Get(x => x.JobId == jobId);
            var listCont = csMawbcontainerService.ListContOfHB(jobId);
            var jobDetail = cstransRepository.Get(x => x.Id == jobId).FirstOrDefault();
            var result = Get(x => x.JobId == jobId).FirstOrDefault();

            if (result == null) return null;
            var partners = partnerRepository.Get();
            var places = placeRepository.Get();
            var users = userRepository.Get();
            result.IssuedUserName = users?.FirstOrDefault(x => x.Id == result.IssuedUser)?.Username;
            result.SupplierName = partners?.FirstOrDefault(x => x.Id == result.Supplier)?.PartnerNameEn;
            result.ConsigneeName = partners?.FirstOrDefault(x => x.Id == result.ConsigneeId)?.PartnerNameEn;
            result.PolName = places?.FirstOrDefault(x => x.Id == result.Pol)?.NameEn;
            result.PodName = places?.FirstOrDefault(x => x.Id == result.Pod)?.NameEn;
            result.ActualShipperName = partners?.FirstOrDefault(x => x.Id == result.ActualShipperId)?.PartnerNameEn;
            result.ActualConsigneeName = partners?.FirstOrDefault(x => x.Id == result.ActualConsigneeId)?.PartnerNameEn;
            result.PackagesNote = jobDetail.TransactionType == "SLE" && listCont.Any() == false ? transDetail.Sum(x => x.PackageQty).ToString() : result.PackagesNote;
            result.PackagesType = result.PackagesType ?? transDetail?.FirstOrDefault()?.PackageType;
            result.ShippingMark = result.ShippingMark ?? String.Join(" ", transDetail.Where(x => x.ShippingMark != null && x.ShippingMark != String.Empty).Select(x => x.ShippingMark));


            return result;
        }
        public Crystal PreviewFCLShippingInstruction(CsShippingInstructionReportModel model)
        {
            Crystal result = new Crystal();

            var instructions = new List<SeaShippingInstruction>();
            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            int totalPackage = 0;
            var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();
            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            string Tel = GetTelPersonalIncharge(model.JobId);
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = model.IssuedUserName ?? string.Empty,
                Tel = Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2
            };
            string jobNo = opsTrans?.JobNo;
            string noPieces = string.Empty;

            var listCont = csMawbcontainerService.GetContainerListByJobId(model.JobId);
            var shippingMarkSI = model.ShippingMark ?? (model.CsTransactionDetails.Where(x => x.ShippingMark != null && x.ShippingMark != String.Empty).Any() ? string.Join(" ", model.CsTransactionDetails.Select(x => x.ShippingMark)) : null);
            var totalCont = listCont.Any() ? String.Join("\n", listCont.GroupBy(x => x.ContainerTypeName).Select(x => x.Count() + " x " + x.Key)) : DocumentConstants.NO_CONTAINER;
            var packagesType = model.PackagesType ?? model.CsTransactionDetails?.FirstOrDefault()?.PackageType;

            foreach (var item in model.CsTransactionDetails)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity ?? 0);
                int? totalPack = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.PackageQuantity);
                totalPackage += (int)(totalPack ?? 0);

                var packages = containerRepository.Get(x => x.Hblid == item.Id).GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.Quantity) + x.Sum(c => c.ContainerTypeId));
                noPieces = string.Join(", ", packages);

                var instruction = new SeaShippingInstruction
                {
                    TRANSID = jobNo,
                    Attn = model.InvoiceNoticeRecevier,
                    ToPartner = model.SupplierName,
                    Re = model.BookingNo,
                    DatePackage = model.InvoiceDate,
                    ShipperDf = model.Shipper,
                    GoodsDelivery = model.ConsigneeDescription,
                    NotitfyParty = model.CargoNoticeRecevier,
                    PortofLoading = model.PolName,
                    PortofDischarge = model.PodName,
                    PlaceDelivery = model.PoDelivery,
                    Vessel = model.VoyNo,
                    Etd = model.LoadingDate?.ToString("dd/MM/yyyy"),
                    ShippingMarks = item.ShippingMark,
                    Containers = item.Packages,
                    NoofPeace = noPieces,
                    SIDescription = item.DesOfGoods,
                    GrossWeight = item.GW,
                    CBM = item.CBM,
                    Qty = total.ToString(),
                    RateRequest = model.Remark,
                    Payment = model.PaymenType,
                    ShippingMarkImport = string.Empty,
                    MaskNos = item.ContSealNo,
                    ShippingMarkSI = shippingMarkSI,
                    PKGType = packagesType,
                    TotalCont = totalCont,
                };
                instructions.Add(instruction);
            }
            if (model.CsTransactionDetails.Count > 1)
            {
                parameter.TotalPackages = totalPackage.ToString();// + " PKG(S)"; CR: 15585 [31/03/2021] 
            }
            else
            {
                parameter.TotalPackages = instructions.FirstOrDefault()?.Containers;
            }
            result = new Crystal
            {
                ReportName = "SeaShippingInstructionNew.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(instructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewFCLContShippingInstruction(Guid id)
        {
            var dataShippingIntruction = DataContext.Get(x => x.JobId == id).FirstOrDefault();
            if (dataShippingIntruction == null) return null;
            var model = mapper.Map<CsShippingInstructionReportConstModel>(dataShippingIntruction);
            model.CsTransactionDetails = transactionDetailService.Get(x => x.JobId == id).ToList();
            if (model.CsTransactionDetails.Any())
            {
                var partners = partnerRepository.Get();
                var places = placeRepository.Get();
                var users = userRepository.Get();
                model.IssuedUserName = users?.FirstOrDefault(x => x.Id == model.IssuedUser)?.Username;
                model.SupplierName = partners?.FirstOrDefault(x => x.Id == model.Supplier)?.PartnerNameEn;
                model.ConsigneeName = partners?.FirstOrDefault(x => x.Id == model.ConsigneeId)?.PartnerNameEn;
                model.PolName = places?.FirstOrDefault(x => x.Id == model.Pol)?.NameEn;
                model.PodName = places?.FirstOrDefault(x => x.Id == model.Pod)?.NameEn;
                var Conts = containerRepository.Get();
                IQueryable<CsMawbcontainer> listConts = null;
                listConts = Conts.Where(x => model.CsTransactionDetails.Select(t => t.Id.ToString()).Contains(x.Hblid.ToString()));
                if (!listConts.Any())
                {
                    listConts = Conts.Where(x => model.CsTransactionDetails.Select(t => t.JobId.ToString()).Contains(x.Mblid.ToString()));
                }
                if (!listConts.Any()) return null;
                Crystal result = new Crystal();
                var instructions = new List<SeaShippingInstruction>();
                var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();
                string jobNo = opsTrans?.JobNo;
                var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
                string Tel = GetTelPersonalIncharge(id);

                var listCont = csMawbcontainerService.GetContainerListByJobId(model.JobId);
                var shippingMarkSI = model.CsTransactionDetails.Where(x => x.ShippingMark != null && x.ShippingMark != String.Empty).Any() ? string.Join(" ", model.CsTransactionDetails.Select(x => x.ShippingMark)) : null;
                var packagesType = model.CsTransactionDetails?.FirstOrDefault()?.PackageType;
                var totalCont = listCont.Any() ? String.Join("\n", listCont.Select(x => x.Quantity + "x" + x.ContainerTypeName + " CONT")) : DocumentConstants.NO_CONTAINER;

                var parameter = new SeaShippingInstructionParameter
                {
                    CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                    CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                    CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                    CompanyDescription = string.Empty,
                    Contact = model.IssuedUserName ?? string.Empty,
                    Tel = Tel ?? string.Empty,
                    Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                    DecimalNo = 2
                };
                foreach (var item in listConts)
                {
                    var Contype = catUnitRepository.Get(x => x.Id == item.ContainerTypeId).Select(t => t.UnitNameEn)?.FirstOrDefault();
                    var PackageType = catUnitRepository.Get(x => x.Id == item.PackageTypeId).Select(t => t.UnitNameEn)?.FirstOrDefault();
                    var instruction = new SeaShippingInstruction
                    {
                        TRANSID = jobNo,
                        Attn = model.InvoiceNoticeRecevier,
                        ToPartner = model.SupplierName,
                        Re = model.BookingNo,
                        DatePackage = model.InvoiceDate,
                        ShipperDf = model.Shipper,
                        GoodsDelivery = model.ConsigneeDescription,
                        NotitfyParty = model.CargoNoticeRecevier,
                        PortofLoading = model.PolName,
                        PortofDischarge = model.PodName,
                        PlaceDelivery = model.PoDelivery,
                        Vessel = model.VoyNo,
                        Etd = model.LoadingDate?.ToString("dd/MM/yyyy"),
                        ShippingMarks = string.Empty,
                        Containers = Contype + (!string.IsNullOrEmpty(item.ContainerNo) ? "/" + item.ContainerNo : string.Empty) + (!string.IsNullOrEmpty(item.SealNo) ? "/" + item.SealNo : string.Empty),
                        NoofPeace = item.PackageQuantity + " " + PackageType,
                        SIDescription = transactionDetailService.Get().Where(x => x.Id == item.Hblid).Select(t => t.DesOfGoods)?.FirstOrDefault(),
                        GrossWeight = item.Gw,
                        CBM = item.Cbm,
                        Qty = item.Quantity?.ToString(),
                        RateRequest = model.Remark,
                        Payment = model.PaymenType,
                        ShippingMarkImport = string.Empty,
                        MaskNos = string.Empty
                    };
                    instructions.Add(instruction);
                }
                parameter.TotalPackages = listConts.Sum(t => t.PackageQuantity)?.ToString();//+ " PKG(S)"; CR: 15585 [31/03/2021]
                result = new Crystal
                {
                    ReportName = "SeaShippingInstructionCont.rpt",
                    AllowPrint = true,
                    AllowExport = true
                };
                // Get path link to report
                CrystalEx._apiUrl = apiUrl.Value.Url;
                string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
                var reportName = "SeaShippingInstructionFCLCont" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".pdf";
                var _pathReportGenerate = folderDownloadReport + "/" + reportName;
                result.PathReportGenerate = _pathReportGenerate;

                result.AddDataSource(instructions);
                result.FormatType = ExportFormatType.PortableDocFormat;
                result.SetParameter(parameter);
                return result;
            }
            else
            {
                return null;
            }
        }

        public Crystal PreviewLCLContShippingInstruction(Guid id)
        {
            var dataShippingIntruction = DataContext.Get(x => x.JobId == id).FirstOrDefault();

            if (dataShippingIntruction == null) return null;
            var model = mapper.Map<CsShippingInstructionReportConstModel>(dataShippingIntruction);
            model.CsTransactionDetails = transactionDetailService.Get(x => x.JobId == id).ToList();
            var listCont = csMawbcontainerService.GetContainerListByJobId(model.JobId);

            if (model.CsTransactionDetails.Any())
            {
                var shippingMarkSI = model.ShippingMark ?? (model.CsTransactionDetails.Where(x => x.ShippingMark != null && x.ShippingMark != String.Empty).Any() ? string.Join(" ", model.CsTransactionDetails.Select(x => x.ShippingMark)) : null);
                var totalCont = listCont.Any() ? String.Join("\n", listCont.GroupBy(x => x.ContainerTypeName).Select(x => x.Count() + " x " + x.Key)) : DocumentConstants.NO_CONTAINER;
                var packagesType = model.PackagesType ?? model.CsTransactionDetails?.FirstOrDefault()?.PackageType;

                var partners = partnerRepository.Get();
                var places = placeRepository.Get();
                var users = userRepository.Get();
                model.IssuedUserName = users?.FirstOrDefault(x => x.Id == model.IssuedUser)?.Username;
                model.SupplierName = partners?.FirstOrDefault(x => x.Id == model.Supplier)?.PartnerNameEn;
                model.ConsigneeName = partners?.FirstOrDefault(x => x.Id == model.ConsigneeId)?.PartnerNameEn;
                model.PolName = places?.FirstOrDefault(x => x.Id == model.Pol)?.NameEn;
                model.PodName = places?.FirstOrDefault(x => x.Id == model.Pod)?.NameEn;
                var Conts = containerRepository.Get();
                var listConts = Conts.Where(x => model.CsTransactionDetails.Select(t => t.Id.ToString()).Contains(x.Hblid.ToString())).GroupBy(x => x.PackageTypeId);
                if (!listConts.Any()) return null;
                Crystal result = new Crystal();
                var instructions = new List<SeaShippingInstruction>();
                var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();
                string jobNo = opsTrans?.JobNo;
                var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
                string Tel = GetTelPersonalIncharge(id);
                var parameter = new SeaShippingInstructionParameter
                {
                    CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                    CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                    CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                    CompanyDescription = string.Empty,
                    Contact = model.IssuedUserName ?? string.Empty,
                    Tel = Tel ?? string.Empty,
                    Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                    DecimalNo = 2
                };
                int? totalPackages = 0;
                foreach (var item in listConts)
                {
                    var PackageType = catUnitRepository.Get(x => x.Id == item.Key).Select(t => t.UnitNameEn).FirstOrDefault();
                    var instruction = new SeaShippingInstruction
                    {
                        TRANSID = jobNo,
                        Attn = model.InvoiceNoticeRecevier,
                        ToPartner = model.SupplierName,
                        Re = model.BookingNo,
                        DatePackage = model.InvoiceDate,
                        ShipperDf = model.Shipper,
                        GoodsDelivery = model.ConsigneeDescription,
                        NotitfyParty = model.CargoNoticeRecevier,
                        PortofLoading = model.PolName,
                        PortofDischarge = model.PodName,
                        PlaceDelivery = model.PoDelivery,
                        Vessel = model.VoyNo,
                        Etd = model.LoadingDate?.ToString("dd/MM/yyyy"),
                        ShippingMarks = string.Empty,
                        Containers = "A Part Of Container",
                        NoofPeace = item.Sum(t => t.PackageQuantity) + " " + PackageType,
                        SIDescription = model.GoodsDescription,
                        GrossWeight = item.Sum(t => t.Gw),
                        CBM = item.Sum(t => t.Cbm),
                        Qty = string.Empty,
                        RateRequest = model.Remark,
                        Payment = model.PaymenType,
                        ShippingMarkImport = string.Empty,
                        MaskNos = string.Empty,
                        TotalCont = totalCont,
                        ShippingMarkSI = shippingMarkSI,
                        PKGType = packagesType,
                    };
                    totalPackages += item.Sum(t => t.PackageQuantity);
                    instructions.Add(instruction);
                }
                parameter.TotalPackages = totalPackages?.ToString(); //+ " PKG(S)"; CR: 15585 [31/03/2021]
                result = new Crystal
                {
                    ReportName = "SeaShippingInstructionCont.rpt",
                    AllowPrint = true,
                    AllowExport = true
                };
                // Get path link to report
                CrystalEx._apiUrl = apiUrl.Value.Url;
                string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
                var reportName = "SeaShippingInstructionLCLCont" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".pdf";
                var _pathReportGenerate = folderDownloadReport + "/" + reportName;
                result.PathReportGenerate = _pathReportGenerate;

                result.AddDataSource(instructions);
                result.FormatType = ExportFormatType.PortableDocFormat;
                result.SetParameter(parameter);
                return result;
            }
            else
            {
                return null;
            }
        }

        private string GetUnitNameById(short? id)
        {
            var result = string.Empty;
            var data = catUnitRepository.Get(g => g.Id == id).FirstOrDefault();
            result = (data != null) ? data.UnitNameEn : string.Empty;
            return result;
        }

        public Crystal PreviewOCL(CsShippingInstructionReportModel model)
        {
            Crystal result = new Crystal();
            var shippingInstructions = new List<OnBoardContainerReportResult>();

            var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();
            var office = officeRepository.Get(x => x.Id == opsTrans.OfficeId).FirstOrDefault();
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty, //office?.DescriptionEn ?? string.Empty,
                Contact = model.IssuedUserName,
                Tel = office?.Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2,
                TotalPackages = string.Empty
            };
            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            foreach (var item in model.CsTransactionDetails)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity != null ? quantity : 0);
                string noPieces = string.Empty;
                if (item.PackageQty != null && item.PackageQty != 0 && item.PackageType != null && item.PackageType != 0)
                {
                    var packageType = unitRepository.Get(x => x.Id == item.PackageType)?.FirstOrDefault();
                    noPieces = noPieces + item.PackageQty + " " + packageType.UnitNameEn;
                }
                var container = new OnBoardContainerReportResult
                {
                    TRANSID = model.BookingNo,
                    ShippingMarkImport = string.Empty,
                    CheckAttachNull = string.Empty,
                    DatePackage = model.InvoiceDate,
                    ToPartner = model.SupplierName,
                    Attn = model.InvoiceNoticeRecevier,
                    Notify = model.InvoiceNoticeRecevier,
                    Re = model.BookingNo,
                    ShipperDf = model.ActualShipperDescription,
                    GoodsDelivery = model.ConsigneeDescription,
                    RealShipper = string.Empty,
                    RealConsignee = string.Empty,
                    PortofLoading = model.PolName,
                    PortofDischarge = model.PodName,
                    PlaceDelivery = model.PoDelivery,
                    Vessel = model.VoyNo,
                    ContSealNo = item.ContSealNo,
                    Etd = model.LoadingDate?.ToString("dd/MM/yyyy"),
                    ShippingMarks = item.ShippingMark,
                    RateRequest = model.Remark,
                    Payment = model.PaymenType,
                    NoofPeace = noPieces,
                    Containers = item.ContSealNo,
                    MaskNos = item.ContSealNo,
                    SIDescription = item.DesOfGoods,
                    GrossWeight = item.GW,
                    CBM = item.CBM,
                    SeaLCL = false,
                    SeaFCL = true,
                    NotifyParty = model.CargoNoticeRecevier,
                    CTNS = string.Empty,
                    Measurement = string.Empty,
                    Qty = total.ToString()
                };
                shippingInstructions.Add(container);
            }

            result = new Crystal
            {
                ReportName = "SeaOnboardContainerList.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(shippingInstructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewFCLShippingInstructionByJobId(Guid jobId)
        {
            var si = GetById(jobId);
            if (si == null)
            {
                return null;
            }
            Crystal result = new Crystal();

            var instructions = new List<SeaShippingInstruction>();
            var opsTrans = cstransRepository.Get(x => x.Id == jobId).FirstOrDefault();
            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            var issueBy = userRepository.Get(x => x.Id == si.IssuedUser).FirstOrDefault()?.Username;
            string Tel = GetTelPersonalIncharge(jobId);
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = issueBy ?? string.Empty,
                Tel = Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2
            };
            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            var housebills = transactionDetailService.Query(criteria);
            if (housebills == null)
            {
                return null;
            }
            var total = 0;
            int totalPackage = 0;
            string noPieces = string.Empty;
            var listCont = csMawbcontainerService.GetContainerListByJobId(jobId);
            var shippingMarkSI = housebills.Where(x => x.ShippingMark != null && x.ShippingMark != String.Empty).Any() ? string.Join(" ", housebills.Select(x => x.ShippingMark)) : null;
            var packagesType = housebills?.FirstOrDefault()?.PackageType;
            var totalCont = listCont.Any() ? String.Join("\n", listCont.Select(x => x.Quantity + "x" + x.ContainerTypeName + " CONT")) : DocumentConstants.NO_CONTAINER;

            foreach (var item in housebills)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity ?? 0);
                int? totalPack = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.PackageQuantity);
                totalPackage += (int)(totalPack ?? 0);

                var packages = containerRepository.Get(x => x.Hblid == item.Id).GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key));
                noPieces = string.Join(", ", packages);

                var instruction = new SeaShippingInstruction
                {
                    TRANSID = opsTrans?.JobNo,
                    Attn = si.InvoiceNoticeRecevier,
                    ToPartner = si.SupplierName,
                    Re = si.BookingNo,
                    DatePackage = si.InvoiceDate,
                    ShipperDf = si.Shipper,
                    GoodsDelivery = si.ConsigneeDescription,
                    NotitfyParty = si.CargoNoticeRecevier,
                    PortofLoading = si.PolName,
                    PortofDischarge = si.PodName,
                    PlaceDelivery = si.PoDelivery,
                    Vessel = si.VoyNo,
                    Etd = si.LoadingDate?.ToString("dd/MM/yyyy"),
                    ShippingMarks = item.ShippingMark,
                    Containers = item.Packages,
                    // ContSealNo = item.SealNo,
                    NoofPeace = noPieces,
                    SIDescription = item.DesOfGoods,
                    GrossWeight = item.GW,
                    CBM = item.CBM,
                    Qty = total.ToString(),
                    RateRequest = si.Remark,
                    Payment = si.PaymenType,
                    ShippingMarkImport = string.Empty,
                    MaskNos = item.ContSealNo
                };
                instructions.Add(instruction);
            }
            if (housebills.Count > 1)
            {
                parameter.TotalPackages = totalPackage + " PKG(S)";
            }
            else
            {
                parameter.TotalPackages = instructions.FirstOrDefault()?.Containers;
            }

            result = new Crystal
            {
                ReportName = "SeaShippingInstructionNew.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            // Get path link to report
            CrystalEx._apiUrl = apiUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = "SeaShippingInstruction" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName;
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(instructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);

            return result;
        }

        public Crystal PreviewSISummary(CsShippingInstructionReportModel model)
        {
            Crystal result = new Crystal();

            var listCont = csMawbcontainerService.GetContainerListByJobId(model.JobId);
            var instructions = new List<SeaShippingInstruction>();

            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();

            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            string Tel = GetTelPersonalIncharge(model.JobId);
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = model.IssuedUserName ?? string.Empty,
                Tel = Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2
            };
            int totalPackage = 0;
            string jobNo = opsTrans?.JobNo;
            foreach (var item in model.CsTransactionDetails)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity ?? 0);

                int? totalPack = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.PackageQuantity);
                totalPackage += (int)(totalPack ?? 0);
            }
            var instruction = new SeaShippingInstruction
            {
                TRANSID = jobNo,
                Attn = model.InvoiceNoticeRecevier,
                ToPartner = model.SupplierName,
                Re = model.BookingNo,
                DatePackage = model.InvoiceDate,
                ShipperDf = model.Shipper,
                GoodsDelivery = model.ConsigneeDescription,
                NotitfyParty = model.CargoNoticeRecevier,
                PortofLoading = model.PolName,
                PortofDischarge = model.PodName,
                PlaceDelivery = model.PoDelivery,
                Vessel = model.VoyNo,
                Etd = model.LoadingDate?.ToString("dd/MM/yyyy"),
                ShippingMarks = string.Empty,
                Containers = model.ContainerNote,
                NoofPeace = model.PackagesNote,
                SIDescription = model.GoodsDescription,
                GrossWeight = model.GrossWeight,
                CBM = model.Volume,
                Qty = total.ToString(),
                RateRequest = model.Remark,
                Payment = model.PaymenType,
                ShippingMarkImport = string.Empty,
                MaskNos = model.ContainerSealNo,
                ShippingMarkSI = model.ShippingMark ?? (model.CsTransactionDetails.Where(x => x.ShippingMark != null && x.ShippingMark != String.Empty).Any() ? string.Join(" ", model.CsTransactionDetails.Select(x => x.ShippingMark)) : null),
                TotalCont = listCont.Any() ? String.Join("\n", listCont.GroupBy(x => x.ContainerTypeName).Select(x => x.Count() + " x " + x.Key)) : DocumentConstants.NO_CONTAINER,
                PKGType = model.PackagesType ?? model.CsTransactionDetails?.FirstOrDefault()?.PackageType
            };

            instructions.Add(instruction);
            parameter.TotalPackages = totalPackage.ToString(); //+ " PKG(S)"; CR: 15585 [31/03/2021]
            result = new Crystal
            {
                ReportName = "SeaShippingInstructionSummary.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(instructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);

            return result;
        }

        public Crystal PreviewSISummaryByJobId(Guid jobId)
        {
            var si = GetById(jobId);
            if (si == null)
            {
                return null;
            }
            Crystal result = new Crystal();
            var instructions = new List<SeaShippingInstruction>();
            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            var housebills = transactionDetailService.Query(criteria);
            if (housebills == null) return result;
            var total = 0;
            var opsTrans = cstransRepository.Get(x => x.Id == jobId).FirstOrDefault();

            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            var issueBy = userRepository.Get(x => x.Id == si.IssuedUser).FirstOrDefault()?.Username;
            string Tel = GetTelPersonalIncharge(jobId);
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = issueBy ?? string.Empty,
                Tel = Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2
            };
            int totalPackage = 0;
            string jobNo = opsTrans?.JobNo;
            foreach (var item in housebills)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity ?? 0);

                int? totalPack = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.PackageQuantity);
                totalPackage += (int)(totalPack ?? 0);
            }
            var instruction = new SeaShippingInstruction
            {
                TRANSID = jobNo,
                Attn = si.InvoiceNoticeRecevier,
                ToPartner = si.SupplierName,
                Re = si.BookingNo,
                DatePackage = si.InvoiceDate,
                ShipperDf = si.Shipper,
                GoodsDelivery = si.ConsigneeDescription,
                NotitfyParty = si.CargoNoticeRecevier,
                PortofLoading = si.PolName,
                PortofDischarge = si.PodName,
                PlaceDelivery = si.PoDelivery,
                Vessel = si.VoyNo,
                Etd = si.LoadingDate?.ToString("dd/MM/yyyy"),
                ShippingMarks = string.Empty,
                Containers = si.ContainerNote,
                // ContSealNo = item.SealNo,
                NoofPeace = si.PackagesNote,
                SIDescription = si.GoodsDescription,
                GrossWeight = si.GrossWeight,
                CBM = si.Volume,
                Qty = total.ToString(),
                RateRequest = si.Remark,
                Payment = si.PaymenType,
                ShippingMarkImport = string.Empty,
                MaskNos = si.ContainerSealNo
            };
            instructions.Add(instruction);
            parameter.TotalPackages = totalPackage + " PKG(S)";
            result = new Crystal
            {
                ReportName = "SeaShippingInstructionSummary.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            // Get path link to report
            CrystalEx._apiUrl = apiUrl.Value.Url;
            string folderDownloadReport = CrystalEx.GetLinkDownloadReports();
            var reportName = "SeaShippingInstructionSummary" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".pdf";
            var _pathReportGenerate = folderDownloadReport + "/" + reportName;
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(instructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        private string GetTelPersonalIncharge(Guid jobId)
        {
            var CreatorJob = cstransRepository.Get(x => x.Id == jobId).Select(t => t.UserCreated)?.FirstOrDefault();
            string EmployeeId = userRepository.Get(x => x.Id == CreatorJob).Select(t => t.EmployeeId)?.FirstOrDefault();
            string Tel = employeeRepository.Get(x => x.Id == EmployeeId).Select(t => t.Tel)?.FirstOrDefault();
            return Tel;
        }

        /// <summary>
        /// Check if Exist Sea Export files attach
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public bool[] CheckExistSIExport(Guid jobId)
        {
            var existList = new bool[3];
            var result = DataContext.Get(x => x.JobId == jobId).FirstOrDefault();
            if (result != null)
            {
                CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
                var housebills = transactionDetailService.Query(criteria);
                if (housebills != null)
                {
                    existList[0] = true;
                    existList[1] = true;
                    var Conts = containerRepository.Get();
                    IQueryable<CsMawbcontainer> listConts = null;
                    listConts = Conts.Where(x => housebills.Select(t => t.Id.ToString()).Contains(x.Hblid.ToString()));
                    if (!listConts.Any())
                    {
                        listConts = Conts.Where(x => housebills.Select(t => t.JobId.ToString()).Contains(x.Mblid.ToString()));
                    }
                    if (listConts.Any())
                    {
                        existList[2] = true;
                    }
                }
            }
            return existList;
        }
    }
}
