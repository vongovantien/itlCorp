using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Provider.Services.IService;
using eFMS.IdentityServer.DL.UserManager;
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
        Task<IQueryable<CsWorkOrder>> QueryAsync(WorkOrderCriteria criteria, ICurrentUser currenUser, PermissionRange range);
        Task<ResponsePagingModel<CsWorkOrderViewModel>> PagingAsync(WorkOrderCriteria criteria, int page, int size);
        Task<HandleState> SaveWorkOrder(WorkOrderRequest model);
        Task<HandleState> UpdateWorkOrder(WorkOrderRequest model);
        Task<HandleState> Delete(Guid id);
        Task<HandleState> DeletePrice(Guid id);
        Task<HandleState> SetActiveInactive(ActiveInactiveRequest request);
        CsWorkOrderViewUpdateModel GetById(Guid id);
        bool CheckExist(WorkOrderRequest model);
    }
}
