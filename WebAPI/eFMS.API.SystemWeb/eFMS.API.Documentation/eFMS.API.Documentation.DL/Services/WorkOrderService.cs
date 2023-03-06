using AutoMapper;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Shipment.Service.Contexts;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.Services
{
    public class WorkOrderService : RepositoryBase<CsWorkOrder, CsWorkOrderModel>, IWorkOrderService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CsWorkOrderPrice> workOrderPriceRepo;
        private readonly IContextBase<CsWorkOrderSurcharge> workOrderSurchargeRepo;
        private readonly IContextBase<CatCharge> catChargeRepo;
        private readonly IContextBase<SysUser> sysUserRepo;
        private readonly IContextBase<CatPartner> catPartnerRepo;
        private readonly IContextBase<CatPlace> catPlaceRepo;
        public WorkOrderService(IOptions<Settings> settings,
            IStringLocalizer<LanguageSub> localizer,
            IContextBase<CsWorkOrder> repository,
            IMapper mapper,
            ICurrentUser currUser,
            IContextBase<CsWorkOrderPrice> workOrderPrice,
            IContextBase<CsWorkOrderSurcharge> workOrderSurcharge,
            IContextBase<CatCharge> catCharge,
            IContextBase<SysUser> sysUser,
            IContextBase<CatPartner> catPartner,
            IContextBase<CatPlace> catPlace
            ) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = currUser;
            workOrderPriceRepo = workOrderPrice;
            workOrderSurchargeRepo = workOrderSurcharge;
            catChargeRepo = catCharge;
            sysUserRepo = sysUser;
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            throw new NotImplementedException();
        }

        public HandleState Delete(Guid id)
        {
            throw new NotImplementedException();
        }

        public CsWorkOrderModel GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        private IQueryable<CsWorkOrderViewModel> FormatWorkOrder(IQueryable<CsWorkOrder> dataQuery)
        {
            List<CsWorkOrderViewModel> list = new List<CsWorkOrderViewModel>();

            if (dataQuery != null && dataQuery.Count() > 0)
            {
                foreach (var item in dataQuery)
                {
                    CsWorkOrderViewModel d = mapper.Map<CsWorkOrderViewModel>(item);

                    d.UserNameCreated = item.UserCreated == null ? null : sysUserRepo.Get(u => u.Id == item.UserCreated).FirstOrDefault().Username;
                    d.UserNameModified = item.UserModified == null ? null : sysUserRepo.Get(u => u.Id == item.UserModified).FirstOrDefault().Username;
                    d.PartnerName = item.PartnerId == null ? null : catPartnerRepo.Get(x => x.Id == item.PartnerId.ToString())?.FirstOrDefault()?.ShortName;
                    d.SalesmanName = item.SalesmanId == null ? null : sysUserRepo.Get(x => x.Id == item.SalesmanId.ToString())?.FirstOrDefault()?.Username;
                    d.PolCode = item.Pol == null ? null : catPlaceRepo.Get(x => x.Id == item.Pol)?.FirstOrDefault()?.Code;
                    d.PodCode = item.Pod == null ? null : catPlaceRepo.Get(x => x.Id == item.Pod)?.FirstOrDefault()?.Code;
                    d.Service = GetTypeFromData.GetTranctionTypeName(item.TransactionType);

                    list.Add(d);
                }
            }
            return list.AsQueryable();
        }

        public IQueryable<CsWorkOrder> QueryByPermission(IQueryable<CsWorkOrder> data, PermissionRange range, ICurrentUser currentUser)
        {
            switch (range)
            {
                case PermissionRange.None:
                    data = null;
                    break;
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    data = data.Where(x => x.UserCreated == currentUser.UserID);
                    break;
                case PermissionRange.Group:
                    data = data.Where(x => x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Department:
                    data = data.Where(x => x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Office:
                    data = data.Where(x => x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID);
                    break;
                case PermissionRange.Company:
                    data = data.Where(x => x.CompanyId == currentUser.CompanyID);
                    break;
            }
            return data;
        }

        public async Task<ResponsePagingModel<CsWorkOrderViewModel>> PagingAsync(WorkOrderCriteria criteria, int page, int size)
        {
            IQueryable<CsWorkOrder> data = await QueryAsync(criteria);

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialWorkOrder);
            PermissionRange permissionRangeList = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            data = QueryByPermission(data, permissionRangeList, _user);
            var rowsCount = data.Count();

            if (page == 0)
            {
                page = 1;
                size = rowsCount;
            }

            var result = FormatWorkOrder(data.Skip((page - 1) * size).Take(size));

            return new ResponsePagingModel<CsWorkOrderViewModel> {
                Data = result,
                Page = page,
                Size = size,
                TotalItems = rowsCount
            };
        }

        public async Task<IQueryable<CsWorkOrder>> QueryAsync(WorkOrderCriteria criteria)
        {
            Expression<Func<CsWorkOrder, bool>> query = x => true;
            if(!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(x => x.PartnerId.ToString() == criteria.PartnerId);
            }
            if (!string.IsNullOrEmpty(criteria.SalesmamId))
            {
                query = query.And(x => x.SalesmanId.ToString() == criteria.SalesmamId);
            }
            if (!string.IsNullOrEmpty(criteria.TransactionType))
            {
                query = query.And(x => x.TransactionType == criteria.TransactionType);
            }
            if (!string.IsNullOrEmpty(criteria.POL))
            {
                query = query.And(x => x.Pol.ToString() == criteria.POL);
            }
            if (!string.IsNullOrEmpty(criteria.POL))
            {
                query = query.And(x => x.Pod.ToString() == criteria.POD);
            }
            if (!string.IsNullOrEmpty(criteria.Source))
            {
                query = query.And(x => x.Source == criteria.Source);
            }
            if (criteria.Active != null)
            {
                query = query.And(x => x.Active == criteria.Active);
            }

            var d = await DataContext.GetAsync(query);
            return d.AsQueryable();
        }

        public async Task<HandleState> SaveWorkOrder(WorkOrderRequest model)
        {
            var result = new HandleState();

            using (var trans = DC.Database.BeginTransaction())
            {
                try
                {
                    model.Id = Guid.NewGuid();
                    model.UserCreated = model.UserModified = currentUser.UserID;
                    model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                    model.GroupId = currentUser.GroupId;
                    model.DepartmentId = currentUser.DepartmentId;
                    model.OfficeId = currentUser.OfficeID;
                    model.CompanyId = currentUser.CompanyID;
                    model.Source = "eFMS";
                    model.Active = false;
                    model.Code = StringHelper.RandomString(7); // TODO
                    CsWorkOrder workOrder = mapper.Map<CsWorkOrder>(model);
                    HandleState hs = DataContext.Add(workOrder);

                    if (hs.Success)
                    {
                        var d = await AddPrice(model.ListPrice, workOrder);
                        if(d.Success)
                        {
                            trans.Commit();
                        }
                    }

                    return hs;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }

        private async Task<HandleState> AddPrice(List<CsWorkOrderPriceModel> list, CsWorkOrder workOrder)
        {
            var hs = new HandleState();
            if (list.Count() == 0)
            {
                return hs;
            }
            using (var trans = workOrderPriceRepo.DC.Database.BeginTransaction())
            {
                try
                {
                    foreach (var item in list)
                    {
                        CsWorkOrderPrice priceItem = mapper.Map<CsWorkOrderPrice>(item);
                        priceItem.Id = Guid.NewGuid();
                        priceItem.WorkOrderId = workOrder.Id;
                        priceItem.UserCreated = priceItem.UserModified = currentUser.UserID;
                        priceItem.DatetimeCreated = priceItem.DatetimeModified = DateTime.Now;

                        if(!string.IsNullOrEmpty(item.ChargeCodeBuying))
                        {
                            var charge = catChargeRepo.Get(x => x.Code == item.ChargeCodeBuying)?.FirstOrDefault();
                            priceItem.ChargeIdBuying = charge?.Id;
                        }
                        if (!string.IsNullOrEmpty(item.ChargeCodeSelling))
                        {
                            var charge = catChargeRepo.Get(x => x.Code == item.ChargeCodeSelling)?.FirstOrDefault();
                            priceItem.ChargeIdSelling = charge?.Id;
                        }
                        workOrderPriceRepo.Add(priceItem, false);
                    }
                    hs = workOrderPriceRepo.SubmitChanges();
                    if(hs.Success)
                    {
                        trans.Commit();
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
            return hs;
        }
    }
}
