using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingPrePaidPaymentService : RepositoryBase<AcctCdnote, AcctCdNoteModel>, IAccountingPrePaidPaymentService
    {
        private readonly ICurrentUser currentUser;

        private eFMSDataContextDefault DC => (eFMSDataContextDefault)DataContext.DC;

        public AccountingPrePaidPaymentService(ICurrentUser currentUser,
            IContextBase<AcctCdnote> repository,
            IMapper mapper,
            IContextBase<CatPartner> catPartnerRepository) : base(repository, mapper)
        {
            this.currentUser = currentUser;
        }

        public IQueryable<AccPrePaidPaymentResult> Paging(AccountingPrePaidPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            IQueryable<AcctCdnote> cdNotes = GetQuery(criteria);
            rowsCount = cdNotes.CountAsync().Result;
            if (rowsCount == 0)
            {
                return Enumerable.Empty<AccPrePaidPaymentResult>().AsQueryable();
            }
            var resultPaging = cdNotes.Skip((page - 1) * size).Take(size);
            IQueryable<AccPrePaidPaymentResult> result = FormatCdNotes(resultPaging);
            return result;
        }

        private IQueryable<AcctCdnote> GetQuery(AccountingPrePaidPaymentCriteria criteria)
        {
            Expression<Func<AcctCdnote, bool>> query = x => (x.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID
            || x.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID);

            if (!string.IsNullOrEmpty(criteria.PartnerId))
            {
                query = query.And(x => x.PartnerId == criteria.PartnerId);
            }
            if (!string.IsNullOrEmpty(criteria.Currency))
            {
                query = query.And(x => x.CurrencyId == criteria.Currency);
            }
            if (!string.IsNullOrEmpty(criteria.Status))
            {
                query = query.And(x => x.Status == criteria.Status);
            }
            if (!string.IsNullOrEmpty(criteria.SalesmanId))
            {
                query = query.And(x => x.SalemanId == criteria.SalesmanId);
            }
            if (criteria.OfficeId != null && criteria.OfficeId != Guid.Empty)
            {
                query = query.And(x => x.OfficeId == criteria.OfficeId);
            }

            if (criteria.DepartmentIds != null && criteria.DepartmentIds.Count > 0)
            {
                query = query.And(x => criteria.DepartmentIds.Contains(x.DepartmentId ?? 1));
            }
            if (criteria.IssueDateFrom != null && criteria.IssueDateTo != null)
            {
                query = query.And(x => x.DatetimeCreated.Value.Date >= criteria.IssueDateFrom.Value.Date && x.DatetimeCreated.Value.Date <= criteria.IssueDateTo.Value.Date);
            }

            if (criteria.ServiceDateFrom != null && criteria.ServiceDateTo != null)
            {
                var jobsOps = DC.OpsTransaction.Where(x => x.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && x.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date)
                    .Select(x => x.Id).ToList();
                var jobsCs = DC.CsTransaction.Where(x => x.ServiceDate.Value.Date >= criteria.ServiceDateFrom.Value.Date && x.ServiceDate.Value.Date <= criteria.ServiceDateTo.Value.Date)
                    .Select(x => x.Id).ToList();

                List<Guid> jobUnion = new List<Guid>();
                if (jobsOps.Count > 0)
                {
                    jobUnion.AddRange(jobsOps);
                }
                if (jobsCs.Count > 0)
                {
                    jobUnion.AddRange(jobsCs);
                }
                if (jobUnion.Count() > 0)
                {
                    query = query.And(x => jobUnion.Contains(x.JobId));
                }
            }

            if (criteria.Keywords != null && criteria.Keywords.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "JobID":
                        var keywordsOps = criteria.Keywords.Where(s => s.Contains("LOG")).ToList();
                        var keywordsCs = criteria.Keywords.Where(s => !s.Contains("LOG")).ToList();
                        var jobsOps = DC.OpsTransaction.Where(x => keywordsOps.Contains(x.JobNo)).Select(x => x.Id).ToList();
                        var jobsCs = DC.CsTransaction.Where(x => keywordsCs.Contains(x.JobNo)).Select(x => x.Id).ToList();

                        List<Guid> jobUnion = new List<Guid>();
                        if (jobsOps.Count > 0)
                        {
                            jobUnion.AddRange(jobsOps);
                        }
                        if (jobsCs.Count > 0)
                        {
                            jobUnion.AddRange(jobsCs);
                        }
                        if (jobUnion.Count() > 0)
                        {
                            query = query.And(x => jobUnion.Contains(x.JobId));
                        }
                        break;
                    case "DebitNo":
                        query = query.And(x => criteria.Keywords.Contains(x.Code));
                        break;
                    default:
                        query = query.And(x => criteria.Keywords.Contains(x.Code));
                        break;
                }
            }

            var cdNotes = DC.AcctCdnote.Where(query).OrderByDescending(x => x.DatetimeModified); ;

            return cdNotes;
        }

        private IQueryable<AccPrePaidPaymentResult> FormatCdNotes(IQueryable<AcctCdnote> cdNotes)
        {
            var queryGroup = from cd in cdNotes
                             join sur in DC.CsShipmentSurcharge on cd.Code equals sur.DebitNo
                             group sur by new {
                                 sur.DebitNo,
                                 cd.Id,
                                 cd.JobId,
                                 cd.SalemanId,
                                 cd.OfficeId,
                                 cd.DepartmentId,
                                 cd.UserCreated,
                                 cd.DatetimeCreated,
                                 cd.PartnerId,
                                 cd.CurrencyId,
                                 cd.SyncStatus,
                                 cd.LastSyncDate,
                                 cd.Code,
                                 cd.Note,
                                 cd.Status,
                                 cd.Total
                             } into cxGroup
                             select cxGroup;

            var result = 
                         from cd in queryGroup
                         join u in DC.SysUser on cd.Key.SalemanId equals u.Id into grps
                         from grp in grps.DefaultIfEmpty()
                         join o in DC.SysOffice on cd.Key.OfficeId equals o.Id
                         join d in DC.CatDepartment on cd.Key.DepartmentId equals d.Id
                         join u2 in DC.SysUser on cd.Key.UserCreated equals u2.Id
                         join p in DC.CatPartner on cd.Key.PartnerId equals p.Id
                         select new AccPrePaidPaymentResult
                         {
                             Id = cd.Key.Id,
                             JobId = cd.Key.JobId,
                             Currency = cd.Key.CurrencyId,
                             DebitNote = cd.Key.Code,
                             SyncStatus = cd.Key.SyncStatus,
                             LastSyncDate = cd.Key.LastSyncDate,
                             Notes = cd.Key.Note,
                             HBL = cd.FirstOrDefault().Hblno,
                             MBL = cd.FirstOrDefault().Mblno,
                             JobNo = cd.FirstOrDefault().JobNo,
                             Status = cd.Key.Status,
                             TotalAmount = cd.Key.Total,
                             SalesmanName = grp.Username,
                             TotalAmountVND = cd.Sum(x => x.AmountVnd + x.VatAmountVnd),
                             TotalAmountUSD = cd.Sum(x => x.AmountUsd + x.VatAmountUsd),
                             PartnerName = p.ShortName,
                             DatetimeCreated = cd.Key.DatetimeCreated,
                             DepartmentName = d.DeptNameAbbr,
                             OfficeName = o.Code,
                             UserCreatedName = u2.Username,
                             TransactionType = cd.FirstOrDefault().TransactionType
                         };

            return result;

        }

        public async Task<HandleState> UpdatePrePaidPayment(List<AccountingPrePaidPaymentUpdateModel> model)
        {
            HandleState result = new HandleState();
            if (model.Count > 0)
            {
                foreach (var item in model)
                {
                    var cd = DataContext.Get(x => x.Id == item.Id)?.FirstOrDefault();
                    if (cd == null || cd.Status == "New")
                    {
                        continue;
                    }
                    cd.Status = item.Status;
                    cd.DatetimeModified = DateTime.Now;
                    cd.UserModified = currentUser.UserID;

                    var d = await DataContext.UpdateAsync(cd, x => x.Id == cd.Id, false);
                }
                result = DataContext.SubmitChanges();
            }
            return result;
        }

        public bool ValidateRevertPayment(Guid Id)
        {
            var isValid = true;
            var debit = DataContext.First(x => x.Id == Id);
            if (debit == null) return false;
            var surcharges = DC.CsShipmentSurcharge.Where(x => x.DebitNo == debit.Code);
            if(surcharges.Count() > 0)
            {
                isValid = !surcharges.Any(x => (x.SyncedFrom == AccountingConstants.STATUS_SYNCED_SOA || x.SyncedFrom == AccountingConstants.STATUS_SYNCED_CDNOTE));
            }

            return isValid;
        }
    }
}
