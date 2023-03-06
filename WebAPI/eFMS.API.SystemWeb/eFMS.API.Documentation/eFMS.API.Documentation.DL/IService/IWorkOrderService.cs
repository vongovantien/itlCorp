using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Provider.Services.IService;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface IWorkOrderService: IRepositoryBase<CsWorkOrder, CsWorkOrderModel>,  IPermissionBaseService<CsWorkOrder>
    {
        //IQueryable<CsWorkOrder> Query(WorkOrderCriteria criteria);
        //IQueryable<CsWorkOrderModel> Paging(WorkOrderCriteria criteria, int page, int size, out int totalItem);
        Task<IQueryable<CsWorkOrder>> QueryAsync(WorkOrderCriteria criteria);
        Task<ResponsePagingModel<CsWorkOrderViewModel>> PagingAsync(WorkOrderCriteria criteria, int page, int size);
        Task<HandleState> SaveWorkOrder(WorkOrderRequest model);
        HandleState Delete(Guid id);
        CsWorkOrderModel GetById(Guid id);
    }
}
