using AutoMapper;
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
        readonly ICsTransactionDetailService transactionDetailService;

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
            ICsTransactionDetailService transDetailService) : base(repository, mapper)
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
        }

        public HandleState AddOrUpdate(CsShippingInstructionModel model)
        {
            var result = new HandleState();
            var modelUpdate = mapper.Map<CsShippingInstruction>(model);
            if(DataContext.Any(x => x.JobId == model.JobId))
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
            return result;
        }
        public Crystal PreviewFCLShippingInstruction(CsShippingInstructionReportModel model)
        {
            Crystal result = new Crystal();
            var instructions = new List<SeaShippingInstruction>();
            var units = unitRepository.Get();
            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            int totalPackage = 0;
            var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();

            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = model.IssuedUserName ?? string.Empty,
                Tel = office?.Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2
            };
            string jobNo = opsTrans?.JobNo;
            string noPieces = string.Empty;
            foreach (var item in model.CsTransactionDetails)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity ?? 0);
                int? totalPack = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.PackageQuantity);
                totalPackage += (int)(totalPack ?? 0);

                var packages = containerRepository.Get(x => x.Hblid == item.Id).GroupBy(x => x.PackageTypeId).Select(x => x.Sum(c => c.PackageQuantity) + " " + GetUnitNameById(x.Key));
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
                    // ContSealNo = item.SealNo,
                    NoofPeace = noPieces,
                    SIDescription = item.DesOfGoods,
                    GrossWeight = item.GW,
                    CBM = item.CBM,
                    Qty = total.ToString(),
                    RateRequest = model.Remark,
                    Payment = model.PaymenType,
                    ShippingMarkImport = string.Empty,
                    MaskNos = item.ContSealNo
                };
                instructions.Add(instruction);
            }
            if(model.CsTransactionDetails.Count > 1)
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
            result.AddDataSource(instructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
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
            var units = unitRepository.Get();
            var opsTrans = cstransRepository.Get(x => x.Id == jobId).FirstOrDefault();
            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            var issueBy = userRepository.Get(x => x.Id == si.IssuedUser).FirstOrDefault()?.Username;
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = issueBy ?? string.Empty,
                Tel = office?.Tel ?? string.Empty,
                Website = office?.Website ?? DocumentConstants.COMPANY_WEBSITE,
                DecimalNo = 2
            };
            CsTransactionDetailCriteria criteria = new CsTransactionDetailCriteria { JobId = jobId };
            var housebills = transactionDetailService.Query(criteria);
            var total = 0;
            int totalPackage = 0;
            string noPieces = string.Empty;
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
            string folderDownloadReport = CrystalEx.GetFolderDownloadReports();
            var _pathReportGenerate = folderDownloadReport + "\\SeaShippingInstruction" + DateTime.Now.ToString("ddMMyyHHssmm") + ".pdf";
            result.PathReportGenerate = _pathReportGenerate;

            result.AddDataSource(instructions);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public Crystal PreviewSISummary(CsShippingInstructionReportModel model)
        {

            Crystal result = new Crystal();
            var instructions = new List<SeaShippingInstruction>();
            var units = unitRepository.Get();
            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            var opsTrans = cstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault();

            var office = officeRepository.Get(x => x.Id == opsTrans.CompanyId).FirstOrDefault();
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = (office?.BranchNameEn) ?? DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = office?.AddressEn ?? DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = office?.AddressVn ?? DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = string.Empty,
                Contact = model.IssuedUserName ?? string.Empty,
                Tel = office?.Tel ?? string.Empty,
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
                // ContSealNo = item.SealNo,
                NoofPeace = model.PackagesNote,
                SIDescription = model.GoodsDescription,
                GrossWeight = model.GrossWeight,
                CBM = model.Volume,
                Qty = total.ToString(),
                RateRequest = model.Remark,
                Payment = model.PaymenType,
                ShippingMarkImport = string.Empty,
                MaskNos = model.ContainerSealNo
            };
            instructions.Add(instruction);
            parameter.TotalPackages = totalPackage + " PKG(S)";
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
    }
}
