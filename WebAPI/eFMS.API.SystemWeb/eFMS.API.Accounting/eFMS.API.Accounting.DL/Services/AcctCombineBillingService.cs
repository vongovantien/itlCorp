﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.CombineBilling;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ReportResults;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.Common.Models;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctCombineBillingService : RepositoryBase<AcctCombineBilling, AcctCombineBillingModel>, IAcctCombineBillingService
    {
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IContextBase<OpsTransaction> opsTransactionRepo;
        private readonly IContextBase<CsTransaction> csTransactionRepo;
        private readonly IContextBase<CsTransactionDetail> csTransactionDetailRepo;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepo;
        private readonly IContextBase<AcctCdnote> cdNoteRepo;
        private readonly IContextBase<CatPartner> partnerRepo;
        private readonly IContextBase<SysUser> userRepo;
        private readonly IContextBase<SysEmployee> employeeRepo;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<AcctSoa> soaRepo;
        private readonly IContextBase<CatPlace> catPlaceRepo;
        private readonly IContextBase<CatCountry> countryRepo;
        private readonly IContextBase<CatCharge> catChargeRepo;
        private readonly IContextBase<CatCurrency> catCurrencyRepo;
        private readonly IContextBase<SysCompany> sysCompanyRepo;
        private readonly IContextBase<SysUserLevel> sysUserLevelRepo;
        private readonly IContextBase<SysOffice> sysOfficeRepo;
        private readonly IContextBase<CatCommodityGroup> catCommodityGroupRepo;
        private readonly IContextBase<CatUnit> catUnitRepo;
        private readonly IContextBase<CatCommodity> catCommodityRepo;
        private decimal _decimalNumber = Constants.DecimalNumber;

        public AcctCombineBillingService(IContextBase<AcctCombineBilling> repository,
            IMapper mapper,
            ICurrentUser cUser,
            ICurrencyExchangeService exchangeService,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<OpsTransaction> opsTransaction,
            IContextBase<CsTransaction> csTransaction,
            IContextBase<CsTransactionDetail> csTransactionDetail,
            IContextBase<CustomsDeclaration> customsDeclaration,
            IContextBase<AcctCdnote> cdNote,
            IContextBase<CatPartner> partner,
            IContextBase<SysUser> user,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<SysEmployee> employee,
            IContextBase<AcctSoa> soa,
            IContextBase<CatPlace> catPlace,
            IContextBase<CatCountry> country,
            IContextBase<CatCharge> catCharge,
            IContextBase<CatCurrency> catCurrency,
            IContextBase<SysCompany> sysCompany,
            IContextBase<SysUserLevel> sysUserLevel,
            IContextBase<SysOffice> sysOffice,
            IContextBase<CatCommodityGroup> catCommodityGroup,
            IContextBase<CatUnit> catUnit,
            IContextBase<CatCommodity> catCommodity

            ) : base(repository, mapper)
        {
            currentUser = cUser;
            currencyExchangeService = exchangeService;
            surchargeRepo = surcharge;
            opsTransactionRepo = opsTransaction;
            csTransactionRepo = csTransaction;
            csTransactionDetailRepo = csTransactionDetail;
            customsDeclarationRepo = customsDeclaration;
            cdNoteRepo = cdNote;
            partnerRepo = partner;
            userRepo = user;
            employeeRepo = employee;
            stringLocalizer = localizer;
            soaRepo = soa;
            catPlaceRepo = catPlace;
            countryRepo = country;
            catChargeRepo = catCharge;
            catCurrencyRepo = catCurrency;
            sysCompanyRepo = sysCompany;
            sysUserLevelRepo = sysUserLevel;
            sysOfficeRepo = sysOffice;
            catCommodityGroupRepo = catCommodityGroup;
            catUnitRepo = catUnit;
            catCommodityRepo = catCommodity;
        }

        /// <summary>
        /// Generate billing no
        /// </summary>
        /// <returns></returns>
        public string GenerateCombineBillingNo()
        {
            var yy = DateTime.Now.ToString("yy");
            var mm = DateTime.Now.ToString("MM");
            var billingNos = DataContext.Get(x => x.CombineBillingNo.Contains("CB")).Select(x => x.CombineBillingNo);
            var numOfOrder = new List<int>();
            var num = string.Empty;
            if (billingNos != null && billingNos.Count() > 0)
            {
                foreach (var code in billingNos)
                {
                    // Lấy 4 ký tự cuối
                    if (code.Length > 6 && isNumeric(code.Substring(code.Length - 4)))
                    {
                        numOfOrder.Add(int.Parse(code.Substring(code.Length - 4)));
                    }
                }
                if (numOfOrder.Count() > 0)
                {
                    int maxCurrentOder = numOfOrder.Max();

                    num += (maxCurrentOder + 1).ToString("0000");
                }
                else
                {
                    num += "0001";
                }
            }
            else
            {
                num = "0001";
            }
            string billingNo = string.Format("CB{0}{1}{2}", yy, mm, num);
            return billingNo;
        }

        /// <summary>
        /// Check if n is a number
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private bool isNumeric(string n)
        {
            return int.TryParse(n, out int _);
        }

        /// <summary>
        /// Update combine billing no for surcharge/soa/cdnote
        /// </summary>
        /// <param name="refNos"></param>
        /// <param name="shipments"></param>
        /// <param name="combineNo"></param>
        /// <returns></returns>
        private sp_UpdateBillingNoForShipment UpdateCombineNoForShipment(string refNos, string shipments, string combineNo)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@refNos", Value = refNos },
                new SqlParameter(){ ParameterName = "@shipment", Value = shipments },
                new SqlParameter(){ ParameterName = "@billingNo", Value = combineNo },
                new SqlParameter(){ ParameterName = "@user", Value = currentUser.UserID },
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_UpdateBillingNoForShipment>(parameters);
            return result.FirstOrDefault();
        }

        /// <summary>
        /// Check Existing Combine Billing No
        /// </summary>
        /// <param name="combineNo"></param>
        /// <returns></returns>
        public bool CheckExistedCombineData(Guid id)
        {
            if (DataContext.Get(x => x.Id == id).FirstOrDefault() == null)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check view detail permission
        /// </summary>
        /// <param name="combineNo"></param>
        /// <returns></returns>
        public bool CheckAllowViewDetailCombine(Guid combineId)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            var permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.Detail);
            if (permissionRange == PermissionRange.None)
                return false;

            var detail = DataContext.Get(x => x.Id == combineId)?.FirstOrDefault();
            if (detail == null) return false;

            BaseUpdateModel baseModel = new BaseUpdateModel
            {
                UserCreated = detail.UserCreated,
                CompanyId = detail.CompanyId,
                DepartmentId = detail.DepartmentId,
                OfficeId = detail.OfficeId,
                GroupId = detail.GroupId
            };
            int code = PermissionExtention.GetPermissionCommonItem(baseModel, permissionRange, _user);

            if (code == 403) return false;

            return true;
        }

        /// <summary>
        /// Insert data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState AddCombineBilling(AcctCombineBillingModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                if (DataContext.Get(x => x.CombineBillingNo == model.CombineBillingNo).FirstOrDefault() != null)
                {
                    model.CombineBillingNo = GenerateCombineBillingNo();
                }
                //model.Id = Guid.NewGuid();
                model.DatetimeCreated = model.DatetimeModified = DateTime.Now;
                model.UserCreated = model.UserModified = userCurrent;
                model.GroupId = currentUser.GroupId;
                model.DepartmentId = currentUser.DepartmentId;
                model.OfficeId = currentUser.OfficeID;
                model.CompanyId = currentUser.CompanyID;
                model.TotalAmountVnd = model.Shipments.Sum(x => x.AmountVnd ?? 0);
                model.TotalAmountUsd = model.Shipments.Sum(x => x.AmountUsd ?? 0);

                var surcharges = surchargeRepo.Get(x => model.Shipments.Any(s => s.Hblid == x.Hblid && s.JobNo == x.JobNo));

                var shipments = string.Empty;
                var refNos = string.Empty;
                if (surcharges != null)
                {
                    shipments = string.Join('+', model.Shipments.Select(x => x.Refno + ';' + x.Hblid).ToList());
                    refNos = string.Join(';', model.Shipments.Select(x => x.Refno).Distinct().ToList());
                }

                var dataAdd = mapper.Map<AcctCombineBilling>(model);
                var hs = DataContext.Add(dataAdd);
                if (hs.Success && !string.IsNullOrEmpty(refNos))
                {
                    var req = UpdateCombineNoForShipment(refNos, shipments, model.CombineBillingNo);
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("AddCombineBilling", ex.ToString());
                var hs = new HandleState((object)ex.Message);
                return hs;
            }
        }

        /// <summary>
        /// Update data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public HandleState UpdateCombineBilling(AcctCombineBillingModel model)
        {
            try
            {
                var userCurrent = currentUser.UserID;
                var currentCombine = DataContext.Get(x => x.Id == model.Id).FirstOrDefault();
                var combine = mapper.Map<AcctCombineBilling>(model);
                combine.DatetimeCreated = currentCombine.DatetimeCreated;
                combine.UserCreated = currentCombine.UserCreated;
                combine.GroupId = currentCombine.GroupId;
                combine.DepartmentId = currentCombine.DepartmentId;
                combine.OfficeId = currentCombine.OfficeId;
                combine.CompanyId = currentCombine.CompanyId;
                combine.DatetimeModified = DateTime.Now;
                combine.UserModified = userCurrent;

                combine.TotalAmountVnd = model.Shipments.Sum(x => x.AmountVnd ?? 0);
                combine.TotalAmountUsd = model.Shipments.Sum(x => x.AmountUsd ?? 0);
                var surcharges = surchargeRepo.Get(x => model.Shipments.Any(s => s.Hblid == x.Hblid && (s.Refno == x.Soano || s.Refno == x.PaySoano || s.Refno == x.DebitNo || s.Refno == x.CreditNo)));

                var shipments = string.Empty;
                var refNos = string.Empty;
                if (surcharges != null)
                {
                    shipments = string.Join('+', model.Shipments.Select(x => x.Refno + ';' + x.Hblid).ToList());
                    refNos = string.Join(';', model.Shipments.Select(x => x.Refno).ToList());
                }

                var hs = DataContext.Update(combine, x => x.Id == combine.Id);
                if (hs.Success && !string.IsNullOrEmpty(refNos))
                {
                    var req = UpdateCombineNoForShipment(refNos, shipments, model.CombineBillingNo);
                }
                return hs;
            }
            catch (Exception ex)
            {
                new LogHelper("UpdateCombineBilling", ex.ToString());
                var hs = new HandleState((object)ex.Message);
                return hs;
            }
        }

        /// <summary>
        /// Delete combine data
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HandleState DeleteCombineBilling(Guid? id)
        {
            try
            {
                using (var trans = DataContext.DC.Database.BeginTransaction())
                {
                    try
                    {
                        var combineData = DataContext.Get(x => x.Id == id).FirstOrDefault();

                        var hs = DataContext.Delete(x => x.Id == combineData.Id, false);
                        if (hs.Success)
                        {
                            // Remove from surcharges
                            var surcharges = surchargeRepo.Get(x => x.CombineBillingNo == combineData.CombineBillingNo || x.ObhcombineBillingNo == combineData.CombineBillingNo).ToList();
                            foreach (var item in surcharges)
                            {
                                item.UserModified = currentUser.UserID;
                                item.DatetimeModified = DateTime.Now;
                                if (item.Type == "OBH")
                                {
                                    item.CombineBillingNo = item.CombineBillingNo == combineData.CombineBillingNo ? null : item.CombineBillingNo;
                                    item.ObhcombineBillingNo = item.ObhcombineBillingNo == combineData.CombineBillingNo ? null : item.ObhcombineBillingNo;
                                }
                                else
                                {
                                    item.CombineBillingNo = null;
                                }
                                var hsUpdateSurcharge = surchargeRepo.Update(item, x => x.Id == item.Id, false);
                            }

                            // Remove from soa
                            var acctSoa = soaRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && x.CombineBillingNo.Contains(combineData.CombineBillingNo)).ToList();
                            foreach (var item in acctSoa)
                            {
                                item.CombineBillingNo = item.CombineBillingNo.Replace(combineData.CombineBillingNo, "");
                                item.CombineBillingNo = string.IsNullOrEmpty(item.CombineBillingNo) ? null : string.Join(";", item.CombineBillingNo.Split(';').Where(x => !string.IsNullOrEmpty(x)));
                                var hsUpdateSoa = soaRepo.Update(item, x => x.Id == item.Id, false);
                            }

                            // Remove from cdNote
                            var cdNote = cdNoteRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && x.CombineBillingNo.Contains(combineData.CombineBillingNo)).ToList();
                            foreach (var item in cdNote)
                            {
                                item.CombineBillingNo = item.CombineBillingNo.Replace(combineData.CombineBillingNo, "");
                                item.CombineBillingNo = string.IsNullOrEmpty(item.CombineBillingNo) ? null : string.Join(";", item.CombineBillingNo.Split(';').Where(x => !string.IsNullOrEmpty(x)));
                                var hsUpdateCdNote = cdNoteRepo.Update(item, x => x.Id == item.Id, false);
                            }
                            DataContext.SubmitChanges();
                            surchargeRepo.SubmitChanges();
                            soaRepo.SubmitChanges();
                            cdNoteRepo.SubmitChanges();
                            trans.Commit();
                        }
                        else
                        {
                            trans.Rollback();
                        }
                        return hs;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        return new HandleState(ex.Message);
                    }
                    finally
                    {
                        trans.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                var hs = new HandleState(ex.Message);
                return hs;
            }
        }
        #region --- LIST PAGING ---
        /// <summary>
        /// Get data paging
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <param name="rowsCount"></param>
        /// <returns></returns>
        public List<AcctCombineBillingResult> Paging(AcctCombineBillingCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = GetData(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            //Phân trang
            var _totalItem = data.Select(s => s.Id).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }

            return data.ToList();
        }

        private IQueryable<AcctCombineBilling> GetCombinePermission()
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctSOA);
            PermissionRange _permissionRange = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (_permissionRange == PermissionRange.None) return null;

            IQueryable<AcctCombineBilling> combine = null;
            switch (_permissionRange)
            {
                case PermissionRange.None:
                    break;
                case PermissionRange.All:
                    combine = DataContext.Get();
                    break;
                case PermissionRange.Owner:
                    combine = DataContext.Get(x => x.UserCreated == _user.UserID);
                    break;
                case PermissionRange.Group:
                    combine = DataContext.Get(x => x.GroupId == _user.GroupId
                                            && x.DepartmentId == _user.DepartmentId
                                            && x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Department:
                    combine = DataContext.Get(x => x.DepartmentId == _user.DepartmentId
                                            && x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Office:
                    combine = DataContext.Get(x => x.OfficeId == _user.OfficeID
                                            && x.CompanyId == _user.CompanyID);
                    break;
                case PermissionRange.Company:
                    combine = DataContext.Get(x => x.CompanyId == _user.CompanyID);
                    break;
            }
            return combine;
        }

        /// <summary>
        /// Get data combine
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AcctCombineBillingResult> GetData(AcctCombineBillingCriteria criteria)
        {
            var query = ExpressionQuery(criteria);
            var dataCombineBilling = GetCombinePermission().Where(query);
            var userId = dataCombineBilling.Select(x => x.UserCreated);
            var users = userRepo.Get(x => userId.Any(z => z == x.Id));
            var partnerId = dataCombineBilling.Select(x => x.PartnerId);
            var partners = partnerRepo.Get(x => partnerId.Any(z => z == x.Id));

            if (criteria.ReferenceNo != null && criteria.ReferenceNo.Count > 0)
            {
                criteria.ReferenceNo = criteria.ReferenceNo.Where(x => !string.IsNullOrEmpty(x)).ToList();
                var combineNos = new List<string>();
                var surcharges = surchargeRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo));
                var chargesFilter = surcharges.Where(x => criteria.ReferenceNo.Any(z => z == x.JobNo || z == x.Soano || z == x.PaySoano || z == x.CreditNo || z == x.DebitNo));
                combineNos.AddRange(chargesFilter.Select(x => x.CombineBillingNo));
                combineNos.AddRange(chargesFilter.Select(x => x.ObhcombineBillingNo));
                combineNos.AddRange(dataCombineBilling.Where(x => criteria.ReferenceNo.Any(z => z == x.CombineBillingNo)).Select(x => x.CombineBillingNo));
                combineNos = combineNos.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (combineNos.Count() > 0)
                {
                    dataCombineBilling = dataCombineBilling.Where(x => combineNos.Any(z => z == x.CombineBillingNo));
                }
                else
                {
                    return null;
                }
            }

            var result = from data in dataCombineBilling
                         join part in partners on data.PartnerId equals part.Id
                         //join surcharge in surcharges on data.CombineBillingNo equals surcharge.CombineBillingNo into grpCombine
                         //from surcharge in grpCombine.DefaultIfEmpty()
                         //join surcharge2 in surcharges on data.CombineBillingNo equals surcharge2.ObhcombineBillingNo into grpCombineObh
                         //from surcharge2 in grpCombineObh.DefaultIfEmpty()
                         join us in users on data.UserCreated equals us.Id
                         join emp in employeeRepo.Get() on us.EmployeeId equals emp.Id
                         select new AcctCombineBillingResult
                         {
                             Id = data.Id,
                             CombineBillingNo = data.CombineBillingNo,
                             PartnerName = part.ShortName,
                             UserCreated = data.UserCreated,
                             TotalAmountVnd = data.TotalAmountVnd,
                             TotalAmountUsd = data.TotalAmountUsd,
                             //JobNo = surcharge != null ? surcharge.JobNo : surcharge2.JobNo,
                             //Soano = surcharge != null ? surcharge.Soano : surcharge2.Soano,
                             //PaySoano = surcharge != null ? surcharge.PaySoano : surcharge2.PaySoano,
                             //CreditNo = surcharge != null ? surcharge.CreditNo : surcharge2.CreditNo,
                             //DebitNo = surcharge != null ? surcharge.DebitNo : surcharge2.DebitNo,
                             UserCreatedName = emp == null ? string.Empty : emp.EmployeeNameEn,
                             DatetimeCreated = data.DatetimeCreated
                         };
            IQueryable<AcctCombineBillingResult> dataResult = null;
            if (result != null && result.Count() > 0)
            {
                dataResult = result.OrderByDescending(x => x.DatetimeCreated).AsQueryable();
            }
            else
            {
                return null;
            }
            return dataResult;
        }

        /// <summary>
        /// Create query to get data
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private Expression<Func<AcctCombineBilling, bool>> ExpressionQuery(AcctCombineBillingCriteria criteria)
        {
            Expression<Func<AcctCombineBilling, bool>> query = q => true;

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(x => x.PartnerId == criteria.PartnerId);
            }

            if (criteria.CreatedDateFrom != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.CreatedDateFrom.Value.Date && x.DatetimeCreated.Value.Date <= criteria.CreatedDateTo.Value.Date);
            }

            if (criteria.Creator != null && criteria.Creator.Count > 0)
            {
                query = query.And(x => criteria.Creator.Any(z => z == x.UserCreated));
            }

            return query;
        }
        #endregion
        /// <summary>
        /// Check Esiting Combine No With Document No
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public string CheckDocumentNoExisted(ShipmentCombineCriteria criteria)
        {
            var existCombineNo = string.Empty;
            if (!string.IsNullOrEmpty(criteria.DocumentType) && criteria.DocumentNo != null && criteria.DocumentNo.Count > 0)
            {
                // Get issued surcharge info with partnerId
                var surchargeInfo = surchargeRepo.Get(x => x.Type != "OBH" && criteria.PartnerId == x.PaymentObjectId && (!string.IsNullOrEmpty(x.Soano) || !string.IsNullOrEmpty(x.PaySoano) || !string.IsNullOrEmpty(x.CreditNo) || !string.IsNullOrEmpty(x.DebitNo)));
                var surchargeOBHInfo = surchargeRepo.Get(x => x.Type == "OBH" && ((criteria.PartnerId == x.PaymentObjectId && (!string.IsNullOrEmpty(x.Soano) || !string.IsNullOrEmpty(x.DebitNo))) || (criteria.PartnerId == x.PayerId && (!string.IsNullOrEmpty(x.PaySoano) || !string.IsNullOrEmpty(x.CreditNo)))));
                switch (criteria.DocumentType)
                {
                    case "CD Note":
                        var existCDNotes = cdNoteRepo.Get(x => criteria.DocumentNo.Any(z => z == x.Code) && !string.IsNullOrEmpty(x.CombineBillingNo)).Select(x => x.Code).ToList();
                        foreach (var item in existCDNotes)
                        {
                            //var existingCredit = surchargeRepo.Get(x => (string.IsNullOrEmpty(x.CombineBillingNo) && string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.CreditNo);
                            // Type Credit
                            var combineNo = surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.CreditNo).Select(x => x.CombineBillingNo).ToList();
                            combineNo.AddRange(surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.CreditNo).Select(x => x.ObhcombineBillingNo).ToList());
                            combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                            if (combineNo.Count > 0 && !combineNo.Any(z => z == criteria.CombineNo))
                            {
                                existCombineNo = item + " has been existed in CB: " + string.Join(";", combineNo);
                                return existCombineNo;
                            }

                            // Type debit
                            combineNo = surchargeRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && item.Trim() == x.DebitNo).Select(x => x.CombineBillingNo).Distinct().ToList();
                            if (combineNo.Count > 0 && !combineNo.Any(z => z == criteria.CombineNo))
                            {
                                existCombineNo = item + " has been existed in CB: " + string.Join(";", combineNo);
                                return existCombineNo;
                            }
                        }
                        break;
                    case "Soa":
                        var existSoa = soaRepo.Get(x => criteria.DocumentNo.Any(z => z == x.Soano) && !string.IsNullOrEmpty(x.CombineBillingNo)).Select(x => x.Soano).ToList();
                        foreach (var item in existSoa)
                        {
                            // Type Debit
                            //var existingSoa = surchargeRepo.Get(x => string.IsNullOrEmpty(x.CombineBillingNo) && item.Trim() == x.Soano);
                            var combineNo = surchargeRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && item.Trim() == x.Soano).Select(x => x.CombineBillingNo).Distinct().ToList();
                            if (combineNo.Count > 0 && !combineNo.Any(z => z == criteria.CombineNo))
                            {
                                existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                return existCombineNo;
                            }

                            // Type Credit
                            //existingSoa = surchargeRepo.Get(x => (string.IsNullOrEmpty(x.CombineBillingNo) && string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.PaySoano);
                            combineNo = surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.PaySoano).Select(x => x.CombineBillingNo).ToList();
                            combineNo.AddRange(surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.PaySoano).Select(x => x.ObhcombineBillingNo).ToList());
                            combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                            if (combineNo.Count > 0 && !combineNo.Any(z => z == criteria.CombineNo))
                            {
                                existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                return existCombineNo;
                            }
                        }
                        break;
                    case "Job No":
                        foreach (var item in criteria.DocumentNo)
                        {
                            var jobNoInfo = surchargeInfo.Where(x => item.Trim() == x.JobNo && (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && (x.CombineBillingNo != criteria.CombineNo && x.ObhcombineBillingNo != criteria.CombineNo));
                            var jobNoObhInfo = surchargeOBHInfo.Where(x => item.Trim() == x.JobNo && (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && (x.CombineBillingNo != criteria.CombineNo && x.ObhcombineBillingNo != criteria.CombineNo));
                            if (jobNoInfo.Count() > 0)
                            {
                                var combineNo = jobNoInfo.Select(x => x.CombineBillingNo).ToList();
                                combineNo.AddRange(jobNoInfo.Select(x => x.ObhcombineBillingNo).ToList());
                                combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                                if (combineNo.Count > 0)
                                {
                                    existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                    return existCombineNo;
                                }
                            }
                            if (jobNoObhInfo.Count() > 0)
                            {
                                var combineNo = jobNoObhInfo.Select(x => x.CombineBillingNo).ToList();
                                combineNo.AddRange(jobNoObhInfo.Select(x => x.ObhcombineBillingNo).ToList());
                                combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                                if (combineNo.Count > 0)
                                {
                                    existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                    return existCombineNo;
                                }
                            }
                        }
                        break;
                    case "HBL No":
                        foreach (var item in criteria.DocumentNo)
                        {
                            var hblnoInfo = surchargeInfo.Where(x => item.Trim() == x.Hblno && (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && (criteria.Services.Count == 0 || criteria.Services.Any(z => z == x.TransactionType)));
                            var hblnoObhInfo = surchargeOBHInfo.Where(x => item.Trim() == x.Hblno && (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && (criteria.Services.Count == 0 || criteria.Services.Any(z => z == x.TransactionType)));

                            if (hblnoInfo.Count() > 0)
                            {
                                var combineNo = hblnoInfo.Select(x => x.CombineBillingNo).ToList();
                                combineNo.AddRange(hblnoInfo.Select(x => x.ObhcombineBillingNo).ToList());
                                combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                                if (combineNo.Count > 0)
                                {
                                    existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                    return existCombineNo;
                                }
                            }

                            if (hblnoObhInfo.Count() > 0)
                            {
                                var combineNo = hblnoObhInfo.Select(x => x.CombineBillingNo).ToList();
                                combineNo.AddRange(hblnoObhInfo.Select(x => x.ObhcombineBillingNo).ToList());
                                combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                                if (combineNo.Count > 0)
                                {
                                    existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                    return existCombineNo;
                                }
                            }
                        }
                        break;
                    case "Custom No":
                        foreach (var item in criteria.DocumentNo)
                        {
                            var jobNo = customsDeclarationRepo.Get(x => x.ClearanceNo == item.Trim() && !string.IsNullOrEmpty(x.JobNo)).Select(x => x.JobNo);
                            var jobNoInfo = surchargeInfo.Where(x => jobNo.Any(z => z == x.JobNo) && (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && (x.CombineBillingNo != criteria.CombineNo && x.ObhcombineBillingNo != criteria.CombineNo));
                            var jobNoObhInfo = surchargeOBHInfo.Where(x => jobNo.Any(z => z == x.JobNo) && (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && (x.CombineBillingNo != criteria.CombineNo && x.ObhcombineBillingNo != criteria.CombineNo));
                            if (jobNoInfo.Count() > 0)
                            {
                                var combineNo = jobNoInfo.Select(x => x.CombineBillingNo).ToList();
                                combineNo.AddRange(jobNoInfo.Select(x => x.ObhcombineBillingNo).ToList());
                                combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                                if (combineNo.Count > 0)
                                {
                                    existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                    return existCombineNo;
                                }
                            }
                            if (jobNoObhInfo.Count() > 0)
                            {
                                var combineNo = jobNoObhInfo.Select(x => x.CombineBillingNo).ToList();
                                combineNo.AddRange(jobNoObhInfo.Select(x => x.ObhcombineBillingNo).ToList());
                                combineNo = combineNo.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                                if (combineNo.Count > 0)
                                {
                                    existCombineNo = item.Trim() + " has been existed in CB: " + string.Join(";", combineNo);
                                    return existCombineNo;
                                }
                            }
                        }
                        break;
                }
            }
            return existCombineNo;
        }

        /// <summary>
        /// Get detail list in combine with search
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private List<sp_GetShipmentDataInCombineBilling> SearchDataCombine(ShipmentCombineCriteria criteria)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@partnerId", Value = criteria.PartnerId },
                new SqlParameter(){ ParameterName = "@issueDateFrom", Value = criteria.IssuedDateFrom?.Date },
                new SqlParameter(){ ParameterName = "@issueDateTo", Value = criteria.IssuedDateTo?.Date },
                new SqlParameter(){ ParameterName = "@serviceDateFrom", Value = criteria.ServiceDateFrom?.Date },
                new SqlParameter(){ ParameterName = "@serviceDateTo", Value = criteria.ServiceDateTo?.Date },
                new SqlParameter(){ ParameterName = "@Type", Value = criteria.Type },
                new SqlParameter(){ ParameterName = "@services", Value = string.Join(';', criteria.Services.Where(x=>!string.IsNullOrEmpty(x.Trim()))) },
                new SqlParameter(){ ParameterName = "@documentType", Value = criteria.DocumentType },
                new SqlParameter(){ ParameterName = "@documentNo", Value = criteria.DocumentNo == null? null: string.Join(';', criteria.DocumentNo.Where(x=>!string.IsNullOrEmpty(x.Trim()))) }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetShipmentDataInCombineBilling>(parameters);
            return list;
        }

        /// <summary>
        /// Get list data in combine billing detail
        /// </summary>
        /// <param name="combineNo"></param>
        /// <returns></returns>
        private List<sp_GetDetailWithCombineNo> GetDataListDetail(string combineNo)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@combineNo", Value = combineNo }
            };
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetDetailWithCombineNo>(parameters);
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public AcctCombineBillingModel GetCombineBillingDetailList(ShipmentCombineCriteria criteria)
        {
            var result = SearchDataCombine(criteria);
            var resultGrp = result.Where(x => !string.IsNullOrEmpty(x.Refno)).GroupBy(x => new { x.Refno, x.HblId }).OrderBy(x => x.Key.Refno).ThenBy(x => x.Key.HblId).Select(x => new { x.Key, data = x.Select(z => new { z.Hwbno, z.Type, z.JobNo, z.Mblno, z.CustomNo, z.Amount, z.Currency, z.AmountUsd, z.AmountVnd }) });
            var dataResult = new AcctCombineBillingModel();
            dataResult.TotalAmountVnd = 0;
            dataResult.TotalAmountUsd = 0;
            dataResult.Shipments = new List<CombineBillingShipmentModel>();
            foreach (var item in resultGrp)
            {
                var detail = new CombineBillingShipmentModel();
                detail.Refno = item.Key.Refno;
                detail.Hblid = item.Key.HblId;
                detail.Type = item.data.FirstOrDefault().Type.ToLower().Contains("debit") ? "Debit" : "Credit";
                detail.JobNo = item.data.FirstOrDefault()?.JobNo;
                detail.Hwbno = item.data.FirstOrDefault()?.Hwbno;
                detail.Mblno = item.data.FirstOrDefault()?.Mblno;
                detail.CustomNo = item.data.FirstOrDefault()?.CustomNo;
                detail.Amount = item.data.Sum(x => x.Amount ?? 0) * (detail.Type.ToLower() == "debit" ? 1 : -1);
                var totalUsd = item.data.Where(x => x.Currency == AccountingConstants.CURRENCY_USD).Sum(x => x.Amount ?? 0);
                var totalVnd = item.data.Where(x => x.Currency == AccountingConstants.CURRENCY_LOCAL).Sum(x => x.Amount ?? 0);
                var totalStr = string.Empty;
                if (detail.Type == "Debit")
                {
                    totalStr = totalUsd != 0 ? (totalUsd < 0 ? "(" + (totalUsd * -1).ToString("N02") + ")" : totalUsd.ToString("N02")) + " USD" : totalStr;
                    totalStr += !string.IsNullOrEmpty(totalStr) && totalVnd != 0 ? " | " : string.Empty;
                    totalStr += totalVnd != 0 ? (totalVnd < 0 ? "(" + (totalVnd * -1).ToString("N0") + ")" : totalVnd.ToString("N0")) + " VND" : string.Empty;
                }
                else
                {
                    totalStr = totalUsd != 0 ? (totalUsd < 0 ? (totalUsd * -1).ToString("N02") : "(" + totalUsd.ToString("N02") + ")") + " USD" : totalStr;
                    totalStr += !string.IsNullOrEmpty(totalStr) && totalVnd != 0 ? " | " : string.Empty;
                    totalStr += totalVnd != 0 ? (totalVnd < 0 ? (totalVnd * -1).ToString("N0") : "(" + totalVnd.ToString("N0") + ")") + " VND" : string.Empty;
                }
                detail.AmountStr = totalStr;
                detail.AmountVnd = item.data.Sum(x => x.AmountVnd ?? 0) * (detail.Type.ToLower() == "debit" ? 1 : -1);
                detail.AmountUsd = item.data.Sum(x => x.AmountUsd ?? 0) * (detail.Type.ToLower() == "debit" ? 1 : -1);
                dataResult.TotalAmountVnd += (detail.AmountVnd ?? 0);
                dataResult.TotalAmountUsd += (detail.AmountUsd ?? 0);
                dataResult.Shipments.Add(detail);
            }
            return dataResult;
        }

        /// <summary>
        /// get data detail of combine
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AcctCombineBillingModel GetCombineBillingDetailWithId(string Id)
        {
            var model = DataContext.Get(x => x.Id.ToString() == Id);
            if (model.FirstOrDefault() == null)
            {
                return null;
            }
            var combine = model.FirstOrDefault();
            var combineData = new AcctCombineBillingModel();
            combineData = mapper.Map<AcctCombineBillingModel>(combine);
            var userCreated = userRepo.Get(x => x.Id == combine.UserCreated);
            var userModified = userRepo.Get(x => x.Id == combine.UserModified);
            combineData.UserCreatedName = userCreated.FirstOrDefault()?.Username;
            combineData.UserModifiedName = userModified.FirstOrDefault()?.Username;

            var result = GetDataListDetail(combine.CombineBillingNo);
            var resultGrp = result.GroupBy(x => new { x.Refno, x.JobNo, x.Mblno, x.HblId, x.Hwbno }).Select(x => new { x.Key, data = x.Select(z => new { z.CustomNo, z.Type, z.Currency, z.Amount, z.AmountVnd, z.AmountUsd }) });

            combineData.Shipments = new List<CombineBillingShipmentModel>();
            foreach (var item in resultGrp)
            {
                var detail = new CombineBillingShipmentModel();
                detail.Refno = item.Key.Refno;
                detail.Hblid = item.Key.HblId;
                detail.Type = item.data.FirstOrDefault().Type.ToLower().Contains("debit") ? "Debit" : "Credit";
                detail.JobNo = item.Key.JobNo;
                detail.Hwbno = item.Key.Hwbno;
                detail.Mblno = item.Key.Mblno;
                detail.CustomNo = item.data.FirstOrDefault()?.CustomNo;
                detail.Amount = item.data.Sum(x => x.Amount ?? 0) * (detail.Type.ToLower() == "debit" ? 1 : -1);
                var totalUsd = item.data.Where(x => x.Currency == AccountingConstants.CURRENCY_USD).Sum(x => x.Amount ?? 0);
                var totalVnd = item.data.Where(x => x.Currency == AccountingConstants.CURRENCY_LOCAL).Sum(x => x.Amount ?? 0);
                var totalStr = string.Empty;
                if (detail.Type == "Debit")
                {
                    totalStr = totalUsd != 0 ? (totalUsd < 0 ? "(" + (totalUsd * -1).ToString("N02") + ")" : totalUsd.ToString("N02")) + " USD" : totalStr;
                    totalStr += !string.IsNullOrEmpty(totalStr) && totalVnd != 0 ? " | " : string.Empty;
                    totalStr += totalVnd != 0 ? (totalVnd < 0 ? "(" + (totalVnd * -1).ToString("N0") + ")" : totalVnd.ToString("N0")) + " VND" : string.Empty;
                }
                else
                {
                    totalStr = totalUsd != 0 ? (totalUsd < 0 ? (totalUsd * -1).ToString("N02") : "(" + totalUsd.ToString("N02") + ")") + " USD" : totalStr;
                    totalStr += !string.IsNullOrEmpty(totalStr) && totalVnd != 0 ? " | " : string.Empty;
                    totalStr += totalVnd != 0 ? (totalVnd < 0 ? (totalVnd * -1).ToString("N0") : "(" + totalVnd.ToString("N0") + ")") + " VND" : string.Empty;
                }
                detail.AmountStr = totalStr;
                detail.AmountVnd = item.data.Sum(x => x.AmountVnd ?? 0) * (detail.Type.ToLower() == "debit" ? 1 : -1);
                detail.AmountUsd = item.data.Sum(x => x.AmountUsd ?? 0) * (detail.Type.ToLower() == "debit" ? 1 : -1);
                combineData.Shipments.Add(detail);
            }
            return combineData;
        }


        /// <summary>
        /// Get Data To Preview Debit
        /// </summary>
        /// <param name="acctCdNoteList">AcctCdnoteModel List</param>
        /// <param name="isOrigin"></param>
        /// <returns></returns>
        public CombineBillingDebitDetailsModel GetDataPreviewDebitNoteTemplate(AcctCombineBillingModel combineDetail)
        {
            var model = new CombineBillingDebitDetailsModel();
            var listJob = combineDetail.Shipments.Select(x => x.JobNo).ToList();
            var opsTransaction = opsTransactionRepo.Get(x => listJob.Any(z => z == x.JobNo)).FirstOrDefault();
            var partner = partnerRepo.Get(x => x.Id == combineDetail.PartnerId).FirstOrDefault();
            var currentCombine = DataContext.Get(x => x.Id == combineDetail.Id).FirstOrDefault();
            model.JobNo = opsTransaction?.JobNo;
            model.CBM = opsTransaction?.SumCbm;
            model.GW = opsTransaction?.SumGrossWeight;
            model.NW = opsTransaction?.SumNetWeight;
            model.ServiceDate = opsTransaction?.ServiceDate;
            model.HbLadingNo = opsTransaction?.Hwbno;
            model.MbLadingNo = opsTransaction?.Hwbno;
            model.SumContainers = opsTransaction?.SumContainers;
            model.SumPackages = opsTransaction?.SumPackages;
            model.ServiceMode = opsTransaction?.ServiceMode;
            model.CommodityGroupId = opsTransaction?.CommodityGroupId;
            model.HbConstainers = opsTransaction?.ContainerDescription;
            model.PartnerId = partner?.Id;
            model.PartnerNameEn = partner?.PartnerNameEn;
            model.PartnerPersonalContact = partner?.ContactPerson;
            model.PartnerShippingAddress = partner?.AddressEn; //Billing Address Name En
            model.PartnerTel = partner?.Tel;
            model.PartnerTaxcode = partner?.TaxCode;
            model.PartnerFax = partner?.Fax;
            model.CreatedDate = DateTime.Now.ToString("dd'/'MM'/'yyyy");
            model.UserCreated = currentCombine?.UserCreated;
            model.GroupId = currentCombine?.GroupId;
            model.DepartmentId = currentCombine?.DepartmentId;
            model.OfficeId = currentCombine?.OfficeId;
            model.CompanyId = currentCombine?.CompanyId;
            model.CombineBillingNo = combineDetail.CombineBillingNo;
            if (opsTransaction?.WarehouseId != null)
            {
                model.WarehouseName = catPlaceRepo.Get(x => x.Id == opsTransaction.WarehouseId).FirstOrDefault()?.NameEn;
            }

            var places = catPlaceRepo.Get();
            var pol = opsTransaction == null ? null : places.FirstOrDefault(x => x.Id == opsTransaction.Pol);
            var pod = opsTransaction == null ? null : places.FirstOrDefault(x => x.Id == opsTransaction.Pod);
            model.Pol = pol?.NameEn;
            if (model.Pol != null)
            {
                model.PolCountry = pol == null ? null : countryRepo.Get().FirstOrDefault(x => x.Id == pol.CountryId)?.NameEn;
            }
            model.PolName = pol?.NameEn;
            model.Pod = pod?.NameEn;
            if (model.Pod != null)
            {
                model.PodCountry = pod == null ? null : countryRepo.Get().FirstOrDefault(x => x.Id == pod.CountryId)?.NameEn;
            }
            model.PodName = pod?.NameEn;

            List<CsShipmentSurchargeDetailsModel> listSurcharges = new List<CsShipmentSurchargeDetailsModel>();
            var catChargeSearch = catChargeRepo.Get().ToLookup(x => x.Id);
            foreach (var shipment in combineDetail.Shipments)
            {
                var charges = surchargeRepo.Get(x => x.JobNo == shipment.JobNo && x.Hblid == shipment.Hblid && (x.CombineBillingNo == combineDetail.CombineBillingNo || x.ObhcombineBillingNo == combineDetail.CombineBillingNo) && (x.Soano == shipment.Refno || x.PaySoano == shipment.Refno || x.CreditNo == shipment.Refno || x.DebitNo == shipment.Refno)).ToList();
                foreach (var item in charges)
                {
                    if (!listSurcharges.Any(x => x.Id == item.Id))
                    {
                        var charge = mapper.Map<CsShipmentSurchargeDetailsModel>(item);
                        var catCharge = catChargeSearch[charge.ChargeId].FirstOrDefault();
                        charge.Currency = catCurrencyRepo.Get(x => x.Id == charge.CurrencyId).FirstOrDefault()?.CurrencyName;
                        charge.ChargeCode = catCharge?.Code;
                        charge.NameEn = catCharge?.ChargeNameEn;
                        charge.BillingType = shipment.Type;
                        listSurcharges.Add(charge);
                    }
                }
            }
            model.ListSurcharges = listSurcharges;
            return model;
        }

        /// <summary>
        /// Preview report with data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public Crystal PreviewCombineDebitTemplate(CombineBillingDebitDetailsModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var user = sysUserLevelRepo.Get(x => x.UserId == model.UserCreated && x.GroupId == model.GroupId && x.DepartmentId == model.DepartmentId && x.OfficeId == model.OfficeId && x.CompanyId == model.CompanyId).FirstOrDefault();
            // Thông tin Company của Creator
            var companyOfUser = user == null ? null : sysCompanyRepo.Get(x => x.Id == user.CompanyId).FirstOrDefault();
            //Lấy thông tin Office của Creator
            var officeOfUser = user == null ? null : sysOfficeRepo.Get(x => x.Id == user.OfficeId).FirstOrDefault();
            var _accountName = officeOfUser?.BankAccountNameVn ?? string.Empty;
            var _accountNameEN = officeOfUser?.BankAccountNameEn ?? string.Empty;
            var _bankName = officeOfUser?.BankNameLocal ?? string.Empty;
            var _bankNameEN = officeOfUser?.BankNameEn ?? string.Empty;
            var _bankAddress = officeOfUser?.BankAddressLocal ?? string.Empty;
            var _bankAddressEN = officeOfUser?.BankAddressEn ?? string.Empty;
            var _swiftAccs = officeOfUser?.SwiftCode ?? string.Empty;
            var _accsUsd = officeOfUser?.BankAccountUsd ?? string.Empty;
            var _accsVnd = officeOfUser?.BankAccountVnd ?? string.Empty;
            var commodity = model.CommodityGroupId == null ? "N/A" : catCommodityGroupRepo.Get(x => x.Id == model.CommodityGroupId).Select(x => x.GroupNameEn).FirstOrDefault();

            IQueryable<CustomsDeclaration> _customClearances = customsDeclarationRepo.Get(x => x.JobNo == model.JobNo && !string.IsNullOrEmpty(model.JobNo));
            CustomsDeclaration _clearance = null;
            if (_customClearances.Count() > 0 || _customClearances != null)
            {
                var orderClearance = _customClearances.OrderBy(x => x.ClearanceDate);
                _clearance = orderClearance.FirstOrDefault();

            }

            var parameter = new CombineDebitTemplatReportParams
            {
                DBTitle = "DEBIT NOTE",
                DebitNo = model.CombineBillingNo,
                TotalDebit = string.Empty,
                TotalCredit = string.Empty,
                DueToTitle = "N/A",
                DueTo = "N/A",
                DueToCredit = "N/A",
                SayWordAll = "N/A",
                CompanyName = companyOfUser?.BunameEn?.ToUpper(),
                CompanyAddress1 = officeOfUser?.AddressEn,
                CompanyAddress2 = "Tel: " + officeOfUser?.Tel + "\nFax: " + officeOfUser?.Fax,
                CompanyDescription = "N/A",
                Website = companyOfUser?.Website,
                IbanCode = "N/A",
                AccountName = _accountName,
                AccountNameEN = _accountNameEN,
                BankName = _bankName,
                BankNameEN = _bankNameEN,
                SwiftAccs = _swiftAccs,
                AccsUSD = _accsUsd,
                AccsVND = _accsVnd,
                BankAddress = _bankAddress,
                BankAddressEN = _bankAddressEN,
                Paymentterms = "N/A",
                DecimalNo = 2,
                CurrDecimal = 2,
                IssueInv = "N/A",
                InvoiceInfo = "N/A",
                Contact = currentUser.UserName,
                IssuedDate = model.CreatedDate,
                OtherRef = "N/A",
                IsOrigin = true
            };
            string trans = string.Empty;
            string port = string.Empty;
            if (model.ServiceMode == "Export")
            {
                trans = "X";
                port = model.Pol;
            }
            else
            {
                port = model.Pod;
            }
            var listSOA = new List<CombineDebitTemplatReport>();
            if (model.ListSurcharges.Count > 0)
            {
                foreach (var item in model.ListSurcharges)
                {
                    string subject = string.Empty;
                    if (item.Type == "OBH")
                    {
                        subject = "ON BEHALF";
                    }

                    decimal? _vatAmount = 0, _vatAmountUsd = 0, exchangeRateToUsd = 0, exchangeRateToVnd = 0;
                    exchangeRateToVnd = currencyExchangeService.CurrencyExchangeRateConvert(item.FinalExchangeRate, item.ExchangeDate, item.CurrencyId, AccountingConstants.CURRENCY_LOCAL);
                    _vatAmount = item.VatAmountVnd;

                    var acctCDNo = new CombineDebitTemplatReport
                    {
                        SortIndex = null,
                        Subject = subject,
                        PartnerID = model.PartnerId,
                        PartnerName = model.PartnerNameEn?.ToUpper(),
                        PersonalContact = model.PartnerPersonalContact?.ToUpper(),
                        Address = model.PartnerShippingAddress?.ToUpper(),
                        Taxcode = model.PartnerTaxcode?.ToUpper(),
                        Workphone = model.PartnerTel?.ToUpper(),
                        Fax = model.PartnerFax?.ToUpper(),
                        TransID = trans,
                        LoadingDate = null,
                        Commodity = commodity,
                        PortofLading = model.PolName?.ToUpper(),
                        PortofUnlading = model.PodName?.ToUpper(),
                        MAWB = model.MbLadingNo,
                        Invoice = item.InvoiceNo,
                        EstimatedVessel = "N/A",
                        ArrivalDate = null,
                        Noofpieces = null,
                        Delivery = null,
                        HWBNO = model.HbLadingNo,
                        Description = item.NameEn,
                        Quantity = item.AmountVnd * (item.BillingType == "Debit" ? 1 : -1),
                        QUnit = "N/A",
                        UnitPrice = 1, //Cộng thêm phần thập phân
                        VAT = ((_vatAmount ?? 0) * (item.BillingType == "Debit" ? 1 : -1)) + _decimalNumber, //Cộng thêm phần thập phân
                        Debit = _decimalNumber, //Cộng thêm phần thập phân
                        Credit = _decimalNumber, //Cộng thêm phần thập phân
                        Notes = item.Notes,
                        InputData = "N/A",
                        PONo = string.Empty,
                        TransNotes = "N/A",
                        Shipper = model.PartnerNameEn?.ToUpper(),
                        Consignee = model.PartnerNameEn?.ToUpper(),
                        ContQty = model.HbConstainers,
                        ContSealNo = "N/A",
                        Deposit = null,
                        DepositCurr = "N/A",
                        DecimalSymbol = "N/A",
                        DigitSymbol = "N/A",
                        DecimalNo = null,
                        CurrDecimalNo = null,
                        VATInvoiceNo = item.InvoiceNo,
                        GW = model.GW,
                        NW = model.NW,
                        SeaCBM = model.CBM,
                        SOTK = _clearance?.ClearanceNo,
                        NgayDK = null,
                        Cuakhau = port,
                        DeliveryPlace = model.WarehouseName?.ToUpper(),
                        TransDate = null,
                        Unit = AccountingConstants.CURRENCY_LOCAL,
                        UnitPieaces = "N/A",
                        CustomDate = _clearance?.ClearanceDate,
                        JobNo = model.JobNo,
                        ExchangeRateToUsd = exchangeRateToUsd,
                        ExchangeRateToVnd = exchangeRateToVnd,
                        ExchangeVATToUsd = (_vatAmountUsd ?? 0) + _decimalNumber
                    };
                    listSOA.Add(acctCDNo);
                }
            }
            else
            {
                return null;
            }

            result = new Crystal
            {
                ReportName = "LogisticsDebitNewDNTT.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(listSOA);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);
            return result;
        }

        public CombineOPSModel GetDataExportCombineOps(string combineBillingNo)
        {
            CombineOPSModel ops = new CombineOPSModel();
            var combine = DataContext.Get(x => x.CombineBillingNo == combineBillingNo).FirstOrDefault();
            if (combine == null) { return ops; }

            var charges = GetChargeByCombineNo(combineBillingNo);

            List<ExportCombineOPS> lstOps = new List<ExportCombineOPS>();
            var res = charges.GroupBy(x => new { BillingNo = x.CombineBillingType == "SOA" ? x.SOANo : x.CDNote, x.JobId, x.HBL }).AsQueryable();
            foreach (var grp in res)
            {
                ExportCombineOPS exportSOAOPS = new ExportCombineOPS();
                exportSOAOPS.Charges = new List<ChargeCombineResult>();
                var csTransactionInfo = csTransactionRepo.Get(x => x.JobNo == grp.Key.JobId).FirstOrDefault();
                var commodity = csTransactionInfo?.Commodity;

                var commodityGroup = opsTransactionRepo.Get(x => x.JobNo == grp.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();
                string commodityName = string.Empty;
                if (commodity != null)
                {
                    // CR: 07/02/22 => air: get commodityName từ combobox, sea: get commodityName từ textbox
                    if (csTransactionInfo.TransactionType == "AI" || csTransactionInfo.TransactionType == "AE")
                    {
                        string[] commodityArr = commodity.Split(',');
                        foreach (var item in commodityArr)
                        {
                            commodityName = commodityName + "," + catCommodityRepo.Get(x => x.Code == item.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                        }
                        commodityName = commodityName.Substring(1);
                    }
                    else
                    {
                        commodityName = commodity.Replace("\n", " ");
                    }
                }
                if (commodityGroup != null)
                {
                    commodityName = catCommodityGroupRepo.Get(x => x.Id == commodityGroup).Select(t => t.GroupNameVn).FirstOrDefault();
                }
                exportSOAOPS.CommodityName = commodityName;

                exportSOAOPS.HwbNo = grp.Select(t => t.HBL).FirstOrDefault();
                exportSOAOPS.CBM = grp.Select(t => t.CBM).FirstOrDefault();
                exportSOAOPS.GW = grp.Select(t => t.GrossWeight).FirstOrDefault();
                exportSOAOPS.PackageContainer = grp.Select(t => t.PackageContainer).FirstOrDefault();
                exportSOAOPS.Charges.AddRange(grp.Select(t => t).ToList());
                lstOps.Add(exportSOAOPS);
            }
            ops.exportOPS = lstOps;
            var partner = partnerRepo.Get(x => x.Id == combine.PartnerId).FirstOrDefault();
            ops.BillingAddressVN = partner?.AddressVn;
            ops.PartnerNameVN = partner?.PartnerNameVn;
            //opssoa.FromDate = soa.SoaformDate;
            ops.No = combineBillingNo;
            var isCommonOffice = sysOfficeRepo.Any(x => x.Id == currentUser.OfficeID && DataTypeEx.IsCommonOffice(x.Code));
            ops.IsDisplayLogo = isCommonOffice;

            foreach (var item in ops.exportOPS)
            {
                foreach (var it in item.Charges)
                {
                    it.VATAmount = it.VATAmountLocal;
                    it.NetAmount = it.AmountVND;
                    if (it.BillingType == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                    {
                        it.VATAmount *= (-1);
                        it.NetAmount *= (-1);
                    }
                }
            }

            if (combine.ServiceDateFrom != null)
            {
                ops.FromDate = combine.ServiceDateFrom;
                ops.ToDate = combine.ServiceDateTo;
            }
            else if (combine.IssuedDateFrom != null)
            {
                ops.FromDate = combine.IssuedDateFrom;
                ops.ToDate = combine.IssuedDateTo;
            }
            else
            {
                var chagerOrder = charges.OrderByDescending(x => x.ExchangeDate);
                var lastdate = chagerOrder.FirstOrDefault();
                var firstdate = chagerOrder.LastOrDefault();
                ops.FromDate = firstdate != null ? firstdate.ExchangeDate : null;
                ops.ToDate = lastdate != null ? lastdate.ExchangeDate : null;
            }

            return ops;
        }

        private IQueryable<ChargeCombineResult> GetChargeByCombineNo(string combineBillingNo)
        {
            var surCharges = surchargeRepo.Get(x => x.CombineBillingNo == combineBillingNo || x.ObhcombineBillingNo == combineBillingNo);

            var result = new List<ChargeCombineResult>();

            foreach (var sur in surCharges)
            {
                var charge = catChargeRepo.Get().Where(x => x.Id == sur.ChargeId).FirstOrDefault();
                var unit = catUnitRepo.Get().Where(x => x.Id == sur.UnitId).FirstOrDefault();
                var cus = customsDeclarationRepo.Get().Where(x => x.JobNo == sur.JobNo).FirstOrDefault();
                DateTime? _serviceDate, _createdDate, _shippmentDate;
                string _service, _userCreated, _commodity, _flightNo, _packageContainer, _customNo;
                Guid? _aol, _aod;
                int? _packageQty;
                decimal? _grossWeight, _chargeWeight, _cbm;
                if (sur.TransactionType == "CL")
                {
                    var opst = opsTransactionRepo.Get().Where(x => x.Hblid == sur.Hblid).FirstOrDefault();
                    _serviceDate = opst?.ServiceDate;
                    _createdDate = opst?.DatetimeCreated;
                    _service = "CL";
                    _userCreated = opst?.UserCreated;
                    _commodity = string.Empty;
                    _flightNo = string.Empty;
                    _shippmentDate = null;
                    _aol = null;
                    _aod = null;
                    _packageContainer = string.Empty;
                    _packageQty = opst?.SumPackages;
                    _grossWeight = opst?.SumGrossWeight;
                    _chargeWeight = opst?.SumChargeWeight;
                    _cbm = opst?.SumCbm;
                    _customNo = cus != null ? cus.ClearanceNo : string.Empty;
                }
                else
                {
                    var csTransDe = csTransactionDetailRepo.Get(x => x.Id == sur.Hblid).FirstOrDefault();
                    var csTrans = csTransDe == null ? new CsTransaction() : csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && x.Id == csTransDe.JobId).FirstOrDefault();
                    //_serviceDate = (csTrans?.TransactionType == "AI" || csTrans?.TransactionType == "SFI" || csTrans?.TransactionType == "SLI" || csTrans?.TransactionType == "SCI") ?
                    //    csTrans?.Eta : csTrans?.Etd;
                    _serviceDate = csTrans?.ServiceDate;
                    _createdDate = csTrans?.DatetimeCreated;
                    _service = csTrans.TransactionType;
                    _userCreated = csTrans?.UserCreated;
                    _commodity = csTrans?.Commodity;
                    _flightNo = csTransDe?.FlightNo;
                    _shippmentDate = csTrans?.TransactionType == "AE" ? csTransDe?.Etd : csTrans?.TransactionType == "AI" ? csTransDe?.Eta : null;
                    _aol = csTrans?.Pol;
                    _aod = csTrans?.Pod;
                    _packageQty = csTransDe?.PackageQty;
                    _grossWeight = csTransDe?.GrossWeight;
                    _chargeWeight = csTransDe?.ChargeWeight;
                    _cbm = csTransDe?.Cbm;
                    _packageContainer = csTransDe?.PackageContainer;
                    _customNo = cus != null ? cus.ClearanceNo : string.Empty;
                }

                bool _isSynced = false;
                if (sur.Type == AccountingConstants.TYPE_CHARGE_OBH)
                {
                    _isSynced = !string.IsNullOrEmpty(sur.PaySyncedFrom) && (sur.PaySyncedFrom.Equals("SOA") || sur.PaySyncedFrom.Equals("CDNOTE") || sur.PaySyncedFrom.Equals("VOUCHER") || sur.PaySyncedFrom.Equals("SETTLEMENT"));
                }
                else
                {
                    _isSynced = !string.IsNullOrEmpty(sur.SyncedFrom) && (sur.SyncedFrom.Equals("SOA") || sur.SyncedFrom.Equals("CDNOTE") || sur.SyncedFrom.Equals("VOUCHER") || sur.SyncedFrom.Equals("SETTLEMENT"));
                }

                var soa = soaRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && x.CombineBillingNo.Contains(combineBillingNo) && (x.Soano == sur.Soano || x.Soano == sur.PaySoano)).FirstOrDefault();
                var cdNote = cdNoteRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && x.CombineBillingNo.Contains(combineBillingNo) && (x.Code == sur.CreditNo || x.Code == sur.DebitNo)).FirstOrDefault();
                var exRate = soa != null ? soa.ExcRateUsdToLocal : cdNote.ExcRateUsdToLocal;

                var chg = new ChargeCombineResult()
                {
                    ID = sur.Id,
                    HBLID = sur.Hblid,
                    ChargeID = sur.ChargeId,
                    ChargeCode = charge?.Code,
                    ChargeName = charge?.ChargeNameEn,
                    JobId = sur.JobNo,
                    HBL = sur.Hblno,
                    MBL = sur.Mblno,
                    Type = sur.Type,
                    CustomNo = _customNo,
                    Debit = sur.Type == AccountingConstants.TYPE_CHARGE_SELL || (sur.Type == AccountingConstants.TYPE_CHARGE_OBH) ? (decimal?)sur.Total : null,
                    Credit = sur.Type == AccountingConstants.TYPE_CHARGE_BUY || (sur.Type == AccountingConstants.TYPE_CHARGE_OBH) ? (decimal?)sur.Total : null,
                    DebitLocal = sur.Type == AccountingConstants.TYPE_CHARGE_SELL || (sur.Type == AccountingConstants.TYPE_CHARGE_OBH) ? (sur.AmountVnd ?? 0) + (sur.VatAmountVnd ?? 0) : (decimal?)null,
                    CreditLocal = sur.Type == AccountingConstants.TYPE_CHARGE_BUY || (sur.Type == AccountingConstants.TYPE_CHARGE_OBH) ? (sur.AmountVnd ?? 0) + (sur.VatAmountVnd ?? 0) : (decimal?)null,
                    DebitUSD = sur.Type == AccountingConstants.TYPE_CHARGE_SELL || (sur.Type == AccountingConstants.TYPE_CHARGE_OBH) ? (sur.AmountUsd ?? 0) + (sur.VatAmountUsd ?? 0) : (decimal?)null,
                    CreditUSD = sur.Type == AccountingConstants.TYPE_CHARGE_BUY || (sur.Type == AccountingConstants.TYPE_CHARGE_OBH) ? (sur.AmountUsd ?? 0) + (sur.VatAmountUsd ?? 0) : (decimal?)null,
                    SOANo = soa != null ? soa.Soano : null,
                    IsOBH = false,
                    Currency = sur.CurrencyId,
                    InvoiceNo = sur.InvoiceNo,
                    Note = sur.Notes,
                    CustomerID = sur.PaymentObjectId,
                    ServiceDate = _serviceDate,
                    CreatedDate = _createdDate,
                    InvoiceIssuedDate = sur.InvoiceDate,
                    TransactionType = _service,
                    UserCreated = _userCreated,
                    Commodity = _commodity,
                    FlightNo = _flightNo,
                    ShippmentDate = _shippmentDate,
                    AOL = _aol,
                    AOD = _aod,
                    Quantity = sur.Quantity,
                    UnitId = sur.UnitId,
                    Unit = unit?.UnitNameEn,
                    UnitPrice = sur.UnitPrice,
                    VATRate = sur.Vatrate,
                    VATAmountLocal = sur.VatAmountVnd,
                    VATAmountUSD = sur.VatAmountUsd,
                    PackageQty = _packageQty,
                    GrossWeight = _grossWeight,
                    ChargeWeight = _chargeWeight,
                    CBM = _cbm,
                    PackageContainer = _packageContainer,
                    DatetimeModified = sur.DatetimeModified,
                    CommodityGroupID = null,
                    Service = _service,
                    CDNote = cdNote != null ? cdNote.Code : null,
                    TypeCharge = charge?.Type,
                    ExchangeDate = sur.ExchangeDate,
                    FinalExchangeRate = exRate,
                    NetAmount = sur.NetAmount,
                    AmountVND = sur.AmountVnd,
                    AmountUSD = sur.AmountUsd,
                    SeriesNo = sur.SeriesNo,
                    InvoiceDate = sur.InvoiceDate,
                    TaxCodeOBH = (sur.Type == AccountingConstants.TYPE_CHARGE_OBH && !string.IsNullOrEmpty(sur.PaymentObjectId)) ? partnerRepo.Get(x => x.Id == sur.PaymentObjectId).Select(x => x.TaxCode).FirstOrDefault() : string.Empty,
                    CombineNo = combineBillingNo,
                    CombineBillingType = soa != null ? "SOA" : "CDNOTE",
                    BillingType = soa != null ? soa.Type.ToUpper() : cdNote.Type.ToUpper()
                    //BillingType = ((soa != null && !string.IsNullOrEmpty(sur.PaySoano)) || (cdNote != null && !string.IsNullOrEmpty(sur.CreditNo))) ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : AccountingConstants.ACCOUNTANT_TYPE_DEBIT
                };
                result.Add(chg);
            }
            return result.OrderBy(x => x.Service).AsQueryable();
        }

        private IQueryable<ChargeCombineResult> GetChargeSellByCombineNo(string combineBillingNo)
        {
            var surCharges = surchargeRepo.Get(x => (x.CombineBillingNo == combineBillingNo || x.ObhcombineBillingNo == combineBillingNo) && x.Type != "BUY");

            var result = new List<ChargeCombineResult>();

            foreach (var sur in surCharges)
            {
                var charge = catChargeRepo.Get().Where(x => x.Id == sur.ChargeId).FirstOrDefault();
                var unit = catUnitRepo.Get().Where(x => x.Id == sur.UnitId).FirstOrDefault();
                var cus = customsDeclarationRepo.Get().Where(x => x.JobNo == sur.JobNo).FirstOrDefault();
                string _commodity, _packageContainer, _customNo, _billingNo = "";
                decimal? _chargeWeight, _cbm;
                if (sur.TransactionType == "CL")
                {
                    var opst = opsTransactionRepo.Get().Where(x => x.Hblid == sur.Hblid).FirstOrDefault();
                    _commodity = opst.Note;
                    _packageContainer = string.Empty;
                    _chargeWeight = opst?.SumChargeWeight;
                    _cbm = opst?.SumCbm;
                    _customNo = cus != null ? cus.ClearanceNo : string.Empty;
                }
                else
                {
                    var csTransDe = csTransactionDetailRepo.Get(x => x.Id == sur.Hblid).FirstOrDefault();
                    var csTrans = csTransDe == null ? new CsTransaction() : csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && x.Id == csTransDe.JobId).FirstOrDefault();
                    _commodity = csTrans?.Commodity;
                    _chargeWeight = csTransDe?.ChargeWeight;
                    _cbm = csTransDe?.Cbm;
                    _packageContainer = csTransDe?.PackageContainer;
                    _customNo = cus != null ? cus.ClearanceNo : string.Empty;
                }

                if (sur.Type == AccountingConstants.TYPE_CHARGE_OBH)
                {
                    switch (sur.PaySyncedFrom)
                    {
                        case "SOA": _billingNo = sur.PaySoano;
                            break;
                        case "CDNOTE": _billingNo = sur.DebitNo;
                            break;
                        case "VOUCHER":
                            _billingNo = sur.VoucherId;
                            break;
                        case "SETTLEMENT":
                            _billingNo = sur.SettlementCode;
                            break;
                    }
                }
                else
                {
                    switch (sur.SyncedFrom)
                    {
                        case "SOA":
                            _billingNo = sur.Soano;
                            break;
                        case "CDNOTE":
                            _billingNo = sur.DebitNo;
                            break;
                        case "VOUCHER":
                            _billingNo = sur.VoucherId;
                            break;
                        case "SETTLEMENT":
                            _billingNo = sur.SettlementCode;
                            break;
                    }
                }

                var chg = new ChargeCombineResult()
                {
                    JobId = sur.JobNo,
                    HBL = sur.Hblno,
                    CustomNo = _customNo,
                    InvoiceNo = sur.InvoiceNo,
                    Commodity = _commodity,
                    UnitId = sur.UnitId,
                    ChargeWeight = _chargeWeight,
                    CBM = _cbm,
                    PackageContainer = _packageContainer,
                    NetAmount = sur.NetAmount,
                    AmountVND = sur.AmountVnd,
                    AmountUSD = sur.AmountUsd,
                    VATAmountLocal = sur.VatAmountVnd,
                    VATAmountUSD = sur.VatAmountUsd,
                    CombineNo = combineBillingNo,
                    TypeCharge = sur.Type,
                    BillingNo = _billingNo,
                    TransactionType = sur.TransactionType,
                    ExchangeDate = sur.ExchangeDate,
                };
                result.Add(chg);
            }
            return result.OrderBy(x => x.Service).AsQueryable();
        }

        public Crystal PreviewConfirmBilling(string combineBillingNo)
        {
            Crystal result = null;
            var combineBilling = DataContext.Get(x => x.CombineBillingNo == combineBillingNo).FirstOrDefault();
            if (combineBilling == null) return null;
            var partner = partnerRepo.Get(x => x.Id == combineBilling.PartnerId).FirstOrDefault();

            //var charges = surchargeRepo.Get(x => x.CombineBillingNo == combineBillingNo);
            //if (charges == null) return null;
            //var grpInvCdNoteByHbl = charges.GroupBy(g => new { g.Hblid, g.InvoiceNo, g.CreditNo, g.DebitNo }).Select(s => new { s.Key.Hblid, s.Key.InvoiceNo, CdNote = s.Key.CreditNo ?? s.Key.DebitNo });

            var charges = GetChargeByCombineNo(combineBillingNo);
            if (combineBilling == null) return null;
            var grpInvCdNoteByHbl = charges.GroupBy(g => new { g.HBLID, g.InvoiceNo, g.CreditNo, g.DebitNo }).Select(s => new { s.Key.HBLID, s.Key.InvoiceNo, CdNote = s.Key.CreditNo ?? s.Key.DebitNo });

            var combineCharges = new List<CombineReportGeneral>();
            foreach (var charge in charges)
            {
                string _mawb = string.Empty;
                string _hwbNo = string.Empty;
                string _customNo = string.Empty;
                string _jobNo = string.Empty;

                #region -- Info MBL, HBL --
                _mawb = charge.MBL;
                _hwbNo = charge.HBL;
                //_customNo = charge.TransactionType == "CL" ? charge.ClearanceNo : string.Empty;
                var cus = customsDeclarationRepo.Get().Where(x => x.JobNo == charge.JobId).FirstOrDefault();
                _customNo = cus != null ? cus.ClearanceNo : string.Empty;
                _jobNo = charge.JobId;
                #endregion -- Info MBL, HBL --

                #region -- Info CD Note --
                var cdNote = cdNoteRepo.Get(x => (charge.Type == "SELL" ? charge.DebitNo : charge.CreditNo) == x.Code).FirstOrDefault();
                #endregion -- Info CD Note --

                // Exchange Rate from currency charge to current soa
                // decimal _amount = currencyExchangeService.ConvertAmountChargeToAmountObj(charge, charge.CurrencyId);
                // decimal _amount = currencyExchangeService.ConvertAmountChargeToAmountObj(charge, "VND");

                decimal _amount = (charge.AmountVND + charge.VATAmountLocal) ?? 0;
                if (charge.BillingType == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                    _amount = _amount * (-1);

                var c = new CombineReportGeneral();
                c.PartnerID = partner?.Id;
                c.PartnerName = partner?.PartnerNameEn?.ToUpper(); //Name En
                c.PersonalContact = partner?.ContactPerson?.ToUpper();
                c.Email = string.Empty; //NOT USE
                c.Address = partner?.AddressEn?.ToUpper(); //Address En 
                c.Workphone = partner?.Tel;
                c.Fax = string.Empty; //NOT USE
                c.Taxcode = string.Empty; //NOT USE
                c.TransID = string.Empty; //NOT USE
                c.MAWB = _mawb; //MBLNo
                c.HWBNO = _hwbNo; //HBLNo
                c.DateofInv = cdNote?.DatetimeCreated?.ToString("MMM dd, yy") ?? string.Empty; //Created Datetime CD Note
                c.Order = string.Empty; //NOT USE
                c.InvID = charge.InvoiceNo;
                c.Amount = _amount + _decimalNumber; //Cộng thêm phần thập phân
                //c.Curr = charge.CurrencyId?.Trim(); //Currency SOA
                c.Curr = "VND"; //Currency SOA
                c.Dpt = charge.Type == AccountingConstants.TYPE_CHARGE_SELL ? true : false;
                c.Vessel = string.Empty; //NOT USE
                c.Routine = string.Empty; //NOT USE
                c.LoadingDate = null; //NOT USE
                c.CustomerID = string.Empty; //NOT USE
                c.CustomerName = string.Empty; //NOT USE
                c.ArrivalDate = null; //NOT USE
                c.TpyeofService = string.Empty; //NOT USE
                c.SOANO = string.Empty; //NOT USE
                c.SOADate = null; //NOT USE
                c.FromDate = null; //NOT USE
                c.ToDate = null; //NOT USE
                c.OAmount = null; //NOT USE
                c.SAmount = null; //NOT USE
                c.CurrOP = string.Empty; //NOT USE
                c.Notes = string.Empty; //NOT USE
                c.IssuedBy = string.Empty; //NOT USE
                c.Shipper = string.Empty; //NOT USE
                c.Consignee = string.Empty; //NOT USE
                c.OtherRef = string.Empty; //NOT USE
                c.Volumne = string.Empty; //NOT USE
                c.POBH = null; //NOT USE
                c.ROBH = (charge.Type == AccountingConstants.TYPE_CHARGE_OBH) ? _amount : 0;
                c.ROBH = c.ROBH + _decimalNumber; //Cộng thêm phần thập phân
                c.CustomNo = _customNo;
                c.JobNo = _jobNo;
                c.CdCode = cdNote?.Code;
                var grpInvCdNote = grpInvCdNoteByHbl.Where(w => (!string.IsNullOrEmpty(w.InvoiceNo) || !string.IsNullOrEmpty(w.CdNote)) && w.HBLID == charge.HBLID).ToList();
                if (grpInvCdNote.Count > 0)
                {
                    c.Docs = string.Join("\r\n", grpInvCdNote.Select(s => !string.IsNullOrEmpty(s.InvoiceNo) ? s.InvoiceNo : s.CdNote).Distinct()); //Ưu tiên: Invoice No >> CD Note Code
                }

                combineCharges.Add(c);
            }
            //Sắp xếp giảm dần theo số Job
            combineCharges = combineCharges.ToArray().OrderByDescending(o => o.JobNo).ToList();
            var parameter = new CombineReportGeneralReportParams();
            var p = partnerRepo.Get(x => x.Id == combineBilling.PartnerId).FirstOrDefault();
            var office = sysOfficeRepo.Get(x => x.Id == p.OfficeId).FirstOrDefault();

            if (combineBilling.ServiceDateFrom != null)
                parameter.UptoDate = string.Format("{0} - {1}", combineBilling.ServiceDateFrom?.ToString("dd/MM/yyyy") ?? string.Empty, combineBilling.ServiceDateTo?.ToString("dd/MM/yyyy") ?? string.Empty); //
            else if (combineBilling.IssuedDateFrom != null)
                parameter.UptoDate = string.Format("{0} - {1}", combineBilling.IssuedDateFrom?.ToString("dd/MM/yyyy") ?? string.Empty, combineBilling.IssuedDateTo?.ToString("dd/MM/yyyy") ?? string.Empty); //
            else
            {
                var chagerOrder = charges.OrderByDescending(x => x.ExchangeDate);
                var lastdate = chagerOrder.FirstOrDefault();
                var firstdate = chagerOrder.LastOrDefault();
                parameter.UptoDate = string.Format("{0} - {1}", firstdate.ExchangeDate?.ToString("dd/MM/yyyy") ?? string.Empty, lastdate.ExchangeDate?.ToString("dd/MM/yyyy") ?? string.Empty); //
            }

            parameter.dtPrintDate = combineBilling.DatetimeCreated?.ToString("dd/MM/yyyy") ?? string.Empty; //Created Date SOA
            parameter.CompanyName = office?.BranchNameEn.ToUpper() ?? string.Empty;
            parameter.CompanyDescription = string.Empty; //NOT USE
            parameter.CompanyAddress1 = office?.AddressEn;
            parameter.CompanyAddress2 = string.Format(@"Tel: {0} Fax: {1}", office?.Tel, office?.Fax);
            parameter.Website = string.Empty; //Office ko có field Website
            parameter.IbanCode = string.Empty; //NOT USE
            parameter.AccountName = office?.BankAccountNameEn?.ToUpper() ?? string.Empty;
            parameter.AccountNameEn = office?.BankAccountNameVn?.ToUpper() ?? string.Empty;
            parameter.BankName = office?.BankNameLocal?.ToUpper() ?? string.Empty;
            parameter.BankNameEn = office?.BankNameEn?.ToUpper() ?? string.Empty;
            parameter.SwiftAccs = office?.SwiftCode ?? string.Empty;
            parameter.AccsUSD = office?.BankAccountUsd ?? string.Empty;
            parameter.AccsVND = office?.BankAccountVnd ?? string.Empty;
            parameter.BankAddress = office?.BankAddressLocal?.ToUpper() ?? string.Empty;
            parameter.BankAddressEn = office?.BankAddressEn?.ToUpper() ?? string.Empty;
            parameter.Paymentterms = string.Empty; //NOT USE
            parameter.Contact = currentUser.UserName?.ToUpper() ?? string.Empty;
            parameter.CurrDecimalNo = 3;
            parameter.RefNo = charges.FirstOrDefault()?.CombineNo; //SOA No
            parameter.Email = office?.Email ?? string.Empty;

            result = new Crystal
            {
                ReportName = "CombineBillingGeneral.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            result.AddDataSource(combineCharges);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);

            return result;
        }

        /// <summary>
        /// Get Combine OPS data with partner and currency
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public CombineOPSModel GetDataExportCombineOpsByPartner(AcctCombineBillingCriteria criteria)
        {
            var combineDatas = GetData(criteria);
            if (criteria.ReferenceNo == null)
            {
                criteria.ReferenceNo = combineDatas == null ? new List<string>() : AppendCombineNo(combineDatas);
            }
            List<string> combineNos = new List<string>();
            foreach(var item in combineDatas)
            {
                combineNos.Add(item.CombineBillingNo);
            }
            var opsModel = new CombineOPSModel();
            //var combineDatas = DataContext.Get(x => criteria.ReferenceNo.Any(z => z == x.CombineBillingNo));
            if (combineDatas == null || combineDatas.Count() == 0) { return opsModel; }
            var surcharges = surchargeRepo.Get(x => combineNos.Contains(x.CombineBillingNo) || combineNos.Contains(x.ObhcombineBillingNo)); 
            var chargeDatas = catChargeRepo.Get();
            var unitDatas = catUnitRepo.Get();
            var clearanceDatas = customsDeclarationRepo.Get(x => !string.IsNullOrEmpty(x.JobNo));
            var jobList = surcharges.Select(x => x.JobNo).ToList();
            var opsData = opsTransactionRepo.Get().Where(x => x.CurrentStatus != TermData.Canceled && jobList.Any(z => z == x.JobNo));
            var csTransData = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled && jobList.Any(z => z == x.JobNo));
            #region Get surcharges
            var dataOps = from sur in surcharges
                          join charge in chargeDatas on sur.ChargeId equals charge.Id
                          join unit in unitDatas on sur.UnitId equals unit.Id
                          join opst in opsData on sur.Hblid equals opst.Hblid
                          select new ChargeCombineResult()
                          {
                              ID = sur.Id,
                              HBLID = sur.Hblid,
                              ChargeID = sur.ChargeId,
                              ChargeCode = charge.Code,
                              ChargeName = charge.ChargeNameEn,
                              JobId = sur.JobNo,
                              HBL = sur.Hblno,
                              MBL = sur.Mblno,
                              Type = sur.Type,
                              CustomNo = null,
                              Debit = null,
                              Credit = null,
                              DebitLocal = null,
                              CreditLocal = null,
                              DebitUSD = null,
                              CreditUSD = null,
                              SOANo = sur.Soano,
                              PaySoaNo = sur.PaySoano,
                              IsOBH = false,
                              Currency = sur.CurrencyId,
                              InvoiceNo = sur.InvoiceNo,
                              Note = sur.Notes,
                              CustomerID = sur.PaymentObjectId,
                              ServiceDate = opst.ServiceDate,
                              CreatedDate = opst.DatetimeCreated,
                              InvoiceIssuedDate = sur.InvoiceDate,
                              TransactionType = sur.TransactionType,
                              UserCreated = opst.UserCreated,
                              Commodity = string.Empty,
                              FlightNo = string.Empty,
                              ShippmentDate = null,
                              AOL = null,
                              AOD = null,
                              Quantity = sur.Quantity,
                              UnitId = sur.UnitId,
                              Unit = unit.UnitNameEn,
                              UnitPrice = sur.UnitPrice,
                              VATRate = sur.Vatrate,
                              VATAmount = criteria.Currency == AccountingConstants.CURRENCY_LOCAL ? sur.VatAmountVnd : sur.VatAmountUsd,
                              VATAmountLocal = null,
                              VATAmountUSD = null,
                              PackageQty = opst.SumPackages,
                              GrossWeight = opst.SumGrossWeight,
                              ChargeWeight = opst.SumChargeWeight,
                              CBM = opst.SumCbm,
                              PackageContainer = opst.ContainerDescription,
                              CreditNo = sur.CreditNo,
                              DebitNo = sur.DebitNo,
                              DatetimeModified = sur.DatetimeModified,
                              CommodityGroupID = null,
                              Service = "CL",
                              CDNote = null,
                              TypeCharge = charge.Type,
                              ExchangeDate = sur.ExchangeDate,
                              FinalExchangeRate = sur.FinalExchangeRate,
                              NetAmount = criteria.Currency == AccountingConstants.CURRENCY_LOCAL ? sur.AmountVnd : sur.AmountUsd,
                              AmountVND = null,
                              AmountUSD = null,
                              SeriesNo = sur.SeriesNo,
                              InvoiceDate = sur.InvoiceDate,
                              TaxCodeOBH = string.Empty,
                              CombineNo = combineNos.Where(com => com == sur.CombineBillingNo || com == sur.ObhcombineBillingNo).FirstOrDefault(),
                          };
            var dataDoc = from sur in surcharges
                          join charge in chargeDatas on sur.ChargeId equals charge.Id
                          join unit in unitDatas on sur.UnitId equals unit.Id
                          join csTransDe in csTransactionDetailRepo.Get() on sur.Hblid equals csTransDe.Id
                          join csTran in csTransData on csTransDe.JobId equals csTran.Id
                          select new ChargeCombineResult()
                          {
                              ID = sur.Id,
                              HBLID = sur.Hblid,
                              ChargeID = sur.ChargeId,
                              ChargeCode = charge.Code,
                              ChargeName = charge.ChargeNameEn,
                              JobId = sur.JobNo,
                              HBL = sur.Hblno,
                              MBL = sur.Mblno,
                              Type = sur.Type,
                              CustomNo = null,
                              Debit = null,
                              Credit = null,
                              DebitLocal = null,
                              CreditLocal = null,
                              DebitUSD = null,
                              CreditUSD = null,
                              SOANo = sur.Soano,
                              PaySoaNo = sur.PaySoano,
                              IsOBH = false,
                              Currency = sur.CurrencyId,
                              InvoiceNo = sur.InvoiceNo,
                              Note = sur.Notes,
                              CustomerID = sur.PaymentObjectId,
                              ServiceDate = csTran.ServiceDate,
                              CreatedDate = csTran.DatetimeCreated,
                              InvoiceIssuedDate = sur.InvoiceDate,
                              TransactionType = csTran.TransactionType,
                              UserCreated = csTran.UserCreated,
                              Commodity = csTran.Commodity,
                              FlightNo = csTransDe.FlightNo,
                              ShippmentDate = csTran.TransactionType == "AE" ? csTransDe.Etd : (csTran.TransactionType == "AI" ? csTransDe.Eta : null),
                              AOL = csTran.Pol,
                              AOD = csTran.Pod,
                              Quantity = sur.Quantity,
                              UnitId = sur.UnitId,
                              Unit = unit.UnitNameEn,
                              UnitPrice = sur.UnitPrice,
                              VATRate = sur.Vatrate,
                              VATAmount = criteria.Currency == AccountingConstants.CURRENCY_LOCAL ? sur.VatAmountVnd : sur.VatAmountUsd,
                              VATAmountLocal = null,
                              VATAmountUSD = null,
                              PackageQty = csTransDe.PackageQty,
                              GrossWeight = csTransDe.GrossWeight,
                              ChargeWeight = csTransDe.ChargeWeight,
                              CBM = csTransDe.Cbm,
                              PackageContainer = csTransDe.PackageContainer,
                              CreditNo = sur.CreditNo,
                              DebitNo = sur.DebitNo,
                              DatetimeModified = sur.DatetimeModified,
                              CommodityGroupID = null,
                              Service = "CL",
                              CDNote = null,
                              TypeCharge = charge.Type,
                              ExchangeDate = sur.ExchangeDate,
                              FinalExchangeRate = sur.FinalExchangeRate,
                              NetAmount = criteria.Currency == AccountingConstants.CURRENCY_LOCAL ? sur.AmountVnd : sur.AmountUsd,
                              AmountVND = null,
                              AmountUSD = null,
                              SeriesNo = sur.SeriesNo,
                              InvoiceDate = sur.InvoiceDate,
                              TaxCodeOBH = string.Empty,
                              CombineNo = combineNos.Where(com => com == sur.CombineBillingNo || com == sur.ObhcombineBillingNo).FirstOrDefault()
                          };
            #endregion
            var dataCharges = new List<ChargeCombineResult>();
            dataCharges.AddRange(dataOps);
            if (dataDoc.Count() > 0)
            {
                dataCharges.AddRange(dataDoc);
            }
            foreach (var item in dataCharges)
            {
                item.CustomNo = item.Service == "CL" ? clearanceDatas.Where(x => x.JobNo == item.JobId).OrderBy(x => x.ClearanceDate).FirstOrDefault()?.ClearanceNo : null;
                var soaData = soaRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && x.CombineBillingNo.Contains(item.CombineNo) && (x.Soano == item.SOANo || x.Soano == item.PaySoaNo)).FirstOrDefault();
                if (soaData != null)
                {
                    item.SOANo = soaData.Soano;
                    item.FinalExchangeRate = soaData.ExcRateUsdToLocal;
                    item.CombineBillingType = "SOA";
                    item.BillingType = soaData.Type.ToLower().Contains(AccountingConstants.ACCOUNTANT_TYPE_CREDIT.ToLower()) ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : AccountingConstants.ACCOUNTANT_TYPE_DEBIT;
                }
                else
                {
                    var cdNote = cdNoteRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && x.CombineBillingNo.Contains(item.CombineNo) && (x.Code == item.CreditNo || x.Code == item.DebitNo)).FirstOrDefault();
                    item.CDNote = cdNote.Code;
                    item.FinalExchangeRate = cdNote.ExcRateUsdToLocal;
                    item.CombineBillingType = "CDNOTE";
                    item.BillingType = cdNote.Type.ToLower().Contains(AccountingConstants.ACCOUNTANT_TYPE_CREDIT.ToLower()) ? AccountingConstants.ACCOUNTANT_TYPE_CREDIT : AccountingConstants.ACCOUNTANT_TYPE_DEBIT;
                }
                if (item.BillingType == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
                {
                    item.VATAmount = item.VATAmount * (-1);
                    item.NetAmount = item.NetAmount * (-1);
                }
            }

            List<ExportCombineOPS> lstOps = new List<ExportCombineOPS>();

            var res = dataCharges.OrderBy(x => x.CombineNo).GroupBy(x => new { x.CombineNo, BillingNo = x.CombineBillingType == "SOA" ? x.SOANo : x.CDNote, x.JobId, x.HBL }).ToList();
            foreach (var grp in res)
            {
                ExportCombineOPS exportSOAOPS = new ExportCombineOPS();
                exportSOAOPS.Charges = new List<ChargeCombineResult>();
                var csTransactionInfo = csTransactionRepo.Get(x => x.JobNo == grp.Key.JobId).FirstOrDefault();
                var commodity = csTransactionInfo?.Commodity;

                var commodityGroup = opsTransactionRepo.Get(x => x.JobNo == grp.Key.JobId).Select(t => t.CommodityGroupId).FirstOrDefault();
                string commodityName = string.Empty;
                if (commodity != null)
                {
                    // CR: 07/02/22 => air: get commodityName từ combobox, sea: get commodityName từ textbox
                    if (csTransactionInfo.TransactionType == "AI" || csTransactionInfo.TransactionType == "AE")
                    {
                        string[] commodityArr = commodity.Split(',');
                        foreach (var item in commodityArr)
                        {
                            commodityName = commodityName + "," + catCommodityRepo.Get(x => x.Code == item.Replace("\n", "")).Select(t => t.CommodityNameEn).FirstOrDefault();
                        }
                        commodityName = commodityName.Substring(1);
                    }
                    else
                    {
                        commodityName = commodity.Replace("\n", " ");
                    }
                }
                if (commodityGroup != null)
                {
                    commodityName = catCommodityGroupRepo.Get(x => x.Id == commodityGroup).Select(t => t.GroupNameVn).FirstOrDefault();
                }
                exportSOAOPS.CommodityName = commodityName;

                exportSOAOPS.HwbNo = grp.Select(t => t.HBL).FirstOrDefault();
                exportSOAOPS.CBM = grp.Select(t => t.CBM).FirstOrDefault();
                exportSOAOPS.GW = grp.Select(t => t.GrossWeight).FirstOrDefault();
                exportSOAOPS.PackageContainer = grp.Select(t => t.PackageContainer).FirstOrDefault();
                exportSOAOPS.Charges.AddRange(grp.Select(t => t).ToList());
                lstOps.Add(exportSOAOPS);
            }

            opsModel.exportOPS = lstOps;
            var partner = partnerRepo.Get(x => x.Id == criteria.PartnerId).FirstOrDefault();
            opsModel.BillingAddressVN = partner?.AddressVn;
            opsModel.PartnerNameVN = partner?.PartnerNameVn;
            var isCommonOffice = sysOfficeRepo.Any(x => x.Id == currentUser.OfficeID && DataTypeEx.IsCommonOffice(x.Code));
            opsModel.IsDisplayLogo = isCommonOffice;
            if (criteria.CreatedDateFrom != null)
            {
                opsModel.FromDate = criteria.CreatedDateFrom;
                opsModel.ToDate = criteria.CreatedDateTo;
            }
            return opsModel;
        }

        private string GetBillingNo(CsShipmentSurcharge sur)
        {
            //if(sur.Type== AccountingConstants.TYPE_CHARGE_SELL)
            //{
            //    switch (sur.SyncedFrom)
            //    {
            //        case "SOA":
            //            return sur.Soano;
            //        case "CDNOTE":
            //            return sur.DebitNo;
            //        default: return "";
            //    }
            //}
            //else if (sur.Type == AccountingConstants.TYPE_CHARGE_OBH)
            //{
            //    switch (sur.SyncedFrom)
            //    {
            //        case "SOA":
            //            return sur.PaySoano;
            //        case "CDNOTE":
            //            return sur.DebitNo;
            //        default: return "";
            //    }
            //}
            //else if(sur.Type== AccountingConstants.TYPE_CHARGE_OBH_BUY)
            //{
            //    switch (sur.SyncedFrom)
            //    {
            //        case "SOA":
            //            return sur.PaySoano;
            //        case "CDNOTE":
            //            return sur.CreditNo;
            //        default: return "";
            //    }
            //}
            var soaData = soaRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && (x.CombineBillingNo.Contains(sur.CombineBillingNo)||x.CombineBillingNo.Contains(sur.ObhcombineBillingNo)) && (x.Soano == sur.Soano || x.Soano == sur.PaySoano)).FirstOrDefault();
            if (soaData != null)
            {
                return soaData.Soano;
            }
            else
            {
                var cdNote = cdNoteRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && (x.CombineBillingNo.Contains(sur.CombineBillingNo) || x.CombineBillingNo.Contains(sur.ObhcombineBillingNo)) && (x.Code == sur.CreditNo || x.Code == sur.DebitNo)).FirstOrDefault();
                return cdNote.Code;
            }
            return null;
        }

        private string getCommonditiJobService(string code)
        {
            switch (code)
            {
                case "AI":
                    return "Air Import";
                case "AE":
                    return "Air Export";
                case "SFE": 
                    return "Sea FCL Export";
                case "SLE": 
                    return "Sea LCL Export";
                case "SFI": 
                    return "Sea FCL Import";
                case "SLI": 
                    return "Sea LCL Import";
                case "SCE": 
                    return "Sea Consol Export";
                case "SCI": 
                    return "Sea Consol Import";
                case "IT": 
                    return "Inland Trucking";
                default:return null;

            }
        }


        private List<string> AppendCombineNo(IQueryable<AcctCombineBillingResult> combineDatas)
        {
            List<string> combineList = new List<string>();
            combineDatas.ToList().ForEach(x =>
            combineList.Add(x.CombineBillingNo)
            );
            return combineList;
        }

        /// <summary>
        /// Get Combine OPS data with partner and currency
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public CombineShipmentModel GetDataExportCombineShipmentByPartner(AcctCombineBillingCriteria criteria)
        {
            var combineDatas = GetData(criteria);
            if(criteria.ReferenceNo == null)
            {
                criteria.ReferenceNo = combineDatas == null ? new List<string>() : AppendCombineNo(combineDatas);
            }
            List<string> combineNos = new List<string>();
            foreach (var item in combineDatas)
            {
                combineNos.Add(item.CombineBillingNo);
            }
            var shipmentModel = new CombineShipmentModel();
            //var combineDatas = DataContext.Get(x => criteria.ReferenceNo.Any(z => z == x.CombineBillingNo));
            if (combineDatas == null || combineDatas.Count() == 0) { return shipmentModel; }
            var surcharges = surchargeRepo.Get(x => combineNos.Contains(x.CombineBillingNo) || combineNos.Contains(x.ObhcombineBillingNo));
            var chargeDatas = catChargeRepo.Get();
            var unitDatas = catUnitRepo.Get();
            // var clearanceDatas = customsDeclarationRepo.Get();
            // var jobList = surcharges.Select(x => x.JobNo).ToList();
            var opsData = opsTransactionRepo.Get().Where(x => x.CurrentStatus != TermData.Canceled);
            var csTransData = csTransactionRepo.Get(x => x.CurrentStatus != TermData.Canceled);
            #region Get surcharges

            var dataOps = from sur in surcharges
                          join charge in chargeDatas on sur.ChargeId equals charge.Id
                          join unit in unitDatas on sur.UnitId equals unit.Id
                          join opst in opsData on sur.Hblid equals opst.Hblid
                          //join clear in clearanceDatas on sur.JobNo equals clear.JobNo
                          select new ChargeCombineResult()
                          {
                              JobId = sur.JobNo,
                              HBL = sur.Hblno,
                              CustomNo = sur.ClearanceNo,
                              InvoiceNo = sur.InvoiceNo,
                              Commodity = opst.Note,
                              UnitId = sur.UnitId,
                              ChargeWeight = opst.SumGrossWeight,
                              CBM = opst.SumCbm,
                              PackageContainer = opst.ContainerDescription,
                              AmountVND = sur.AmountVnd,
                              AmountUSD = sur.AmountUsd,
                              VATAmountLocal = sur.VatAmountVnd,
                              VATAmountUSD = sur.VatAmountUsd,
                              CombineNo = combineNos.Where(com => com == sur.CombineBillingNo || com == sur.ObhcombineBillingNo).FirstOrDefault(),
                              TypeCharge = sur.Type,
                              BillingNo = GetBillingNo(sur),
                              TransactionType = sur.TransactionType,
                              ExchangeDate = sur.ExchangeDate,
                              CreditNo=sur.CreditNo,
                              DebitNo=sur.DebitNo,
                              ServiceDate=opst.ServiceDate,
                          };
            var dataDoc = from sur in surcharges
                          join charge in chargeDatas on sur.ChargeId equals charge.Id
                          join unit in unitDatas on sur.UnitId equals unit.Id
                          join csTransDe in csTransactionDetailRepo.Get() on sur.Hblid equals csTransDe.Id
                          join csTran in csTransData on csTransDe.JobId equals csTran.Id
                          // join clear in clearanceDatas on sur.JobNo equals clear.JobNo
                          select new ChargeCombineResult()
                          {
                              JobId = sur.JobNo,
                              HBL = sur.Hblno,
                              CustomNo = sur.ClearanceNo,
                              InvoiceNo = sur.InvoiceNo,
                              Commodity = getCommonditiJobService(csTran.TransactionType),
                              UnitId = sur.UnitId,
                              ChargeWeight = csTran.GrossWeight,
                              CBM = csTransDe.Cbm,
                              PackageContainer = csTransDe.PackageContainer,
                              AmountVND = sur.AmountVnd,
                              AmountUSD = sur.AmountUsd,
                              VATAmountLocal = sur.VatAmountVnd,
                              VATAmountUSD = sur.VatAmountUsd,
                              CombineNo = combineNos.Where(com => com == sur.CombineBillingNo || com == sur.ObhcombineBillingNo).FirstOrDefault(),
                              TypeCharge = sur.Type,
                              BillingNo = GetBillingNo(sur),
                              TransactionType = sur.TransactionType,
                              ExchangeDate = sur.ExchangeDate,
                              CreditNo = sur.CreditNo,
                              DebitNo = sur.DebitNo,
                              ServiceDate=csTran.ServiceDate,
                          };
            #endregion
            var dataCharges = Enumerable.Empty<ChargeCombineResult>().AsQueryable();
            //dataCharges.AddRange(dataOps);
            //if (dataDoc.Count() > 0)
            //{
            //    dataCharges.AddRange(dataDoc);
            //}
            dataCharges = dataOps.Union(dataDoc);

            List<ExportCombineShipment> lstShipment = new List<ExportCombineShipment>();

            var res = dataCharges.OrderBy(x => x.CombineNo).GroupBy(x => new { x.CombineNo, x.JobId, x.HBL, x.BillingNo }).ToList();
            foreach (var grp in res)
            {
                var grpData = grp.FirstOrDefault();
                ExportCombineShipment exportCombineShipment = new ExportCombineShipment();
                exportCombineShipment.CommodityName = grpData.Commodity;
                exportCombineShipment.JobNo = grpData.JobId;
                exportCombineShipment.CustomDeclarationNo = grpData.CustomNo;
                exportCombineShipment.InvoiceNo = String.Join(";", grp.Where(t => t.TransactionType == "CL" && t.TypeCharge != "OBH" && !string.IsNullOrEmpty(t.InvoiceNo)).Select(t => t.InvoiceNo));
                exportCombineShipment.OBHInvoice = String.Join(";", grp.Where(t => t.TypeCharge == "OBH" && !string.IsNullOrEmpty(t.InvoiceNo)).Select(t => t.InvoiceNo));
                exportCombineShipment.FreInvoice = String.Join(";", grp.Where(t => t.TransactionType != "CL" && t.TypeCharge != "OBH" && !string.IsNullOrEmpty(t.InvoiceNo)).Select(t => t.InvoiceNo));
                exportCombineShipment.KGS = grpData.ChargeWeight;
                exportCombineShipment.CBM = grpData.CBM;
                exportCombineShipment.CombineNo = grpData.CombineNo;
                exportCombineShipment.BillingNo = grpData.BillingNo;
                exportCombineShipment.CusFee = grp.Where(t => t.TypeCharge != "OBH" && t.TransactionType == "CL").Select(t => criteria.Currency == "VND" ? (t.TypeCharge == "BUY" ? t.AmountVND * -1 : t.AmountVND) : (t.TypeCharge == "BUY" ? t.AmountUSD * -1 : t.AmountUSD)).Sum();
                exportCombineShipment.CusVAT = grp.Where(t => t.TypeCharge != "OBH" && t.TransactionType == "CL").Select(t => criteria.Currency == "VND" ? (t.TypeCharge == "BUY" ? t.VATAmountLocal*-1:t.VATAmountLocal) : (t.TypeCharge == "BUY" ? t.VATAmountUSD*-1:t.VATAmountUSD)).Sum();
                exportCombineShipment.AuthFee = grp.Where(t => t.TypeCharge == "OBH").Select(t => criteria.Currency == "VND" ? (t.CreditNo!=null && t.DebitNo==null? t.AmountVND*-1:t.AmountVND) : ((t.CreditNo != null && t.DebitNo == null) ? t.AmountUSD * -1 : t.AmountUSD)).Sum();
                exportCombineShipment.AuthVAT = grp.Where(t => t.TypeCharge == "OBH").Select(t => criteria.Currency == "VND" ? (t.CreditNo != null && t.DebitNo == null ? t.VATAmountLocal*-1: t.VATAmountLocal): ((t.CreditNo != null && t.DebitNo == null) ? t.VATAmountUSD*-1:t.VATAmountUSD)).Sum();
                exportCombineShipment.FreFee = grp.Where(t => t.TransactionType != "CL" && t.TypeCharge != "OBH").Select(t => criteria.Currency == "VND" ? (t.TypeCharge == "BUY" ? t.AmountVND * -1 : t.AmountVND) : (t.TypeCharge == "BUY" ? t.AmountUSD * -1 : t.AmountUSD)).Sum();
                exportCombineShipment.FreVAT = grp.Where(t => t.TransactionType != "CL" && t.TypeCharge != "OBH").Select(t => criteria.Currency == "VND" ? (t.TypeCharge == "BUY" ? t.VATAmountLocal*-1: t.VATAmountLocal) : (t.TypeCharge == "BUY" ? t.VATAmountUSD*-1:t.VATAmountUSD)).Sum();
                exportCombineShipment.HwbNo = grpData.HBL;
                exportCombineShipment.PackageContainer = grpData.PackageContainer;
                exportCombineShipment.TransactionType = grpData.TransactionType;
                exportCombineShipment.ServiceDate = grpData.ServiceDate;
                lstShipment.Add(exportCombineShipment);
            }

            shipmentModel.exportShipment = lstShipment;
            var partner = partnerRepo.Get(x => x.Id == criteria.PartnerId).FirstOrDefault();
            shipmentModel.BillingAddressVN = partner?.AddressVn;
            shipmentModel.PartnerNameVN = partner?.PartnerNameVn;
            if (criteria.CreatedDateFrom != null)
            {
                shipmentModel.FromDate = criteria.CreatedDateFrom;
                shipmentModel.ToDate = criteria.CreatedDateTo;
            }
            return shipmentModel;
        }

        public CombineShipmentModel GetDataExportCombineShipment(string combineBillingNo)
        {
            CombineShipmentModel shipment = new CombineShipmentModel();
            var combine = DataContext.Get(x => x.CombineBillingNo == combineBillingNo).FirstOrDefault();
            if (combine == null) { return shipment; }

            var charges = GetChargeSellByCombineNo(combineBillingNo);

            List<ExportCombineShipment> lstShipment = new List<ExportCombineShipment>();
            var res = charges.GroupBy(x => new { x.JobId, x.HBL }).AsQueryable();
            foreach (var grp in res)
            {
                ExportCombineShipment exportCombineShipment = new ExportCombineShipment();
                exportCombineShipment.CommodityName = grp.Select(t => t.Commodity).FirstOrDefault();
                exportCombineShipment.JobNo = grp.Select(t => t.JobId).FirstOrDefault();
                exportCombineShipment.CustomDeclarationNo = grp.Select(t => t.CustomNo).FirstOrDefault();
                exportCombineShipment.InvoiceNo = String.Join(";", grp.Where(t => t.TransactionType != "OBH"&&!string.IsNullOrEmpty(t.InvoiceNo)).Select(t => t.InvoiceNo));
                exportCombineShipment.FreInvoice = String.Join(";", grp.Where(t => t.TransactionType == "OBH" && !string.IsNullOrEmpty(t.InvoiceNo)).Select(t => t.InvoiceNo));
                exportCombineShipment.KGS = grp.Select(t => t.ChargeWeight).FirstOrDefault();
                exportCombineShipment.CBM = grp.Select(t => t.CBM).FirstOrDefault();
                exportCombineShipment.CombineNo = combineBillingNo;
                exportCombineShipment.BillingNo = grp.Select(t => t.BillingNo).FirstOrDefault();
                exportCombineShipment.CusFee = grp.Where(t => t.TypeCharge != "OBH"&&t.TransactionType=="CL").Select(t => t.AmountVND).Sum();
                exportCombineShipment.CusVAT = grp.Where(t => t.TypeCharge != "OBH"&&t.TransactionType=="CL").Select(t => t.VATAmount).Sum();
                exportCombineShipment.AuthFee = grp.Where(t => t.TypeCharge == "OBH").Select(t => t.AmountVND).Sum();
                exportCombineShipment.AuthVAT = grp.Where(t => t.TypeCharge == "OBH").Select(t => t.VATAmount).Sum();
                exportCombineShipment.FreFee = grp.Where(t => t.TransactionType != "CL" && t.TypeCharge != "OBH").Select(t => t.AmountVND).Sum();
                exportCombineShipment.FreVAT = grp.Where(t => t.TransactionType != "CL" && t.TypeCharge != "OBH").Select(t => t.VATAmount).Sum();
                exportCombineShipment.HwbNo = grp.Select(t => t.HBL).FirstOrDefault();
                //exportSOAOPS.CBM = grp.Select(t => t.CBM).FirstOrDefault();
                //exportSOAOPS.GW = grp.Select(t => t.GrossWeight).FirstOrDefault();
                //exportSOAOPS.PackageContainer = grp.Select(t => t.PackageContainer).FirstOrDefault();
                //exportSOAOPS.Charges.AddRange(grp.Select(t => t).ToList());
                lstShipment.Add(exportCombineShipment);
            }
            shipment.exportShipment = lstShipment;
            var partner = partnerRepo.Get(x => x.Id == combine.PartnerId).FirstOrDefault();
            shipment.BillingAddressVN = partner?.AddressVn;
            shipment.PartnerNameVN = partner?.PartnerNameVn;
            //opssoa.FromDate = soa.SoaformDate;
            shipment.No = combineBillingNo;

            //foreach (var item in ops.exportOPS)
            //{
            //    foreach (var it in item.Charges)
            //    {
            //        it.VATAmount = it.VATAmountLocal;
            //        it.NetAmount = it.AmountVND;
            //        if (it.BillingType == AccountingConstants.ACCOUNTANT_TYPE_CREDIT)
            //        {
            //            it.VATAmount *= (-1);
            //            it.NetAmount *= (-1);
            //        }
            //    }
            //}

            if (combine.ServiceDateFrom != null)
            {
                shipment.FromDate = combine.ServiceDateFrom;
                shipment.ToDate = combine.ServiceDateTo;
            }
            else if (combine.IssuedDateFrom != null)
            {
                shipment.FromDate = combine.IssuedDateFrom;
                shipment.ToDate = combine.IssuedDateTo;
            }
            else
            {
                var chagerOrder = charges.OrderByDescending(x => x.ExchangeDate);
                var lastdate = chagerOrder.FirstOrDefault();
                var firstdate = chagerOrder.LastOrDefault();
                shipment.FromDate = firstdate != null ? firstdate.ExchangeDate : null;
                shipment.ToDate = lastdate != null ? lastdate.ExchangeDate : null;
            }

            return shipment;
        }

    }
}
