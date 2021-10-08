
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctDebitManagementArService : RepositoryBase<AcctDebitManagementAr, AcctDebitManagementArModel>, IAcctDebitManagementARService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IAccountingManagementService accMngtService;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepository;
        private readonly IContextBase<AccAccountingManagement> accMngtRepository;
        public AcctDebitManagementArService(
            IContextBase<AcctDebitManagementAr> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IAccountingManagementService accMngt,
            IContextBase<CsShipmentSurcharge> surchargeRepo,
            IContextBase<AccAccountingManagement> accMngtRepo
            ) : base(repository, mapper)
        {
            currentUser = curUser;
            stringLocalizer = localizer;
            accMngtService = accMngt;
            surchargeRepository = surchargeRepo;
            accMngtRepository = accMngtRepo;
        }

        public async Task<HandleState> AddAndUpdate(Guid Id)
        {
            HandleState result = new HandleState();
            AccAccountingManagement invoice = accMngtService.Get(x => x.Id == Id)?.FirstOrDefault();
            if (invoice == null)
            {
                return result;
            }

            var oldDebits = DataContext.Get(x => x.AcctManagementId == Id);
            if (oldDebits.Count() > 0)
            {
                foreach (var item in oldDebits)
                {
                    var hs = DataContext.DeleteAsync(x => x.Id == item.Id, false);
                }
            }
            IQueryable<AcctDebitManagementAr> debits = surchargeRepository.Get(x => x.Type == AccountingConstants.TYPE_CHARGE_SELL && x.AcctManagementId == Id)
                .GroupBy(x => new { x.Hblid })
                .Select(x => new AcctDebitManagementAr
                {
                    Id = Guid.NewGuid(),
                    Hblid = x.Key.Hblid,
                    RefNo = !string.IsNullOrEmpty(x.FirstOrDefault().Soano) ? x.FirstOrDefault().Soano : x.FirstOrDefault().DebitNo,
                    AcctManagementId = Id,
                    CompanyId = currentUser.CompanyID,
                    PartnerId = x.FirstOrDefault().PaymentObjectId,
                    PaidAmount = 0,
                    PaidAmountUsd = 0,
                    PaidAmountVnd = 0,
                    PaymentStatus = AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID,
                    UnpaidAmount = invoice.Currency == AccountingConstants.CURRENCY_LOCAL ? x.Sum(s => s.AmountVnd + s.VatAmountVnd) : x.Sum(s => s.AmountUsd + s.VatAmountUsd),
                    UnpaidAmountVnd = x.Sum(s => s.AmountVnd + s.VatAmountVnd),
                    UnpaidAmountUsd = x.Sum(s => s.AmountUsd + s.VatAmountUsd),
                    TotalAmount = invoice.Currency == AccountingConstants.CURRENCY_LOCAL ? x.Sum(s => s.AmountVnd + s.VatAmountVnd) : x.Sum(s => s.AmountUsd + s.VatAmountUsd),
                    TotalAmountVnd = x.Sum(s => s.AmountVnd + s.VatAmountVnd),
                    TotalAmountUsd = x.Sum(s => s.AmountUsd + s.VatAmountUsd),
                    UserCreated = currentUser.UserID,
                    UserModified = currentUser.UserID,
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    DepartmentId = currentUser.DepartmentId,
                    OfficeId = x.FirstOrDefault().OfficeId.ToString(),
                });
            if (debits != null && debits.Count() > 0)
            {
                foreach (var item in debits)
                {
                    var hs = DataContext.AddAsync(item, false);
                }
            }

            result = DataContext.SubmitChanges();
            return result;
        }

        public async void DeleteDebit(Guid Id)
        {
            var debits = DataContext.Get(x => x.AcctManagementId == Id);

            if(debits.Count() > 0)
            {
                foreach (var item in debits)
                {
                    await DataContext.DeleteAsync(x => x.Id == item.Id, false);
                }

                DataContext.SubmitChanges();
            }
        }
    }
}
