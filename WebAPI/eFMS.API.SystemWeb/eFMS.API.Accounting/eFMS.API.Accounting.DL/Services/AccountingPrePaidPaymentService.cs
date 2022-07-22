using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
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

            return null;
        }
    }
}
