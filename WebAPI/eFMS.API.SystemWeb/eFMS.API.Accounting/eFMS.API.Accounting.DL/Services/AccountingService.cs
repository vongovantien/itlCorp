using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Accounting;
using eFMS.API.Accounting.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Collections.Generic;
using eFMS.IdentityServer.DL.UserManager;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccountingService : RepositoryBase<AccAccountingManagement, AccAccountingManagementModel>, IAccountingService
    {
        private readonly ICurrentUser currentUser;
        private readonly ICurrencyExchangeService currencyExchangeService;
        private readonly IContextBase<AcctAdvancePayment> AdvanceRepository;
        private readonly IContextBase<AcctAdvanceRequest> AdvanceRequestRepository;
        private readonly IStringLocalizer stringLocalizer;
        private readonly IContextBase<SysUser> UserRepository;
        private readonly IContextBase<SysEmployee> EmployeeRepository;
        private readonly IContextBase<SysOffice> SysOfficeRepository;
        readonly IContextBase<CatCurrencyExchange> catCurrencyExchangeRepo;




        public AccountingService(
            ICurrencyExchangeService exchangeService,
            IContextBase<SysUser> UserRepo,
            IContextBase<SysOffice> SysOfficeRepo,
            IContextBase<SysEmployee> EmployeeRepo,
            IContextBase<AcctAdvancePayment> AdvanceRepo,
            IContextBase<AccAccountingManagement> repository,
            IContextBase<AcctAdvanceRequest> AdvanceRequestRepo,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<CatCurrencyExchange> catCurrencyExchange,
            ICurrentUser cUser,
            IMapper mapper) : base(repository, mapper)
        {
            AdvanceRepository = AdvanceRepo;
            stringLocalizer = localizer;
            AdvanceRequestRepository = AdvanceRequestRepo;
            UserRepository = UserRepo;
            SysOfficeRepository = SysOfficeRepo;
            EmployeeRepository = EmployeeRepo;
            currencyExchangeService = exchangeService;
            catCurrencyExchangeRepo = catCurrencyExchange;
        }

        public List<BravoAdvanceModel> GetListAdvanceToSyncBravo(List<Guid> Ids)
        {
            List<BravoAdvanceModel> result = new List<BravoAdvanceModel>();

            if (Ids.Count > 0)
            {
                IQueryable<SysUser> users = UserRepository.Get();
                IQueryable<SysEmployee> employees = EmployeeRepository.Get();
                IQueryable<SysOffice> offices = SysOfficeRepository.Get();

                var adv = AdvanceRepository.Get(x => Ids.Contains(x.Id));
                //Quy đổi tỉ giá theo ngày Request Date


                IQueryable<BravoAdvanceModel> queryAdv = from ad in adv
                                                         join u in users on ad.Requester equals u.Id
                                                         join employee in employees on u.EmployeeId equals employee.Id
                                                         join office in offices on ad.OfficeId equals office.Id
                                                         select new BravoAdvanceModel
                                                         {
                                                             Stt = ad.Id,
                                                             ReferenceNo = ad.AdvanceNo,
                                                             CurrencyCode = ad.AdvanceCurrency,
                                                             Description0 = ad.AdvanceNote,
                                                             CustomerName = employee.EmployeeNameVn,
                                                             CustomerCode = employee.StaffCode,
                                                             OfficeCode = office.Code,
                                                             DocDate = ad.RequestDate,
                                                             ExchangeRate = GetExchangeRate(ad.RequestDate, ad.AdvanceCurrency)
                                                         };
                List<BravoAdvanceModel> data = queryAdv.ToList();
                foreach (var item in data)
                {
                    var advR = AdvanceRequestRepository.Get(x => x.AdvanceNo == item.ReferenceNo).Select(x => new BravoAdvanceRequestModel
                    {
                        RowId = x.Id,
                        Ma_SpHt = x.JobId,
                        BillEntryNo = x.Hbl,
                        MasterBillNo = x.Mbl,
                        OriginalAmount = x.Amount,
                        Description = x.RequestNote,
                        DeptCode = GetDeptCode(x.JobId),
                    }).ToList();

                    if (advR.Count > 0)
                    {
                        item.Details = advR;
                    }
                }

                result = data;
            }

            return result;
        }

        private decimal GetExchangeRate(DateTime? date, string currency)
        {
            decimal exchangeRate = 0;
            // List tỷ giá theo ngày
            List<CatCurrencyExchange> currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == date).ToList();

            if (currencyExchange.Count == 0)
            {
                // Lấy ngày mới nhất
                DateTime? maxDateCreated = catCurrencyExchangeRepo.Get().Max(s => s.DatetimeCreated);
                currencyExchange = catCurrencyExchangeRepo.Get(x => x.DatetimeCreated.Value.Date == maxDateCreated.Value.Date).ToList();
            }

            exchangeRate = currencyExchangeService.GetRateCurrencyExchange(currencyExchange, currency, AccountingConstants.CURRENCY_LOCAL);

            return exchangeRate;
        }

        private string GetDeptCode(string JobNo)
        {
            string deptCode = "ITLCS";

            if (JobNo.Contains("LOG"))
            {
                //_deptCode = "OPS";
                deptCode = "ITLOPS";
            }
            else if (JobNo.Contains("A"))
            {
                //_deptCode = "AIR";
                deptCode = "ITLAIR";
            }
            else if (JobNo.Contains("S"))
            {
                //_deptCode = "SEA";
                deptCode = "ITLCS";
            }

            return deptCode;
        }
    }
}
