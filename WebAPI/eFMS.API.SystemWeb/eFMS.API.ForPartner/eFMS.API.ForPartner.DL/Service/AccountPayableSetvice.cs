using AutoMapper;
using eFMS.API.ForPartner.DL.Common;
using eFMS.API.ForPartner.DL.IService;
using eFMS.API.ForPartner.DL.Models;
using eFMS.API.ForPartner.DL.Models.Payable;
using eFMS.API.ForPartner.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.ForPartner.DL.Service
{
  
    public class AccountPayableSetvice : RepositoryBase<AccAccountPayable, AccountPayableModel>, IAccountPayableService
    {
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<SysOffice> officeRepository;
        private readonly IContextBase<AccAccountPayablePayment> paymentRepository;
        public AccountPayableSetvice(IContextBase<AccAccountPayable> repository,
            IContextBase<CatPartner> partnerRepo,
            ICurrentUser cUser,
            IContextBase<SysOffice> officeRepo,
             IContextBase<AccAccountPayablePayment> paymentRepo,
            IMapper mapper) : base(repository, mapper)
        {
            currentUser = cUser;
            partnerRepository = partnerRepo;
            officeRepository = officeRepo;
            paymentRepository = paymentRepo;
        }

        public async Task<HandleState> InsertAccPayable(VoucherSyncCreateModel model)
        {
            HandleState hsAddPayable = new HandleState();
            List<AccAccountPayable> payables = new List<AccAccountPayable>();
            using (var trans = DataContext.DC.Database.BeginTransaction())
            {
                try
                {
                    var grpVoucherDetail = model.Details
              .GroupBy(x => new { x.VoucherNo, x.TransactionType, model.DocType, model.DocCode, x.BravoRefNo })
              .Select(s => s).ToList();
                    CatPartner customer = partnerRepository.Get(x => x.AccountNo == model.CustomerCode)?.FirstOrDefault();
                    SysOffice office = officeRepository.Get(x => x.Code == model.OfficeCode)?.FirstOrDefault();

                    grpVoucherDetail.ForEach(c =>
                    {
                        if (c.Key.TransactionType != "NONE")
                        {
                            AccAccountPayable payable = new AccAccountPayable
                            {
                                Currency = c.FirstOrDefault().Currency,
                                PartnerId = customer?.Id,
                                PaymentAmount = 0,
                                PaymentAmountVnd = 0,
                                PaymentAmountUsd = 0,
                                RemainAmount = 0,
                                RemainAmountVnd = 0,
                                RemainAmountUsd = 0,
                                TotalAmountVnd = c.Sum(pa => pa.VatAmountVnd + pa.AmountVnd),
                                TotalAmountUsd = c.Sum(pa => pa.VatAmountUsd + pa.AmountUsd),
                                ReferenceNo = c.FirstOrDefault().BravoRefNo,
                                Status = ForPartnerConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID,
                                CompanyId = currentUser.CompanyID,
                                OfficeId = office.Id,
                                GroupId = currentUser.GroupId,
                                DepartmentId = currentUser.DepartmentId,
                                TransactionType = c.FirstOrDefault().TransactionType,
                                VoucherNo = c.FirstOrDefault().VoucherNo,
                                BillingNo = model.DocCode,
                                BillingType = model.DocType,
                                ExchangeRate = c.FirstOrDefault().ExchangeRate,
                                UserCreated = currentUser.UserID,
                                DatetimeCreated = DateTime.Now,
                                UserModified = currentUser.UserID,
                                DatetimeModified = DateTime.Now
                            };

                            payables.Add(payable);
                        }
                    });

                    foreach (var item in payables)
                    {
                        item.PaymentAmount = item.Currency == ForPartnerConstants.CURRENCY_LOCAL ? item.TotalAmountVnd : item.TotalAmountUsd;
                        item.RemainAmount = item.TotalAmountVnd; ;
                        item.RemainAmountVnd = item.TotalAmountVnd;
                        item.RemainAmountUsd = item.TotalAmountUsd;

                        await DataContext.AddAsync(item, false);
                    }
                    hsAddPayable = DataContext.SubmitChanges();
                    if (hsAddPayable.Success)
                    {
                        trans.Commit();
                    }
                    return hsAddPayable;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    return new HandleState((object)ex.Message);
                }
                finally
                {
                    trans.Dispose();
                }
            }
        }
        
        public bool IsPayableHasPayment(VoucherSyncDeleteModel model)
        {

            bool IshasPayment = false;

            List<AccAccountPayable> payable = DataContext.Get(x => x.VoucherNo == model.VoucherNo 
            && x.BillingNo == model.DocCode
            && x.BillingType == model.DocType).ToList();
            if(payable.Count > 0)
            {
                List<string> refNos = payable.Select(x => x.ReferenceNo).Distinct().ToList();
                IshasPayment = paymentRepository.Any(x => refNos.Contains(x.ReferenceNo));

            }

            return IshasPayment;
        }
    }
}