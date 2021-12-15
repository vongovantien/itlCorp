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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountPayables"></param>
        /// <returns></returns>
        public HandleState CheckIsValidPayable(List<AccAccountPayableModel> accountPayables)
        {
            var officeCodes = accountPayables.Select(x => x.OfficeCode).ToList();
            foreach (var office in officeCodes)
            {
                var invalidOffice = sysOfficeRepo.Get(x => x.Code == office).FirstOrDefault();
                if (invalidOffice == null)
                {
                    return new HandleState("Office " + (object)office + " không tồn tại");
                }
            }
            var customerCodes = accountPayables.Select(x => x.CustomerCode).ToList();
            foreach (var customer in customerCodes)
            {
                var invalidCustomer = catPartnerRepo.Get(x => x.AccountNo == customer).FirstOrDefault();
                if (invalidCustomer == null)
                {
                    return new HandleState("Customer " + (object)customer + " không tồn tại");
                }
            }
            foreach (var acc in accountPayables)
            {
                
                foreach (var detail in acc.PaymentDetail)
                {
                    
                        if (detail.TransactionType != AccountingConstants.TRANSACTION_TYPE_PAYABLE_CREDIT && detail.TransactionType != AccountingConstants.TRANSACTION_TYPE_PAYABLE_OBH && detail.TransactionType != AccountingConstants.TRANSACTION_TYPE_PAYABLE_ADV && detail.TransactionType != AccountingConstants.TRANSACTION_TYPE_PAYABLE_ADV)
                    {
                        return new HandleState("Loại giao dịch " + (object)detail.TransactionType + " không hợp lệ");
                    }
                    if (detail.TransactionType != "COMBINE")
                    {
                        if (!DataContext.Any(x => x.ReferenceNo == detail.BravoRefNo && x.TransactionType == detail.TransactionType))
                        {
                            return new HandleState("Số ref " + (object)detail.BravoRefNo + " không tồn tại");
                        }
                    }
                }
            }
            return new HandleState();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accountPayables"></param>
        /// <returns></returns>
        public HandleState InsertAccountPayable(List<AccAccountPayableModel> accountPayables)
        {
            HandleState hs = new HandleState();
            foreach (var acc in accountPayables)
            {
                var accPayablePayment = new AccAccountPayablePayment();
                var partner = catPartnerRepo.Get(x => x.AccountNo == acc.CustomerCode).FirstOrDefault();
                foreach (var detail in acc.PaymentDetail)
                {
                    accPayablePayment.Id = Guid.NewGuid();
                    accPayablePayment.PartnerId = partner.Id;
                    if (detail.TransactionType == AccountingConstants.TRANSACTION_TYPE_PAYABLE_CREDIT || detail.TransactionType == AccountingConstants.TRANSACTION_TYPE_PAYABLE_OBH)
                    {

                    }
                    else if (detail.TransactionType == AccountingConstants.TRANSACTION_TYPE_PAYABLE_ADV)
                    {

                    }
                    else if (detail.TransactionType == AccountingConstants.TRANSACTION_TYPE_PAYABLE_ADV)
                    {

                    }
                }
                
            }
            return hs;
        }

        public HandleState CancelAccountPayable(List<AccAccountPayable> accountPayables)
        {
            HandleState result = new HandleState();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var ids = new List<Guid>();
                    foreach(var acc in accountPayables)
                    {
                        var accPayableExist = DataContext.Get(x => x.VoucherId == acc.VoucherId && x.ReferenceNo == acc.ReferenceNo);
                        if (accPayableExist.Count() > 0)
                        {
                            return new HandleState((object)string.Format("Phiếu Hoạch toán đã có thanh toán, vui lòng hủy thanh toán toán trước khi hủy voucher"));
                        }
                        ids.AddRange(accPayableExist.Select(x => x.Id));
                    }
                    result = DataContext.Delete(x => ids.Contains(x.Id));
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    result = new HandleState(ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
                return result;
            }
        }

        private sp_InsertAccountingPayable InsertOrUpdateReceivableList(List<AccAccountPayable> accountPayables, List<AccAccountPayablePayment> accountPayablePayments)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@AccoutPayable",
                    Value = DataHelper.ToDataTable(accountPayables),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[AccountPayableTable]"
                },
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@AccoutPayablePM",
                    Value = DataHelper.ToDataTable(accountPayablePayments),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[AccountPayablePaymentTable]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_InsertAccountingPayable>(parameters);
            return result.FirstOrDefault();
        }

        public Task<HandleState> UpdateAccountPayable()
        {
            throw new NotImplementedException();
        }
    }
}
