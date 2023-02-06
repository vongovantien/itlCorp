using eFMS.API.Accounting.DL.Common;
using eFMS.API.Accounting.DL.IService;
using eFMS.API.Accounting.DL.Models;
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
using AutoMapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Accounting.DL.Services
{
    public class AccAccountReceivableHostedService : RepositoryBase<AccAccountReceivable, AccAccountReceivableModel>, IAccAccountReceivableHostedService
    {
        private readonly IContextBase<CatPartner> partnerRepo;
        private readonly IContextBase<CatContract> contractPartnerRepo;
        private readonly IContextBase<SysUser> userRepo;
        private readonly IContextBase<CsTransaction> transactionRepo;
        private readonly IContextBase<CsTransactionDetail> transactionDetailRepo;
        private readonly IContextBase<OpsTransaction> opsRepo;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly ICurrencyExchangeService currencyExchangeService;
        public AccAccountReceivableHostedService(IContextBase<AccAccountReceivable> repository, 
            IMapper mapper,
            ICurrencyExchangeService currencyExchange,
            IContextBase<CatPartner> partner,
            IContextBase<CatContract> contract,
            IContextBase<SysUser> user,
            IContextBase<CsTransaction> transaction,
            IContextBase<CsTransactionDetail> transactionDetail,
            IContextBase<OpsTransaction> ops,
            IContextBase<CsShipmentSurcharge> surcharge
            ) : base(repository, mapper)
        {
            partnerRepo = partner;
            contractPartnerRepo = contract;
            userRepo = user;
            transactionRepo = transaction;
            transactionDetailRepo = transactionDetail;
            opsRepo = ops;
            surchargeRepo = surcharge;
            currencyExchangeService = currencyExchange;
        }

        public async Task<HandleState> CalculatorReceivableDebitAmountAsync(List<ObjectReceivableModel> models)
        {
            var receivables = new List<AccAccountReceivableModel>();
            HandleState hs = new HandleState();

            try
            {
                receivables = CalculatorReceivableDataDebitAmount(models);

                var receivablesModel = mapper.Map<List<ReceivableTable>>(receivables);
                var hsInsertOrUpdate = InsertOrUpdateReceivableList(receivablesModel);
                if (!hsInsertOrUpdate.Status)
                {
                    hs = new HandleState((object)hsInsertOrUpdate.Message);
                }
                WriteLogInsertOrUpdateReceivable(hsInsertOrUpdate.Status, hsInsertOrUpdate.Message, receivables, models);

                var partnerIds = receivables.Select(s => s.PartnerId).Distinct().ToList();
                var receivableRelatives = DataContext.Get(x => partnerIds.Contains(x.PartnerId)).ToList();
                var agreementIds = receivableRelatives.Where(x => x.ContractId != null).Select(s => s.ContractId).Distinct().ToList();

                await UpdateAgreementPartnersAsync(partnerIds, agreementIds);

                return hs;
            }
            catch (Exception ex)
            {
                WriteLogInsertOrUpdateReceivable(false, ex.Message, receivables, models);
                return new HandleState((object)ex.Message);
            }
        }

        private List<AccAccountReceivableModel> CalculatorReceivableDataDebitAmount(List<ObjectReceivableModel> models)
        {
            var receivables = new List<AccAccountReceivableModel>();
            var surchargesDb = surchargeRepo.Get(x => models.Any(a => a.Office == x.OfficeId && a.Service == x.TransactionType && a.PartnerId == x.PaymentObjectId));

            foreach (var fe in models)
            {
                var partner = partnerRepo.Get(x => x.Id == fe.PartnerId).FirstOrDefault();
                if (partner == null || partner.PartnerMode == "Internal")
                {
                    continue;
                }
                var surcharges = surchargesDb.Where(x => (x.Type == AccountingConstants.TYPE_CHARGE_SELL)
                                                 && string.IsNullOrEmpty(x.InvoiceNo)
                                                 && x.AcctManagementId == null);
                var surchargesOBH = surchargesDb.Where(x => x.Type == AccountingConstants.TYPE_CHARGE_OBH && x.AcctManagementId == null);

                IQueryable<OpsTransaction> opsJob = opsRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                               && x.ServiceDate.Value.Date <= DateTime.Now.Date);
                IQueryable<CsTransaction> csJob = transactionRepo.Get(x => x.CurrentStatus != AccountingConstants.CURRENT_STATUS_CANCELED
                                               && x.ServiceDate.Value.Date <= DateTime.Now.Date);

                var currentReceivables = DataContext.Get(x => x.PartnerId == fe.PartnerId && x.Office == fe.Office && x.Service == fe.Service).ToList();
                foreach (var item in currentReceivables)
                {
                    item.DebitAmount = 0;
                    item.SellingNoVat = 0;
                    item.ObhAmount = 0;
                    item.BillingUnpaid = 0;
                    item.ObhUnpaid = 0;
                    item.BillingAmount = 0;
                    item.ObhBilling = 0;
                    item.PaidAmount = 0;
                    item.ObhPaid = 0;
                    if (item.ContractId != null)
                    {
                        var contractC = contractPartnerRepo.First(x => x.PartnerId == item.PartnerId && x.Id == item.ContractId && x.SaleManId == item.SaleMan);
                        if (contractC == null || contractC.Active == false)
                        {
                            item.ContractId = null; // những khách cũ k đi hàng, hd đang inactive, cn vẫn tính có contract.
                            item.ContractCurrency = "VND";
                        }
                    }
                }
                receivables.AddRange(mapper.Map<List<AccAccountReceivableModel>>(currentReceivables));
                var receivableIdModified = new List<Guid>();

                #region SellingNoVat
                IQueryable<SalesmanSurcharge> _chargeWithSalemanSell = Enumerable.Empty<SalesmanSurcharge>().AsQueryable();
                if (fe.Service == "CL")
                {
                    _chargeWithSalemanSell = from ops in opsJob
                                             join sur in surcharges on ops.Hblid equals sur.Hblid into grpOps
                                             from surGrp in grpOps.DefaultIfEmpty()
                                             where surGrp.OfficeId == fe.Office
                                             && surGrp.PaymentObjectId == fe.PartnerId
                                             && surGrp.TransactionType == fe.Service
                                             select new SalesmanSurcharge
                                             {
                                                 Surcharge = surGrp,
                                                 Salesman = ops.SalemanId,
                                                 Office = surGrp.OfficeId,
                                                 PartnerId = surGrp.PaymentObjectId,
                                                 TransactionType = surGrp.TransactionType
                                             };
                }
                else
                {
                    _chargeWithSalemanSell = from cs in csJob
                                             join csd in transactionDetailRepo.Get() on cs.Id equals csd.JobId
                                             join sur in surcharges on csd.Id equals sur.Hblid into grpOps
                                             from surGrp in grpOps.DefaultIfEmpty()
                                             where surGrp.OfficeId == fe.Office
                                             && surGrp.PaymentObjectId == fe.PartnerId
                                             && surGrp.TransactionType == fe.Service
                                             select new SalesmanSurcharge
                                             {
                                                 Surcharge = surGrp,
                                                 Salesman = csd.SaleManId,
                                                 Office = surGrp.OfficeId,
                                                 PartnerId = surGrp.PaymentObjectId,
                                                 TransactionType = surGrp.TransactionType
                                             };
                }
                if (_chargeWithSalemanSell.Count() > 0)
                {
                    var _chargeGrpWithSalesman = _chargeWithSalemanSell
                   .GroupBy(x => new { x.PartnerId, x.Office, x.TransactionType, x.Salesman })
                   .Select(x => new { x.Key.Salesman, Surcharges = x.Select(i => i.Surcharge), x.Key.PartnerId, x.Key.Office, Service = x.Key.TransactionType });

                    foreach (var item in _chargeGrpWithSalesman)
                    {
                        var surchargeInGrp = item.Surcharges;
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                        && x.Office == item.Office && x.Service == item.Service && x.SaleMan == item.Salesman);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.Salesman
                             && x.Active == true
                             && x.OfficeId.Contains(item.Office.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalAmount = 0;
                            foreach (var charge in surchargeInGrp)
                            {
                                totalAmount += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, currentReceivable.ContractCurrency);
                            }
                            currentReceivable.SellingNoVat = totalAmount;
                            receivableIdModified.Add(currentReceivable.Id);

                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.Salesman
                             && x.Active == true
                             && x.OfficeId.Contains(item.Office.ToString())
                             && x.SaleService.Contains(item.Service));

                            if (contract != null)
                            {
                                decimal? sellingNoVat = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, contract.CreditCurrency);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.Salesman,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    SellingNoVat = sellingNoVat
                                });
                            }
                            else
                            {
                                decimal? sellingNoVat = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    sellingNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, AccountingConstants.CURRENCY_LOCAL);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.Salesman,
                                    ContractCurrency = "VND",
                                    SellingNoVat = sellingNoVat
                                });
                            }
                        }
                    }
                }
                #endregion SellingNoVat

                #region OBH Amount
                IQueryable<SalesmanSurcharge> _chargeWithSalemanOBH = Enumerable.Empty<SalesmanSurcharge>().AsQueryable();
                if (fe.Service == "CL")
                {
                    _chargeWithSalemanOBH = from ops in opsJob
                                            join sur in surchargesOBH on ops.Hblid equals sur.Hblid into grpOps
                                            from surGrp in grpOps.DefaultIfEmpty()
                                            where surGrp.OfficeId == fe.Office
                                            && surGrp.PaymentObjectId == fe.PartnerId
                                            && surGrp.TransactionType == fe.Service
                                            select new SalesmanSurcharge
                                            {
                                                Surcharge = surGrp,
                                                Salesman = ops.SalemanId,
                                                Office = surGrp.OfficeId,
                                                PartnerId = surGrp.PaymentObjectId,
                                                TransactionType = surGrp.TransactionType
                                            };
                }
                else
                {
                    _chargeWithSalemanOBH = from cs in csJob
                                            join csd in transactionDetailRepo.Get() on cs.Id equals csd.JobId
                                            join sur in surchargesOBH on csd.Id equals sur.Hblid into grpOps
                                            from surGrp in grpOps.DefaultIfEmpty()
                                            where surGrp.OfficeId == fe.Office
                                            && surGrp.PaymentObjectId == fe.PartnerId
                                            && surGrp.TransactionType == fe.Service
                                            select new SalesmanSurcharge
                                            {
                                                Surcharge = surGrp,
                                                Salesman = csd.SaleManId,
                                                Office = surGrp.OfficeId,
                                                PartnerId = surGrp.PaymentObjectId,
                                                TransactionType = surGrp.TransactionType
                                            };
                }
                if (_chargeWithSalemanOBH.Count() > 0)
                {
                    var _chargeGrpWithSalesmanOBH = _chargeWithSalemanOBH
                    .GroupBy(x => new { x.PartnerId, x.Office, x.TransactionType, x.Salesman })
                    .Select(x => new { x.Key.Salesman, Surcharges = x.Select(i => i.Surcharge), x.Key.PartnerId, x.Key.Office, Service = x.Key.TransactionType });

                    foreach (var item in _chargeGrpWithSalesmanOBH)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                            && x.Office == item.Office && x.Service == item.Service && x.SaleMan == item.Salesman);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.Salesman
                             && x.Active == true
                             && x.OfficeId.Contains(item.Office.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? obhNoVat = 0;
                            var surchargeInGrp = item.Surcharges;
                            foreach (var charge in surchargeInGrp)
                            {
                                obhNoVat += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, currentReceivable.ContractCurrency);
                            }
                            currentReceivable.ObhAmount = obhNoVat;
                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.Salesman
                            && x.Active == true
                            && x.OfficeId.Contains(item.Office.ToString())
                            && x.SaleService.Contains(item.Service));
                            var surchargeInGrp = item.Surcharges;

                            if (contract != null)
                            {
                                decimal? obhTotal = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    obhTotal += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, contract.CreditCurrency);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    ObhAmount = obhTotal,
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.Salesman,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                });
                            }
                            else
                            {
                                decimal? obhTotal = 0;
                                foreach (var charge in surchargeInGrp)
                                {
                                    obhTotal += currencyExchangeService.ConvertAmountChargeToAmountObj(charge, AccountingConstants.CURRENCY_LOCAL);
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    ObhAmount = obhTotal,
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.Salesman,
                                    ContractCurrency = "VND",
                                    SellingNoVat = 0
                                });
                            }
                        }
                    }
                }
                #endregion OBH Amount

                #region Billing Amount - Billing Unpaid
                var dataInvoicesWithSalesmanInvoice = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTING_INVOICE_TYPE, null);
                var dataGrpSalesmanInvoice = dataInvoicesWithSalesmanInvoice.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                    .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoice.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoice)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                            && x.Office == item.OfficeId && x.Service == fe.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));

                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalAmount = 0;
                            decimal? totalUnpaidAmount = 0;
                            decimal? totalUnpaidAmountPerService = 0;
                            decimal? totalAmountPerService = 0;
                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalAmount += invoice.TotalAmountUSD;
                                }

                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                }
                                totalUnpaidAmountPerService = (totalUnpaidAmount / qtyService);
                                totalAmountPerService = (totalAmount / qtyService);
                            }

                            currentReceivable.BillingAmount = totalAmountPerService;
                            currentReceivable.BillingUnpaid = totalUnpaidAmountPerService;

                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if (contract != null)
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalAmount += invoice.TotalAmountVND;
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalAmount += invoice.TotalAmountUSD;
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }
                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    BillingAmount = totalAmountPerService,
                                    BillingUnpaid = totalUnpaidAmountPerService
                                });
                            }
                            else
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    BillingAmount = totalAmountPerService,
                                    BillingUnpaid = totalUnpaidAmountPerService
                                });
                            }
                        }
                    }
                }

                #endregion

                #region OBH Billing - OBH Unpaid
                var dataInvoicesWithSalesmanInvoiceTemp = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE, null);
                var dataGrpSalesmanInvoiceTemp = dataInvoicesWithSalesmanInvoiceTemp.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                  .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoiceTemp.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoiceTemp)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                           && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }

                            decimal? totalAmount = 0;
                            decimal? totalUnpaidAmount = 0;
                            decimal? totalUnpaidAmountPerService = 0;
                            decimal? totalAmountPerService = 0;

                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalAmount += invoice.TotalAmountUSD;
                                }

                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                }
                                totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                totalAmountPerService = totalAmount / qtyService;
                            }
                            currentReceivable.ObhBilling = totalAmountPerService;
                            currentReceivable.ObhUnpaid = totalUnpaidAmountPerService;
                            currentReceivable.ObhAmount += currentReceivable.ObhUnpaid;

                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if (contract != null)
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalAmount += invoice.TotalAmountVND;
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalAmount += invoice.TotalAmountUSD;
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalUnpaidAmount += invoice.UnpaidAmountUSD;
                                    }
                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    ObhBilling = totalAmountPerService,
                                    ObhUnpaid = totalUnpaidAmountPerService,
                                    ObhAmount = totalUnpaidAmountPerService
                                });
                            }
                            else
                            {
                                decimal? totalAmount = 0;
                                decimal? totalUnpaidAmount = 0;
                                decimal? totalUnpaidAmountPerService = 0;
                                decimal? totalAmountPerService = 0;

                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalAmount += invoice.TotalAmountVND;
                                    totalUnpaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalUnpaidAmountPerService = totalUnpaidAmount / qtyService;
                                    totalAmountPerService = totalAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    ObhBilling = totalAmountPerService,
                                    ObhUnpaid = totalUnpaidAmountPerService,
                                    ObhAmount = totalUnpaidAmountPerService
                                });
                            }
                        }
                    }
                }

                #endregion

                #region OBH Paid
                var dataInvoicesWithSalesmanInvoiceTempPaidApart = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTING_INVOICE_TEMP_TYPE, AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                var dataGrpSalesmanInvoiceTempPaidApart = dataInvoicesWithSalesmanInvoiceTempPaidApart.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                  .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoiceTempPaidApart.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoiceTempPaidApart)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                           && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalPaidAmount = 0;
                            decimal? totalPaidAmountPerService = 0;
                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalPaidAmount += invoice.PaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalPaidAmount += invoice.PaidAmountUSD;
                                }
                                totalPaidAmountPerService = totalPaidAmount / qtyService;
                            }
                            currentReceivable.ObhPaid = totalPaidAmountPerService;
                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if (contract != null)
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    ObhPaid = totalPaidAmountPerService

                                });
                            }
                            else
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalPaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    ObhPaid = totalPaidAmountPerService
                                });
                            }
                        }
                    }
                }

                #endregion

                #region  Paid Amount
                var dataInvoicesWithSalesmanInvoicePaidApart = GetDataBillingSalesman(fe.PartnerId, fe.Office, fe.Service, AccountingConstants.ACCOUNTANT_TYPE_INVOICE, AccountingConstants.ACCOUNTING_PAYMENT_STATUS_PAID_A_PART);
                var dataGrpSalesmanInvoicePaidApart = dataInvoicesWithSalesmanInvoicePaidApart.GroupBy(x => new { x.SalesmanId, x.PartnerId, x.OfficeId, x.Service })
                  .Select(x => new { x.Key.SalesmanId, x.Key.PartnerId, x.Key.OfficeId, x.Key.Service, invoices = x });
                if (dataGrpSalesmanInvoicePaidApart.Count() > 0)
                {
                    foreach (var item in dataGrpSalesmanInvoicePaidApart)
                    {
                        var currentReceivable = receivables.FirstOrDefault(x => x.PartnerId == item.PartnerId
                           && x.Office == item.OfficeId && x.Service == item.Service && x.SaleMan == item.SalesmanId);
                        if (currentReceivable != null)
                        {
                            // kiểm tra hop dong hien tai cua sales
                            var currentContract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                             && x.SaleManId == item.SalesmanId
                             && x.Active == true
                             && x.OfficeId.Contains(item.OfficeId.ToString())
                             && x.SaleService.Contains(item.Service));
                            if (currentContract != null)
                            {
                                currentReceivable.ContractId = currentContract.Id;
                                currentReceivable.ContractCurrency = currentContract.CreditCurrency;
                            }
                            decimal? totalPaidAmount = 0;
                            decimal? totalPaidAmountPerService = 0;
                            var invoicesData = item.invoices;
                            foreach (var invoice in invoicesData)
                            {
                                int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                       .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                qtyService = (qtyService == 0) ? 1 : qtyService;
                                if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_LOCAL)
                                {
                                    totalPaidAmount += invoice.PaidAmountVND;
                                }
                                else if (currentReceivable.ContractCurrency == AccountingConstants.CURRENCY_USD)
                                {
                                    totalPaidAmount += invoice.PaidAmountUSD;
                                }
                                totalPaidAmountPerService = totalPaidAmount / qtyService;
                            }
                            currentReceivable.PaidAmount = totalPaidAmountPerService;

                            receivableIdModified.Add(currentReceivable.Id);
                        }
                        else
                        {
                            var contract = contractPartnerRepo.First(x => x.PartnerId == partner.Id
                            && x.SaleManId == item.SalesmanId
                            && x.Active == true
                            && x.OfficeId.Contains(item.OfficeId.ToString())
                            && x.SaleService.Contains(item.Service));
                            if (contract != null)
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;
                                    if (contract.CreditCurrency == AccountingConstants.CURRENCY_LOCAL)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountVND;
                                    }
                                    else if (contract.CreditCurrency == AccountingConstants.CURRENCY_USD)
                                    {
                                        totalPaidAmount += invoice.UnpaidAmountUSD;
                                    }

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    ContractCurrency = contract.CreditCurrency,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId,
                                    ContractId = contract.Id,
                                    SaleMan = item.SalesmanId,
                                    UserCreated = contract.UserCreated,
                                    UserModified = contract.UserModified,
                                    OfficeId = fe.Office,
                                    CompanyId = contract.CompanyId,
                                    PaidAmount = totalPaidAmountPerService
                                });
                            }
                            else
                            {
                                decimal? totalPaidAmount = 0;
                                decimal? totalPaidAmountPerService = 0;
                                var invoicesData = item.invoices;
                                foreach (var invoice in invoicesData)
                                {
                                    totalPaidAmount += invoice.UnpaidAmountVND;

                                    int qtyService = !string.IsNullOrEmpty(invoice.Service) ? invoice.Service.Split(';')
                                        .Where(x => x.ToString() != string.Empty).ToArray().Count() : 1;
                                    qtyService = (qtyService == 0) ? 1 : qtyService;

                                    totalPaidAmountPerService = totalPaidAmount / qtyService;
                                }
                                receivables.Add(new AccAccountReceivableModel
                                {
                                    PartnerId = fe.PartnerId,
                                    Office = fe.Office,
                                    Service = fe.Service,
                                    AcRef = partner.ParentId ?? partner.Id,
                                    ContractId = null,
                                    SaleMan = item.SalesmanId,
                                    ContractCurrency = "VND",
                                    PaidAmount = totalPaidAmountPerService
                                });
                            }
                        }
                    }
                }

                #endregion

            }

            receivables.ForEach(fe =>
            {
                fe.DebitAmount = (fe.SellingNoVat ?? 0) + (fe.BillingUnpaid ?? 0) + (fe.ObhAmount ?? 0);
                fe.DatetimeCreated = DateTime.Now;
                fe.DatetimeModified = DateTime.Now;
            });

            return receivables;
        }

        private List<sp_GetBillingWithSalesman> GetDataBillingSalesman(string partner, Guid? officeId, string service, string type, string paymentStatus)
        {
            var parameters = new[]{
                new SqlParameter(){ ParameterName = "@partner", Value = partner},
                new SqlParameter(){ ParameterName = "@officeId", Value = officeId},
                new SqlParameter(){ ParameterName = "@service", Value = service},
                new SqlParameter(){ ParameterName = "@type", Value = type},
                new SqlParameter(){ ParameterName = "@paymentStatus", Value = paymentStatus},
                new SqlParameter(){ ParameterName = "@salesman", Value = null },
                new SqlParameter(){ ParameterName = "@overDue", Value = null },
            };
            var data = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetBillingWithSalesman>(parameters);
            return data;
        }

        private sp_InsertOrUpdateReceivable InsertOrUpdateReceivableList(List<ReceivableTable> receivables)
        {
            var parameters = new[]{
                new SqlParameter()
                {
                    Direction = ParameterDirection.Input,
                    ParameterName = "@Receivables",
                    Value = DataHelper.ToDataTable(receivables),
                    SqlDbType = SqlDbType.Structured,
                    TypeName = "[dbo].[ReceivableTable]"
                }
            };
            var result = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_InsertOrUpdateReceivable>(parameters);
            return result.FirstOrDefault();
        }

        private async Task<HandleState> UpdateAgreementPartnersAsync(List<string> partnerIds, List<Guid?> agreementIds)
        {
            var hs = new HandleState();
            foreach (var partnerId in partnerIds)
            {
                var partner = partnerRepo.Get(x => x.Id == partnerId).FirstOrDefault();
                if (partner != null)
                {
                    var contractPartner = Enumerable.Empty<CatContract>().AsQueryable();
                    //Agreement của partner
                    if (agreementIds.Count == 0)
                    {
                        contractPartner = contractPartnerRepo.Get(x => x.PartnerId == partnerId);
                    }
                    else
                    {
                        contractPartner = contractPartnerRepo.Get(x => x.PartnerId == partnerId
                                                                    && agreementIds.Contains(x.Id));
                    }

                    if (contractPartner.Count() > 0)
                    {
                        foreach (var item in contractPartner)
                        {
                            var agreementPartner = CalculatorAgreement(item);
                            await contractPartnerRepo.UpdateAsync(item, x => x.Id == agreementPartner.Id);

                            if (agreementPartner.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEE)
                            {
                                var relateGuaranteeContracts = contractPartnerRepo.Get(x => x.Active == true
                                    && x.SaleManId == agreementPartner.SaleManId
                                    && x.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEE);

                                if (relateGuaranteeContracts.Count() > 0)
                                {
                                    var _totalDebitAmount = relateGuaranteeContracts.Sum(x => x.DebitAmount);
                                    var _totalAdv = relateGuaranteeContracts.Sum(x => x.CustomerAdvanceAmountVnd);

                                    var salesman = userRepo.Get(x => x.Id == agreementPartner.SaleManId)?.FirstOrDefault();

                                    var _creditRate = ((_totalDebitAmount - _totalAdv) / (salesman.CreditLimit ?? 1)) * 100;
                                    if (_creditRate >= AccountingConstants.MAX_CREDIT_LIMIT_RATE_CONTRACT)
                                    {
                                        foreach (var contract in relateGuaranteeContracts)
                                        {
                                            contract.CreditRate = _creditRate;
                                            contract.IsOverLimit = true;
                                            contractPartnerRepo.Update(contract, x => x.Id == contract.Id, false);
                                        }
                                    }
                                    else
                                    {
                                        foreach (var i in relateGuaranteeContracts)
                                        {
                                            i.CreditRate = _creditRate;
                                            i.IsOverLimit = false;
                                            contractPartnerRepo.Update(i, x => x.Id == i.Id, false);
                                        }
                                    }

                                    salesman.CreditRate = _creditRate;
                                    var hsUpdateUser = await userRepo.UpdateAsync(salesman, x => x.Id == salesman.Id, false);
                                }
                                hs = contractPartnerRepo.SubmitChanges();
                                if (hs.Success && agreementPartner.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEE)
                                {
                                    userRepo.SubmitChanges();
                                }
                            }
                        }
                    }
                    else
                    {
                        //Agreement của AcRef của partner
                        var contractParent = contractPartnerRepo.Get(x => x.Active == true
                                                                       && x.PartnerId == partner.ParentId
                                                                       && agreementIds.Contains(x.Id)).FirstOrDefault();
                        foreach (var item in contractPartner)
                        {
                            var agreementPartner = CalculatorAgreement(item);
                            await contractPartnerRepo.UpdateAsync(item, x => x.Id == agreementPartner.Id);
                        }
                    }
                }
            }
            return hs;
        }

        private CatContract CalculatorAgreement(CatContract agreement)
        {
            var receivables = DataContext.Get(x => x.PartnerId == agreement.PartnerId
                                                && x.SaleMan == agreement.SaleManId
                                                && agreement.SaleService.Contains(x.Service)
                                                && agreement.OfficeId.Contains(x.Office.ToString()));
            return ModifiedAgreementWithReceivables(agreement, receivables);
        }

        private CatContract ModifiedAgreementWithReceivables(CatContract agreement, IQueryable<AccAccountReceivable> receivables)
        {
            agreement.BillingAmount = 0;
            agreement.DebitAmount = 0;
            agreement.UnpaidAmount = 0;
            agreement.PaidAmount = 0;
            agreement.CreditRate = 0;
            agreement.IsOverDue = false;
            agreement.IsOverLimit = false;
            if (receivables != null && receivables.Count() > 0)
            {
                var partnerChildIds = partnerRepo.Get(x => x.ParentId == agreement.PartnerId).Select(s => s.Id).ToList();
                List<CatContract> contractChildOfPartner = new List<CatContract>();
                if (partnerChildIds != null)
                {
                    contractChildOfPartner = contractPartnerRepo.Get(x => partnerChildIds.Any(a => a == x.PartnerId.ToString()) && x.ContractType == "Parent Contract").ToList();
                }

                agreement.BillingAmount = receivables.Sum(su => (su.BillingAmount ?? 0) + (su.ObhBilling ?? 0)); //Sum BillingAmount + BillingOBH
                                                                                                                 //Credit Amount ~ Debit Amount
                agreement.DebitAmount = receivables.Sum(su => su.DebitAmount ?? 0) + contractChildOfPartner.Sum(su => su.DebitAmount ?? 0); //Sum DebitAmount + Debit Amount (của các đối tượng con)
                agreement.UnpaidAmount = receivables.Sum(su => (su.BillingUnpaid ?? 0) + (su.ObhUnpaid ?? 0)) + contractChildOfPartner.Sum(su => su.UnpaidAmount ?? 0); //Sum BillingUnpaid + ObhUnpaid + BillingUnpaid (của các đối tượng con)
                agreement.PaidAmount = receivables.Sum(su => (su.PaidAmount ?? 0) + (su.ObhPaid ?? 0)) + contractChildOfPartner.Sum(su => su.PaidAmount ?? 0); //Sum PaidAmount + ObhPaid + PaidAmount (của các đối tượng con)

                decimal? _creditRate = agreement.CreditRate;
                if (agreement.ContractType == AccountingConstants.ARGEEMENT_TYPE_TRIAL)
                {
                    _creditRate = DataTypeEx.IsNullOrValue(agreement.TrialCreditLimited, 0) ? 0 : (((agreement.DebitAmount ?? 0) - (agreement.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (agreement.CustomerAdvanceAmountVnd ?? 0) : (agreement.CustomerAdvanceAmountUsd ?? 0))) / agreement.TrialCreditLimited) * 100; //((DebitAmount - CusAdv)/TrialCreditLimit)*100
                }
                if (agreement.ContractType == AccountingConstants.ARGEEMENT_TYPE_OFFICIAL || agreement.ContractType == AccountingConstants.ARGEEMENT_TYPE_GUARANTEE)
                {
                    _creditRate = DataTypeEx.IsNullOrValue(agreement.CreditLimit, 0) ? 0 : (((agreement.DebitAmount ?? 0) - (agreement.CreditCurrency == AccountingConstants.CURRENCY_LOCAL ? (agreement.CustomerAdvanceAmountVnd ?? 0) : (agreement.CustomerAdvanceAmountUsd ?? 0))) / agreement.CreditLimit) * 100; //((DebitAmount - CusAdv)/CreditLimit)*100
                }
                if (agreement.ContractType == AccountingConstants.ARGEEMENT_TYPE_PARENT)
                {
                    var parentId = partnerRepo.Get(x => x.Id == agreement.PartnerId).FirstOrDefault()?.ParentId;
                    if (parentId != null)
                    {
                        //Lấy Credit Rate của đối tượng cha (Partner)
                        var creditRateContractParent = contractPartnerRepo.Get(x => x.PartnerId.ToString() == parentId && x.Active == true).FirstOrDefault()?.CreditRate;
                        _creditRate = (creditRateContractParent != null) ? creditRateContractParent : agreement.CreditRate;
                    }
                }
                agreement.CreditRate = _creditRate;
                agreement.DatetimeModified = DateTime.Now;

                if (agreement.CreditRate > AccountingConstants.MAX_CREDIT_LIMIT_RATE_CONTRACT)
                {
                    agreement.IsOverLimit = true; // vượt hạn mức
                }
                else
                {
                    agreement.IsOverLimit = false;
                }

                agreement.IsOverDue = receivables.Any(x => !DataTypeEx.IsNullOrValue(x.Over30Day, 0)); // CR 17536
            }
            return agreement;
        }

        private void WriteLogInsertOrUpdateReceivable(bool status, string message, List<AccAccountReceivableModel> receivables, List<ObjectReceivableModel> models = null)
        {
            string logMessage = string.Format("InsertOrUpdateReceivable by {0} at {1} \n ** models {2} \n ** Message: {3} \n ** Receivables: {4} \n\n---------------------------\n\n",
                            "CalculateReceivable",
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"),
                            models != null ? JsonConvert.SerializeObject(models) : "{}",
                            message,
                            receivables != null ? JsonConvert.SerializeObject(receivables) : "[]");
            string logName = string.Format("InsertOrUpdateReceivable_{0}_eFMS_LOG", (status ? "Success" : "Fail"));
            new LogHelper(logName, logMessage);
        }
    }
}
