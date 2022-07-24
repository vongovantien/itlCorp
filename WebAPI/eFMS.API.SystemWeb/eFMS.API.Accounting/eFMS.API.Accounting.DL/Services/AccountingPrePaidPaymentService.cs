using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingPrePaidPaymentService : IAccountingPrePaidPaymentService
    {
        private readonly ICurrentUser currentUser;
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)cdNoteRepository.DC;
        private readonly IContextBase<AcctCdnote> cdNoteRepository;

        public AccountingPrePaidPaymentService(ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
        }

        public IQueryable<AccPrePaidPaymentResult> Paging(AccountingPrePaidPaymentCriteria criteria, int page, int size, out int rowsCount)
        {
            IQueryable<AccPrePaidPaymentResult> data = GetQuery(criteria);
            rowsCount = data.CountAsync().Result;

            var resultPaging = data.Skip((page - 1) * size).Take(size);

            return resultPaging;
        }

        private IQueryable<AccPrePaidPaymentResult> GetQuery(AccountingPrePaidPaymentCriteria criteria)
        {
            Expression<Func<AcctCdnote, bool>> query = x => x.CurrencyId == criteria.Currency && x.Status == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID;
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
            if (criteria.Keywords.Count > 0)
            {
                switch (criteria.SearchType)
                {
                    case "JobID":
                        var jobs = DC.CsTransaction.Where(x => criteria.Keywords.Contains(x.JobNo)).Select(x => x.Id);
                        if (jobs.Count() > 0)
                        {
                            query = query.And(x => jobs.Contains(x.JobId));
                        }
                        break;
                    case "DebitNote":
                        query = query.And(x => criteria.Keywords.Contains(x.Code));
                        break;
                    default:
                        break;
                }
            }
            var cdNotes = DC.AcctCdnote.Select(query);
            if(cdNotes.Count() > 0)
            {
                var d = cdNotes.Select(x => new AccPrePaidPaymentResult {
                    Id = x.,
                    Currency = x.cur

                });
            }
            return null;
        }
    }
}
