﻿using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.ExportResults;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.API.Infrastructure.Extensions;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccAccountingPaymentService : RepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>, IAccAccountingPaymentService
    {
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<AccAccountingManagement> accountingManaRepository;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<AcctSoa> soaRepository;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<AcctReceipt> acctReceiptRepository;
        private readonly IContextBase<AcctCdnote> cdNoteRepository;
        private readonly IContextBase<CustomsDeclaration> customsDeclarationRepository;
        private readonly IContextBase<CatContract> catContractRepository;
        private readonly IContextBase<SysEmployee> sysEmployeeRepository;
        private readonly IContextBase<CsTransaction> csTransactionRepository;
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;

        public AccAccountingPaymentService(IContextBase<AccAccountingPayment> repository,
            IMapper mapper,
            IContextBase<SysUser> userRepo,
            IContextBase<AccAccountingManagement> accountingManaRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<AcctReceipt> acctReceiptRepo,
            IContextBase<AcctCdnote> cdNoteRepo,
            IContextBase<CustomsDeclaration> customsDeclarationRepo,
             IContextBase<CatContract> catContractRepo,
             IContextBase<SysEmployee> sysEmployeeRepo,
             IContextBase<CsTransaction> csTransRepo,
             ICurrencyExchangeService exchangeService,

            ICurrentUser currUser) : base(repository, mapper)
        {
            userRepository = userRepo;
            accountingManaRepository = accountingManaRepo;
            partnerRepository = partnerRepo;
            soaRepository = soaRepo;
            surchargeRepository = surchargeRepo;
            currentUser = currUser;
            acctReceiptRepository = acctReceiptRepo;
            cdNoteRepository = cdNoteRepo;
            customsDeclarationRepository = customsDeclarationRepo;
            catContractRepository = catContractRepo;
            sysEmployeeRepository = sysEmployeeRepo;
            csTransactionRepository = csTransRepo;
            currencyExchangeService = exchangeService;
        }

        public IQueryable<AccAccountingPaymentModel> GetBy(string refNo, string type)
        {
            IQueryable<AccAccountingPayment> data = null;
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            if (type == "DEBIT" || type == "OBH")
            {
                data = DataContext.Get(x => x.BillingRefNo == refNo && x.Type == type).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
            }
            else if (type == "NETOFF")
            {
                data = DataContext.Get(x => x.BillingRefNo == refNo).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
            }
            else
            {
                var receiptId = receiptData.Where(x => x.PaymentRefNo == refNo).FirstOrDefault()?.Id;
                data = DataContext.Get(x => x.ReceiptId == receiptId).OrderBy(x => x.PaidDate).ThenBy(x => x.PaymentNo);
            }
            var dataPM = data.Join(receiptData, pm => pm.ReceiptId, re => re.Id, (pm, rc) => rc);
            var results = new List<AccAccountingPaymentModel>();
            var grpData = dataPM.GroupBy(x => x.Id).Select(x => new { x.Key, receipt = x.Select(z => z) });
            foreach (var x in grpData)
            {
                var item = new AccAccountingPaymentModel();
                //var receipt = receiptData.Where(acct => acct.Id == x.Key).FirstOrDefault();
                var receipt = x.receipt.FirstOrDefault();
                item.ReceiptNo = receipt.PaymentRefNo;
                item.PaymentAmount = receipt.PaidAmount ?? 0;
                item.CurrencyId = receipt.CurrencyId;
                item.Balance = receipt.Balance ?? 0;
                item.PaidDate = receipt.PaymentDate;
                item.PaymentMethod = receipt.PaymentMethod;
                item.Note = receipt.Description;
                results.Add(item);
            }

            return results?.OrderBy(x => x.ReceiptNo).AsQueryable();
        }

        public IQueryable<AccountingPaymentModel> Paging(PaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            //var _totalItem = data.Select(s => s.RefId).Distinct().Count();
            //rowsCount = (_totalItem > 0) ? _totalItem : 0;
            var _totalItem = 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }

                data = GetReferencesData(data, criteria);
                if (data == null)
                {
                    rowsCount = 0;
                    return null;
                }
                _totalItem = data.Count();
                data = data.Skip((page - 1) * size).Take(size);
            }
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            return data;
        }
        public IQueryable<AccountingPaymentModel> ExportAccountingPayment(PaymentCriteria criteria)
        {
            var data = Query(criteria);
            if (data == null) return null;
            data = GetReferencesData(data, criteria);
            return data;
        }

        private IQueryable<AccountingPaymentModel> GetReferencesData(IQueryable<AccountingPaymentModel> data, PaymentCriteria criteria)
        {
            IQueryable<AccountingPaymentModel> results = null;
            if (data?.Count() > 0)
            {
                if (criteria.PaymentType == PaymentType.Invoice)
                {
                    results = GetReferencesInvoiceData(data, criteria);
                }
                else
                {
                    results = GetReferencesOBHData(data);
                }
            }

            var soaCredit = GetCreditSoaPayment(criteria);
            if (soaCredit?.Count() > 0)
            {
                results = results != null ? results.Union(soaCredit) : soaCredit;
            }
            var creditNote = GetCreditNotePayment(criteria);
            if (creditNote?.Count() > 0)
            {
                results = results != null ? results.Union(creditNote) : creditNote;
            }
            var advData = GetReferencesAdvanceData(criteria);
            if (advData?.Count() > 0)
            {
                results = results != null ? results.Union(advData) : advData;
            }
            return results?.OrderBy(x => x.PartnerName).ThenBy(x => x.RefNo);
        }


        private Expression<Func<AccAccountingPayment, bool>> GetQueryADVPermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AccAccountingPayment, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        private IQueryable<AccAccountingPayment> QueryInvoiceDataPayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.Type == "DEBIT" || x.Type == "OBH") && x.Negative != true;
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "DebitInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.BillingRefNo, StringComparer.OrdinalIgnoreCase) || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "Soa":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.BillingRefNo, StringComparer.OrdinalIgnoreCase));
                        break;
                    case "ReceiptNo":
                        var lstReceipt = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(x.PaymentRefNo)).Select(x => x.Id).ToList();
                        if (lstReceipt.Count > 0)
                        {
                            query = query.And(x => lstReceipt.Any(r => r == x.ReceiptId));
                        }
                        else
                        {
                            query = query.And(x => false);
                        }
                        break;
                }
            }
            if (criteria.FromUpdatedDate != null)
            {
                query = query.And(x => x.PaidDate != null && x.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date);
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var data = DataContext.Get(query);
            return data;
        }

        private IQueryable<AccountingPaymentModel> GetReferencesOBHData(IQueryable<AccountingPaymentModel> data)
        {
            var partners = partnerRepository.Get();
            var resultsQuery = (from soa in data
                                join partner in partners on soa.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                join payment in DataContext.Get() on soa.RefId.ToLower() equals payment.RefId into grpPayments
                                from detail in grpPayments.DefaultIfEmpty()
                                select new
                                {
                                    soa,
                                    part.ShortName,
                                    Balance = detail != null ? detail.Balance : 0,
                                    PaymentAmount = detail != null ? detail.PaymentAmount : 0
                                }).ToList();
            var resultGroups = resultsQuery.GroupBy(x => new {
                x.soa.RefId,
                x.soa.SOANo,
                x.soa.PartnerId,
                x.ShortName,
                x.soa.Currency,
                x.soa.IssuedDate,
                x.soa.DueDate,
                x.soa.Status,
                x.soa.OverdueDays,
                x.soa.ExtendDays,
                x.soa.ExtendNote
            });
            var results = resultGroups
                            .Select(x => new AccountingPaymentModel
                            {
                                RefId = x.Key.RefId,
                                SOANo = x.Key.SOANo,
                                PartnerId = x.Key.PartnerId,
                                PartnerName = x.Key.ShortName,
                                Amount = x.Sum(y => y.soa.Amount),
                                Currency = x.Key.Currency,
                                IssuedDate = x.Key.IssuedDate,
                                DueDate = x.Key.DueDate,
                                OverdueDays = x.Key.OverdueDays,
                                Status = x.Key.Status,
                                ExtendDays = x.Key.ExtendDays,
                                ExtendNote = x.Key.ExtendNote,
                                PaidAmount = x.Sum(c => c.PaymentAmount),
                                UnpaidAmount = x.Sum(c => c.Balance)
                            }).AsQueryable();
            return results;
        }


        private IQueryable<AccountingPaymentModel> GetReferencesInvoiceData(IQueryable<AccountingPaymentModel> data, PaymentCriteria criteria)
        {
            var partners = partnerRepository.Get();
            var paymentData = QueryInvoiceDataPayment(criteria);
            var surchargeData = surchargeRepository.Get();
            var resultsQuery = (from invoice in data
                                join surcharge in surchargeData on invoice.RefId equals surcharge.AcctManagementId.ToString()
                                join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                join payments in paymentData on invoice.RefId.ToLower() equals payments.RefId into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                select new
                                {
                                    invoice,
                                    ShortName = part == null ? string.Empty : part.ShortName,
                                    BillingRefNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano,
                                    InvoiceNo = invoice.Type != "Invoice" ? string.Empty : invoice.InvoiceNoReal,
                                    Type = surcharge.Type == "OBH" ? "OBH" : "DEBIT",
                                    payment
                                }).ToList();

            var resultGroups = resultsQuery.GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.BillingRefNo,
                x.Type,
                x.ShortName,
                x.InvoiceNo,
            }).Select(s => new { invoice = s.Select(i => i.invoice), s.Key, payment = s.Select(f => new { f.payment?.PaymentAmount, f.payment?.UnpaidPaymentAmountUsd, f.payment?.UnpaidPaymentAmountVnd }) });

            var results = resultGroups
                            .Select(x => new AccountingPaymentModel
                            {
                                RefNo = x.Key.BillingRefNo,
                                Type = x.Key.Type,
                                PartnerId = x.Key.PartnerId,
                                InvoiceNoReal = x.Key.InvoiceNo,
                                PartnerName = x.Key.ShortName,
                                Amount = x.Key.Type == "OBH" ? x.invoice.Sum(z => z.Amount) : x.invoice.FirstOrDefault().Amount,
                                Currency = x.invoice.FirstOrDefault().Currency,
                                IssuedDate = x.invoice.Any(inv => inv.Type == "Invoice") ? x.invoice.FirstOrDefault().IssuedDate : null,
                                Serie = x.invoice.FirstOrDefault().Serie,
                                DueDate = x.invoice.FirstOrDefault().DueDate,
                                OverdueDays = x.invoice.FirstOrDefault().OverdueDays,
                                Status = x.invoice.FirstOrDefault().Status,
                                ExtendDays = x.invoice.FirstOrDefault().ExtendDays,
                                PaidAmount = x.payment == null ? 0 : (x.Key.Type == "OBH" ? x.payment.Sum(i => i.PaymentAmount ?? 0) : x.payment.FirstOrDefault()?.PaymentAmount),
                                UnpaidAmount = x.payment == null ? (x.Key.Type == "OBH" ? x.invoice.Sum(z => z.UnpaidAmount ?? 0) : (x.invoice.FirstOrDefault().UnpaidAmount ?? 0)) :
                                                                    (x.invoice.FirstOrDefault().Currency == AccountingConstants.CURRENCY_LOCAL ? (x.payment.FirstOrDefault().UnpaidPaymentAmountVnd ?? 0) : (x.payment.FirstOrDefault().UnpaidPaymentAmountUsd ?? 0)),
                            });
            return results.AsQueryable();
        }

        private Expression<Func<AcctSoa, bool>> SoaCreditExpressionQuery(PaymentCriteria criteria)
        {
            Expression<Func<AcctSoa, bool>> query = q => q.Type.ToUpper() == "CREDIT";
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.Customer == criteria.PartnerId);
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var soaNo = new List<string>();
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "DebitInvoice":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        break;
                    case "Soa":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.Soano));
                        break;
                    case "CreditNote":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "HBL":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "MBL":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "JobNo":
                        soaNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.PaySoano).Distinct().ToList();
                        if (soaNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                }

                if (soaNo != null && soaNo.Count > 0)
                {
                    query = query.And(x => soaNo.Any(s => s == x.Soano));
                }
            }
            return query;
        }

        /// <summary>
        /// Get payment data with type = CREDITSOA
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AccountingPaymentModel> GetCreditSoaPayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            var results = new List<AccountingPaymentModel>();
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.Type == "CREDITSOA" && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId)));
            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null)
            {
                query = query.And(x => false);
            }
            if (criteria.FromDueDate != null && criteria.ToDueDate != null)
            {
                query = query.And(x => false);
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                case Common.OverDueDate.Between16_30:
                case Common.OverDueDate.Between31_60:
                case Common.OverDueDate.Between61_90:
                    query = query.And(x => false);
                    break;
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var paymentData = DataContext.Get(query);
            var partners = partnerRepository.Get();
            var querySoa = SoaCreditExpressionQuery(criteria);
            var soaData = soaRepository.Get(querySoa);
            var surchargeData = surchargeRepository.Get(x => !string.IsNullOrEmpty(x.VoucherId) || !string.IsNullOrEmpty(x.VoucherIdre));
            var creditSoaData = (from soa in soaData
                                 join surcharge in surchargeData on soa.Soano equals surcharge.PaySoano
                                 join payments in paymentData on soa.Id equals payments.RefId into grpPayment
                                 from payment in grpPayment.DefaultIfEmpty()
                                 join partner in partners on soa.Customer equals partner.Id into grpPartners
                                 from part in grpPartners.DefaultIfEmpty()
                                 select new
                                 {
                                     BillingRefNo = soa.Soano,
                                     Type = payment == null ? "CREDIT" : payment.Type,
                                     PartnerId = part.Id,
                                     part.ShortName,
                                     soa.NetOff,
                                     soa.Currency,
                                     VoucherId = surcharge.Type == "OBH" ? surcharge.VoucherIdre : surcharge.VoucherId,
                                     Amount = soa.CreditAmount,
                                     PaidAmount = payment == null ? 0 : payment.PaymentAmount,
                                     UnpaidAmount = payment == null ? soa.CreditAmount : soa.Currency == AccountingConstants.CURRENCY_LOCAL ? (payment.UnpaidPaymentAmountVnd ?? 0) : (payment.UnpaidPaymentAmountUsd ?? 0),
                                     PaidDate = payment == null ? null : payment.PaidDate,
                                     ReceiptId = payment == null ? null : payment.ReceiptId
                                 }).ToList();
            var resultGroups = creditSoaData.GroupBy(x => new
            {
                x.BillingRefNo,
                x.Type,
                x.PartnerId,
                x.ShortName,
            }).Select(x => new { grp = x.Key, payment = x.Select(z => new { z.NetOff, z.VoucherId, z.PaidDate, z.ReceiptId, z.Amount, z.Currency, z.PaidAmount, z.UnpaidAmount }) });
            foreach (var item in resultGroups)
            {
                var payment = new AccountingPaymentModel();
                var acctPayment = item.payment.FirstOrDefault();
                payment.RefNo = item.grp.BillingRefNo;
                payment.ReceiptId = acctPayment.ReceiptId;
                payment.PaidDate = acctPayment.PaidDate;
                payment.Type = "NETOFF";
                payment.PartnerId = item.grp.PartnerId;
                payment.InvoiceNoReal = string.Empty;
                payment.PartnerName = item.grp.ShortName;
                payment.Amount = acctPayment.Amount ?? 0;
                payment.Currency = acctPayment.Currency;
                payment.PaidAmount = acctPayment.PaidAmount ?? 0;
                payment.UnpaidAmount = acctPayment.UnpaidAmount;
                payment.Status = acctPayment.NetOff == true ? "Paid" : "Unpaid";
                results.Add(payment);
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = acctReceiptRepository.Get(receipt => receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.Id).ToList();
                    results = results.Where(x => listReceiptInfo.Any(z => z == x.ReceiptId)).ToList();
                }
            }
            if (criteria.FromUpdatedDate != null)
            {
                results = results.Where(x => x.PaidDate != null && x.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date).ToList();
            }
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains("Other"))
            {
                results = results.Where(x => criteria.PaymentStatus.Contains(x.Status ?? "Unpaid")).ToList();
            }
            return results.AsQueryable();
        }

        private Expression<Func<AcctCdnote, bool>> CreditNoteExpressionQuery(PaymentCriteria criteria)
        {
            Expression<Func<AcctCdnote, bool>> query = q => q.Type == "CREDIT";
            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(q => q.PartnerId == criteria.PartnerId);
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                var creditNo = new List<string>();
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        break;
                    case "Soa":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.PaySoano, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "DebitInvoice":
                    case "CreditNote":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.Code));
                        break;
                    case "HBL":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "MBL":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "JobNo":
                        creditNo = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.CreditNo).Distinct().ToList();
                        if (creditNo.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                }

                if (creditNo != null && creditNo.Count > 0)
                {
                    query = query.And(x => creditNo.Contains(x.Code));
                }
            }
            return query;
        }

        /// <summary>
        /// Get payment data with type = CREDITNOTE
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AccountingPaymentModel> GetCreditNotePayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            var results = new List<AccountingPaymentModel>();
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.Type == "CREDITNOTE" || x.PaymentType == "CREDIT") && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId));

            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null)
            {
                query = query.And(x => false);
            }
            if (criteria.FromDueDate != null && criteria.ToDueDate != null)
            {
                query = query.And(x => false);
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                case Common.OverDueDate.Between16_30:
                case Common.OverDueDate.Between31_60:
                case Common.OverDueDate.Between61_90:
                    query = query.And(x => false);
                    break;
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var paymentData = DataContext.Get(query);
            //if (paymentData.Count() == 0)
            //{
            //    return results.AsQueryable();
            //}
            var partners = partnerRepository.Get();
            var queryCdNote = CreditNoteExpressionQuery(criteria);
            var cdNoteData = cdNoteRepository.Get(queryCdNote);
            var surchargeData = surchargeRepository.Get(x => !string.IsNullOrEmpty(x.VoucherId) || !string.IsNullOrEmpty(x.VoucherIdre));
            var creditSoaData = (from cdNote in cdNoteData
                                 join surcharge in surchargeData on cdNote.Code equals surcharge.CreditNo
                                 join payments in paymentData on cdNote.Id.ToString() equals payments.RefId into grpPayment
                                 from payment in grpPayment.DefaultIfEmpty()
                                 join partner in partners on cdNote.PartnerId equals partner.Id into grpPartners
                                 from part in grpPartners.DefaultIfEmpty()
                                 select new
                                 {
                                     BillingRefNo = cdNote.Code,
                                     Type = payment == null ? "CREDIT" : payment.Type,
                                     payment,
                                     PartnerId = part.Id,
                                     part.ShortName,
                                     cdNote.NetOff,
                                     VoucherId = surcharge.Type == "OBH" ? surcharge.VoucherIdre : surcharge.VoucherId,
                                     InvoiceNo = payment == null ? surcharge.InvoiceNo : payment.InvoiceNo,
                                     Amount = cdNote.Total,
                                     Currency = cdNote.CurrencyId,
                                     PaidAmount = payment == null ? 0 : payment.PaymentAmount,
                                     UnpaidAmount = payment == null ? cdNote.Total : cdNote.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (payment.UnpaidPaymentAmountVnd ?? 0) : (payment.UnpaidPaymentAmountUsd ?? 0),
                                     PaidDate = payment == null ? null : payment.PaidDate,
                                     ReceiptId = payment == null ? null : payment.ReceiptId
                                 }).ToList();
            var resultGroups = creditSoaData.GroupBy(x => new
            {
                x.BillingRefNo,
                x.Type,
                x.PartnerId,
                x.ShortName,
            }).Select(x => new { grp = x.Key, payment = x.Select(z => new { z.NetOff, z.VoucherId, z.PaidDate, z.InvoiceNo, z.ReceiptId, z.Amount, z.Currency, z.PaidAmount, z.UnpaidAmount }) });
            foreach (var item in resultGroups)
            {
                var payment = new AccountingPaymentModel();
                var acctPayment = item.payment.FirstOrDefault();
                payment.RefNo = item.grp.BillingRefNo;
                payment.ReceiptId = acctPayment.ReceiptId;
                payment.Type = "NETOFF";
                payment.PartnerId = item.grp.PartnerId;
                payment.InvoiceNoReal = acctPayment.InvoiceNo;
                payment.PartnerName = item.grp.ShortName;
                payment.Amount = acctPayment.Amount;
                payment.Currency = acctPayment.Currency;
                payment.PaidAmount = acctPayment.PaidAmount;
                payment.UnpaidAmount = acctPayment.UnpaidAmount;
                payment.Status = acctPayment.NetOff == true ? "Paid" : "Unpaid";
                payment.PaidDate = acctPayment.PaidDate;
                results.Add(payment);
            }
            if (criteria.ReferenceNos != null && criteria.ReferenceNos.Count > 0)
            {
                if (criteria.SearchType == "ReceiptNo")
                {
                    var listReceiptInfo = acctReceiptRepository.Get(receipt => receipt.Status == AccountingConstants.RECEIPT_STATUS_DONE && criteria.ReferenceNos.Contains(receipt.PaymentRefNo)).Select(x => x.Id).ToList();
                    results = results.Where(x => listReceiptInfo.Any(z => z == x.ReceiptId)).ToList();
                }
            }
            if (criteria.FromUpdatedDate != null)
            {
                results = results.Where(x => x.PaidDate != null && x.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date).ToList();
            }
            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0 && !criteria.PaymentStatus.Contains("Other"))
            {
                results = results.Where(x => criteria.PaymentStatus.Contains(x.Status ?? "Unpaid")).ToList();
            }
            return results.AsQueryable();
        }

        /// <summary>
        /// Get payment data with type = Advance
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private IQueryable<AccountingPaymentModel> GetReferencesAdvanceData(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingPayment, bool>> perQuery = GetQueryADVPermission(rangeSearch, _user);
            Expression<Func<AccAccountingPayment, bool>> query = x => (x.Type != "DEBIT" && x.Type != "OBH" && x.PaymentType != "CREDIT") && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId));
            var receiptData = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "ReceiptNo":
                        var receiptLst = receiptData.Where(x => criteria.ReferenceNos.Any(re => re == x.PaymentRefNo)).Select(x => x.Id).ToList();
                        if (receiptLst.Count > 0)
                        {
                            query = query.And(x => receiptLst.Any(r => r == x.ReceiptId));
                        }
                        else
                        {
                            query = query.And(x => false);
                        }
                        break;
                    default:
                        query = query.And(x => false);
                        break;
                }
            }

            if (criteria.PaymentStatus != null && !criteria.PaymentStatus.Contains("Other"))
            {
                query = query.And(x => false);
            }
            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.FromIssuedDate.Value.Date && x.DatetimeCreated.Value.Date <= criteria.ToIssuedDate.Value.Date);
            }
            if (criteria.FromDueDate != null && criteria.ToDueDate != null)
            {
                query = query.And(x => false);
            }
            if (criteria.FromUpdatedDate != null)
            {
                query = query.And(x => x.PaidDate != null && x.PaidDate.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaidDate.Value.Date <= criteria.ToUpdatedDate.Value.Date);
            }
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                case Common.OverDueDate.Between16_30:
                case Common.OverDueDate.Between31_60:
                case Common.OverDueDate.Between61_90:
                    query = query.And(x => false);
                    break;
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var paymentAdv = DataContext.Get(query);
            var results = new List<AccountingPaymentModel>();
            if (paymentAdv.Count() <= 0)
            {
                return results.AsQueryable();
            }
            var partners = partnerRepository.Get();
            var advData = (from adv in paymentAdv
                           join rcpt in receiptData on adv.ReceiptId equals rcpt.Id
                           join partner in partners on rcpt.CustomerId equals partner.Id into grpPartners
                           from part in grpPartners.DefaultIfEmpty()
                           select new
                           {
                               adv,
                               PartnerId = rcpt.CustomerId,
                               part.ShortName,
                               rcpt.PaymentRefNo,
                               Currency = rcpt.CurrencyId
                           }).ToList();
            var resultGroups = advData.GroupBy(x => new
            {
                x.PaymentRefNo,
                x.adv.Type,
                x.PartnerId,
                x.ShortName,
            }).Select(x => new { grp = x.Key, payment = x.Select(z => z.adv) });

            foreach (var item in resultGroups)
            {
                var payment = new AccountingPaymentModel();
                var acctPayment = item.payment.FirstOrDefault();
                payment.RefNo = item.grp.PaymentRefNo;
                payment.Type = item.grp.Type;
                payment.PartnerId = item.grp.PartnerId;
                payment.InvoiceNoReal = acctPayment.InvoiceNo;
                payment.PartnerName = item.grp.ShortName;
                payment.Amount = acctPayment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (acctPayment.PaymentAmountVnd ?? 0) : (acctPayment.PaymentAmountUsd ?? 0);
                payment.Currency = acctPayment.CurrencyId;
                payment.PaidAmount = acctPayment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (acctPayment.PaymentAmountVnd ?? 0) : (acctPayment.PaymentAmountUsd ?? 0);
                payment.UnpaidAmount = acctPayment.CurrencyId == AccountingConstants.CURRENCY_LOCAL ? (acctPayment.UnpaidPaymentAmountVnd ?? 0) : (acctPayment.UnpaidPaymentAmountUsd ?? 0);
                payment.Status = "Paid";
                results.Add(payment);
            }
            return results.AsQueryable();
        }

        private IQueryable<AccountingPaymentModel> Query(PaymentCriteria criteria)
        {
            IQueryable<AccountingPaymentModel> results = null;
            switch (criteria.PaymentType)
            {
                case Common.PaymentType.Invoice:
                    results = QueryInvoicePayment(criteria);
                    break;
                case Common.PaymentType.OBH:
                    results = QueryOBHPayment(criteria);
                    break;
            }
            return results;
        }

        // get charges that have type OBH and SOANo(Debit)
        private IQueryable<AccountingPaymentModel> QueryOBHPayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AcctSoa, bool>> perQuery = GetQueryOBHPermission(rangeSearch, _user);
            Expression<Func<AcctSoa, bool>> query = x => (x.Customer == criteria.PartnerId || criteria.PartnerId == null)
                                                      && (criteria.ReferenceNos.Contains(x.Soano) || criteria.ReferenceNos == null)
                                                      && !string.IsNullOrEmpty(x.PaymentStatus);
            if (criteria.PaymentStatus.Count > 0)
            {
                query = query.And(x => criteria.PaymentStatus.Contains(x.PaymentStatus));
            }
            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.FromIssuedDate.Value.Date && x.DatetimeCreated.Value.Date <= criteria.ToIssuedDate.Value.Date);
            }
            if (criteria.FromDueDate != null && criteria.ToDueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date >= criteria.FromDueDate.Value.Date && x.PaymentDueDate.Value.Date <= criteria.ToDueDate.Value.Date);
            }
            if (criteria.FromUpdatedDate != null)
            {
                query = query.And(x => x.PaymentDatetimeUpdated != null && x.PaymentDatetimeUpdated.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDatetimeUpdated.Value.Date <= criteria.ToUpdatedDate.Value.Date);
            }

            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var data = soaRepository.Get(query);
            if (data == null) return null;
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 16 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 0);
                    break;
                case Common.OverDueDate.Between16_30:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 31 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 15);
                    break;
                case Common.OverDueDate.Between31_60:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 61 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 30);
                    break;
                case Common.OverDueDate.Between61_90:
                    data = data.Where(x => x.PaymentDueDate.HasValue && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days < 90 && (DateTime.Now.Date - x.PaymentDueDate.Value.Date).Days > 60);
                    break;
            }

            if (data == null) return null;
            var surcharges = surchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH
                                                        && !string.IsNullOrEmpty(x.Soano)
                                                        && (
                                                        (criteria.ReferenceNos.Contains(x.Mblno) || criteria.ReferenceNos == null)
                                                        || (criteria.ReferenceNos.Contains(x.Hblno) || criteria.ReferenceNos == null)
                                                        || (criteria.ReferenceNos.Contains(x.Soano) || criteria.ReferenceNos == null))
                                                        );

            var dataJoin = (from soa in data
                           join charge in surcharges on soa.Soano equals charge.Soano
                           select new { soa, TotalOBH = charge.Total });
            var results = dataJoin?.OrderByDescending(x => x.soa.PaymentDatetimeUpdated).Select(x => new AccountingPaymentModel
            {
                RefId = x.soa.Id.ToString(),
                SOANo = x.soa.Soano,
                PartnerId = x.soa.Customer,
                Amount = x.TotalOBH,
                Currency = x.soa.Currency,
                IssuedDate = x.soa.DatetimeCreated,
                DueDate = x.soa.PaymentDueDate,
                OverdueDays = (DateTime.Today > x.soa.PaymentDueDate.Value.Date) ? (DateTime.Today - x.soa.PaymentDueDate.Value.Date).Days : 0,
                Status = x.soa.PaymentStatus,
                ExtendDays = x.soa.PaymentExtendDays,
                ExtendNote = x.soa.PaymentNote
            });
            return results;
        }

        private Expression<Func<AcctSoa, bool>> GetQueryOBHPermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AcctSoa, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        private IQueryable<AccountingPaymentModel> QueryInvoicePayment(PaymentCriteria criteria)
        {
            ICurrentUser _user = PermissionExtention.GetUserMenuPermission(currentUser, Menu.acctARP);
            PermissionRange rangeSearch = PermissionExtention.GetPermissionRange(_user.UserMenuPermission.List);
            if (rangeSearch == PermissionRange.None) return null;
            Expression<Func<AccAccountingManagement, bool>> perQuery = GetQueryInvoicePermission(rangeSearch, _user);
            Expression<Func<AccAccountingManagement, bool>> query = x => x.InvoiceNoReal != null && (x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE || x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE)
                                                                      && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId)); // TH synce inv từ bravo && x.Status != "New"
            var acctManagementIds = new List<Guid?>();
            if (criteria.ReferenceNos?.Count(x => !string.IsNullOrEmpty(x)) > 0)
            {
                switch (criteria.SearchType)
                {
                    case "VatInvoice":
                        query = query.And(x => criteria.ReferenceNos.Contains(x.InvoiceNoReal));
                        break;
                    case "DebitInvoice":
                        acctManagementIds = surchargeRepository.Get(x => (criteria.ReferenceNos.Contains(x.DebitNo, StringComparer.OrdinalIgnoreCase))
                        || criteria.ReferenceNos.Contains(x.InvoiceNo, StringComparer.OrdinalIgnoreCase)
                        ).Select(se => se.AcctManagementId).Distinct().ToList();
                        if(acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "Soa":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Soano, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "ReceiptNo":
                        var hasReceiptInfo = acctReceiptRepository.Get(x => x.Status == AccountingConstants.RECEIPT_STATUS_DONE).Any(receipt => criteria.ReferenceNos.Contains(receipt.PaymentRefNo));
                        if (!hasReceiptInfo)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "CreditNote":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.CreditNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "HBL":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Hblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "MBL":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.Mblno, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                    case "JobNo":
                        acctManagementIds = surchargeRepository.Get(x => criteria.ReferenceNos.Contains(x.JobNo, StringComparer.OrdinalIgnoreCase)).Select(se => se.AcctManagementId).Distinct().ToList();
                        if (acctManagementIds.Count == 0)
                        {
                            query = query.And(x => false);
                        }
                        break;
                }
            }
            if (acctManagementIds != null && acctManagementIds.Count > 0)
            {
                query = query.And(x => acctManagementIds.Contains(x.Id));
            }

            if (criteria.PaymentStatus != null && criteria.PaymentStatus.Count > 0)
            {
                query = query.And(x => criteria.PaymentStatus.Contains(x.PaymentStatus ?? "Unpaid"));
            }
            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null)
            {
                query = query.And(x => x.Date.Value.Date >= criteria.FromIssuedDate.Value.Date && x.Date.Value.Date <= criteria.ToIssuedDate.Value.Date);
            }
            if (criteria.FromDueDate != null && criteria.ToDueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date >= criteria.FromDueDate.Value.Date && x.PaymentDueDate.Value.Date <= criteria.ToDueDate.Value.Date);
            }
            if (perQuery != null)
            {
                query = query.And(perQuery);
            }
            var data = accountingManaRepository.Get(query);
            if (data == null) return null;
            var results = data.OrderByDescending(x => x.PaymentDatetimeUpdated).Select(x => new AccountingPaymentModel
            {
                RefId = x.Id.ToString(),
                InvoiceNoReal = x.InvoiceNoReal,
                PartnerId = x.PartnerId,
                Amount = x.TotalAmount,
                Currency = x.Currency,
                IssuedDate = x.Date,
                Serie = x.Serie,
                DueDate = x.PaymentDueDate,
                OverdueDays = (DateTime.Today > x.PaymentDueDate.Value.Date) ? (DateTime.Today - x.PaymentDueDate.Value.Date).Days : 0,
                Status = x.PaymentStatus ?? "Unpaid",
                ExtendDays = x.PaymentExtendDays,
                ExtendNote = x.PaymentNote,
                PaidAmount = x.PaidAmount,
                UnpaidAmount = x.UnpaidAmount,
                UnpaidAmountVnd = x.UnpaidAmountVnd,
                VoucherId = x.VoucherId,
                ConfirmBillingDate = x.ConfirmBillingDate,
                Type = x.Type,
                OfficeId = x.OfficeId,
                ServiceType = x.ServiceType
            });
            if (results == null) return null;
            switch (criteria.OverDueDays)
            {
                case Common.OverDueDate.Between1_15:
                    results = results.ToList().Where(x => x.OverdueDays < 16 && x.OverdueDays > 0).AsQueryable();
                    break;
                case Common.OverDueDate.Between16_30:
                    results = results.ToList().Where(x => x.OverdueDays < 31 && x.OverdueDays > 15).AsQueryable();
                    break;
                case Common.OverDueDate.Between31_60:
                    results = results.ToList().Where(x => x.OverdueDays < 61 && x.OverdueDays > 30).AsQueryable();
                    break;
                case Common.OverDueDate.Between61_90:
                    results = results.ToList().Where(x => x.OverdueDays < 91 && x.OverdueDays > 60).AsQueryable();
                    break;
            }
            return results;
        }

        private Expression<Func<AccAccountingManagement, bool>> GetQueryInvoicePermission(PermissionRange rangeSearch, ICurrentUser user)
        {
            Expression<Func<AccAccountingManagement, bool>> perQuery = null;
            switch (rangeSearch)
            {
                case PermissionRange.All:
                    break;
                case PermissionRange.Owner:
                    perQuery = x => x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Group:
                    perQuery = x => (x.GroupId == user.GroupId && x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Department:
                    perQuery = x => (x.DepartmentId == user.DepartmentId && x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == user.UserID;
                    break;
                case PermissionRange.Office:
                    perQuery = x => (x.OfficeId == user.OfficeID && x.CompanyId == user.CompanyID)
                                                || x.UserCreated == currentUser.UserID;
                    break;
                case PermissionRange.Company:
                    perQuery = x => x.CompanyId == user.CompanyID
                                                || x.UserCreated == currentUser.UserID;
                    break;
            }
            return perQuery;
        }

        public HandleState UpdateExtendDate(ExtendDateUpdatedModel model)
        {
            HandleState result = new HandleState();
            switch (model.PaymentType)
            {
                case Common.PaymentType.Invoice:
                    result = UpdateExtendDateVATInvoice(model);
                    break;
                case Common.PaymentType.OBH:
                    result = UpdateExtendDateOBH(model);
                    break;
            }
            return result;
        }

        private HandleState UpdateExtendDateOBH(ExtendDateUpdatedModel model)
        {
            string id = model.RefId;
            var soa = soaRepository.Get(x => x.Id == id).FirstOrDefault();
            soa.PaymentExtendDays = model.NumberDaysExtend;
            soa.PaymentNote = model.Note;
            soa.PaymentDueDate = soa.PaymentDueDate.Value.AddDays(model.NumberDaysExtend);
            soa.PaymentDatetimeUpdated = DateTime.Now;
            soa.UserModified = currentUser.UserID;
            soa.DatetimeModified = DateTime.Now;
            var result = soaRepository.Update(soa, x => x.Id == id);
            return result;
        }

        private HandleState UpdateExtendDateVATInvoice(ExtendDateUpdatedModel model)
        {

            HandleState hs = new HandleState();
            var refIdLst = DataContext.Get(x => x.BillingRefNo == model.RefId).Select(x => x.RefId).ToList();
            foreach (var refId in refIdLst)
            {
                Guid id = new Guid(refId);
                var vatInvoice = accountingManaRepository.Get(x => x.Id == id).FirstOrDefault();
                vatInvoice.PaymentExtendDays = model.NumberDaysExtend;
                vatInvoice.PaymentNote = model.Note;
                vatInvoice.PaymentDueDate = vatInvoice.PaymentDueDate.Value.AddDays(model.NumberDaysExtend);
                vatInvoice.PaymentDatetimeUpdated = DateTime.Now;
                vatInvoice.UserModified = currentUser.UserID;
                vatInvoice.DatetimeModified = DateTime.Now;
                accountingManaRepository.Update(vatInvoice, x => x.Id == id, false);
            }
            hs = accountingManaRepository.SubmitChanges();
            return hs;
        }

        public HandleState Delete(Guid id)
        {
            HandleState result = new HandleState();
            var item = DataContext.Get(x => x.Id == id).FirstOrDefault();
            AccAccountingManagement vatInvoice = null;
            AcctSoa soa = null;
            if (item.Type == "INVOICE")
            {
                vatInvoice = UpdateVATPaymentStatus(item);
            }
            else
            {
                soa = UpdateSOAPaymentStatus(item);
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    result = DataContext.Delete(x => x.Id == id);
                    if (result.Success)
                    {
                        if(vatInvoice != null)
                        {
                            var hsVAT = accountingManaRepository.Update(vatInvoice, x => x.Id == vatInvoice.Id);
                        }
                        else if(soa != null)
                        {
                            var hsSOA = soaRepository.Update(soa, x => x.Id == soa.Id);
                        }
                        DataContext.SubmitChanges();
                        accountingManaRepository.SubmitChanges();
                        soaRepository.SubmitChanges();
                        trans.Commit();
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = new HandleState(ex.Message);
                }
                finally{
                    trans.Dispose();
                }
                return result;
            }
        }

        private AccAccountingManagement UpdateVATPaymentStatus(AccAccountingPayment item)
        {
            var invoice = accountingManaRepository.Get(x => x.Id == new Guid(item.RefId)).FirstOrDefault();
            var totalPaid = DataContext.Get(x => x.RefId == item.RefId && x.Id != item.Id).Sum(x => x.PaymentAmount);
            if(totalPaid == 0)
            {
                invoice.PaymentStatus = "Unpaid";
            }
            else
            {
                invoice.PaymentStatus = "Paid A Part";
            }
            return invoice;
        }

        private AcctSoa UpdateSOAPaymentStatus(AccAccountingPayment item)
        {
            var soa = soaRepository.Get(x => x.Id == item.RefId).FirstOrDefault();
            var totalPaid = DataContext.Get(x => x.RefId == item.RefId && x.Id != item.Id).Sum(x => x.PaymentAmount);
            if (totalPaid == 0)
            {
                soa.PaymentStatus = "Unpaid";
            }
            else
            {
                soa.PaymentStatus = "Paid A Part";
            }
            return soa;

        }

        public List<AccountingPaymentImportModel> CheckValidImportInvoicePayment(List<AccountingPaymentImportModel> list)
        {
            IQueryable<CatPartner> partners = partnerRepository.Get();
            IQueryable<AccAccountingManagement> invoices = accountingManaRepository.Get();

            list.ForEach(item =>
            {
                CatPartner partner = partners.Where(x => x.AccountNo == item.PartnerAccount)?.FirstOrDefault();
                if(partner == null)
                {
                    item.PartnerAccountError = "Not found partner " + item.PartnerAccount;
                    item.IsValid = false;
                }
                else
                {
                    item.PartnerName = partner.ShortName;
                    var accountManagement = invoices.FirstOrDefault(x => x.Serie == item.SerieNo && x.InvoiceNoReal == item.InvoiceNo && x.PartnerId == partner.Id);
                    if (accountManagement == null)
                    {
                        item.PartnerAccountError = "Not found " + item.SerieNo + " and " + item.InvoiceNo + " of " + item.PartnerAccount;
                        item.IsValid = false;
                    }
                    else if (accountManagement.Currency != item.CurrencyId)
                    {
                        item.IsValid = false;
                        item.CurrencyIdError = "Currency not match with invoice";
                    }
                    else
                    {
                        if(accountManagement.PaymentStatus == "Paid")
                        {
                            item.InvoiceNoError = "This invoice has been paid";
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerId = partner.Id;
                            item.RefId = accountManagement.Id.ToString();

                            AccAccountingPayment lastItem = DataContext.Get(x => x.RefId == item.RefId)?.OrderByDescending(x => x.PaidDate).FirstOrDefault();
                            if (lastItem != null)
                            {
                                if (item.PaidDate < lastItem.PaidDate)
                                {
                                    item.PaidDateError = item.PaidDate + " invalid";
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        public HandleState ImportInvoicePayment(List<AccountingPaymentImportModel> list)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();
            List<AccAccountingManagement> managements = new List<AccAccountingManagement>();
            var groups = list.GroupBy(x => x.RefId);
            foreach(var group in groups)
            {
                var refPayment = accountingManaRepository.Get(x => x.Id == new Guid(group.Key)).FirstOrDefault();
                var existedPayments = DataContext.Get(x => x.RefId == refPayment.Id.ToString());
                decimal? totalExistedPayment = 0;
                if (group.Any())
                {
                    totalExistedPayment = existedPayments.Sum(x => x.PaymentAmount);
                    var ItemInGroups = group.OrderBy(x => x.PaidDate).ToList();
                    int i = 0;
                    bool isPaid = false;
                    foreach (var item in ItemInGroups)
                    {
                        i = i + 1;
                        int paymentNo = existedPayments.Count() + i;
                        totalExistedPayment = totalExistedPayment + item.PaymentAmount;
                        decimal? balance = refPayment.TotalAmount - totalExistedPayment;
                        if(balance <= 0)
                        {
                            isPaid = true;
                        }
                        AccAccountingPayment payment = new AccAccountingPayment
                        {
                            Id = Guid.NewGuid(),
                            RefId = item.RefId,
                            PaymentNo = item.InvoiceNo + "_" + string.Format("{0:00}", paymentNo),
                            PaymentAmount = item.PaymentAmount,
                            Balance = balance,
                            CurrencyId = refPayment.Currency,
                            PaidDate = item.PaidDate,
                            PaymentType = item.PaymentType,
                            Type = "INVOICE",
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            GroupId = currentUser.GroupId,
                            DepartmentId = currentUser.DepartmentId,
                            OfficeId = currentUser.OfficeID,
                            CompanyId = currentUser.CompanyID,

                            PaymentMethod = item.PaymentMethod,
                            ExchangeRate = item.ExchangeRate
                        };
                        results.Add(payment);
                    }
                    if(isPaid == true)
                    {
                        refPayment.PaymentStatus = "Paid";
                        refPayment.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    else
                    {
                        refPayment.PaymentStatus = "Paid A Part";
                        refPayment.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    managements.Add(refPayment);
                }
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(results);
                    if (hs.Success)
                    {
                        foreach(var item in managements)
                        {
                            var s = accountingManaRepository.Update(item, x => x.Id == item.Id);
                        }
                        trans.Commit();
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

        public ExtendDateUpdatedModel GetInvoiceExtendedDate(string refNo)
        {
            var billingNoList = DataContext.Get(x => x.BillingRefNo == refNo).Select(x => x.RefId).ToList();
            var invoice = accountingManaRepository.Get(x => billingNoList.Any(b => b.ToUpper() == x.Id.ToString())).FirstOrDefault();
            if (invoice == null) return null;
            return new ExtendDateUpdatedModel { RefId = refNo,
                Note = invoice.PaymentNote,
                NumberDaysExtend = invoice.PaymentExtendDays == null ? 0 : (int)invoice.PaymentExtendDays,
                PaymentType = PaymentType.Invoice
            };
        }

        public ExtendDateUpdatedModel GetOBHSOAExtendedDate(string id)
        {
            var soa = soaRepository.Get(x => x.Id == id).FirstOrDefault();
            if (soa == null) return null;
            return new ExtendDateUpdatedModel { RefId = id,
                Note = soa.PaymentNote,
                NumberDaysExtend = soa.PaymentExtendDays != null?(int)soa.PaymentExtendDays: 0,
                PaymentType = PaymentType.OBH
            };
        }

        public HandleState ImportOBHPayment(List<AccountingPaymentOBHImportTemplateModel> list)
        {
            List<AccAccountingPayment> results = new List<AccAccountingPayment>();
            List<AcctSoa> soas = new List<AcctSoa>();
            var groups = list.GroupBy(x => x.RefId);
            foreach (var group in groups)
            {
                AcctSoa refSOA = soaRepository.Get(x => x.Id == group.Key).FirstOrDefault();
                IQueryable<CsShipmentSurcharge> surcharges = surchargeRepository.Get(x => x.Soano == refSOA.Soano && x.Type == AccountingConstants.TYPE_CHARGE_OBH);
                IQueryable<AccAccountingPayment> existedPayments = DataContext.Get(x => x.RefId == refSOA.Id.ToString());

                decimal? totalExistedPayment = 0;
                if (group.Any())
                {
                    totalExistedPayment = existedPayments.Sum(x => x.PaymentAmount);
                    var ItemInGroups = group.OrderBy(x => x.PaidDate).ToList();
                    int i = 0;
                    bool isPaid = false;
                    foreach (var item in ItemInGroups)
                    {
                        i = i + 1;
                        int paymentNo = existedPayments.Count() + i;
                        totalExistedPayment = totalExistedPayment + item.PaymentAmount;
                        decimal? balance = surcharges.Sum(x => x.Total) - totalExistedPayment;
                        if (balance <= 0)
                        {
                            isPaid = true;
                        }
                        AccAccountingPayment payment = new AccAccountingPayment
                        {
                            Id = Guid.NewGuid(),
                            RefId = item.RefId,
                            PaymentNo = item.SoaNo + "_" + string.Format("{0:00}", paymentNo),
                            PaymentAmount = item.PaymentAmount,
                            Balance = balance,
                            CurrencyId = refSOA.Currency,
                            PaidDate = item.PaidDate,
                            PaymentType = item.PaymentType,
                            Type = "OBH",
                            UserCreated = currentUser.UserID,
                            UserModified = currentUser.UserID,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            GroupId = currentUser.GroupId,
                            DepartmentId = currentUser.DepartmentId,
                            OfficeId = currentUser.OfficeID,
                            CompanyId = currentUser.CompanyID,

                            PaymentMethod = item.PaymentMethod,
                            ExchangeRate = item.ExchangeRate,

                        };
                        results.Add(payment);
                    }
                    if (isPaid == true)
                    {
                        refSOA.PaymentStatus = "Paid";
                        refSOA.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    else
                    {
                        refSOA.PaymentStatus = "Paid A Part";
                        refSOA.PaymentDatetimeUpdated = DateTime.Now;
                    }
                    soas.Add(refSOA);
                }
            }
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var hs = DataContext.Add(results);
                    if (hs.Success)
                    {
                        foreach (var item in soas)
                        {
                            var s = soaRepository.Update(item, x => x.Id == item.Id);
                        }
                        trans.Commit();
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

        public List<AccountingPaymentOBHImportTemplateModel> CheckValidImportOBHPayment(List<AccountingPaymentOBHImportTemplateModel> list)
        {
            IQueryable<CatPartner> partners = partnerRepository.Get();
            IQueryable<AcctSoa> soas = soaRepository.Get();

            list.ForEach(item =>
            {
                CatPartner partner = partners.Where(x => x.AccountNo == item.PartnerId)?.FirstOrDefault();
                if (partner == null)
                {
                    item.PartnerAccountError = "'" + item.PartnerId + "' Not found";
                    item.IsValid = false;
                }
                else
                {
                    item.PartnerName = partner.ShortName;
                    var soa = soas.FirstOrDefault(x => x.Soano == item.SoaNo && x.Customer == partner.Id);
                    if (soa == null)
                    {
                        item.SoaNoError = "Not found SOA No '" + item.SoaNo + "' of '" + item.PartnerId + "'";
                        item.IsValid = false;
                    }
                    else if (soa.Currency.Trim() != item.CurrencyId.Trim())
                    {
                        item.IsValid = false;
                        item.CurrencyIdError = "Currency not match with soa";
                    }
                    else
                    {
                        if (soa.PaymentStatus == "Paid")
                        {
                            item.SoaNoError = "This SOA has been paid";
                            item.IsValid = false;
                        }
                        else
                        {
                            item.PartnerId = partner.AccountNo;
                            item.RefId = soa.Id.ToString();
                            var lastItem = DataContext.Get(x => x.RefId == item.RefId)?.OrderByDescending(x => x.PaidDate).FirstOrDefault();
                            if (lastItem != null)
                            {
                                if (item.PaidDate < lastItem.PaidDate)
                                {
                                    item.PaidDateError = item.PaidDate + " invalid";
                                }
                            }
                        }
                    }
                }
            });
            return list;
        }

        /// <summary>
        /// Get data export Statement of Receivable Customers
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public IQueryable<AccountingCustomerPaymentExport> GetDataExportAccountingCustomerPayment(PaymentCriteria criteria)
        {
            var data = Query(criteria);
            if (data == null) return null;
            var partners = partnerRepository.Get();
            var paymentData = QueryInvoiceDataPayment(criteria);
            var surchargeData = surchargeRepository.Get();
            var receiptData = acctReceiptRepository.Get(x=>x.Status == AccountingConstants.RECEIPT_STATUS_DONE);
            var resultsQuery = (from invoice in data
                                join surcharge in surchargeData on invoice.RefId equals surcharge.AcctManagementId.ToString()
                                join payments in paymentData on invoice.RefId.ToLower() equals payments.RefId into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                join parent in partners on part.ParentId equals parent.Id into grpParents
                                from pa in grpParents.DefaultIfEmpty()
                                join rcpts in receiptData on payment.ReceiptId equals rcpts.Id into grpReceipts
                                from rcpt in grpReceipts.DefaultIfEmpty()
                                select new
                                {
                                    invoice,
                                    surcharge.JobNo,
                                    surcharge.Hblno,
                                    surcharge.Mblno,
                                    PartnerCode = part.AccountNo,
                                    ParentCode = pa != null ? pa.AccountNo : string.Empty,
                                    PartnerName = part.PartnerNameEn,
                                    BillingRefNo = string.IsNullOrEmpty(surcharge.Soano) ? surcharge.DebitNo : surcharge.Soano,
                                    PaymentType = payment == null ? string.Empty : payment.Type,
                                    PaymentAmountVnd = payment == null ? 0 : payment.PaymentAmountVnd,
                                    UnpaidPaymentAmountVnd = payment == null ? 0 : payment.UnpaidPaymentAmountVnd,
                                    Negative = payment == null ? null : payment.Negative,
                                    PaymentRefNo = rcpt == null ? null : rcpt.PaymentRefNo,
                                    PaymentDate = rcpt == null ? null : rcpt.PaymentDate
                                }).ToList();

            var resultGroups = resultsQuery.GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.PartnerCode,
                x.PartnerName,
                x.ParentCode,
                x.BillingRefNo,
            }).Select(x => new { grp = x.Key, invoice = x.Select(z => z.invoice), surcharge = x.Select(z => new { z.JobNo, z.Mblno, z.Hblno }), payment = x.Select(z => new { z.PaymentType, z.PaymentRefNo, z.PaymentDate, z.PaymentAmountVnd, z.UnpaidPaymentAmountVnd, z.Negative }) });
            var results = new List<AccountingCustomerPaymentExport>();
            var soaLst = soaRepository.Get().ToLookup(x => x.Soano);
            var cdNoteLst = cdNoteRepository.Get().ToLookup(x => x.Code);
            var userLst = userRepository.Get().ToLookup(x => x.Id);
            var employeeLst = sysEmployeeRepository.Get().ToLookup(x => x.Id);
            foreach (var item in resultGroups)
            {
                var payment = new AccountingCustomerPaymentExport();
                var invoice = item.invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE);
                var invoiceObh = item.invoice.Where(x => x.Type == AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE);
                var acctPayment = item.payment.FirstOrDefault();
                var sur = item.surcharge.FirstOrDefault();
                payment.PartnerCode = item.grp.PartnerCode;
                payment.PartnerName = item.grp.PartnerName;
                payment.ParentCode = item.grp.ParentCode;
                payment.InvoiceNo = invoice.FirstOrDefault()?.InvoiceNoReal;
                payment.InvoiceDate = invoice.FirstOrDefault()?.IssuedDate;
                payment.BillingRefNo = item.grp.BillingRefNo;
                payment.BillingDate = invoice.FirstOrDefault()?.ConfirmBillingDate;
                payment.DueDate = invoice.FirstOrDefault()?.DueDate;

                // Use amount VND
                payment.UnpaidAmountInv = acctPayment == null ? (invoice.FirstOrDefault()?.UnpaidAmountVnd ?? 0) : item.payment.Where(x => x.PaymentType != "OBH").FirstOrDefault()?.UnpaidPaymentAmountVnd ?? 0;
                payment.UnpaidAmountOBH = acctPayment == null ? invoiceObh.Sum(x => x.UnpaidAmountVnd ?? 0) : (item.payment.Where(x => x.PaymentType == "OBH").FirstOrDefault()?.UnpaidPaymentAmountVnd ?? 0);
                payment.PaidAmount = acctPayment == null ? 0 : item.payment.Where(x => x.PaymentType != "OBH").FirstOrDefault()?.PaymentAmountVnd;
                payment.PaidAmountOBH = acctPayment == null ? 0 : item.payment.Where(x => x.PaymentType == "OBH").Sum(x => x.PaymentAmountVnd ?? 0);
                payment.JobNo = sur?.JobNo;
                payment.MBL = sur?.Mblno;
                payment.HBL = sur?.Hblno;
                payment.CustomNo = customsDeclarationRepository.Get(x => x.JobNo == sur.JobNo).FirstOrDefault()?.ClearanceNo;
                // Get saleman name
                var salemanId = catContractRepository.Get(x => x.Active == true && x.PartnerId == item.grp.PartnerId
                                                                               && x.OfficeId.Contains(item.invoice.FirstOrDefault().OfficeId.ToString())
                                                                               && x.SaleService.Contains(item.invoice.FirstOrDefault().ServiceType)).FirstOrDefault()?.SaleManId;
                if (!string.IsNullOrEmpty(salemanId))
                {
                    var employeeId = userLst[salemanId].FirstOrDefault()?.EmployeeId;
                    payment.Salesman = salemanId == null ? string.Empty : employeeLst[employeeId].FirstOrDefault().EmployeeNameEn;
                }
                // Get creator name
                var creatorId = soaLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                if (string.IsNullOrEmpty(creatorId))
                {
                    creatorId = cdNoteLst[item.grp.BillingRefNo].FirstOrDefault()?.UserCreated;
                    var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                    payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                }
                else
                {
                    var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                    payment.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                }

                payment.receiptDetail = new List<AccountingReceiptDetail>();
                var receiptGroup = item.payment.Where(x => !string.IsNullOrEmpty(x.PaymentRefNo)).GroupBy(x => new { x.PaymentRefNo }).Select(x => new { grp = x.Key, Payment = x.Select(z => new { z.PaymentDate, z.PaymentType, z.PaymentAmountVnd, z.Negative }) });
                foreach (var rcp in receiptGroup)
                {
                    var detail = new AccountingReceiptDetail();
                    detail.PaymentRefNo = rcp.grp.PaymentRefNo;
                    detail.PaymentDate = rcp.Payment.FirstOrDefault().PaymentDate;
                    detail.PaidAmount = rcp.Payment.Where(z => z.PaymentType != "OBH").FirstOrDefault()?.PaymentAmountVnd ?? 0;
                    detail.PaidAmountOBH = rcp.Payment.Where(z => z.PaymentType == "OBH").Sum(x => x.PaymentAmountVnd ?? 0);
                    payment.receiptDetail.Add(detail);
                }
                results.Add(payment);
            }
            return results?.OrderBy(x => x.PartnerName).ThenBy(x => x.BillingRefNo).AsQueryable();
        }

        public IQueryable<AccountingAgencyPaymentExport> GetDataExportAccountingAgencyPayment(PaymentCriteria criteria)
        {
            var data = Query(criteria);
            if (data == null) return null;
            var partners = partnerRepository.Get(x=>x.PartnerType == "Agent");
            var paymentData = QueryInvoiceDataPayment(criteria);
            var resultsQuery = (from invoice in data
                                join payment in paymentData on invoice.RefId.ToLower() equals payment.RefId into grpPayment
                                from payment in grpPayment.DefaultIfEmpty()
                                join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                join parent in partners on part.ParentId equals parent.Id into grpParents
                                from pa in grpParents.DefaultIfEmpty()
                                join rcpt in acctReceiptRepository.Get() on payment.ReceiptId equals rcpt.Id into grpReceipts
                                from rcpts in grpReceipts.DefaultIfEmpty()
                                where part != null //Lấy partner có type Agent
                                where rcpts.Status == "Done" 
                                select new
                                {
                                    invoice,
                                    PartnerCode = part.AccountNo,
                                    ParentCode = pa != null ? pa.AccountNo : string.Empty,
                                    PartnerName = part.PartnerNameEn,
                                    //payment.Id,
                                    RefNo= payment.BillingRefNo,
                                    InvoiceNo = payment.Type == "OBH" ? string.Empty : payment.InvoiceNo,
                                    PaymentType = payment.Type,
                                    PaymentNo = payment.PaymentNo,
                                    PaidDate = payment.PaidDate,
                                    payment.PaymentAmount,
                                    //payment.ReceiptId,
                                    payment.PaymentAmountUsd,
                                    payment.UnpaidPaymentAmountUsd,
                                    payment.CreditAmountUsd,
                                    rcpts.PaymentRefNo,
                                    rcpts.PaymentDate,
                                    rcpts.PaidAmountUsd
                                }).ToList();


            var resultGroups = resultsQuery.GroupBy(x => new
            {
                x.invoice.PartnerId,
                x.invoice.Type,
                x.invoice.IssuedDate,
                x.PartnerCode,
                x.ParentCode,
                x.RefNo,
                x.PartnerName,
                x.InvoiceNo,
                x.PaymentType,
                x.PaidDate,
                x.invoice.OfficeId,
                x.invoice.DueDate,
                x.invoice.ServiceType
            });

            var results = new List<AccountingAgencyPaymentExport>();
            var soaLst = soaRepository.Get().ToLookup(x => x.Soano);
            var cdNoteLst = cdNoteRepository.Get().ToLookup(x => x.Code);
            var userLst = userRepository.Get().ToLookup(x => x.Id);
            var employeeLst = sysEmployeeRepository.Get().ToLookup(x => x.Id);

            foreach (var item in resultGroups)
            {
                var agent = new AccountingAgencyPaymentExport();
                agent.AgentParentCode = item.Key.ParentCode;
                agent.AgentPartnerCode = item.Key.PartnerCode;
                agent.AgentPartnerName = item.Key.PartnerName;
                agent.InvoiceNo = item.Key.InvoiceNo;
                agent.InvoiceDate = item.Key.IssuedDate;
                agent.PaidDate = item.Key.PaidDate;
                agent.RefNo = item.Key.RefNo;

                var billingDebit = surchargeRepository.Get(x => x.DebitNo == agent.RefNo || x.Soano == agent.RefNo).FirstOrDefault();
                agent.JobNo = billingDebit?.JobNo;
                agent.MBL = billingDebit?.Mblno;
                agent.HBL = billingDebit?.Hblno;

                var trans = csTransactionRepository.Get(x => x.JobNo == agent.JobNo).FirstOrDefault();
                agent.EtaDate = trans?.Eta;
                agent.EtdDate = trans?.Etd;

                agent.UnpaidAmountInv = item.Where(x => x.invoice.Type == "Invoice").FirstOrDefault()?.UnpaidPaymentAmountUsd ?? 0;
                agent.UnpaidAmountOBH = item.Where(x => x.invoice.Type == "InvoiceTemp").FirstOrDefault()?.UnpaidPaymentAmountUsd ?? 0;
                agent.PaidAmount = item.Where(x => x.invoice.Type == "Invoice").FirstOrDefault()?.PaidAmountUsd ?? 0;
                agent.PaidAmountOBH = item.Where(x => x.invoice.Type == "InvoiceTemp").Sum(x => x.PaidAmountUsd ?? 0);

                var a = currencyExchangeService.CurrencyExchangeRateConvert(agent.UnpaidAmountInv, item.Key.IssuedDate, AccountingConstants.CURRENCY_LOCAL, AccountingConstants.CURRENCY_USD);
                agent.UnpaidAmountOBH = currencyExchangeService.CurrencyExchangeRateConvert(agent.UnpaidAmountOBH, item.Key.IssuedDate, AccountingConstants.CURRENCY_LOCAL, AccountingConstants.CURRENCY_USD);

                if (item.Key.PaymentType == "CREDIT")
                    agent.CreditAmount = item.FirstOrDefault().UnpaidPaymentAmountUsd??0;
                else
                    agent.CreditAmount = item.FirstOrDefault().CreditAmountUsd ?? 0;

                //Get saleman name
                var salemanId = catContractRepository.Get(x => x.Active == true && x.PartnerId == item.Key.PartnerId
                                                                               && x.OfficeId.Contains(item.Key.OfficeId.ToString())
                                                                               && x.SaleService.Contains(item.Key.ServiceType)).FirstOrDefault()?.SaleManId;
                if (!string.IsNullOrEmpty(salemanId))
                    if (!string.IsNullOrEmpty(salemanId))
                {
                    var employeeId = userLst[salemanId].FirstOrDefault()?.EmployeeId;
                    agent.Salesman = salemanId == null ? string.Empty : employeeLst[employeeId].FirstOrDefault().EmployeeNameEn;
                }
                // Get creator name
                var creatorId = soaLst[item.Key.RefNo].FirstOrDefault()?.UserCreated;
                if (string.IsNullOrEmpty(creatorId))
                {
                    creatorId = cdNoteLst[item.Key.RefNo].FirstOrDefault()?.UserCreated;
                    var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                    agent.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                }
                else
                {
                    var creator = string.IsNullOrEmpty(creatorId) ? string.Empty : userLst[creatorId].FirstOrDefault()?.EmployeeId;
                    agent.Creator = string.IsNullOrEmpty(creatorId) ? string.Empty : employeeLst[creator].FirstOrDefault()?.EmployeeNameEn;
                }

                agent.details = new List<AccountingAgencyPaymentExportDetail>();
                foreach (var rcp in item)
                {
                    var detail = new AccountingAgencyPaymentExportDetail();
                    detail.RefNo = rcp.PaymentRefNo;
                    detail.PaidDate = rcp.PaymentDate;
                    detail.Debit = rcp.PaidAmountUsd ?? 0;
                    detail.Credit = 0;

                    agent.details.Add(detail);
                }
                results.Add(agent);
            }

            return results.AsQueryable();
        }
    }
}
