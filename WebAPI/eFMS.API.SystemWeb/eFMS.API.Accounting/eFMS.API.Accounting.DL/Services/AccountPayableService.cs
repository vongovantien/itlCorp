using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models.AccountPayable;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.AccountReceivable;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.ViewModel;
using eFMS.API.Accounting.Service.Contexts;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Accounting.Service.ViewModels;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace eFMS.API.Accounting.DL.Services
{
    public class AccountPayableService : RepositoryBase<AccAccountPayable, AccAccountPayableModel>, IAccountPayableService
    {
        private readonly IContextBase<AcctSoa> soaRepo;
        private readonly IContextBase<AcctAdvancePayment> advancePaymentRepo;
        private readonly IContextBase<AcctSettlementPayment> settlementPaymentRepo;
        private readonly IContextBase<AccAccountingManagement> accountingManagementRepo;
        private readonly IContextBase<AcctCdnote> cdNoteRepo;
        private readonly IContextBase<AccAccountPayablePayment> accAccountPayablePaymentRepo;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IContextBase<SysOffice> sysOfficeRepo;
        private readonly IContextBase<CatPartner> catPartnerRepo;

        public AccountPayableService(IContextBase<AccAccountPayable> repository,
            IContextBase<AcctSoa> soa,
            IContextBase<AcctAdvancePayment> advancePayment,
            IContextBase<AcctSettlementPayment> settlementPayment,
            IContextBase<AccAccountingManagement> accountingManagement,
            IContextBase<AcctCdnote> acctCdNote,
            IContextBase<AccAccountPayablePayment> accAccountPayablePayment,
            IContextBase<CsShipmentSurcharge> surcharge,
            IContextBase<SysOffice> sysOffice,
            IContextBase<CatPartner> catPartner,
            IMapper mapper) : base(repository, mapper)
        {
            soaRepo = soa;
            advancePaymentRepo = advancePayment;
            settlementPaymentRepo = settlementPayment;
            accountingManagementRepo = accountingManagement;
            cdNoteRepo = acctCdNote;
            accAccountPayablePaymentRepo = accAccountPayablePayment;
            sysOfficeRepo = sysOffice;
            catPartnerRepo = catPartner;
        }

        public Task<HandleState> DeleteAccountPayable()
        {
            throw new NotImplementedException();
        }

        public Task<HandleState> UpdateAccountPayable()
        {
            throw new NotImplementedException();
        }
    }
}
