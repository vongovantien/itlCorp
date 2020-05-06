using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
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
        private readonly IContextBase<OpsTransaction> opstransRepository;
        public CsShippingInstructionService(IContextBase<CsShippingInstruction> repository, 
            IMapper mapper,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<CatPlace> placeRepo,
            IContextBase<SysUser> userRepo,
            IContextBase<CsMawbcontainer> containerRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<OpsTransaction> opstransRepo) : base(repository, mapper)
        {
            partnerRepository = partnerRepo;
            placeRepository = placeRepo;
            userRepository = userRepo;
            containerRepository = containerRepo;
            unitRepository = unitRepo;
            opstransRepository = opstransRepo;
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
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = "itl company",
                Contact = model.IssuedUserName,
                Tel = string.Empty,
                Website = string.Empty,
                DecimalNo = 2
            };
            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            string jobNo = opstransRepository.Get(x => x.Id == model.JobId).FirstOrDefault()?.JobNo;
            foreach (var item in model.CsTransactionDetails)
            {
                int? quantity = containerRepository.Get(x => x.Hblid == item.Id).Sum(x => x.Quantity);
                total += (int)(quantity != null?quantity: 0);
                string noPieces = string.Empty;
                if(item.PackageQty != null && item.PackageQty != 0 && item.PackageType != null && item.PackageType != 0)
                {
                    var packageType = unitRepository.Get(x => x.Id == item.PackageType)?.FirstOrDefault();
                    noPieces = noPieces + item.PackageQty + " " + packageType.UnitNameEn;
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

        public Crystal PreviewOCL(CsShippingInstructionReportModel model)
        {
            Crystal result = new Crystal();
            var shippingInstructions = new List<OnBoardContainerReportResult>();
            var parameter = new SeaShippingInstructionParameter
            {
                CompanyName = DocumentConstants.COMPANY_NAME,
                CompanyAddress1 = DocumentConstants.COMPANY_ADDRESS1,
                CompanyAddress2 = DocumentConstants.COMPANY_ADDRESS2,
                CompanyDescription = "itl company",
                Contact = model.IssuedUserName,
                Tel = string.Empty,
                Website = string.Empty,
                DecimalNo = 2
            };
            if (model.CsTransactionDetails == null) return result;
            var total = 0;
            foreach (var item in model.CsTransactionDetails)
            {
                total = (int)(total + containerRepository.Get(x => x.Id == item.Id).FirstOrDefault()?.Quantity);
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
                    MaskNos = item.ShippingMark,
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
    }
}
