using AutoMapper;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountingPayment;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.Service.Models;
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

        public AccAccountingPaymentService(IContextBase<AccAccountingPayment> repository, 
            IMapper mapper, 
            IContextBase<SysUser> userRepo,
            IContextBase<AccAccountingManagement> accountingManaRepo,
            IContextBase<CatPartner> partnerRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<CsShipmentSurcharge> surchargeRepo) : base(repository, mapper)
        {
            userRepository = userRepo;
            accountingManaRepository = accountingManaRepo;
            partnerRepository = partnerRepo;
            soaRepository = soaRepo;
            surchargeRepository = surchargeRepo;
        }

        public IQueryable<AccAccountingPaymentModel> GetBy(string refId)
        {
            var data = Get(x => x.RefId == refId);
            var users = userRepository.Get();
            var results = data.Join(users, x => x.UserModified, y => y.Id, (x, y) => new AccAccountingPaymentModel
            {
                Id = x.Id,
                RefNo = x.RefNo,
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
                           select new { invoice, part.ShortName, detail.Balance, detail.PaymentAmount })
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
                         })
                         .Select(x => new AccountingPaymentModel {
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
            List<string> refNos = GetRefNo(criteria.RefNos);
            Expression<Func<AcctSoa, bool>> query = x => (x.Customer == criteria.PartnerId || criteria.PartnerId == null)
                                                      && (refNos.Contains(x.Soano) || refNos.Count == 0)
                                                      && (x.PaymentStatus == criteria.PaymentStatus || string.IsNullOrEmpty(x.PaymentStatus) || string.IsNullOrEmpty(criteria.PaymentStatus));
            if (criteria.IssuedDate != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date == criteria.IssuedDate.Value.Date);
            }
            if (criteria.DueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date == criteria.DueDate.Value.Date);
            }
            if (criteria.UpdatedDate != null)
            {
                query = query.And(x => x.PaymentDatetimeUpdated != null && x.PaymentDatetimeUpdated.Value.Date == criteria.UpdatedDate.Value.Date);
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
                                                        && (refNos.Contains(x.Mblno) || refNos.Count == 0)
                                                        && (refNos.Contains(x.Hblno) || refNos.Count == 0));
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

        private List<string> GetRefNo(string refNos)
        {
            List<string> results = null;
            if (!string.IsNullOrEmpty(refNos))
            {
                results = refNos.Split('\n').Select(x => x.Trim()).Where(x => x != null).ToList();
            }
            return results;
        }

        private IQueryable<AccountingPaymentModel> QueryInvoicePayment(PaymentCriteria criteria)
        {
            List<string> refNos = GetRefNo(criteria.RefNos);
            Expression<Func<AccAccountingManagement, bool>> query = x => x.InvoiceNoReal != null && x.Status != "New"
                                                                      && (x.PartnerId == criteria.PartnerId || string.IsNullOrEmpty(criteria.PartnerId))
                                                                      && (refNos.Contains(x.InvoiceNoReal) || refNos.Count == 0)
                                                                      && (x.PaymentStatus == criteria.PaymentStatus || string.IsNullOrEmpty(x.PaymentStatus) || string.IsNullOrEmpty(criteria.PaymentStatus));
            if (criteria.IssuedDate != null) {
                query = query.And(x => x.Date.Value.Date == criteria.IssuedDate.Value.Date);
            }
            if(criteria.DueDate != null)
            {
                query = query.And(x => x.PaymentDueDate.Value.Date == criteria.DueDate.Value.Date);
            }
            if(criteria.UpdatedDate != null)
            {
                query = query.And(x => x.PaymentDatetimeUpdated != null && x.PaymentDatetimeUpdated.Value.Date == criteria.UpdatedDate.Value.Date);
            }

            var data = accountingManaRepository.Get(query);
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
            var results = data.OrderByDescending(x => x.PaymentDatetimeUpdated).Select(x => new AccountingPaymentModel {
                RefId = x.VoucherId,
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
            return results;
        }
    }
}
