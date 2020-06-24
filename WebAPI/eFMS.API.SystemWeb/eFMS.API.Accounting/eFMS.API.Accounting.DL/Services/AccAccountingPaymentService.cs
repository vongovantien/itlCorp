﻿using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
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
        private readonly ICurrentUser currentUser;

        public AccAccountingPaymentService(IContextBase<AccAccountingPayment> repository, 
            IMapper mapper, 
            IContextBase<SysUser> userRepo,
            IContextBase<AccAccountingManagement> accountingManaRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            ICurrentUser currUser) : base(repository, mapper)
        {
            userRepository = userRepo;
            accountingManaRepository = accountingManaRepo;
            partnerRepository = partnerRepo;
            soaRepository = soaRepo;
            surchargeRepository = surchargeRepo;
            currentUser = currUser;
        }

        public IQueryable<AccAccountingPaymentModel> GetBy(string refId)
        {
            var data = DataContext.Get(x => x.RefId == refId).OrderBy(x => x.PaidDate);
            var users = userRepository.Get();
            var results = data.Join(users, x => x.UserModified, y => y.Id, (x, y) => new AccAccountingPaymentModel
            {
                Id = x.Id,
                RefNo = x.RefId,
                PaymentNo = x.PaymentNo,
                PaymentAmount = x.PaymentAmount,
                Balance = x.Balance,
                CurrencyId = x.CurrencyId,
                PaidDate = x.PaidDate,
                PaymentType = x.PaymentType,
                UserCreated = x.UserCreated,
                DatetimeCreated = x.DatetimeCreated,
                UserModified = x.UserModified,
                DatetimeModified = x.DatetimeModified,
                UserModifiedName = y.Username
            });
            return results;
        }

        public IQueryable<AccountingPaymentModel> Paging(PaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            if (data == null)
            {
                rowsCount = 0;
                return null;
            }

            var _totalItem = data.Select(s => s.RefId).Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            if (size > 0)
            {
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
                data = GetReferencesData(data);
            }

            return data;
        }

        private IQueryable<AccountingPaymentModel> GetReferencesData(IQueryable<AccountingPaymentModel> data)
        {
            var partners = partnerRepository.Get();
            var results = (from invoice in data
                                join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                from part in grpPartners.DefaultIfEmpty()
                                    join payment in DataContext.Get() on invoice.RefId equals payment.RefId into grpPayments
                                    from detail in grpPayments.DefaultIfEmpty()
                                select new { invoice, part.ShortName, 
                                    Balance = detail != null? detail.Balance: 0,
                                    PaymentAmount = detail != null? detail.PaymentAmount: 0 })
                            .GroupBy(x => new AccountingPaymentModel {
                             RefId = x.invoice.RefId,
                             PartnerId = x.invoice.PartnerId,
                             InvoiceNoReal = x.invoice.InvoiceNoReal,
                             PartnerName = x.ShortName,
                             Amount = x.invoice.Amount,
                             Currency = x.invoice.Currency,
                             IssuedDate = x.invoice.IssuedDate,
                             Serie = x.invoice.Serie,
                             DueDate = x.invoice.DueDate,
                             OverdueDays = x.invoice.OverdueDays,
                             Status = x.invoice.Status,
                             ExtendDays = x.invoice.ExtendDays,
                             ExtendNote = x.invoice.ExtendNote
                         }).Select(x => new AccountingPaymentModel {
                             RefId = x.Key.RefId,
                             PartnerId = x.Key.PartnerId,
                             InvoiceNoReal = x.Key.InvoiceNoReal,
                             PartnerName = x.Key.PartnerName,
                             Amount = x.Key.Amount,
                             Currency = x.Key.Currency,
                             IssuedDate = x.Key.IssuedDate,
                             Serie = x.Key.Serie,
                             DueDate = x.Key.DueDate,
                             OverdueDays = x.Key.OverdueDays,
                             Status = x.Key.Status,
                             ExtendDays = x.Key.ExtendDays,
                             ExtendNote = x.Key.ExtendNote,
                             PaidAmount = x.Sum(c => c.PaymentAmount),
                             UnpaidAmount = x.Sum(c => c.Balance)
                         });
            return results;
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

        // get charges that have type OBH and SOANo
        private IQueryable<AccountingPaymentModel> QueryOBHPayment(PaymentCriteria criteria)
        {
            Expression<Func<AcctSoa, bool>> query = x => (x.Customer == criteria.PartnerId || criteria.PartnerId == null)
                                                      && (criteria.ReferenceNos.Contains(x.Soano) || criteria.ReferenceNos == null)
                                                      && (x.PaymentStatus == criteria.PaymentStatus || string.IsNullOrEmpty(x.PaymentStatus) || string.IsNullOrEmpty(criteria.PaymentStatus));
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
                query = query.And(x => x.PaymentDatetimeUpdated != null && x.PaymentDatetimeUpdated.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDatetimeUpdated.Value.Date <= criteria.FromUpdatedDate.Value.Date);
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
            var surcharges = surchargeRepository.Get(x => x.Type == "OBH" 
                                                        && !string.IsNullOrEmpty(x.Soano)
                                                        && (criteria.ReferenceNos.Contains(x.Mblno) || criteria.ReferenceNos.Count == 0)
                                                        && (criteria.ReferenceNos.Contains(x.Hblno) || criteria.ReferenceNos.Count == 0));
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

        private IQueryable<AccountingPaymentModel> QueryInvoicePayment(PaymentCriteria criteria)
        {
            var status = criteria.PaymentStatus.Split();
            Expression<Func<AccAccountingManagement, bool>> query = x => x.InvoiceNoReal != null && x.Status != "New"
                                                                      && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId))
                                                                      && (criteria.ReferenceNos.Contains(x.InvoiceNoReal) || criteria.ReferenceNos == null)
                                                                      && (criteria.PaymentStatus.Contains(x.PaymentStatus) || string.IsNullOrEmpty(x.PaymentStatus) || string.IsNullOrEmpty(criteria.PaymentStatus));
            if (criteria.FromIssuedDate != null && criteria.ToIssuedDate != null) {
                query = query.And(x => x.Date.Value.Date >= criteria.FromIssuedDate.Value.Date && x.Date.Value.Date <= criteria.ToIssuedDate.Value.Date);
            }
            if(criteria.FromDueDate != null && criteria.ToDueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date >= criteria.FromDueDate.Value.Date && x.PaymentDueDate.Value.Date <= criteria.ToDueDate.Value.Date);
            }
            if(criteria.FromUpdatedDate != null)
            {
                query = query.And(x => x.PaymentDatetimeUpdated != null && x.PaymentDatetimeUpdated.Value.Date >= criteria.FromUpdatedDate.Value.Date && x.PaymentDatetimeUpdated.Value.Date <= criteria.FromUpdatedDate.Value.Date);
            }

            var data = accountingManaRepository.Get(query);
            if (data == null) return null;
            var results = data.OrderByDescending(x => x.PaymentDatetimeUpdated).Select(x => new AccountingPaymentModel {
                RefId = x.Id.ToString(),
                InvoiceNoReal = x.InvoiceNoReal,
                PartnerId = x.PartnerId,
                Amount = x.TotalAmount,
                Currency = x.Currency,
                IssuedDate = x.Date,
                Serie = x.Serie,
                DueDate = x.PaymentDueDate,
                OverdueDays = (DateTime.Today > x.PaymentDueDate.Value.Date)? (DateTime.Today - x.PaymentDueDate.Value.Date).Days: 0,
                Status = x.PaymentStatus,
                ExtendDays = x.PaymentExtendDays,
                ExtendNote = x.PaymentNote
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
            int id = Convert.ToInt32(model.RefId);
            var soa = soaRepository.Get(x => x.Id == id).FirstOrDefault();
            soa.PaymentExtendDays = (short)model.NumberDaysExtend;
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
            Guid id = new Guid(model.RefId);
            var vatInvoice = accountingManaRepository.Get(x => x.Id == id).FirstOrDefault();
            vatInvoice.PaymentExtendDays = (short)model.NumberDaysExtend;
            vatInvoice.PaymentNote = model.Note;
            vatInvoice.PaymentDueDate = vatInvoice.PaymentDueDate.Value.AddDays(model.NumberDaysExtend);
            vatInvoice.PaymentDatetimeUpdated = DateTime.Now;
            vatInvoice.UserModified = currentUser.UserID;
            vatInvoice.DatetimeModified = DateTime.Now;
            var result = accountingManaRepository.Update(vatInvoice, x => x.Id == id);
            return result;
        }

        public HandleState Delete(Guid id)
        {
            var item = DataContext.Get(x => x.Id == id).FirstOrDefault();
            var hs = DataContext.Delete(x => x.Id == id, false);
            if (hs.Success)
            {
                if(item.Type == "INVOICE")
                {
                    var s = UpdateVATPaymentStatus(item);
                }
                else
                {
                    var s = UpdateSOAPaymentStatus(item);
                }
            }
            return hs;
        }

        private HandleState UpdateVATPaymentStatus(AccAccountingPayment item)
        {
            var invoice = accountingManaRepository.Get(x => x.Id == new Guid(item.RefId)).FirstOrDefault();
            var remainPayments = DataContext.Get(x => x.RefId == item.RefId && x.Id != item.Id).Sum(x => x.PaymentAmount);
            return null;
        }

        private HandleState UpdateSOAPaymentStatus(AccAccountingPayment item)
        {
            throw new NotImplementedException();
        }

        public List<AccountingPaymentImportModel> CheckValidImportInvoicePayment(List<AccountingPaymentImportModel> list)
        {
            var partners = partnerRepository.Get().ToList();
            var invoices = accountingManaRepository.Get().ToList();
            list.ForEach(item =>
            {
                var partner = partners.Where(x => x.AccountNo == item.PartnerAccount)?.FirstOrDefault();
                if(partner == null)
                {
                    item.PartnerAccountError = "Not found partner " + item.PartnerAccount;
                    item.IsValid = false;
                }
                else
                {
                    var accountManagement = invoices.FirstOrDefault(x => x.Serie == item.SerieNo && x.InvoiceNoReal == item.InvoiceNo && x.PartnerId == partner.Id);
                    if (accountManagement == null)
                    {
                        item.PartnerAccountError = "Not found " + item.SerieNo + " and " + item.InvoiceNo + " of " + item.PartnerAccount;
                        item.IsValid = false;
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

        public HandleState Import(List<AccountingPaymentImportModel> list)
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
                        var payment = new AccAccountingPayment
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
                            CompanyId = currentUser.CompanyID
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
    }
}
