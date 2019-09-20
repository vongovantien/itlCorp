using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Documentation.DL.Common;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.DL.Models.Criteria;
using eFMS.API.Documentation.DL.Models.ReportResults;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using eFMS.API.Documentation.Service.ViewModels;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace eFMS.API.Documentation.DL.Services
{
    public class OpsTransactionService : RepositoryBase<OpsTransaction, OpsTransactionModel>, IOpsTransactionService
    {
        //private ICatStageApiService catStageApi;
        //private ICatPlaceApiService catplaceApi;
        //private ICatPartnerApiService catPartnerApi;
        //private ISysUserApiService sysUserApi;
        private readonly ICurrentUser currentUser;
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICsShipmentSurchargeService surchargeService;
        private readonly IContextBase<CatPartner> partnerRepository;
        private readonly IContextBase<SysUser> userRepository;
        private readonly IContextBase<CatUnit> unitRepository;
        private readonly IContextBase<CatPlace> placeRepository;


        public OpsTransactionService(IContextBase<OpsTransaction> repository, 
            IMapper mapper, 
            ICurrentUser user, 
            IStringLocalizer<LanguageSub> localizer, 
            ICsShipmentSurchargeService surcharge, 
            IContextBase<CatPartner> partner, 
            IContextBase<SysUser> userRepo,
            IContextBase<CatUnit> unitRepo,
            IContextBase<CatPlace> placeRepo) : base(repository, mapper)
        {
            //catStageApi = stageApi;
            //catplaceApi = placeApi;
            //catPartnerApi = partnerApi;
            //sysUserApi = userApi;
            currentUser = user;
            stringLocalizer = localizer;
            surchargeService = surcharge;
            partnerRepository = partner;
            userRepository = userRepo;
            unitRepository = unitRepo;
            placeRepository = placeRepo;
        }
        public override HandleState Add(OpsTransactionModel model)
        {
            model.Id = Guid.NewGuid();
            model.CreatedDate = DateTime.Now;
            model.UserCreated = currentUser.UserID;
            model.ModifiedDate = model.CreatedDate;
            model.UserModified = model.UserCreated;
            //model.CurrentStatus = "InSchedule";
            var dayStatus = (int)(model.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
            if(dayStatus > 0)
            {
                model.CurrentStatus = TermData.InSchedule;
            }
            else
            {
                model.CurrentStatus = TermData.Processing;
            }
            int countNumberJob = ((eFMSDataContext)DataContext.DC).OpsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
            model.JobNo = GenerateID.GenerateOPSJobID(Constants.OPS_SHIPMENT, countNumberJob);
            var entity = mapper.Map<OpsTransaction>(model);
            return DataContext.Add(entity);
        }
        public HandleState Delete(Guid id)
        {

            var result = DataContext.Delete(x => x.Id == id);
            if (result.Success)
            {
                var assigneds = ((eFMSDataContext)DataContext.DC).OpsStageAssigned.Where(x => x.JobId == id);
                if(assigneds != null)
                {
                    ((eFMSDataContext)DataContext.DC).OpsStageAssigned.RemoveRange(assigneds);
                }
                var detail = ((eFMSDataContext)DataContext.DC).OpsTransaction.FirstOrDefault(x => x.Id == id);
                if(detail != null)
                {
                    var surcharges = ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.Where(x => x.Hblid == detail.Hblid && x.Soano == null);
                    if (surcharges != null)
                    {
                        ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge.RemoveRange(surcharges);
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();
            }
            return result;
        }
        public OpsTransactionModel GetDetails(Guid id)
        {
            var details = DataContext.Where(x => x.Id == id).FirstOrDefault();
            OpsTransactionModel OpsDetails = new OpsTransactionModel();
            OpsDetails = mapper.Map<OpsTransactionModel>(details);

            if (details != null)
            {
                var agent = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == details.AgentId).FirstOrDefault();
                OpsDetails.AgentName = agent == null ? null : agent.PartnerNameEn;

                var supplier = ((eFMSDataContext)DataContext.DC).CatPartner.Where(x => x.Id == details.SupplierId).FirstOrDefault();
                OpsDetails.SupplierName = supplier == null ? null : supplier.PartnerNameEn; 
            }

            return OpsDetails;
        }

        public OpsTransactionResult Paging(OpsTransactionCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = Query(criteria);
            rowsCount = data.Count();
            var totalProcessing = data.Count(x => x.CurrentStatus == TermData.Processing);
            var totalfinish = data.Count(x => x.CurrentStatus == TermData.Finish);
            var totalOverdued = data.Count(x => x.CurrentStatus == TermData.Overdue);
            int totalCanceled = 0;
            if (criteria.ServiceDateFrom == null && criteria.ServiceDateTo == null)
            {
                int year = DateTime.Now.Year - 2;
                criteria.ServiceDateFrom = new DateTime(year, 1, 1);
                criteria.ServiceDateTo = new DateTime(DateTime.Now.Year, 12, 31);
            }
            totalCanceled = DataContext.Count(x => x.CurrentStatus == TermData.Canceled && x.ServiceDate >= criteria.ServiceDateFrom && x.ServiceDate <= criteria.ServiceDateTo); //data.Count(x => x.CurrentStatus == DataTypeEx.GetJobStatus(JobStatus.Canceled));
            if (rowsCount == 0) return null;
            if (size > 1)
            {
                data = data.OrderByDescending(x => x.ModifiedDate);
                if (page < 1)
                {
                    page = 1;
                }
                data = data.Skip((page - 1) * size).Take(size);
            }
            var results = new OpsTransactionResult
            {
                OpsTransactions = data,
                ToTalInProcessing = totalProcessing,
                ToTalFinish = totalfinish,
                TotalOverdued = totalOverdued,
                TotalCanceled = totalCanceled
            };
            return results;
        }
        public bool CheckAllowDelete(Guid jobId)
        {
            var query = (from detail in ((eFMSDataContext)DataContext.DC).OpsTransaction
                         where detail.Id == jobId && detail.CurrentStatus != TermData.Canceled
                         join surcharge in ((eFMSDataContext)DataContext.DC).CsShipmentSurcharge on detail.Hblid equals surcharge.Hblid
                         where surcharge.CreditNo != null || surcharge.DebitNo != null || surcharge.Soano != null || surcharge.PaymentRefNo != null
                         select detail);
            if (query.Any())
            {
                return false;
            }
            return true;
        }
        public IQueryable<OpsTransactionModel> Query(OpsTransactionCriteria criteria)
        {
            var data = GetView().AsQueryable();
            if (data == null)
                return null;
            if(criteria.ServiceDateFrom == null && criteria.ServiceDateTo == null)
            {
                int year = DateTime.Now.Year -2;
                DateTime startDay = new DateTime(year, 1, 1);
                DateTime lastDay = new DateTime(DateTime.Now.Year, 12, 31);
                data = data.Where(x => x.ServiceDate >= startDay && x.ServiceDate <= lastDay);
            }
            if (criteria.All == null)
            {
                data = data.Where(x => (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.HWBNO ?? "").IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                && (x.CustomerID == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                && (x.FieldOpsID == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                && (x.ShipmentMode == criteria.ShipmentMode || string.IsNullOrEmpty(criteria.ShipmentMode))
                                && ((x.ServiceDate ?? null) >= criteria.ServiceDateFrom || criteria.ServiceDateFrom == null)
                                && ((x.ServiceDate ?? null) <= criteria.ServiceDateTo || criteria.ServiceDateTo == null)
                            ).OrderByDescending(x => x.ModifiedDate);
            }
            else
            {
                data = data.Where(x => (x.JobNo ?? "").IndexOf(criteria.JobNo ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.HWBNO ?? "").IndexOf(criteria.Hwbno ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ProductService ?? "").IndexOf(criteria.ProductService ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.ServiceMode ?? "").IndexOf(criteria.ServiceMode ?? "", StringComparison.OrdinalIgnoreCase) > -1
                                   || (x.CustomerID == criteria.CustomerId || string.IsNullOrEmpty(criteria.CustomerId))
                                   || (x.FieldOpsID == criteria.FieldOps || string.IsNullOrEmpty(criteria.FieldOps))
                                   || (x.ShipmentMode == criteria.ShipmentMode || string.IsNullOrEmpty(criteria.ShipmentMode))
                               && ((x.ServiceDate ?? null) >= (criteria.ServiceDateFrom ?? null) && (x.ServiceDate ?? null) <= (criteria.ServiceDateTo ?? null))
                               ).OrderByDescending(x => x.ModifiedDate);
            }
            List<OpsTransactionModel> results = new List<OpsTransactionModel>();
            results = mapper.Map<List<OpsTransactionModel>>(data);
            return results.AsQueryable();
        }
        private List<sp_GetOpsTransaction> GetView()
        {
            var list = ((eFMSDataContext)DataContext.DC).ExecuteProcedure<sp_GetOpsTransaction>(null);
            return list;
        }

        public Crystal PreviewCDNOte(AcctCDNoteDetailsModel model)
        {
            if (model == null)
            {
                return null;
            }
            Crystal result = null;
            var parameter = new AcctSOAReportParams
            {
                DBTitle = "DB title",
                DebitNo = model.CDNote.Code,
                TotalDebit = model.TotalDebit?.ToString(),
                TotalCredit = model.TotalCredit?.ToString(),
                DueToTitle = "",
                DueTo = "",
                DueToCredit = "",
                SayWordAll = "",
                CompanyName = "",
                CompanyDescription="",
                CompanyAddress1 = "",
                CompanyAddress2 = "",
                Website = "efms.itlvn.com",
                IbanCode = "",
                AccountName = "",
                BankName = "",
                SwiftAccs = "",
                AccsUSD = "",
                AccsVND = "",
                BankAddress = "",
                Paymentterms = "",
                DecimalNo = null,
                CurrDecimal = null,
                IssueInv = "",
                InvoiceInfo = "",
                Contact = "",


            };






            return result;
        }

        public HandleState ConvertClearanceToJob(OpsTransactionClearanceModel model)
        {
            var result = new HandleState();
            try
            {
                var existedMessage = CheckExist(model.OpsTransaction);
                if (existedMessage != null)
                {
                    return new HandleState(existedMessage);
                }
                if (CheckExistClearance(model.CustomsDeclaration, model.CustomsDeclaration.Id))
                {
                    result = new HandleState(stringLocalizer[LanguageSub.MSG_CLEARANCENO_EXISTED, model.CustomsDeclaration.ClearanceNo].Value);
                    return result;
                }
                if(model.CustomsDeclaration.JobNo == null)
                {
                    model.OpsTransaction.Id = Guid.NewGuid();
                    model.OpsTransaction.Hblid = Guid.NewGuid();
                    model.OpsTransaction.CreatedDate = DateTime.Now;
                    model.OpsTransaction.UserCreated = currentUser.UserID; //currentUser.UserID;
                    model.OpsTransaction.ModifiedDate = DateTime.Now;
                    model.OpsTransaction.UserModified = currentUser.UserID;
                    int countNumberJob = ((eFMSDataContext)DataContext.DC).OpsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
                    model.OpsTransaction.JobNo = GenerateID.GenerateOPSJobID(Constants.OPS_SHIPMENT, countNumberJob);
                    var dayStatus = (int)(model.OpsTransaction.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
                    if (dayStatus > 0)
                    {
                        model.OpsTransaction.CurrentStatus = TermData.InSchedule;
                    }
                    else
                    {
                        model.OpsTransaction.CurrentStatus = TermData.Processing;
                    }
                    var transaction = mapper.Map<OpsTransaction>(model.OpsTransaction);
                    DataContext.Add(transaction, false);
                }

                var clearance = mapper.Map<CustomsDeclaration>(model.CustomsDeclaration);
                clearance.ConvertTime = DateTime.Now;
                if (clearance.Id > 0)
                {
                    clearance.DatetimeModified = DateTime.Now;
                    clearance.UserModified = currentUser.UserID;
                    clearance.JobNo = model.OpsTransaction.JobNo;
                    ((eFMSDataContext)DataContext.DC).CustomsDeclaration.Update(clearance);
                }
                else
                {
                    clearance.DatetimeCreated = DateTime.Now;
                    clearance.DatetimeModified = DateTime.Now;
                    clearance.UserCreated = model.CustomsDeclaration.UserModified = currentUser.UserID;
                    clearance.Source = Constants.CLEARANCE_FROM_EFMS;
                    clearance.JobNo = model.OpsTransaction.JobNo;
                    ((eFMSDataContext)DataContext.DC).CustomsDeclaration.Add(clearance);
                }
                DataContext.DC.SaveChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }
        private bool CheckExistClearance(CustomsDeclarationModel model, decimal id)
        {
            if (id == 0)
            {
                if (((eFMSDataContext)DataContext.DC).CustomsDeclaration.Any(x => x.ClearanceNo == model.ClearanceNo && x.ClearanceDate == model.ClearanceDate))
                {
                    return true;
                }
            }
            else
            {
                if (((eFMSDataContext)DataContext.DC).CustomsDeclaration.Any(x => (x.ClearanceNo == model.ClearanceNo && x.Id != id && x.ClearanceDate == model.ClearanceDate)))
                {
                    return true;
                }
            }
            return false;
        }

        public HandleState ConvertExistedClearancesToJobs(List<OpsTransactionClearanceModel> list)
        {
            var result = new HandleState();
            try
            {
                int i = 0;
                foreach (var item in list)
                {
                    var existedMessage = CheckExist(item.OpsTransaction);
                    if (existedMessage != null)
                    {
                        return new HandleState(existedMessage);
                    }
                    if (item.CustomsDeclaration.JobNo == null)
                    {
                        item.OpsTransaction.Id = Guid.NewGuid();
                        item.OpsTransaction.Hblid = Guid.NewGuid();
                        item.OpsTransaction.CreatedDate = DateTime.Now;
                        item.OpsTransaction.UserCreated = currentUser.UserID; //currentUser.UserID;
                        item.OpsTransaction.ModifiedDate = DateTime.Now;
                        item.OpsTransaction.UserModified = currentUser.UserID;
                        int countNumberJob = ((eFMSDataContext)DataContext.DC).OpsTransaction.Count(x => x.CreatedDate.Value.Month == DateTime.Now.Month && x.CreatedDate.Value.Year == DateTime.Now.Year);
                        item.OpsTransaction.JobNo = GenerateID.GenerateOPSJobID(Constants.OPS_SHIPMENT, (countNumberJob + i));
                        var dayStatus = (int)(item.OpsTransaction.ServiceDate.Value.Date - DateTime.Now.Date).TotalDays;
                        if (dayStatus > 0)
                        {
                            item.OpsTransaction.CurrentStatus = TermData.InSchedule;
                        }
                        else
                        {
                            item.OpsTransaction.CurrentStatus = TermData.Processing;
                        }
                        var transaction = mapper.Map<OpsTransaction>(item.OpsTransaction);
                        DataContext.Add(transaction, false);

                        item.CustomsDeclaration.JobNo = item.OpsTransaction.JobNo;
                        item.CustomsDeclaration.UserModified = currentUser.UserID;
                        item.CustomsDeclaration.DatetimeModified = DateTime.Now;
                        item.CustomsDeclaration.ConvertTime = DateTime.Now;
                        var clearance = mapper.Map<CustomsDeclaration>(item.CustomsDeclaration);
                        ((eFMSDataContext)DataContext.DC).CustomsDeclaration.Update(clearance);
                        i = i + 1;
                    }
                }
                ((eFMSDataContext)DataContext.DC).SaveChanges();
            }
            catch (Exception ex)
            {
                result = new HandleState(ex.Message);
            }
            return result;
        }

        public HandleState SoftDeleteJob(Guid id)
        {
            var result = new HandleState();
            var job = DataContext.First(x => x.Id == id && x.CurrentStatus != TermData.Canceled);
            if(job == null)
            {
                result = new HandleState(stringLocalizer[LanguageSub.MSG_DATA_NOT_FOUND]);
            }
            else
            {
                job.CurrentStatus = TermData.Canceled;
                result = DataContext.Update(job, x => x.Id == id);
                if (result.Success)
                {
                    var clearances = ((eFMSDataContext)DataContext.DC).CustomsDeclaration.Where(x => x.JobNo == job.JobNo);
                    if (clearances != null)
                    {
                        foreach(var item in clearances)
                        {
                            item.JobNo = null;
                            ((eFMSDataContext)DataContext.DC).CustomsDeclaration.Update(item);
                        }
                    }
                    ((eFMSDataContext)DataContext.DC).SaveChanges();
                }
            }
            return result;
        }
        public string CheckExist(OpsTransactionModel model)
        {
            var existedHBL = DataContext.Any(x => x.Id != model.Id && x.Hwbno == model.Hwbno && x.CurrentStatus != TermData.Canceled);
            var existedMBL = DataContext.Any(x => x.Id != model.Id && x.Mblno == model.Mblno && x.CurrentStatus != TermData.Canceled);
            if (existedHBL)
            {
                return stringLocalizer[LanguageSub.MSG_HBNO_EXISTED, model.Hwbno].Value;
            }
            if (existedMBL)
            {
                return stringLocalizer[LanguageSub.MSG_MAWB_EXISTED, model.Mblno].Value;
            }
            return null;
        }

        public Crystal PreviewFormPLsheet(Guid id, string currency)
        {
            var shipment = DataContext.First(x => x.Id == id);
            Crystal result = null;
            var parameter = new FormPLsheetReportParameter
            {
                Contact = currentUser.UserName,
                CompanyName = "CompanyName",
                CompanyDescription = "CompanyDescription",
                CompanyAddress1 = "CompanyAddress1",
                CompanyAddress2 = "CompanyAddress2",
                Website = "Website",
                CurrDecimalNo = 2,
                DecimalNo = 2,
                HBLList = shipment.Hwbno
            };

            result = new Crystal
            {
                ReportName = "FormPLsheet.rpt",
                AllowPrint = true,
                AllowExport = true
            };
            var dataSources = new List<FormPLsheetReport>{};
            var agent = partnerRepository.Get(x => x.Id == shipment.AgentId).FirstOrDefault();
            var supplier = partnerRepository.Get(x => x.Id == shipment.SupplierId).FirstOrDefault();
            var surcharges = surchargeService.GetByHB(shipment.Hblid);
            var user = userRepository.Get(x => x.Id == shipment.SalemanId).FirstOrDefault();
            var units = unitRepository.Get();
            var polName = placeRepository.Get(x => x.Id == shipment.Pol).FirstOrDefault()?.NameEn;
            var podName = placeRepository.Get(x => x.Id == shipment.Pod).FirstOrDefault()?.NameEn;
            if(surcharges != null)
            {
                foreach(var item in surcharges)
                {
                    var unitCode = units.FirstOrDefault(x => x.Id == item.UnitId)?.Code;
                    bool isOBH = false;
                    decimal cost = 0;
                    decimal revenue = 0;
                    decimal saleProfit = 0;
                    string partnerName = string.Empty;
                    if (item.Type == "OBH")
                    {
                        isOBH = true;
                        partnerName = item.PayerName;
                    }
                    if(item.Type == "BUY")
                    {
                        cost = item.Total;
                    }
                    if(item.Type == "SELL")
                    {
                        revenue = item.Total;
                    }
                    saleProfit = cost + revenue;

                    var surchargeRpt = new FormPLsheetReport
                    {
                        COSTING = "COSTING Test",
                        TransID = shipment.JobNo,
                        TransDate = (DateTime)shipment.CreatedDate,
                        HWBNO = shipment.Hwbno,
                        MAWB = shipment.Mblno,
                        PartnerName = "PartnerName",
                        ContactName = user?.Username,
                        ShipmentType = "Logistics",
                        NominationParty = string.Empty,
                        Nominated = true,
                        POL = polName,
                        POD = podName,
                        Commodity = string.Empty,
                        Volumne = string.Empty,
                        Carrier = supplier.PartnerNameEn,
                        Agent = agent?.PartnerNameEn,
                        ContainerNo = item.ContNo,
                        OceanVessel = string.Empty,
                        LocalVessel = string.Empty,
                        FlightNo = shipment.FlightVessel,
                        SeaImpVoy = string.Empty,
                        LoadingDate = ((DateTime)shipment.ServiceDate).ToString("dd' 'MMM' 'yyyy"),
                        ArrivalDate = shipment.FinishDate!= null?((DateTime)shipment.FinishDate).ToString("dd' 'MM' 'yyyy"): null,
                        FreightCustomer = "FreightCustomer",
                        FreightColoader = 128,
                        PayableAccount = item.PartnerName,
                        Description = item.ChargeNameEn,
                        Curr = item.CurrencyId,
                        VAT = (decimal)item.Vatrate,
                        VATAmount = 12,
                        Cost = cost,
                        Revenue = revenue,
                        Exchange = 13,
                        VNDExchange = 12,
                        Paid = true,
                        DatePaid = DateTime.Now,
                        Docs = item.InvoiceNo,
                        Notes = item.Notes,
                        InputData = "InputData",
                        SalesProfit = saleProfit,
                        Quantity = item.Quantity,
                        UnitPrice = (decimal)item.UnitPrice,
                        Unit = unitCode,
                        LastRevised = string.Empty,
                        OBH = isOBH,
                        ExtRateVND = 34,
                        KBck = true,
                        NoInv = true,
                        Approvedby = string.Empty,
                        ApproveDate = DateTime.Now,
                        SalesCurr = currency,
                        GW = shipment.SumGrossWeight ?? 0,
                        MCW = 13,
                        HCW = shipment.SumChargeWeight ?? 0,
                        PaymentTerm = string.Empty,
                        DetailNotes = string.Empty,
                        ExpressNotes = string.Empty,
                        InvoiceNo = "InvoiceNo",
                        CodeVender = "CodeVender",
                        CodeCus = "CodeCus",
                        Freight = true,
                        Collect = true,
                        FreightPayableAt = "FreightPayableAt",
                        PaymentTime = 1,
                        PaymentTimeCus = 1,
                        Noofpieces = 12,
                        UnitPieaces = "UnitPieaces",
                        TpyeofService = "TpyeofService",
                        ShipmentSource = "FREE-HAND",
                        RealCost = true
                    };
                    dataSources.Add(surchargeRpt);
                }
            }
            result.AddDataSource(dataSources);
            result.FormatType = ExportFormatType.PortableDocFormat;
            result.SetParameter(parameter);

            return result;
        }
    }
}
