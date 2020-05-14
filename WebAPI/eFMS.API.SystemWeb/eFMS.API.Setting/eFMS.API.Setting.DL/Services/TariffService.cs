using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.API.Setting.DL.Common;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace eFMS.API.Setting.DL.Services
{
    public class TariffService : RepositoryBase<SetTariff, SetTariffModel>, ITariffService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<SetTariffDetail> setTariffDetailRepo;
        private readonly IDistributedCache cache;
        private readonly IContextBase<CatCharge> catChargeRepo;
        private readonly IContextBase<CatCommodityGroup> catCommodityGroupRepo;
        private readonly IContextBase<CatPartner> catPartnerRepo;
        private readonly IContextBase<CatPlace> catPlaceRepo;
        private readonly IContextBase<SysUser> userRepository;

        public TariffService(IContextBase<SetTariff> repository,
            IMapper mapper,
            ICurrentUser user,
            IContextBase<SetTariffDetail> setTariffDetail,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatCommodityGroup> catCommodityGroup,
            IContextBase<CatPartner> catPartner,
            IContextBase<SysUser> userRepo,
            IContextBase<CatPlace> catPlace) : base(repository, mapper)
        {
            currentUser = user;
            setTariffDetailRepo = setTariffDetail;
            catChargeRepo = catCharge;
            catCommodityGroupRepo = catCommodityGroup;
            catPartnerRepo = catPartner;
            catPlaceRepo = catPlace;
            userRepository = userRepo;
        }

        /// <summary>
        /// * Check tồn tại tariff. Check theo các field: 
        /// - Tariff Name (Không được trùng tên), 
        /// - Effective Date - Expried Date
        /// - Tariff Type, Product Service, Cargo Type,  Service Mode
        /// * Check list tariff detail không được phép trống
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState CheckExistsDataTariff(TariffModel model)
        {
            try
            {
                var hs = CheckDuplicateTariff(model.setTariff);
                if (!hs.Success)
                {
                    return hs;
                }

                //Check list tariff detail không được phép trống
                if (model.setTariffDetails.Count == 0)
                {
                    return new HandleState("Please add tariff to create new OPS tariff");
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public HandleState CheckDuplicateTariff(SetTariffModel model)
        {
            try
            {
                if (model == null)
                {
                    return new HandleState("Tariff is not null");
                }

                //Ngày ExpiredDate không được nhỏ hơn ngày EffectiveDate
                if (model.EffectiveDate.Date > model.ExpiredDate.Date)
                {
                    return new HandleState("Expired Date cannot be less than the Effective Date");
                }

                //Trường hợp Insert (Id of tariff is null or empty)
                if (model.Id == Guid.Empty)
                {
                    var tariffNameExists = DataContext.Get(x => x.TariffName == model.TariffName).Any();
                    if (tariffNameExists)
                    {
                        return new HandleState("Tariff name already exists");
                    }

                    //Check theo bộ 4
                    var tariff = DataContext.Get(x => x.TariffType == model.TariffType
                                                    && x.ProductService == model.ProductService
                                                    && x.CargoType == model.CargoType
                                                    && x.ServiceMode == model.ServiceMode);
                    if (tariff.Any())
                    {
                        //Check nằm trong khoảng EffectiveDate - ExpiredDate
                        tariff = tariff
                            .Where(x => model.EffectiveDate.Date >= x.EffectiveDate.Date
                                     && model.ExpiredDate.Date <= x.ExpiredDate.Date);
                        if (tariff.Any())
                        {
                            return new HandleState(ErrorCode.Existed, "Already exists");
                        }
                    }
                }
                else //Trường hợp Update (Id of tariff is not null & not empty)
                {
                    var tariffNameExists = DataContext.Get(x => x.Id != model.Id
                                                             && x.TariffName == model.TariffName).Any();
                    if (tariffNameExists)
                    {
                        return new HandleState("Tariff name already exists");
                    }

                    //Check theo bộ 4
                    var tariff = DataContext.Get(x => x.Id != model.Id
                                                    && x.TariffType == model.TariffType
                                                    && x.ProductService == model.ProductService
                                                    && x.CargoType == model.CargoType
                                                    && x.ServiceMode == model.ServiceMode);
                    if (tariff.Any())
                    {
                        //Check nằm trong khoảng EffectiveDate - ExpiredDate
                        tariff = tariff
                            .Where(x => model.EffectiveDate.Date >= x.EffectiveDate.Date
                                     && model.ExpiredDate.Date <= x.ExpiredDate.Date);
                        if (tariff.Any())
                        {
                            return new HandleState(ErrorCode.Existed, "Already exists");
                        }
                    }
                }

                return new HandleState();
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Add tariff & list tariff detail
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState AddTariff(TariffModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var today = DateTime.Now;

                model.setTariff.GroupId = currentUser.GroupId;
                model.setTariff.DepartmentId = currentUser.DepartmentId;
                model.setTariff.OfficeId = currentUser.OfficeID;
                model.setTariff.CompanyId = currentUser.CompanyID;
                //Insert SetTariff
                var tariff = mapper.Map<SetTariff>(model.setTariff);
                tariff.Id = model.setTariff.Id = Guid.NewGuid();
                tariff.UserCreated = tariff.UserModified = userCurrent;
                tariff.DatetimeCreated = tariff.DatetimeModified = today;
                var hs = DataContext.Add(tariff);
                if (hs.Success)
                {
                    //Insert list SetTariffDetail
                    var tariffDetails = mapper.Map<List<SetTariffDetail>>(model.setTariffDetails);
                    tariffDetails.ForEach(r =>
                    {
                        r.Id = Guid.NewGuid();
                        r.TariffId = tariff.Id;
                        r.UserCreated = r.UserModified = userCurrent;
                        r.DatetimeCreated = r.DatetimeModified = today;
                    });
                    var hsTariffDetail = setTariffDetailRepo.Add(tariffDetails);
                }
                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Update tariff & list tariff model
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState UpdateTariff(TariffModel model)
        {
            try
            {
                var today = DateTime.Now;
                //Update SetTariff
                var tariff = mapper.Map<SetTariff>(model.setTariff);
                tariff.DatetimeModified = today;

                var hs = DataContext.Update(tariff, x => x.Id == tariff.Id);
                if (hs.Success)
                {
                    var tariffDetails = mapper.Map<List<SetTariffDetail>>(model.setTariffDetails);

                    //Remove các tariff mà user đã gỡ bỏ
                    var listIdTariffDetail = tariffDetails.Select(s => s.Id);
                    var listIdTariffDetailNeedRemove = setTariffDetailRepo.Get(x => x.TariffId == tariff.Id
                                                                                 && !listIdTariffDetail.Contains(x.Id)).Select(x => x.Id);
                    if (listIdTariffDetailNeedRemove.Count() > 0)
                    {
                        var hsTariffDetailDel = setTariffDetailRepo.Delete(x => listIdTariffDetailNeedRemove.Contains(x.Id));
                    }

                    //Update các tariff detail cũ
                    var tariffDetailOld = tariffDetails.Where(x => x.Id != Guid.Empty);
                    //&& setTariffDetailRepo.Get(g => g.TariffId == tariff.Id).Select(s => s.Id).Contains(x.Id));
                    if (tariffDetailOld.Count() > 0)
                    {
                        foreach (var item in tariffDetailOld)
                        {
                            //item.UserCreated = setTariffDetailRepo.Get(x => x.Id == item.Id).FirstOrDefault().UserCreated;
                            item.UserModified = model.setTariff.UserModified;
                            //item.DatetimeCreated = setTariffDetailRepo.Get(x => x.Id == item.Id).FirstOrDefault().DatetimeCreated;
                            item.DatetimeModified = DateTime.Now;
                            var hsTariffDetailUpdate = setTariffDetailRepo.Update(item, x => x.Id == item.Id);
                        }
                    }

                    //Add các tariff detail mới
                    var tariffDetailNew = tariffDetails.Where(x => x.Id == Guid.Empty).ToList();
                    if (tariffDetailNew.Count > 0)
                    {
                        tariffDetailNew.ForEach(r =>
                        {
                            r.Id = Guid.NewGuid();
                            r.TariffId = tariff.Id;
                            r.UserCreated = r.UserModified = model.setTariff.UserModified;
                            r.DatetimeCreated = r.DatetimeModified = DateTime.Now;
                        });
                        var hsTariffDetailAdd = setTariffDetailRepo.Add(tariffDetailNew);
                    }
                }

                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        /// <summary>
        /// Delete tariff & list tariff model
        /// </summary>
        /// <param name="idTariff"></param>
        /// <returns></returns>
        public HandleState DeleteTariff(Guid tariffId)
        {
            try
            {
                var hs = DataContext.Delete(x => x.Id == tariffId);
                if (hs.Success)
                {
                    var hsTariffDetail = setTariffDetailRepo.Delete(x => x.TariffId == tariffId);
                }

                return hs;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        public List<SetTariffModel> GetAllTariff()
        {
            //var clearanceCaching = RedisCacheHelper.GetObject<List<SetTariffModel>>(cache, Templates.CustomDeclaration.NameCaching.ListName);
            List<SetTariffModel> setTariffModels = null;
            //get from view data
            var list = DataContext.Get();
            setTariffModels = mapper.Map<List<SetTariffModel>>(list);
            //RedisCacheHelper.SetObject(cache, Templates.CustomDeclaration.NameCaching.ListName, customClearances);
            return setTariffModels;
        }


        public List<TariffViewModel> Query(TariffCriteria criteria)
        {
            var tariff = GetAllTariff();
            var partner = catPartnerRepo.Get();

            var query = from t in tariff
                        join p in partner on t.CustomerId equals p.Id into partnerdata
                        from p in partnerdata.DefaultIfEmpty()
                        join s in partner on t.SupplierId equals s.Id into supplierdata
                        from s in supplierdata.DefaultIfEmpty()
                        select new { t, CustomerName = p != null ? p.ShortName : null, SupplierName = s != null ? s.ShortName : null };
            query = query.Where(x =>
            ((x.t.TariffName ?? "").IndexOf(criteria.Name ?? "", StringComparison.OrdinalIgnoreCase)) >= 0
            && (x.t.CustomerId ?? "").IndexOf(criteria.CustomerID ?? "", StringComparison.OrdinalIgnoreCase) >= 0
            && (x.t.TariffType == criteria.TariffType || string.IsNullOrEmpty(criteria.TariffType))
            && (x.t.ServiceMode == criteria.ServiceMode || string.IsNullOrEmpty(criteria.ServiceMode))
            && (x.t.SupplierId == criteria.SupplierID || string.IsNullOrEmpty(criteria.SupplierID))
            && (x.t.OfficeId == criteria.OfficeId || criteria.OfficeId == Guid.Empty)
            && (x.t.Status == criteria.Status || criteria.Status == null));
            if (criteria.DateType == "CreateDate" && criteria.ToDate.HasValue && criteria.FromDate.HasValue)
            {
                query = query.Where(x =>
                (x.t.DatetimeCreated.HasValue && x.t.DatetimeCreated.Value.Date >= criteria.FromDate.Value.Date && x.t.DatetimeCreated.Value.Date <= criteria.ToDate.Value.Date));
            }
            else if (criteria.DateType == "EffectiveDate" && criteria.ToDate.HasValue && criteria.FromDate.HasValue)
            {
                query = query.Where(x =>
                (x.t.EffectiveDate.Date >= criteria.FromDate && x.t.EffectiveDate <= criteria.ToDate));
            }
            else if (criteria.DateType == "ModifiedDate" && criteria.ToDate.HasValue && criteria.FromDate.HasValue)
            {
                query = query.Where(x =>
                (x.t.DatetimeModified.HasValue && x.t.DatetimeModified.Value.Date >= criteria.FromDate.Value.Date && x.t.DatetimeModified.Value.Date <= criteria.ToDate.Value.Date));
            }
            else if (criteria.DateType == "ExpiredDate")
            {
                query = query.Where(x =>
                (x.t.ExpiredDate.Date >= criteria.FromDate && x.t.ExpiredDate.Date <= criteria.ToDate));
            }
            else
            {
                query = query.Where(x =>
                   (((x.t.DatetimeCreated.HasValue && x.t.DatetimeCreated.Value.Date >= criteria.FromDate) &&
                   (x.t.DatetimeCreated.Value.Date <= criteria.ToDate))
                    && ((x.t.EffectiveDate.Date >= criteria.FromDate)
                     && (x.t.EffectiveDate.Date <= criteria.ToDate))
                    && ((x.t.DatetimeModified.HasValue && x.t.DatetimeModified.Value.Date >= criteria.FromDate)
                     && (x.t.DatetimeModified.Value.Date <= criteria.ToDate))
                     && ((x.t.ExpiredDate.Date >= criteria.FromDate)
                     && (x.t.ExpiredDate.Date <= criteria.ToDate)) || criteria.FromDate == null || criteria.ToDate == null)
                   );


                //|| ((x.t.EffectiveDate.Date >= criteria.FromDate)
                //&& (x.t.EffectiveDate.Date <= criteria.ToDate))

                //|| ((x.t.DatetimeModified.HasValue && x.t.DatetimeModified.Value.Date >= criteria.FromDate)
                //&& (x.t.DatetimeModified.Value.Date <= criteria.ToDate))

                ////|| ((x.t.DatetimeModified.HasValue ? x.t.DatetimeModified.Value.Date >= criteria.FromDate || criteria.FromDate == null : x.t.DatetimeModified >= criteria.FromDate || criteria.FromDate == null)
                ////&& (x.t.DatetimeModified.HasValue ? x.t.DatetimeModified.Value.Date <= criteria.ToDate || criteria.ToDate == null : x.t.DatetimeModified <= criteria.ToDate || criteria.ToDate == null))

                //|| ((x.t.ExpiredDate.Date >=  criteria.FromDate.Value.Date)
                //&& (x.t.ExpiredDate.Date <= criteria.ToDate.Value.Date)) || criteria.FromDate == null || criteria.ToDate == null);


            }

            if (query == null) return null;
            query = query.ToArray().OrderByDescending(x => x.t.DatetimeModified).AsQueryable();
            List<TariffViewModel> results = new List<TariffViewModel>();
            foreach (var item in query)
            {
                var tariffView = mapper.Map<TariffViewModel>(item.t);
                tariffView.CustomerName = item.CustomerName != null ? item.CustomerName.ToString() : null;
                tariffView.SupplierName = item.SupplierName != null ? item.SupplierName.ToString() : null;
                tariffView.setTariff = new SetTariffModel
                {
                    CompanyId = item.t.CompanyId,
                    OfficeId = item.t.OfficeId,
                    DepartmentId = item.t.DepartmentId,
                    GroupId = item.t.GroupId
                };

            results.Add(tariffView);
            }
            return results;
        }

        private IQueryable<TariffViewModel> QueryPermission(TariffCriteria criteria, PermissionRange range)
        {
            var list = Query(criteria);
            if (list == null)
            {
                return null;
            }
            IQueryable<TariffViewModel> data = null;

            switch (range)
            {
                case PermissionRange.Owner:
                    data = list.Where(x => x.UserCreated == currentUser.UserID).AsQueryable();
                    break;
                case PermissionRange.Group:
                    data = list.Where(x => x.UserCreated == currentUser.UserID
                    || x.setTariff.GroupId == currentUser.GroupId
                    && x.setTariff.DepartmentId == currentUser.DepartmentId
                    && x.setTariff.OfficeId == currentUser.OfficeID
                    && x.setTariff.CompanyId == currentUser.CompanyID).AsQueryable();
                    break;
                case PermissionRange.Department:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.setTariff.DepartmentId == currentUser.DepartmentId && x.setTariff.OfficeId == currentUser.OfficeID
                    && x.setTariff.CompanyId == currentUser.CompanyID).AsQueryable();
                    break;
                case PermissionRange.Office:
                    data = list.Where(x => x.UserCreated == currentUser.UserID || x.setTariff.OfficeId == currentUser.OfficeID && x.setTariff.CompanyId == currentUser.CompanyID).AsQueryable();
                    break;
                case PermissionRange.Company:
                    data = list.Where(x => x.UserCreated == currentUser.UserID ||  x.setTariff.CompanyId == currentUser.CompanyID).AsQueryable();
                    break;
                case PermissionRange.All:
                    data = list.AsQueryable();
                    break;
                default:
                    break;
            }
            return data;
        }

        public IQueryable<TariffViewModel> Paging(TariffCriteria criteria, int page, int size, out int rowsCount)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);
            var rangeSearch = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.List);

            if (rangeSearch == PermissionRange.None)
            {
                rowsCount = 0;
                return null;
            }

            List<TariffViewModel> results = null;
            // var list = Query(criteria);
            IQueryable<TariffViewModel> list = QueryPermission(criteria, rangeSearch);

            if (list == null)
            {
                rowsCount = 0;
                return null;
            }
            list = list.OrderByDescending(x => x.DatetimeModified);
            rowsCount = list.ToList().Count;
            if (size > 1)
            {
                if (page < 1)
                {
                    page = 1;
                }
                results = list.Skip((page - 1) * size).Take(size).ToList();
            }
            return results.AsQueryable();
        }

        public SetTariffModel GetTariffById(Guid tariffId)
        {

            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.settingTariff);
            var permissionRangeWrite = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Write);
            var permissionRangeDelete = PermissionExtention.GetPermissionRange(currentUser.UserMenuPermission.Delete);


            var tariff = DataContext.Get(x => x.Id == tariffId).FirstOrDefault();
            var data = mapper.Map<SetTariffModel>(tariff);

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = data.UserCreated,
                CompanyId = data.CompanyId,
                DepartmentId = data.DepartmentId,
                OfficeId = data.OfficeId,
                GroupId = data.GroupId
            };
            data.Permission = new PermissionAllowBase
            {
                AllowUpdate = PermissionExtention.GetPermissionDetail(permissionRangeWrite, baseModel, currentUser),
                AllowDelete = PermissionExtention.GetPermissionDetail(permissionRangeDelete, baseModel, currentUser),
            };

            data.UserCreatedName = userRepository.Get(x => x.Id == data.UserCreated).FirstOrDefault()?.Username;
            data.UserModifieddName = userRepository.Get(x => x.Id == data.UserModified).FirstOrDefault()?.Username;

            return data;
        }

        public SetTariffDetailModel GetTariffDetailById(Guid tariffDetailId)
        {
            var tariffDetail = setTariffDetailRepo.Get(x => x.Id == tariffDetailId).FirstOrDefault();
            var data = mapper.Map<SetTariffDetailModel>(tariffDetail);
            return data;
        }

        public IQueryable<SetTariffDetailModel> GetListTariffDetailByTariffId(Guid tariffId)
        {
            var tariffDetails = setTariffDetailRepo.Get(x => x.TariffId == tariffId);
            var charges = catChargeRepo.Get();
            var commoditiGrps = catCommodityGroupRepo.Get();
            var payers = catPartnerRepo.Get();
            var ports = catPlaceRepo.Get(x => x.PlaceTypeId == CatPlaceConstant.Port);
            var warehouses = catPlaceRepo.Get(x => x.PlaceTypeId == CatPlaceConstant.Warehouse);

            //var tariffDetailsModel = tariffDetails.ProjectTo<SetTariffDetailModel>(mapper.ConfigurationProvider);
            var queryData = from tariff in tariffDetails
                            join charge in charges on tariff.ChargeId equals charge.Id into charge2
                            from charge in charge2.DefaultIfEmpty()
                            join commoditiGrp in commoditiGrps on tariff.CommodityId equals commoditiGrp.Id into commoditiGrp2
                            from commoditiGrp in commoditiGrp2.DefaultIfEmpty()
                            join payer in payers on tariff.PayerId equals payer.Id into payer2
                            from payer in payer2.DefaultIfEmpty()
                            join port in ports on tariff.PortId equals port.Id into port2
                            from port in port2.DefaultIfEmpty()
                            join warehouse in warehouses on tariff.WarehouseId equals warehouse.Id into warehouse2
                            from warehouse in warehouse2.DefaultIfEmpty()
                            select new SetTariffDetailModel
                            {
                                Id = tariff.Id,
                                TariffId = tariff.TariffId,
                                ChargeId = tariff.ChargeId,
                                UseFor = tariff.UseFor,
                                Route = tariff.Route,
                                CommodityId = tariff.CommodityId,
                                PayerId = tariff.PayerId,
                                PortId = tariff.PortId,
                                WarehouseId = tariff.WarehouseId,
                                Type = tariff.Type,
                                RangeType = tariff.RangeType,
                                RangeFrom = tariff.RangeFrom,
                                RangeTo = tariff.RangeTo,
                                UnitPrice = tariff.UnitPrice,
                                Min = tariff.Min,
                                Max = tariff.Max,
                                NextUnit = tariff.NextUnit,
                                NextUnitPrice = tariff.NextUnitPrice,
                                UnitId = tariff.UnitId,
                                CurrencyId = tariff.CurrencyId,
                                Vatrate = tariff.Vatrate,
                                UserCreated = tariff.UserCreated,
                                DatetimeCreated = tariff.DatetimeCreated,
                                UserModified = tariff.UserModified,
                                DatetimeModified = tariff.DatetimeModified,
                                ChargeName = charge.ChargeNameEn,
                                ChargeCode = charge.Code,
                                CommodityName = commoditiGrp.GroupNameEn,
                                PayerName = payer.ShortName,
                                PortName = port.NameEn,
                                WarehouseName = warehouse.NameEn
                            };
            return queryData;
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            var result = new TariffModel();
            result.setTariff = GetTariffById(id);
            if (result.setTariff == null)
            {
                return false;
            }

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = result.setTariff.UserCreated,
                CompanyId = result.setTariff.CompanyId,
                DepartmentId = result.setTariff.DepartmentId,
                OfficeId = result.setTariff.OfficeId,
                GroupId = result.setTariff.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, range, currentUser);

            if (code == 403) return false;

            return true;
        }
    }
}
