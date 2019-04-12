using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ITL.NetCore.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Models.ReportResults;

namespace eFMS.API.Documentation.DL.Services
{
    public class CsShippingInstructionService : RepositoryBase<CsShippingInstruction, CsShippingInstructionModel>, ICsShippingInstructionService
    {
        public CsShippingInstructionService(IContextBase<CsShippingInstruction> repository, IMapper mapper) : base(repository, mapper)
        {
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
            var query = (from shipping in ((eFMSDataContext)DataContext.DC).CsShippingInstruction
                         where shipping.JobId == jobId
                         join user in ((eFMSDataContext)DataContext.DC).SysUser on shipping.IssuedUser equals user.Id
                         join supplier in ((eFMSDataContext)DataContext.DC).CatPartner on shipping.Supplier equals supplier.Id
                         join consignee in ((eFMSDataContext)DataContext.DC).CatPartner on shipping.ConsigneeId equals consignee.Id
                         join pod in ((eFMSDataContext)DataContext.DC).CatPlace on shipping.Pod equals pod.Id
                         join pol in ((eFMSDataContext)DataContext.DC).CatPlace on shipping.Pol equals pol.Id
                         join shipper in ((eFMSDataContext)DataContext.DC).CatPartner on shipping.ActualShipperId equals shipper.Id into grpShipper
                         from actualShipper in grpShipper.DefaultIfEmpty()
                         join realConsignee in ((eFMSDataContext)DataContext.DC).CatPartner on shipping.ActualConsigneeId equals realConsignee.Id into grpConsignee
                         from actualConsignee in grpConsignee.DefaultIfEmpty()
                         select new
                         {
                             shipping,
                             IssuedUserName = user.Username,
                             SupplierName = supplier.PartnerNameEn,
                             ConsigneeName = consignee.PartnerNameEn,
                             PolName = pol.NameEn,
                             PodName = pod.NameEn,
                             ActualConsigneeName = actualConsignee.PartnerNameEn,
                             ActualShipperName = actualShipper.PartnerNameEn
                         }).FirstOrDefault();
            if (query == null) return null;
            var result = mapper.Map<CsShippingInstructionModel>(query.shipping);
            result.IssuedUserName = query.IssuedUserName;
            result.SupplierName = query.SupplierName;
            result.ConsigneeName = query.ConsigneeName;
            result.PolName = query.PolName;
            result.PodName = query.PodName;
            result.ActualShipperName = query.ActualShipperName;
            result.ActualConsigneeName = query.ActualConsigneeName;
            return result;
        }

        public Crystal PreviewFCLShippingInstruction(CsShippingInstructionReportModel model)
        {
            Crystal result = new Crystal();
            var contHouseBills = new List<ShippingInstructionContainer>();
            List<ContainerObject> listContainerTypes = new List<ContainerObject>();
            List<ContainerObject> listPackageTypes = new List<ContainerObject>();
            if (model.CsTransactionDetails != null)
            {
                decimal? sumGW = 0;
                decimal? sumCBM = 0;
                foreach (var transactionDetail in model.CsTransactionDetails)
                {
                    var item = new ShippingInstructionContainer();
                    if (transactionDetail.CsMawbcontainers != null)
                    {
                        item.ContainerSealNo = string.Empty;
                        item.PackagesNote = string.Empty;
                        foreach (var container in transactionDetail.CsMawbcontainers)
                        {
                            listContainerTypes.Add(new ContainerObject { Quantity = (int)container.Quantity, Name = container.ContainerTypeName });
                            item.ContainerSealNo += container.Quantity + "X" + container.ContainerTypeName + " ";
                            if (!string.IsNullOrEmpty(container.ContainerNo) && !string.IsNullOrEmpty(container.SealNo))
                            {
                                item.ContainerSealNo += container.ContainerNo + "/" + item.ContainerSealNo + ", ";
                            }
                            if (container.PackageQuantity != null && container.PackageTypeId != null)
                            {
                                item.PackagesNote += container.PackageQuantity + " " + container.PackageTypeName;
                                listPackageTypes.Add(new ContainerObject { Quantity = (int)container.PackageQuantity, Name = container.PackageTypeName });
                            }
                            item.DesOfGoods = string.Join(",", transactionDetail.CsMawbcontainers.Select(x => x.Description));
                            item.GW = transactionDetail.CsMawbcontainers.Sum(x => x.Gw);
                            item.CBM = transactionDetail.CsMawbcontainers.Sum(x => x.Cbm);
                            sumCBM += item.CBM;
                            sumGW += item.GW;
                        }
                    }
                    contHouseBills.Add(item);
                }
                if(contHouseBills.Count > 0)
                {
                    contHouseBills.ForEach(x => {
                        x.SumGrossWeight = sumGW;
                        x.SumVolume = sumCBM;
                    });
                }
            }
            var s = listContainerTypes.GroupBy(x => new { x.Quantity, x.Name });
            var t = listPackageTypes.GroupBy(x => new { x.Quantity, x.Name });
            var shippingIns = new List<ShippingInstructionReportResult>();
            var si = new ShippingInstructionReportResult
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
                PaymenType = model.PaymenType
            };
            shippingIns.Add(si);
            result.ReportName = "rptShippingInstruction.rpt";
            result.AddDataSource(shippingIns);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.AddSubReport("ShippingInstructionContainerList", contHouseBills);
            return result;
        }

        public Crystal PreviewOCL(CsShippingInstructionReportModel model)
        {
            var result = new Crystal();
            var oclList = new List<OnBoardContainerReportResult>
                {
                    new OnBoardContainerReportResult {
                        POD = model.PodName,
                        POL = model.PolName,
                        VoyNo = model.VoyNo,
                        FDestination = model.PoDelivery
                    }
                };
            var oclContainerList = new List<OCLContainerReportResult>();
            if (model.CsMawbcontainers != null)
            {
                int i = 0;
                foreach (var container in model.CsMawbcontainers)
                {
                    i = i + 1;
                    decimal? cbm = container.Cbm;
                    decimal? gw = container.Gw;
                    var item = new OCLContainerReportResult
                    {
                        STT = i,
                        CBM = cbm != null ? (decimal)cbm : 0,
                        GW = gw != null ? (decimal)gw : 0,
                        ContainerNo = container.ContainerNo,
                        SealNo = container.SealNo
                    };
                    oclContainerList.Add(item);
                }
            }
            result.ReportName = "rptOnBoardContainerList.rpt";
            result.AddDataSource(oclList);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.AddSubReport("OnBoardContainerList", oclContainerList);
            return result;
        }
    }
}
