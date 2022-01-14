using AutoMapper;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Linq;

namespace eFMS.API.ForPartner.DL.Service
{
    public class AccAccountingPaymentService : RepositoryBase<AccAccountingPayment, AccAccountingPaymentModel>, IAccountingPaymentService
    {
        private readonly IContextBase<AcctReceipt> acctReceiptRepository;
        public AccAccountingPaymentService(IContextBase<AccAccountingPayment> repository, IMapper mapper, IContextBase<AcctReceipt> acctReceiptRepo) : base(repository, mapper)
        {
            acctReceiptRepository = acctReceiptRepo;
        }

        public bool CheckInvoicePayment(Guid Id)
        {
            bool result = false;
            var receipts = acctReceiptRepository.Get(x => x.Status != ForPartnerConstants.STATUS_CANCEL_RECEIPT);
            var query = from payment in DataContext.Get()
                        join receipt in receipts on payment.ReceiptId equals receipt.Id
                        where payment.RefId == Id.ToString();
            if (query.Any())
            {
                result = true;
            }
            return result;
        }
    }
}
