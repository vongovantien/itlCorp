using AutoMapper;
using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
using eFMS.API.Accounting.DL.Models.Criteria;
using eFMS.API.Accounting.DL.Models.Receipt;
using eFMS.API.Accounting.Service.Models;
using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Accounting.DL.Services
{
    public class AcctReceiptService : RepositoryBase<AcctReceipt, AcctReceiptModel>, IAcctReceiptService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<AccAccountingManagement> acctMngtRepository;
        private readonly IContextBase<CatPartner> catPartnerRepository;
        private readonly IContextBase<CatContract> catContractRepository;

        public AcctReceiptService(
            IContextBase<AcctReceipt> repository,
            IMapper mapper,
            ICurrentUser curUser,
            IStringLocalizer<AccountingLanguageSub> localizer,
            IContextBase<AccAccountingManagement> acctMngtRepo,
            IContextBase<CatPartner> catPartnerRepo,
            IContextBase<CatContract> catContractRepo
            ) : base(repository, mapper)
        {
            acctMngtRepository = acctMngtRepo;
            catPartnerRepository = catPartnerRepo;
            catContractRepository = catContractRepo;
        }

        public bool CheckAllowPermissionAction(Guid id, PermissionRange range)
        {
            throw new NotImplementedException();
        }

        public HandleState Delete(Guid id)
        {
            throw new NotImplementedException();
        }



        public IQueryable<AcctReceiptModel> Paging(AcctReceiptCriteria criteria, int page, int size, out int rowsCount)
        {
            throw new NotImplementedException();
        }

        public IQueryable<AcctReceiptModel> Query(AcctReceiptCriteria criteria)
        {
            throw new NotImplementedException();
        }

        public IQueryable<AcctReceiptModel> QueryByPermission(IQueryable<AcctReceiptModel> data, PermissionRange range, ICurrentUser currentUser)
        {
            throw new NotImplementedException();
        }
        public string GenerateReceiptNo()
        {
            string ReceiptNo = "PT" + DateTime.Now.ToString("yyMM");

            IQueryable<string> codes = DataContext.Where(x => x.PaymentRefNo.Contains(ReceiptNo)).Select(x => x.PaymentRefNo);

            List<int> oders = new List<int>();

            if (codes != null & codes.Count() > 0)
            {
                foreach (string code in codes)
                {
                    if (code.Length > 9 && int.TryParse(code.Substring(code.Length - 4), out int _))
                    {
                        oders.Add(int.Parse(code.Substring(code.Length - 4)));
                    }
                }

                if (oders.Count() > 0)
                {
                    int maxCurrentOder = oders.Max();

                    ReceiptNo += (maxCurrentOder + 1).ToString("0000");
                }
                else
                {
                    ReceiptNo += "0001";
                }
            }
            else
            {
                ReceiptNo += "0001";
            }

            return ReceiptNo;
        }

        public List<ReceiptInvoiceModel> GetInvoiceForReceipt(ReceiptInvoiceCriteria criteria)
        {
            List<ReceiptInvoiceModel> results = new List<ReceiptInvoiceModel>();
            string agreementService = String.Empty;

            if (criteria.AgreementID != Guid.Empty)
            {
                CatContract agreement = catContractRepository.Get(x => x.Id == criteria.AgreementID).FirstOrDefault();
                if (agreement != null)
                {
                    agreementService = agreement.SaleService;
                }
            }

            Expression<Func<AccAccountingManagement, bool>> queryInvoice = null;
            queryInvoice = x => ((x.Type == AccountingConstants.ACCOUNTING_INVOICE_TYPE)
             && (x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_UNPAID || x.PaymentStatus == AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART)
             && ((x.ServiceType ?? "").Contains(agreementService ?? "", StringComparison.OrdinalIgnoreCase)));

            IQueryable<AccAccountingManagement> invoices = acctMngtRepository.Get(queryInvoice); // Get danh sách hóa đơn

            if (invoices != null)
            {
                IQueryable<CatPartner> partners = catPartnerRepository.Get();

                var queryReceiptInvoice = from invoice in invoices
                                          join partner in partners on invoice.PartnerId equals partner.Id into grpPartners
                                          from grpPartner in grpPartners.DefaultIfEmpty()
                                          select new { invoice, grpPartner };

                if (queryReceiptInvoice != null)
                {
                    results = queryReceiptInvoice.Select(x => new ReceiptInvoiceModel
                    {
                        InvoiceNo = x.invoice.InvoiceNoReal,
                        Currency = x.invoice.Currency,
                        SerieNo = x.invoice.Serie,
                        InvoiceDate = x.invoice.Date,
                        UnpaidAmount = x.invoice.UnpaidAmount ?? 0,
                        Type = "Debit",
                        PaymentStatus = x.invoice.PaymentStatus,
                        PartnerName = x.grpPartner.ShortName,
                        TaxCode = x.grpPartner.TaxCode,
                        BillingDate = null // Cập nhật sau
                    }).ToList();
                }
            }

            return results;
        }
    }
}
