﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.CombineBilling;
using eFMS.API.Accounting.DL.Models.Criteria;
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
            IContextBase<CatCommodityGroup> catCommodityGroup
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
        }

        /// <summary>
        /// Generate billing no
        /// </summary>
        /// <returns></returns>
        public string GenerateCombineBillingNo()
        {
            var yy = DateTime.Now.ToString("yy");
            var mm = DateTime.Now.ToString("MM");
            var billingNos = DataContext.Get(x=>x.CombineBillingNo.Contains("CB")).Select(x => x.CombineBillingNo);
            var numOfOrder = new List<int>();
            var num = string.Empty;
            if(billingNos != null && billingNos.Count() > 0)
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

                var billingNos = string.Empty;
                var hblIds = string.Empty;
                if (surcharges != null)
                {
                    billingNos = string.Join(';', model.Shipments.Select(x => x.Refno).ToList());
                    hblIds = string.Join(';', surcharges.Select(x => x.Hblid).Distinct().ToList());
                }

                var dataAdd = mapper.Map<AcctCombineBilling>(model);
                var hs = DataContext.Add(dataAdd);
                if (hs.Success && !string.IsNullOrEmpty(hblIds))
                {
                    var req = UpdateCombineNoForShipment(billingNos, hblIds, model.CombineBillingNo);
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

                var billingNos = string.Empty;
                var hblIds = string.Empty;
                if (surcharges != null)
                {
                    billingNos = string.Join(';', model.Shipments.Select(x => x.Refno).ToList());
                    hblIds = string.Join(';', surcharges.Select(x => x.Hblid).Distinct().ToList());
                }

                var hs = DataContext.Update(combine, x => x.Id == combine.Id);
                if (hs.Success && !string.IsNullOrEmpty(hblIds))
                {
                    var req = UpdateCombineNoForShipment(billingNos, hblIds, model.CombineBillingNo);
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
                            foreach(var item in surcharges)
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
                            var acctSoa = soaRepo.Get(x => x.CombineBillingNo == combineData.CombineBillingNo).ToList();
                            foreach(var item in acctSoa)
                            {
                                item.CombineBillingNo = null;
                                var hsUpdateSoa = soaRepo.Update(item, x => x.Id == item.Id, false);
                            }

                            // Remove from cdNote
                            var cdNote = cdNoteRepo.Get(x => x.CombineBillingNo == combineData.CombineBillingNo).ToList();
                            foreach (var item in cdNote)
                            {
                                item.CombineBillingNo = null;
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
            var partners = partnerRepo.Get();
            var users = userRepo.Get();
            var employee = employeeRepo.Get();
            var dataCombineBilling = GetCombinePermission().Where(query);
            var surcharges = surchargeRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo));
            var result = from data in dataCombineBilling
                         join part in partners on data.PartnerId equals part.Id
                         join surcharge in surcharges on data.CombineBillingNo equals surcharge.CombineBillingNo into grpCombine
                         from surcharge in grpCombine.DefaultIfEmpty()
                         join surcharge2 in surcharges on data.CombineBillingNo equals surcharge2.CombineBillingNo into grpCombineObh
                         from surcharge2 in grpCombineObh.DefaultIfEmpty()
                         join us in users on data.UserCreated equals us.Id
                         join emp in employee on us.EmployeeId equals emp.Id
                         select new AcctCombineBillingResult
                         {
                             Id = data.Id,
                             CombineBillingNo = data.CombineBillingNo,
                             PartnerName = part.ShortName,
                             UserCreated = data.UserCreated,
                             TotalAmountVnd = data.TotalAmountVnd,
                             TotalAmountUsd = data.TotalAmountUsd,
                             JobNo = surcharge != null ? surcharge.JobNo : surcharge2.JobNo,
                             Soano = surcharge != null ? surcharge.Soano : surcharge2.Soano,
                             PaySoano = surcharge != null ? surcharge.PaySoano : surcharge2.PaySoano,
                             CreditNo = surcharge != null ? surcharge.CreditNo : surcharge2.CreditNo,
                             DebitNo = surcharge != null ? surcharge.DebitNo : surcharge2.DebitNo,
                             UserCreatedName = emp == null ? string.Empty : emp.EmployeeNameEn,
                             DatetimeCreated = data.DatetimeCreated
                         };
            if (criteria.ReferenceNo != null && criteria.ReferenceNo.Count > 0)
            {
                result = result.Where(x => criteria.ReferenceNo.Any(z => z.Trim() == x.CombineBillingNo) ||
                                                            criteria.ReferenceNo.Any(z => z.Trim() == x.JobNo) ||
                                                            criteria.ReferenceNo.Any(z => z.Trim() == x.Soano) || criteria.ReferenceNo.Any(z => z.Trim() == x.PaySoano) ||
                                                            criteria.ReferenceNo.Any(z => z.Trim() == x.CreditNo) || criteria.ReferenceNo.Any(z => z.Trim() == x.DebitNo));
            }
            var dataResult = new List<AcctCombineBillingResult>();
            if (result != null && result.Count() > 0)
            {
                var dataGrp = result.Where(x => x.Id != null).GroupBy(x => new { x.Id, x.CombineBillingNo });
                foreach (var item in dataGrp)
                {
                    var billing = new AcctCombineBillingResult();
                    var firstData = item.FirstOrDefault();
                    billing.Id = item.Key.Id;
                    billing.CombineBillingNo = item.Key.CombineBillingNo;
                    billing.TotalAmountVnd = firstData?.TotalAmountVnd ?? 0;
                    billing.TotalAmountUsd = firstData?.TotalAmountUsd ?? 0;
                    billing.PartnerName = firstData?.PartnerName;
                    billing.UserCreatedName = firstData?.UserCreatedName;
                    billing.DatetimeCreated = firstData?.DatetimeCreated;
                    billing.UserCreated = firstData?.UserCreated;
                    dataResult.Add(billing);
                }
            }
            return dataResult?.OrderByDescending(x => x.DatetimeCreated).AsQueryable();
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
            var existCombineNo = new List<string>();
            if (!string.IsNullOrEmpty(criteria.DocumentType) && criteria.DocumentNo != null && criteria.DocumentNo.Count > 0)
            {
                switch (criteria.DocumentType)
                {
                    case "CD Note":
                        var existCDNotes = cdNoteRepo.Get(x => criteria.DocumentNo.Any(z => z == x.Code) && !string.IsNullOrEmpty(x.CombineBillingNo)).Select(x => x.Code).ToList();
                        foreach (var item in existCDNotes)
                        {
                            var credit = surchargeRepo.Get(x => item.Trim() == x.CreditNo).Select(x => x.Hblid).ToList();
                            var existingCredit = surchargeRepo.Get(x => ((x.Type != "OBH" && !string.IsNullOrEmpty(x.CombineBillingNo)) || (x.Type == "OBH" && !string.IsNullOrEmpty(x.ObhcombineBillingNo))) && item.Trim() == x.CreditNo).Select(x => x.Hblid).ToList();
                            if (credit.Count == existingCredit.Count && existingCredit.Count > 0)
                            {
                                existCombineNo.Add(item);
                            }
                            var debit = surchargeRepo.Get(x => item.Trim() == x.DebitNo).Select(x => x.Hblid).ToList();
                            var existingDebit = surchargeRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && item.Trim() == x.DebitNo).Select(x => x.Hblid).ToList();
                            if (debit.Count == existingDebit.Count && existingDebit.Count > 0)
                            {
                                existCombineNo.Add(item);
                            }
                        }
                        break;
                    case "Soa":
                        var existSoa = soaRepo.Get(x => criteria.DocumentNo.Any(z => z == x.Soano) && !string.IsNullOrEmpty(x.CombineBillingNo)).Select(x => x.Soano).ToList();
                        foreach (var item in existSoa)
                        {
                            var soa = surchargeRepo.Get(x => item.Trim() == x.Soano).Select(x => x.Hblid).ToList();
                            var existingSoa = surchargeRepo.Get(x => !string.IsNullOrEmpty(x.CombineBillingNo) && item.Trim() == x.Soano).Select(x => x.Hblid).ToList();
                            if (soa.Count == existingSoa.Count && existingSoa.Count > 0)
                            {
                                existCombineNo.Add(item);
                            }
                            soa = surchargeRepo.Get(x => item.Trim() == x.PaySoano).Select(x => x.Hblid).ToList();
                            existingSoa = surchargeRepo.Get(x => ((x.Type != "OBH" && !string.IsNullOrEmpty(x.CombineBillingNo)) || (x.Type == "OBH" && !string.IsNullOrEmpty(x.ObhcombineBillingNo))) && item.Trim() == x.PaySoano).Select(x => x.Hblid).ToList();
                            if (soa.Count == existingSoa.Count && existingSoa.Count > 0)
                            {
                                existCombineNo.Add(item);
                            }
                        }
                        break;
                    case "Job No":
                        foreach (var item in criteria.DocumentNo)
                        {
                            var job = surchargeRepo.Get(x => item.Trim() == x.JobNo).Select(x => x.Hblid).ToList();
                            var existingJobNo = surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.JobNo).Select(x => x.Hblid).ToList();
                            if (job.Count == existingJobNo.Count && existingJobNo.Count > 0)
                            {
                                existCombineNo.Add(item);
                            }
                        }
                        break;
                    case "HBL No":
                        foreach (var item in criteria.DocumentNo)
                        {
                            var existingHblno = surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.Hblno).Select(x => x.Hblno).Distinct().ToList();
                            if (existingHblno.Count > 0)
                            {
                                existCombineNo.AddRange(existingHblno);
                            }
                        }
                        break;
                    case "Custom No":
                        foreach (var item in criteria.DocumentNo)
                        {
                            var custom = surchargeRepo.Get(x => item.Trim() == x.ClearanceNo).Select(x => x.Hblid).ToList();
                            var existingCusno = surchargeRepo.Get(x => (!string.IsNullOrEmpty(x.CombineBillingNo) || !string.IsNullOrEmpty(x.ObhcombineBillingNo)) && item.Trim() == x.ClearanceNo).Select(x => x.Hblid).ToList();
                            if (custom.Count == existingCusno.Count && existingCusno.Count > 0)
                            {
                                existCombineNo.Add(item);
                            }
                        }
                        break;
                }
            }
            if (existCombineNo.Count > 0)
            {
                return string.Join(';', existCombineNo.Distinct());
            }
            return string.Empty;
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
                    totalStr = totalUsd != 0 ? totalUsd < 0 ? (totalUsd * -1).ToString("N02") : ("(" + totalUsd.ToString("N02") + ")") + " USD" : totalStr;
                    totalStr += !string.IsNullOrEmpty(totalStr) && totalVnd != 0 ? " | " : string.Empty;
                    totalStr += totalVnd != 0 ? totalVnd < 0 ? (totalVnd * -1).ToString("N0") : ("(" + totalVnd.ToString("N0") + ")") + " VND" : string.Empty;
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
                    totalStr = totalUsd != 0 ? totalUsd < 0 ? (totalUsd * -1).ToString("N02") : ("(" + totalUsd.ToString("N02") + ")") + " USD" : totalStr;
                    totalStr += !string.IsNullOrEmpty(totalStr) && totalVnd != 0 ? " | " : string.Empty;
                    totalStr += totalVnd != 0 ? totalVnd < 0 ? (totalVnd * -1).ToString("N0") : ("(" + totalVnd.ToString("N0") + ")") + " VND" : string.Empty;
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
            var catChargeSearch = catChargeRepo.Get().ToLookup(x=>x.Id);
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
    }
}
