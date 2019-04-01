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
    }
}
