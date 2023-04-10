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
        private readonly IContextBase<CatUnit> catUnitrepo;
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
            IContextBase<CatPlace> catPlace,
            IContextBase<CatUnit> catUnit
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
            catUnitrepo = catUnit;
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            CsWorkOrder wo = DataContext.Get(o => o.Id == id).FirstOrDefault();
            if (wo == null)
            {
                return false;
            }

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = wo.UserCreated,
                CompanyId = wo.CompanyId,
                DepartmentId = wo.DepartmentId,
                OfficeId = wo.OfficeId,
                GroupId = wo.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;
        }

        public async Task<HandleState> Delete(Guid Id)
        {
            var hs = new HandleState();
            CsWorkOrder workOrder = DataContext.Get(x => x.Id == Id).FirstOrDefault();
            if (workOrder == null)
            {
                return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
            }

            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    hs = DataContext.Delete(x => x.Id == Id);
                    if (hs.Success)
                    {
                        var hsDeletePrice = await DeletePriceWorkOrder(Id);
                        if (hsDeletePrice.Success)
                        {
                            trans.Commit();
                        }
                        else
                        {
                            string innerMes = hsDeletePrice?.Exception?.InnerException?.Message;
                            if (!string.IsNullOrEmpty(innerMes))
                            {
                                throw new Exception(innerMes);
                            }
                            else
                            {
                                throw new Exception(hsDeletePrice?.Message?.ToString());
                            }
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

        private async Task<HandleState> DeletePriceWorkOrder(Guid Id)
        {
            var hs = new HandleState();
            using (var trans = workOrderPriceRepo.DC.Database.BeginTransaction())
            {
                try
                {
                    var prices = await workOrderPriceRepo.GetAsync(x => x.WorkOrderId == Id);
                    if (prices.Count == 0)
                    {
                        return hs;
                    }

                    using (var tranSurcharge = workOrderSurchargeRepo.DC.Database.BeginTransaction())
                    {
                        var hsDeleteSurcharges = await workOrderSurchargeRepo.DeleteAsync(x => x.WorkOrderId == Id);
                        if (hsDeleteSurcharges.Success)
                        {
                            var hsDeletePrices = await workOrderPriceRepo.DeleteAsync(x => x.WorkOrderId == Id);
                            if (hsDeletePrices.Success)
                            {
                                tranSurcharge.Commit();
                                trans.Commit();
                            }
                            else
                            {
                                tranSurcharge.Rollback();
                            }
                        }
                        else
                        {
                            string innerMes = hsDeleteSurcharges?.Exception?.InnerException?.Message;
                            if (!string.IsNullOrEmpty(innerMes))
                            {
                                throw new Exception(innerMes);
                            }
                            else
                            {
                                throw new Exception(hsDeleteSurcharges?.Message?.ToString());
                            }
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

        public CsWorkOrderViewUpdateModel GetById(Guid id)
        {
            CsWorkOrder workOrder = DataContext.Get(x => x.Id == id).FirstOrDefault();
            if (workOrder == null) return new CsWorkOrderViewUpdateModel();

            CsWorkOrderViewUpdateModel workOrderViewUpdateModel = mapper.Map<CsWorkOrderViewUpdateModel>(workOrder);

            var portIds = new List<Guid?>();
            if (workOrderViewUpdateModel.Pol != null)
            {
                portIds.Add(workOrder.Pol);
            }
            if (workOrderViewUpdateModel.Pod != null)
            {
                portIds.Add(workOrder.Pod);
            }

            var ports = catPlaceRepo.Get(x => portIds.Contains(x.Id)).ToList();
            workOrderViewUpdateModel.PolName = ports.Where(x => x.Id == workOrder.Pol)?.FirstOrDefault()?.NameVn;
            workOrderViewUpdateModel.PodName = ports.Where(x => x.Id == workOrder.Pod)?.FirstOrDefault()?.NameVn;

            string partnerId = workOrder.PartnerId.ToString();
            workOrderViewUpdateModel.PartnerName = catPartnerRepo.Get(x => x.Id == partnerId)?.FirstOrDefault()?.ShortName;

            var userIds = new List<string> { workOrder.UserCreated, workOrder.UserModified, workOrder.SalesmanId?.ToString() };
            var users = sysUserRepo.Get(x => userIds.Contains(x.Id)).ToList();
            workOrderViewUpdateModel.UserNameCreated = users.Where(x => x.Id == workOrder.UserCreated)?.FirstOrDefault()?.Username;
            workOrderViewUpdateModel.UserNameModified = users.Where(x => x.Id == workOrder.UserModified)?.FirstOrDefault()?.Username;
            workOrderViewUpdateModel.SalesmanName = users.Where(x => x.Id == workOrder.SalesmanId.ToString())?.FirstOrDefault()?.Username;

            workOrderViewUpdateModel.TransactionTypeName = GetTypeFromData.GetTranctionTypeName(workOrder.TransactionType);

            var listPriceWorkOrder = workOrderPriceRepo.Get(x => x.WorkOrderId == workOrder.Id).ToList();
            if (listPriceWorkOrder.Count > 0)
            {
                List<CsWorkOrderPriceModel> prices = new List<CsWorkOrderPriceModel>();
                foreach (var item in listPriceWorkOrder)
                {
                    var workOrderPriceModel = mapper.Map<CsWorkOrderPriceModel>(item);
                    workOrderPriceModel.TransactionType = workOrder.TransactionType;

                    string coloaderId = item.PartnerId.ToString();
                    var coloaderPriceItem = catPartnerRepo.Get(x => x.Id == coloaderId)?.FirstOrDefault();
                    workOrderPriceModel.PartnerName = coloaderPriceItem?.ShortName;

                    var freightChargeIds = new List<Guid?> { item.ChargeIdBuying, item.ChargeIdSelling };
                    var charges = catChargeRepo.Get(x => freightChargeIds.Contains(x.Id));
                    workOrderPriceModel.ChargeCodeBuying = charges.Where(x => x.Id == item.ChargeIdBuying)?.FirstOrDefault().Code;
                    workOrderPriceModel.ChargeCodeSelling = charges.Where(x => x.Id == item.ChargeIdSelling)?.FirstOrDefault().Code;

                    var unit = catUnitrepo.Get(x => x.Id == item.UnitId)?.FirstOrDefault();
                    workOrderPriceModel.UnitCode = unit.Code;

                    var surcharges = workOrderSurchargeRepo.Get(x => x.WorkOrderId == workOrder.Id && x.WorkOrderPriceId == item.Id).ToList();

                    if (surcharges.Count > 0)
                    {
                        var surchargesPartnerIds = surcharges.Select(x => x.PartnerId).ToList();
                        var surchargesPartners = catPartnerRepo.Get(x => surchargesPartnerIds.Contains(x.Id));

                        var surchargeChargeIds = surcharges.Select(x => x.ChargeId);
                        var surchargeCharges = catChargeRepo.Get(x => surchargeChargeIds.Contains(x.Id));

                        List<CsWorkOrderSurchargeModel> listModelSurcharges = new List<CsWorkOrderSurchargeModel>();
                        foreach (var surcharge in surcharges)
                        {
                            CsWorkOrderSurchargeModel surchargeModel = mapper.Map<CsWorkOrderSurchargeModel>(surcharge);
                            surchargeModel.PartnerName = surchargesPartners.Where(x => x.Id == surcharge.PartnerId)?.FirstOrDefault()?.ShortName;
                            surchargeModel.ChargeName = surchargeCharges.Where(x => x.Id == surcharge.ChargeId)?.FirstOrDefault()?.ChargeNameVn;

                            listModelSurcharges.Add(surchargeModel);
                        }

                        workOrderPriceModel.Surcharges = listModelSurcharges;
                    }
                    else
                    {
                        workOrderPriceModel.Surcharges = new List<CsWorkOrderSurchargeModel>();
                    }
                    prices.Add(workOrderPriceModel);
                }
                workOrderViewUpdateModel.ListPrice = prices;
            }

            // Update permission
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialWorkOrder);
            PermissionRange permissionRangeWrite = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Write);

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = workOrderViewUpdateModel.UserCreated,
                CompanyId = workOrderViewUpdateModel.CompanyId,
                DepartmentId = workOrderViewUpdateModel.DepartmentId,
                OfficeId = workOrderViewUpdateModel.OfficeId,
                GroupId = workOrderViewUpdateModel.GroupId
            };
            workOrderViewUpdateModel.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
            };

            return workOrderViewUpdateModel;
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
            return list.AsQueryable().OrderByDescending(x => x.DatetimeModified);
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
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.commercialWorkOrder);
            PermissionRange permissionRangeList = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (permissionRangeList == PermissionRange.None)
            {
                return new ResponsePagingModel<CsWorkOrderViewModel>
                {
                    Data = Enumerable.Empty<CsWorkOrderViewModel>().AsQueryable(),
                    Page = page,
                    Size = size,
                    TotalItems = 0
                };
            }

            IQueryable<CsWorkOrder> data = await QueryAsync(criteria, _user, permissionRangeList);

            var rowsCount = data.Count();

            if (page == 0)
            {
                page = 1;
                size = rowsCount;
            }

            var result = FormatWorkOrder(data.Skip((page - 1) * size).Take(size));

            return new ResponsePagingModel<CsWorkOrderViewModel>
            {
                Data = result,
                Page = page,
                Size = size,
                TotalItems = rowsCount
            };
        }

        private IQueryable<CsWorkOrder> QueryData(WorkOrderCriteria criteria)
        {
            var criteriaBuilder = BuildCriteriaQuery(criteria);
            var query = criteriaBuilder.Apply(DataContext.Get());
            return query;
        }

        private CriteriaBuilder<CsWorkOrder> BuildCriteriaQuery(WorkOrderCriteria criteria)
        {
            var criteriaBuilder = new CriteriaBuilder<CsWorkOrder>()
              .WhereIf(criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0,
                   x => criteria.ReferenceNos.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
              .WhereIf(!string.IsNullOrEmpty(criteria.PartnerId),
                   x => x.PartnerId.ToString() == criteria.PartnerId)
              .WhereIf(!string.IsNullOrEmpty(criteria.SalesmanId),
                   x => x.SalesmanId.ToString() == criteria.SalesmanId)
              .WhereIf(!string.IsNullOrEmpty(criteria.TransactionType),
                   x => x.TransactionType == criteria.TransactionType)
              .WhereIf(!string.IsNullOrEmpty(criteria.POL),
                   x => x.Pol.ToString() == criteria.POL)
              .WhereIf(!string.IsNullOrEmpty(criteria.POD),
                   x => x.Pod.ToString() == criteria.POD)
              .WhereIf(!string.IsNullOrEmpty(criteria.Source),
                   x => x.Source == criteria.Source)
              .WhereIf(criteria.Active != null,
                   x => x.Active == criteria.Active);

            return criteriaBuilder;
        }

        public async Task<IQueryable<CsWorkOrder>> QueryAsync(WorkOrderCriteria criteria)
        {
            var criteriaBuilder = new CriteriaBuilder<CsWorkOrder>()
               .WhereIf(criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0,
                    x => criteria.ReferenceNos.Contains(x.Code, StringComparer.OrdinalIgnoreCase))
               .WhereIf(!string.IsNullOrEmpty(criteria.PartnerId),
                    x => x.PartnerId.ToString() == criteria.PartnerId)
               .WhereIf(!string.IsNullOrEmpty(criteria.SalesmanId),
                    x => x.SalesmanId.ToString() == criteria.SalesmanId)
               .WhereIf(!string.IsNullOrEmpty(criteria.TransactionType),
                    x => x.TransactionType == criteria.TransactionType)
               .WhereIf(!string.IsNullOrEmpty(criteria.POL),
                    x => x.Pol.ToString() == criteria.POL)
               .WhereIf(!string.IsNullOrEmpty(criteria.POD),
                    x => x.Pod.ToString() == criteria.POD)
               .WhereIf(!string.IsNullOrEmpty(criteria.Source),
                    x => x.Source == criteria.Source)
               .WhereIf(criteria.Active != null,
                    x => x.Active == criteria.Active);

            var query = criteriaBuilder.Apply(DataContext.Get());
            return query;
        }

        public async Task<IQueryable<CsWorkOrder>> QueryAsync(WorkOrderCriteria criteria, ICurrentUser currentUser, PermissionRange range)
        {
            var criteriaBuilder = BuildCriteriaQuery(criteria);
            criteriaBuilder
                .WhereIf(range == PermissionRange.Owner, x => x.UserCreated == currentUser.UserID)
                .WhereIf(range == PermissionRange.Group, x => x.GroupId == currentUser.GroupId && x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                .WhereIf(range == PermissionRange.Department, x => x.DepartmentId == currentUser.DepartmentId && x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                .WhereIf(range == PermissionRange.Office, x => x.OfficeId == currentUser.OfficeID && x.CompanyId == currentUser.CompanyID)
                .WhereIf(range == PermissionRange.Company, x => x.UserCreated == currentUser.UserID)
                .WhereIf(range == PermissionRange.None, x => x.CompanyId == currentUser.CompanyID);

            var query = criteriaBuilder.Apply(DataContext.Get());
            return query;
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
                    result = DataContext.Add(workOrder);

                    if (result.Success)
                    {
                        var d = await AddPrice(model.ListPrice, workOrder);
                        if (d.Success)
                        {
                            trans.Commit();
                        }
                    }

                    return result;
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
                    hs = await HandleAddPrice(list, workOrder);
                    if (hs.Success)
                    {
                        HandleState hsSurcharge = workOrderSurchargeRepo.SubmitChanges();
                        if (hsSurcharge.Success)
                        {
                            trans.Commit();
                        }
                        else
                        {
                            string innerMes = hsSurcharge?.Exception?.InnerException?.Message;
                            if (!string.IsNullOrEmpty(innerMes))
                            {
                                throw new Exception(innerMes);
                            }
                            else
                            {
                                throw new Exception(hsSurcharge?.Message?.ToString());
                            }
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

        private async Task<HandleState> HandleAddPrice(List<CsWorkOrderPriceModel> list, CsWorkOrder workOrder)
        {
            foreach (var item in list)
            {
                List<CsWorkOrderSurcharge> surchargesInPriceItem = new List<CsWorkOrderSurcharge>();
                CsWorkOrderPrice priceItem = mapper.Map<CsWorkOrderPrice>(item);

                priceItem.Id = Guid.NewGuid();
                priceItem.WorkOrderId = workOrder.Id;
                priceItem.UserCreated = priceItem.UserModified = currentUser.UserID;
                priceItem.DatetimeCreated = priceItem.DatetimeModified = DateTime.Now;

                if (!string.IsNullOrEmpty(item.ChargeCodeBuying))
                {
                    var charge = catChargeRepo.Get(x => x.Code == item.ChargeCodeBuying)?.FirstOrDefault();
                    priceItem.ChargeIdBuying = charge?.Id;
                }
                if (!string.IsNullOrEmpty(item.ChargeCodeSelling))
                {
                    var charge = catChargeRepo.Get(x => x.Code == item.ChargeCodeSelling)?.FirstOrDefault();
                    priceItem.ChargeIdSelling = charge?.Id;
                }
                if (item.Surcharges.Count > 0)
                {
                    foreach (var i in item.Surcharges)
                    {
                        i.Id = Guid.NewGuid();
                        i.WorkOrderId = priceItem.WorkOrderId;
                        i.WorkOrderPriceId = priceItem.Id;
                        i.UserCreated = currentUser.UserID;
                        i.UserModified = currentUser.UserID;
                        surchargesInPriceItem.Add(i);
                    }
                }
                await workOrderSurchargeRepo.AddAsync(surchargesInPriceItem, false);
                await workOrderPriceRepo.AddAsync(priceItem, false);
            }
            return workOrderPriceRepo.SubmitChanges();
        }

        private async Task<HandleState> HandleUpdatePrice(List<CsWorkOrderPriceModel> list)
        {
            List<CsWorkOrderSurcharge> newSurchargesInPriceItem = new List<CsWorkOrderSurcharge>();
            List<CsWorkOrderSurcharge> updateSurchargesInPriceItem = new List<CsWorkOrderSurcharge>();
            List<CsWorkOrderSurcharge> remainSurcharge = new List<CsWorkOrderSurcharge>();
            foreach (var item in list)
            {
                CsWorkOrderPrice priceItem = mapper.Map<CsWorkOrderPrice>(item);

                priceItem.UserModified = currentUser.UserID;
                priceItem.DatetimeModified = DateTime.Now;

                if (item.Surcharges != null && item.Surcharges.Count > 0)
                {
                    var surchargesAdd = item.Surcharges.Where(x => x.Id == null || x.Id == Guid.Empty).ToList();
                    var surchargesModified = item.Surcharges.Where(x => x.Id != null && x.Id != Guid.Empty).ToList();

                    if (surchargesAdd.Count > 0)
                    {
                        foreach (var i in surchargesAdd)
                        {
                            i.Id = Guid.NewGuid();
                            i.WorkOrderId = item.WorkOrderId;
                            i.WorkOrderPriceId = item.Id;
                            i.DatetimeModified = DateTime.Now;
                            i.DatetimeCreated = DateTime.Now;
                            i.UserModified = currentUser.UserID;
                            i.UserCreated = currentUser.UserID;

                            newSurchargesInPriceItem.Add(mapper.Map<CsWorkOrderSurcharge>(i));
                        }
                    }
                    if (surchargesModified.Count > 0)
                    {
                        var surchargeModifiedIds = surchargesModified.Select(x => x.Id).ToList();
                        foreach (var i in surchargesModified)
                        {
                            i.DatetimeModified = DateTime.Now;
                            i.UserModified = currentUser.UserID;
                            updateSurchargesInPriceItem.Add(mapper.Map<CsWorkOrderSurcharge>(i));
                        }

                        var remainSurchargeOfPriceItem = workOrderSurchargeRepo.Get(x => x.WorkOrderPriceId == item.Id && !surchargeModifiedIds.Contains(x.Id)).ToList();

                        remainSurcharge.AddRange(remainSurchargeOfPriceItem);
                    }
                }
                workOrderPriceRepo.Update(priceItem, x => x.Id == item.Id, false);
            }

            // ADD.
            if (newSurchargesInPriceItem.Count > 0)
            {
                foreach (var sur in newSurchargesInPriceItem)
                {
                    workOrderSurchargeRepo.Add(sur, false);
                }
            }

            // UPDATE.
            if (updateSurchargesInPriceItem.Count > 0)
            {
                foreach (var sur in updateSurchargesInPriceItem)
                {
                    workOrderSurchargeRepo.Update(sur, x => x.Id == sur.Id, false);
                }
            }

            // DELETE.
            if (remainSurcharge.Count > 0)
            {
                var deleteIds = remainSurcharge.Select(x => x.Id).ToList();
                workOrderSurchargeRepo.Delete(x => deleteIds.Contains(x.Id), false);
            }

            return workOrderPriceRepo.SubmitChanges();
        }
        private async Task<HandleState> UpdatePrice(List<CsWorkOrderPriceModel> list, CsWorkOrder workOrder)
        {
            var hs = new HandleState();

            if (list.Count() == 0)
            {
                return hs;
            }

            using (var trans = await workOrderPriceRepo.DC.Database.BeginTransactionAsync())
            {
                try
                {
                    var listAdded = list.Where(x => x.Id == null || x.Id == Guid.Empty).ToList();
                    var listModified = list.Where(x => x.Id != null && x.Id != Guid.Empty).ToList();

                    List<HandleState> hsAddAndModified = new List<HandleState>();

                    if (listAdded.Count > 0 || listModified.Count > 0)
                    {
                        var tasks = new List<Task<HandleState>>();
                        if (listAdded.Count > 0)
                        {
                            tasks.Add(HandleAddPrice(listAdded, workOrder));
                        }
                        if (listModified.Count > 0)
                        {
                            tasks.Add(HandleUpdatePrice(listModified));
                        }
                        var results = await Task.WhenAll(tasks);

                        if (results.All(x => x.Success == true))
                        {
                            HandleState hsSurcharge = workOrderSurchargeRepo.SubmitChanges();
                            if (hsSurcharge.Success)
                            {
                                trans.Commit();
                                return hs;
                            }
                            else
                            {
                                string innerMes = hsSurcharge?.Exception?.InnerException?.Message;
                                if (!string.IsNullOrEmpty(innerMes))
                                {
                                    throw new Exception(innerMes);
                                }
                                else
                                {
                                    throw new Exception(hsSurcharge?.Message?.ToString());
                                }
                            }
                        }
                        else
                        {
                            var errorHandleState = hsAddAndModified.First(x => x.Success == false);
                            string innerMes = errorHandleState?.Exception?.InnerException?.Message;
                            if (!string.IsNullOrEmpty(innerMes))
                            {
                                throw new Exception(innerMes);
                            }
                            else
                            {
                                throw new Exception(errorHandleState?.Message?.ToString());
                            }
                            throw new Exception(innerMes);
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

        public async Task<HandleState> DeletePrice(Guid id)
        {
            HandleState hs = new HandleState();

            using (var trans = workOrderPriceRepo.DC.Database.BeginTransaction())
            {
                try
                {
                    var priceItem = await workOrderPriceRepo.GetAsync(x => x.Id == id);
                    var priceItemDelete = priceItem.FirstOrDefault();
                    if (priceItemDelete != null)
                    {
                        var hsDeletePrice = await workOrderPriceRepo.DeleteAsync(x => x.Id == priceItemDelete.Id);
                        var surcharges = workOrderSurchargeRepo.Get(x => x.WorkOrderPriceId == id).ToList();

                        if (hsDeletePrice.Success)
                        {
                            if (surcharges.Count > 0)
                            {
                                var hsDeleteSurcharge = await workOrderSurchargeRepo.DeleteAsync(x => x.WorkOrderPriceId == id);
                                if (hsDeleteSurcharge.Success)
                                {
                                    trans.Commit();
                                }
                                else
                                {
                                    string innerMes = hsDeleteSurcharge?.Exception?.InnerException?.Message;
                                    if (!string.IsNullOrEmpty(innerMes))
                                    {
                                        throw new Exception(innerMes);
                                    }
                                    else
                                    {
                                        throw new Exception(hsDeleteSurcharge?.Message?.ToString());
                                    }
                                }
                            }
                            else
                            {
                                trans.Commit();
                            }
                        }
                        else
                        {
                            string innerMes = hsDeletePrice?.Exception?.InnerException?.Message;
                            if (!string.IsNullOrEmpty(innerMes))
                            {
                                throw new Exception(innerMes);
                            }
                            else
                            {
                                throw new Exception(hsDeletePrice?.Message?.ToString());
                            }
                        }
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

        public async Task<HandleState> UpdateWorkOrder(WorkOrderRequest model)
        {
            var result = new HandleState();

            using (var trans = await DataContext.DC.Database.BeginTransactionAsync())
            {
                try
                {

                    CsWorkOrder workOrderEntity = DataContext.First(x => x.Id == model.Id);
                    if (workOrderEntity == null)
                    {
                        return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
                    }

                    CsWorkOrder workOrder = mapper.Map<CsWorkOrder>(model);
                    workOrder.Source = workOrderEntity.Source;
                    workOrder.UserModified = currentUser.UserID;
                    workOrder.DatetimeModified = DateTime.Now;
                    workOrder.UserCreated = workOrderEntity.UserCreated;
                    workOrder.DatetimeCreated = workOrderEntity.DatetimeCreated;
                    workOrder.GroupId = workOrderEntity.GroupId;
                    workOrder.DepartmentId = workOrderEntity.DepartmentId;
                    workOrder.OfficeId = workOrderEntity.OfficeId;
                    workOrder.CompanyId = workOrderEntity.CompanyId;
                    workOrder.ApprovedStatus = workOrderEntity.ApprovedStatus;
                    workOrder.ApprovedDate = workOrderEntity.ApprovedDate;
                    workOrder.CrmquotationNo = workOrderEntity.CrmquotationNo;
                    workOrder.SysMappingId = workOrderEntity.SysMappingId;
                    workOrder.ReasonReject = workOrderEntity.ReasonReject;
                    workOrder.SyncedStatus = workOrderEntity.SyncedStatus;
                    workOrder.Active = workOrderEntity.Active;

                    result = DataContext.Update(workOrder, x => x.Id == workOrder.Id);
                    if (result.Success)
                    {
                        HandleState hsUpdatePrice = await UpdatePrice(model.ListPrice, workOrder);
                        if (hsUpdatePrice.Success)
                        {
                            trans.Commit();
                        }
                    }
                    return result;
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

        public async Task<HandleState> SetActiveInactive(ActiveInactiveRequest request)
        {
            var hs = new HandleState();
            CsWorkOrder workOrder = DataContext.Get(x => x.Id == request.Id).FirstOrDefault();
            if (workOrder == null)
            {
                return new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
            }

            using (var trans = await DataContext.DC.Database.BeginTransactionAsync())
            {
                try
                {
                    workOrder.Active = request.Active;
                    workOrder.DatetimeModified = DateTime.Now;
                    workOrder.UserModified = currentUser.UserID;

                    hs = await DataContext.UpdateAsync(workOrder, x => x.Id == workOrder.Id);
                    if (hs.Success)
                    {
                        trans.Commit();
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

        public bool CheckExist(WorkOrderRequest model)
        {
            bool found = false;
            List<CsWorkOrder> workorderSameCriteria = new List<CsWorkOrder>();
            if (model.Id == Guid.Empty)
            {
                workorderSameCriteria = DataContext.Get(x => x.Active == true
                && x.PartnerId == model.PartnerId
                && x.Pol == model.Pol
                && x.Pod == model.Pod
                && x.SalesmanId == model.SalesmanId).ToList();
            }
            else
            {
                workorderSameCriteria = DataContext.Get(x => x.Active == true
                && x.Id != model.Id
                && x.PartnerId == model.PartnerId
                && x.Pol == model.Pol
                && x.Pod == model.Pod
                && x.SalesmanId == model.SalesmanId).ToList();
            }

            if (workorderSameCriteria.Count > 0)
            {
                var workOrderIds = workorderSameCriteria.Select(x => x.Id);
                if (model.ListPrice.Count > 0)
                {
                    foreach (var priceItem in model.ListPrice)
                    {
                        var worKorderPrices = workOrderPriceRepo.Any(x => !workOrderIds.Contains(x.Id)
                        && priceItem.PartnerId == x.PartnerId
                        && priceItem.QuantityFromRange == x.QuantityFromRange
                        && priceItem.QuantityToRange == x.QuantityToRange);

                        if (worKorderPrices)
                        {
                            found = true;
                            break;
                        }

                    }
                }
            }
            return found;
        }
    }
}
