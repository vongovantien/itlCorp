using AutoMapper;
using eFMS.API.Common.Globals;
using eFMS.API.Setting.DL.IService;
using eFMS.API.Setting.DL.Models;
using eFMS.API.Setting.DL.Models.Criteria;
using eFMS.API.Setting.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace eFMS.API.Setting.DL.Services
{
    public class FileManagementService : RepositoryBase<SysImage, SysImageModel>, IFileManagementService
    {
        private readonly IStringLocalizer stringLocalizer;
        private readonly ICurrentUser currentUser;
        private readonly IContextBase<AcctSoa> acctSOARepo;
        private readonly IContextBase<AcctSettlementPayment> acctSettleRepo;
        private readonly IContextBase<AcctAdvancePayment> acctAdvanceRepo;
        private readonly IContextBase<CsTransaction> csTranRepo;
        private readonly IContextBase<CsTransactionDetail> csTranDetailRepo;
        private readonly IContextBase<OpsTransaction> opsTranRepo;
        private readonly IContextBase<SysImageDetail> edocRepo;
        private readonly IContextBase<AccAccountingManagement> accManageRepo;
        private readonly IContextBase<CsShipmentSurcharge> surchargeRepo;
        private readonly IContextBase<SysAttachFileTemplate> docTypeRepo;
        private readonly IContextBase<AcctCdnote> cdNoteRepo;

        public FileManagementService(IStringLocalizer<LanguageSub> localizer, IContextBase<AcctCdnote> cdNoteRepository, IContextBase<SysAttachFileTemplate> docTypeRepository, IContextBase<CsShipmentSurcharge> surchargeRepository, IContextBase<AccAccountingManagement> accManageRepository, IContextBase<CsTransactionDetail> csTranDetailRepoitory, IContextBase<OpsTransaction> opsTranRepository, IContextBase<SysImage> repository, IContextBase<CsTransaction> csTran, IContextBase<AcctSoa> acctSOA, IContextBase<SysImageDetail> EDoc, IContextBase<AcctSettlementPayment> accSettle, IContextBase<AcctAdvancePayment> acctAdvance, IMapper mapper, ICurrentUser user) : base(repository, mapper)
        {
            stringLocalizer = localizer;
            currentUser = user;
            acctSOARepo = acctSOA;
            acctSettleRepo = accSettle;
            acctAdvanceRepo = acctAdvance;
            csTranRepo = csTran;
            csTranDetailRepo = csTranDetailRepoitory;
            edocRepo = EDoc;
            opsTranRepo = opsTranRepository;
            accManageRepo = accManageRepository;
            surchargeRepo = surchargeRepository;
            docTypeRepo = docTypeRepository;
            cdNoteRepo = cdNoteRepository;
        }

        public IQueryable<SysImageViewModel> Get(FileManagementCriteria criteria, int page, int size, out int rowsCount)
        {
            var data = DataContext.Where(s => s.Folder == criteria.FolderName).OrderByDescending(s => s.DatetimeModified).GroupBy(s => s.ObjectId).Select(s => s.FirstOrDefault());
            //Phân trang
            var _totalItem = data.Count();
            rowsCount = (_totalItem > 0) ? _totalItem : 0;
            var settlements = acctSettleRepo.Get();
            var soas = acctSOARepo.Get();
            var advs = acctAdvanceRepo.Get();
            var shms = csTranRepo.Get();
            var query = Enumerable.Empty<SysImageViewModel>().AsQueryable();
            switch (criteria.FolderName)
            {
                case "Shipment":
                    var queryJoinShipment = from d in data
                                            join shm in shms on d.ObjectId equals shm.Id.ToString()
                                            where
                                            (
                                                criteria.Keywords != null && criteria.Keywords.Count > 0 ? criteria.Keywords.Contains(shm.JobNo, StringComparer.OrdinalIgnoreCase) : true
                                            )
                                            select new SysImageViewModel
                                            {
                                                FolderName = shm.JobNo,
                                                DateTimeCreated = d.DateTimeCreated,
                                                UserCreated = d.UserCreated,
                                                ObjectId = d.ObjectId,
                                                FolderType = shm.TransactionType,
                                            };
                    if (!string.IsNullOrEmpty(criteria.FolderType))
                    {
                        query = queryJoinShipment.Where(s => s.FolderType == criteria.FolderType).AsQueryable();
                        break;
                    }
                    query = queryJoinShipment.GroupBy(s => s.FolderType).Select(s => s.FirstOrDefault()).AsQueryable();
                    break;
                case "SOA":
                    var queryJoinSoa = from d in data
                                       join soa in soas on d.ObjectId equals soa.Id.ToString()
                                       where
                                       (
                                           criteria.Keywords != null && criteria.Keywords.Count > 0 ? criteria.Keywords.Contains(soa.Soano, StringComparer.OrdinalIgnoreCase) : true
                                       )
                                       select new SysImageViewModel
                                       {
                                           FolderName = "SOA" + soa.Soano,
                                           DateTimeCreated = d.DateTimeCreated,
                                           UserCreated = d.UserCreated,
                                           ObjectId = d.ObjectId
                                       };
                    query = queryJoinSoa.AsQueryable();
                    break;
                case "Settlement":
                    var queryJoinSm = from d in data
                                      join sm in settlements on d.ObjectId equals sm.Id.ToString()
                                      where
                                        (
                                            criteria.Keywords != null && criteria.Keywords.Count > 0 ? criteria.Keywords.Contains(sm.SettlementNo, StringComparer.OrdinalIgnoreCase) : true
                                        )
                                      select new SysImageViewModel
                                      {
                                          FolderName = sm.SettlementNo,
                                          DateTimeCreated = d.DateTimeCreated,
                                          UserCreated = d.UserCreated,
                                          ObjectId = d.ObjectId
                                      };
                    query = queryJoinSm.AsQueryable();
                    break;
                case "Advance":
                    var queryJoinAdv = from d in data
                                       join adv in advs on d.ObjectId equals adv.Id.ToString()
                                       where
                                       (
                                           criteria.Keywords != null && criteria.Keywords.Count > 0 ? criteria.Keywords.Contains(adv.AdvanceNo, StringComparer.OrdinalIgnoreCase) : true
                                       )
                                       select new SysImageViewModel
                                       {
                                           FolderName = adv.AdvanceNo,
                                           DateTimeCreated = d.DateTimeCreated,
                                           UserCreated = d.UserCreated,
                                           ObjectId = d.ObjectId
                                       };
                    query = queryJoinAdv.AsQueryable();
                    break;
                default:
                    break;
            }

            rowsCount = query.Count();

            if (page == 0)
            {
                page = 1;
                size = rowsCount;
            }

            return query.Skip((page - 1) * size).Take(size);
        }

        public List<SysImageViewModel> GetDetail(string folderName, string objectId)
        {
            var data = DataContext.Where(s => s.ObjectId == objectId && s.Folder == folderName).ToList();
            var result = mapper.Map<List<SysImageViewModel>>(data);
            return result;
        }

        private List<SysImageDetail> ConvertToEDocOther(List<SysImage> images)
        {
            List<SysImageDetail> result = new List<SysImageDetail>();
            images.ForEach(x =>
            {
                if (isOther(x.Id).Result==false)
                {
                    var detail = new SysImageDetail
                    {
                        UserCreated = x.UserCreated,
                        DatetimeCreated = x.DateTimeCreated,
                        SystemFileName = "OTH_" + x.Name,
                        UserFileName = x.Name,
                        Id = x.Id,
                        SysImageId = x.Id,
                        UserModified = x.UserModified,
                        Source = x.Folder,
                        DatetimeModified = x.DatetimeModified,
                        BillingNo = GetBillingNo(x.ObjectId, x.Folder).Result,
                        BillingType = x.Folder
                    };
                    result.Add(detail);
                }
            });
            return result;
        }

        private async Task<string> GetBillingNo(string billingId,string billingType)
        {
            string billingNo = "";
            switch (billingType)
            {
                case "SOA":
                    var soa= await acctSOARepo.GetAsync(x => x.Id == billingId);
                    billingNo= soa.FirstOrDefault()?.Soano;
                    break;
                case "Settlement":
                    var settle = await acctSettleRepo.GetAsync(x => x.Id.ToString() == billingId);
                    billingNo= settle.FirstOrDefault()?.SettlementNo;
                    break;
                default: break;
            }
            return billingNo;
        }

        private async Task<List<string>> GetACRefNo(string billingNo, string Type,bool isOther)
        {
            var result = new List<AccAccountingManagement>();
            if (billingNo == null)
            {
                return null;
            }
            result = await accManageRepo.GetAsync(x => x.AttachDocInfo == billingNo);
            return result.Select(x => x.VoucherId).ToList();
        }

        private List<string> getListRefNo(EDocManagementCriterial criteria)
        {
            if(criteria.ReferenceNo == null){
                return null;
            }
            criteria.ReferenceNo = criteria.ReferenceNo.Replace(" ", "").Replace("\t", ";").Replace("\n", ";");
            return criteria.ReferenceNo.Split(";").ToList();
        }

        public async Task<FileManageResponse> GetEdocManagement(EDocManagementCriterial criterial,int page,int size)
        {
            var data = await edocRepo.WhereAsync(await ExpressionQuery(criterial));
            List<EDocFile> eDocFiles = new List<EDocFile>();
            var result = new List<SysImageDetail>();
            var other = new List<SysImageDetail>();
            int totalData = 0;
            if (criterial.isAcc)
            {
                var countData = size - data.Count();
                if (countData > 0)
                {
                    var query = QueryOther(criterial).Result;
                    var otherData = await DataContext.WhereAsync(query);
                    other = ConvertToEDocOther(otherData);
                    totalData = other.Count() + data.Count();
                    result = data.Concat(other).AsQueryable().OrderBy(x=>x.BillingNo).Skip((page - 1) * (size + other.Count())).Take(size).ToList();
                }
                else
                {
                    totalData = data.Count();
                    result = data.AsQueryable().Skip((page - 1) * (size + other.Count())).Take(size).ToList();
                }
            }
            else
            {
                totalData = data.Count();
                result = data.AsQueryable().Skip((page - 1) * size).Take(size).ToList();
            }
            bool isAcc = criterial.AccountantTypes != null;
            result.ToList().ForEach(x =>
            {
                var edocFile = MappingEDocFile(x, isAcc);
                eDocFiles.Add(edocFile.Result);
            });
            var response = new FileManageResponse()
            {
                Data = eDocFiles.AsQueryable(),
                TotalItem = totalData,
            };
            return response;
        }

        private async Task<List<AccAccountingManagement>> GetByDate(EDocManagementCriterial criteria, bool isRef, string Type)
        {
            if (!isRef)
            {
                    if (Type == "INVOICE")
                    {
                        return await accManageRepo.WhereAsync(x => (x.VoucherType.ToUpper().Replace(" ", "").Contains("Invoice") || x.VoucherType.ToUpper().Replace(" ", "").Contains("Invoice Temp")) && x.InvoiceNoReal != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1));
                    }
                if (Type == null)
                {
                    return await accManageRepo.WhereAsync(x => x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1));
                }
                return await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1));
            }
            else
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue && criteria.DateMode != DateMode.CreateDate)
                {
                    if (Type == "INVOICE")
                    {
                        var accManageIV = await accManageRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.InvoiceNoReal) && x.Type != null);
                        return accManageIV.ToList().Where(x => (x.Type.ToUpper().Replace(" ", "").Contains("INVOICE") || x.Type.ToUpper().Replace(" ", "").Contains("INVOICETEMP")) && x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1)).ToList();
                    }
                    if (Type == null)
                    {
                        var accManageAll = await accManageRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.VoucherId) && x.VoucherType != null);
                        return accManageAll.ToList().Where(x => x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1)).ToList();
                    }
                    var accManage = await accManageRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.VoucherId) && x.VoucherType != null);
                    return accManage.ToList().Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1)).ToList();
                }
                else
                {

                    if (Type == "INVOICE")
                    {
                        var accManageIV = await accManageRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.InvoiceNoReal) && x.Type != null);
                        return accManageIV.ToList().Where(x => (x.Type.ToUpper().Replace(" ", "").Contains("INVOICE") || x.Type.ToUpper().Replace(" ", "").Contains("INVOICETEMP")) && x.VoucherId != null).ToList();
                    }
                    if (Type == null)
                    {
                        var accManageAll = await accManageRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.VoucherId) && x.VoucherType != null);
                        return accManageAll.ToList().Where(x => x.VoucherId != null).ToList();
                    }
                    var accManage = await accManageRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.VoucherId) && x.VoucherType != null);
                    return accManage.ToList().Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null).ToList();
                }

            }
        }

        private Boolean haveAcc(string billingNo,string accType, List<string> refNo)
        {
            if (accType == null)
            {
                if (refNo != null && refNo.Count() > 0)
                {
                    return accManageRepo.Any(z => z.AttachDocInfo == billingNo && refNo.Contains(z.VoucherId));
                }
                else
                {
                    return accManageRepo.Any(z => z.AttachDocInfo == billingNo);
                }
            }
            else
            {
                if (refNo != null && refNo.Count() > 0)
                {
                    return accManageRepo.Any(z => z.AttachDocInfo == billingNo && z.VoucherType == accType && refNo.Contains(z.VoucherId));
                }
                else
                {
                    return accManageRepo.Any(z => z.AttachDocInfo == billingNo && z.VoucherType == accType);
                }
            }
          
        }

        private List<Guid> GetOtherByCreateDate(EDocManagementCriterial criterial,string accType)
        {
            var result = new List<Guid>();
            var refNo = getListRefNo(criterial);
            var data =  DataContext.Get(x => x.DateTimeCreated <= criterial.ToDate.Value.AddDays(1) && x.DateTimeCreated >= criterial.FromDate && (x.Folder == "SOA"|| x.Folder == "Settlement"));
            data.Select(x => new { id = x.Id, objId = x.ObjectId, type = x.Folder }).ToList().ForEach( x =>
            {
                switch (x.type)
                {
                    case "SOA":
                        var soa = acctSOARepo.Get(z => z.Id == x.objId);
                        var soaNo = soa.FirstOrDefault()?.Soano;
                        if (soaNo != null)
                        {
                            if (haveAcc(soaNo,accType,refNo))
                            {
                                result.Add(x.id);
                            }
                        }
                        break;
                    case "Settlement":
                        var sm = acctSettleRepo.Get(z => z.Id.ToString() == x.objId);
                        var smNo = sm.FirstOrDefault()?.SettlementNo;
                        if (smNo != null)
                        {
                            if (haveAcc(smNo, accType, refNo))
                            {
                                result.Add(x.id);
                            }
                        }
                        break;
                    default:
                        break;
                }
            });

            return result;
        }

        private async Task<Expression<Func<SysImage, bool>>> QueryOther(EDocManagementCriterial criteria)
        {
            Expression<Func<SysImage, bool>> query = q => true;
            var isRefNo = criteria.ReferenceNo != null && criteria.ReferenceNo != "";
            bool haveQuery = false;
            var imageId=new List<Guid>();
            var objectid = new List<string>();
            var result = new List<EDocFile>();
            if (criteria.AccountantTypes != null && criteria.AccountantTypes.Count() > 0)
            {
                if (criteria.ReferenceType != ReferenceType.InvoiceNo)
                {
                    for (int i = 0; i < criteria.AccountantTypes.Count; i++)
                    {
                        switch (criteria.AccountantTypes[i])
                        {
                            case AccountantType.OtherEntry:
                                if (criteria.DateMode == DateMode.AccountingDate)
                                {
                                    var voucherOTIds = GetByDate(criteria, isRefNo, "OTHERENTRY").Result;
                                    var voucherOTId = voucherOTIds.Select(x => x.VoucherId).ToList();
                                    var otherOTId = GetListOtherFile(voucherOTId);
                                    if (otherOTId.Count() > 0)
                                    {   
                                        haveQuery = true;
                                        //query = query.And(x => otherOTId.Contains(x.ObjectId));
                                        objectid.AddRange(otherOTId);
                                    }
                                }
                                else
                                {
                                    var otherOTId = GetOtherByCreateDate(criteria, "Other Entry");
                                    if (otherOTId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherOTId.Contains(x.Id));
                                        imageId.AddRange(otherOTId);
                                    }
                                }
                                
                                break;
                            case AccountantType.CashPayment:
                                if (criteria.DateMode == DateMode.AccountingDate)
                                {
                                    var voucherCPIds = GetByDate(criteria, isRefNo, "CASHPAYMENT").Result;
                                    var voucherCPId = voucherCPIds.Select(x => x.VoucherId).ToList();
                                    var otherCPId = GetListOtherFile(voucherCPId);
                                    if (otherCPId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherCPId.Contains(x.ObjectId));
                                        objectid.AddRange(otherCPId);
                                    }
                                }
                                else
                                {
                                    var otherCPId = GetOtherByCreateDate(criteria, "Cash payment");
                                    if (otherCPId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherCPId.Contains(x.Id));
                                        imageId.AddRange(otherCPId);
                                    }
                                }
                                break;
                            case AccountantType.PurchasingNote:
                                if (criteria.DateMode == DateMode.AccountingDate)
                                {
                                    var voucherPNIds = GetByDate(criteria, isRefNo, "PURCHASINGNOTE").Result;
                                    var voucherPNId = voucherPNIds.Select(x => x.VoucherId).ToList();
                                    var otherPNId = GetListOtherFile(voucherPNId);
                                    if (otherPNId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherPNId.Contains(x.ObjectId));
                                        objectid.AddRange(otherPNId);
                                    }
                                }
                                else
                                {
                                    var otherPNId = GetOtherByCreateDate(criteria, "Purchasing note");
                                    if (otherPNId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherPNId.Contains(x.Id));
                                        imageId.AddRange(otherPNId);
                                    }
                                }
                                break;
                            case AccountantType.CreditSlip:
                                if (criteria.DateMode == DateMode.AccountingDate)
                                {
                                    var voucherCLIds = GetByDate(criteria, isRefNo, "CREDITSLIP").Result;
                                    var voucherCLId = voucherCLIds.Select(x => x.VoucherId).ToList();
                                    var otherCLId = GetListOtherFile(voucherCLId);
                                    if (otherCLId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherCLId.Contains(x.ObjectId));
                                        objectid.AddRange(otherCLId);
                                    }
                                }
                                else
                                {
                                    var otherCLId = GetOtherByCreateDate(criteria, "Credit Slip");
                                    if (otherCLId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherCLId.Contains(x.Id));
                                        imageId.AddRange(otherCLId);
                                    }
                                }
                                break;
                            case AccountantType.CashReceipt:
                                if (criteria.DateMode == DateMode.AccountingDate)
                                {
                                    var voucherCRIds = GetByDate(criteria, isRefNo, "CASHRECEIPT").Result;
                                    var voucherCRId = voucherCRIds.Select(x => x.VoucherId).ToList();
                                    var otherCRId = GetListOtherFile(voucherCRId);
                                    if (otherCRId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherCRId.Contains(x.Id.ToString()));
                                        objectid.AddRange(otherCRId);
                                    }
                                }
                                else
                                {
                                    var otherCRId = GetOtherByCreateDate(criteria, "Cash receipt");
                                    if (otherCRId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherCRId.Contains(x.Id));
                                        imageId.AddRange(otherCRId);
                                    }
                                }
                                break;
                            case AccountantType.All:
                                if (criteria.DateMode == DateMode.AccountingDate)
                                {
                                    var voucherALLIds = GetByDate(criteria, isRefNo, null).Result;
                                    var voucherALLId = voucherALLIds.Select(x => x.VoucherId).ToList();
                                    var otherALLId = GetListOtherFile(voucherALLId);
                                    if (otherALLId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherALLId.Contains(x.Id.ToString()));
                                        objectid.AddRange(otherALLId);
                                    }
                                }
                                else
                                {
                                    var otherALLId = GetOtherByCreateDate(criteria, null);
                                    if (otherALLId.Count() > 0)
                                    {
                                        haveQuery = true;
                                        //query = query.And(x => otherALLId.Contains(x.Id));
                                        imageId.AddRange(otherALLId);
                                    }
                                }
                                break;
                        }
                    };
                    if (!haveQuery)
                    {
                        query = q => false;
                    }
                    if(haveQuery)
                    {
                        if (imageId.Count > 0)
                        {
                            query = query.And(x => imageId.Contains(x.Id));
                        }
                        if (objectid.Count > 0)
                        {
                            query = query.And(x => objectid.Contains(x.ObjectId));
                        }
                            
                    }
                }
                else
                {
                    var voucherCRIds = GetByDate(criteria, isRefNo, "INVOICE").Result;
                    var voucherCRId = voucherCRIds.Select(x => x.VoucherId).ToList();
                    var otherId = GetListOtherFile(voucherCRId.ToList());
                    query = query.And(x => otherId.Contains(x.Id.ToString()));
                }
            }
            var data = DataContext.Where(query).ToList();
             if (criteria.DateMode == DateMode.CreateDate)
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    query = query.And((z => z.DateTimeCreated >= criteria.FromDate.Value && z.DateTimeCreated <= criteria.ToDate.Value.AddDays(1)));
                }
            }
            query = query.And(x => x.Folder == "Settlement" || x.Folder == "SOA");
            return query;
        }
        private string getFileNameWithWxtention(Guid? sysImageId, bool isOTH)
        {
            if (isOTH)
            {
                return "OTH" + DataContext.Get(x => x.Id == sysImageId).FirstOrDefault().Name;
            }
            return DataContext.Get(x => x.Id == sysImageId).FirstOrDefault().Name;
        }

        private async Task<EDocFile> MappingEDocFile(SysImageDetail imageDetail, bool isAcc)
        {
            var docType = await docTypeRepo.GetAsync(x => x.Id == imageDetail.DocumentTypeId);
            var edocFile = _mapper.Map<EDocFile>(imageDetail);
            if (docType.Count() > 0)
            {
                edocFile.DocumentType = docType.FirstOrDefault().NameEn;
            }
            if (docType.Count == 0)
            {
                edocFile.Type = "Accountant";
                edocFile.ImageUrl = DataContext.Get(x => x.Id == imageDetail.SysImageId).FirstOrDefault()?.Url;
                edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId, true);
                edocFile.DocumentType = "Other";
                edocFile.Source = edocFile.BillingType;

            }
            else if (docType.FirstOrDefault().TransactionType == "CL")
            {
                var jobOps = await opsTranRepo.GetAsync(x => x.Id == edocFile.JobId);
                edocFile.JobRef = jobOps.FirstOrDefault().JobNo;
                edocFile.HBLNo = imageDetail.Hblid != null && imageDetail.Hblid != Guid.Empty ? jobOps.Where(x => x.Hblid == imageDetail.Hblid).FirstOrDefault().Hwbno : null;
                edocFile.Type = docType.FirstOrDefault().Type;
                edocFile.ImageUrl = DataContext.Get(x => x.Id == imageDetail.SysImageId).FirstOrDefault()?.Url;

                if (imageDetail.SystemFileName.Substring(0, 3) == "OTH")
                {
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId, true);
                }
                else
                {
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId, false);
                }
            }
            else
            {
                var jobCS = await csTranRepo.GetAsync(x => x.Id == edocFile.JobId);
                edocFile.JobRef = jobCS.FirstOrDefault().JobNo;
                var detailCs = await csTranDetailRepo.GetAsync(x => x.Id == edocFile.Hblid);
                edocFile.HBLNo = imageDetail.Hblid != null && imageDetail.Hblid != Guid.Empty ? detailCs.FirstOrDefault().Hwbno : null;
                edocFile.Type = docType.FirstOrDefault().Type;
                edocFile.ImageUrl = DataContext.Get(x => x.Id == imageDetail.SysImageId).FirstOrDefault()?.Url;
                if (imageDetail.SystemFileName.Substring(0, 3) == "OTH")
                {
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId, true);
                }
                else
                {
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId, false);
                }
            }
            edocFile.BillingNo = imageDetail.BillingNo;
            edocFile.BillingType = imageDetail.BillingType;
            if (isAcc)
            {
                edocFile.AcRef = GetACRefNo(imageDetail.BillingNo, imageDetail.Source, true).Result;
            }
            return edocFile;
        }
        private async Task<Expression<Func<SysImageDetail, bool>>> QueryAccType(List<AccountantType> accTypes, List<AccAccountingManagement> accManage)
        {
            Expression<Func<SysImageDetail, bool>> query1 = q => true;
            var Acctype = new List<Guid>();
            bool haveQuery = false;
            for (int i = 0; i < accTypes.Count; i++)
            {
                switch (accTypes[i])
                {
                    case AccountantType.OtherEntry:
                        //if (accManage == null)
                        //{
                        //    var voucherOTIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("OTHERENTRY") && x.VoucherId != null); ;
                        //    var billingNo = voucherOTIds.Select(x => x.AttachDocInfo);
                        //    var edocOTs = await edocRepo.GetAsync(x => billingNo.Contains(x.BillingNo));
                        //    Acctype.AddRange(edocOTs.Select(x => x.Id).ToList());
                        //}
                        //else
                        //{
                            var voucherOTIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("OTHERENTRY") && x.VoucherId != null);
                            var billingNoOT = voucherOTIds.Select(x => x.AttachDocInfo);
                            var edocOTs = edocRepo.Get(x => billingNoOT.Contains(x.BillingNo));
                            Acctype.AddRange(edocOTs.Select(x => x.Id).ToList());
                        //}
                        break;
                    case AccountantType.CashPayment:
                        //if (accManage == null)
                        //{
                        //    var voucherCPIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHPAYMENT") && x.VoucherId != null);
                        //    var billingNo = voucherCPIds.Select(x => x.AttachDocInfo);
                        //    var edocCPs = await edocRepo.GetAsync(x => billingNo.Contains(x.BillingNo));
                        //    Acctype.AddRange(edocCPs.Select(x => x.Id).ToList());
                        //}
                        //else
                        //{
                            var voucherCPIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHPAYMENT") && x.VoucherId != null);
                            var billingNoCP = voucherCPIds.Select(x => x.AttachDocInfo);
                            var edocCPs = edocRepo.Get(x => billingNoCP.Contains(x.BillingNo));
                            Acctype.AddRange(edocCPs.Select(x => x.Id).ToList());
                        //}
                        break;
                    case AccountantType.PurchasingNote:
                        //if (accManage == null)
                        //{
                        //    var voucherPNIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("PURCHASINGNOTE") && x.VoucherId != null);
                        //    var billingNo = voucherPNIds.Select(x => x.AttachDocInfo).Distinct();
                        //    var edocPNs = await edocRepo.GetAsync(x => billingNo.Contains(x.BillingNo));
                        //    Acctype.AddRange(edocPNs.Select(x => x.Id).ToList());
                        //}
                        //else
                        //{
                            var voucherPNIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("PURCHASINGNOTE") && x.VoucherId != null);
                            var billingNoPN = voucherPNIds.Select(x => x.AttachDocInfo);
                            var edocPNs = edocRepo.Get(x => billingNoPN.Contains(x.BillingNo));
                            Acctype.AddRange(edocPNs.Select(x => x.Id).ToList());
                        //}
                        break;
                    case AccountantType.CreditSlip:
                        //if (accManage == null)
                        //{
                        //    var voucherCLIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CREDITSLIP") && x.VoucherId != null);
                        //    var billingNo = voucherCLIds.Select(x => x.AttachDocInfo).Distinct();
                        //    var edocCLs = await edocRepo.GetAsync(x => billingNo.Contains(x.BillingNo));
                        //    Acctype.AddRange(edocCLs.Select(x => x.Id).ToList());
                        //}
                        //else
                        //{
                            var voucherCLIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CREDITSLIP") && x.VoucherId != null);
                            var billingNoCL = voucherCLIds.Select(x => x.AttachDocInfo);
                            var edocCLs = edocRepo.Get(x => billingNoCL.Contains(x.BillingNo));
                            Acctype.AddRange(edocCLs.Select(x => x.Id).ToList());
                        //}
                        break;
                    case AccountantType.CashReceipt:
                        //if (accManage == null)
                        //{
                        //    var voucherRCIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHRECEIPT") && x.VoucherId != null);
                        //    var billingNo = voucherRCIds.Select(x => x.AttachDocInfo);
                        //    var edocRCs = await edocRepo.GetAsync(x => billingNo.Contains(x.BillingNo));
                        //    Acctype.AddRange(edocRCs.Select(x => x.Id).ToList());
                        //}
                        //else
                        //{
                            var voucherRCIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHRECEIPT") && x.VoucherId != null);
                            var billingNoRC = voucherRCIds.Select(x => x.AttachDocInfo);
                            var edocRCs = edocRepo.Get(x => billingNoRC.Contains(x.BillingNo));
                            Acctype.AddRange(edocRCs.Select(x => x.Id).ToList());
                        //}
                        break;
                    case AccountantType.All:
                        //if (accManage == null)
                        //{
                        //    var voucherRCIds = await accManageRepo.WhereAsync(x => x.VoucherId != null);
                        //    var billingNo = voucherRCIds.Select(x => x.AttachDocInfo);
                        //    var edocRCs = await edocRepo.GetAsync(x => billingNo.Contains(x.BillingNo));
                        //    Acctype.AddRange(edocRCs.Select(x => x.Id).ToList());
                        //}
                        //else
                        //{
                            var voucherALLIds = accManage.Where(x =>x.VoucherId != null);
                            var billingNoALL = voucherALLIds.Select(x => x.AttachDocInfo);
                            var edocALLs = edocRepo.Get(x => billingNoALL.Contains(x.BillingNo));
                            Acctype.AddRange(edocALLs.Select(x => x.Id).ToList());
                        //}
                        break;
                }
                if (Acctype.Count() > 0)
                {
                    query1 = query1.And(x => Acctype.Contains((Guid)x.Id));
                    haveQuery = true;
                }
            };
            if (haveQuery)
            {
                return query1;
            }
            return query1.And(x => x.Id == null);
        }

        private async Task<Expression<Func<SysImageDetail, bool>>> ExpressionQuery(EDocManagementCriterial criteria)
        {
            Expression<Func<SysImageDetail, bool>> query = q => true;
            var lstRefNo = new List<string>();
            var isRefNo = criteria.ReferenceNo != null && criteria.ReferenceNo != "";
            if (isRefNo)
            {
             lstRefNo = getListRefNo(criteria);
            }

            var jobOps = new List<OpsTransaction>();
            var jobCs = new List<CsTransaction>();
            var jobCsde = new List<CsTransactionDetail>();
            var surCharges = new List<CsShipmentSurcharge>();
            var accManages = new List<AccAccountingManagement>();
            var lstId1 = new List<Guid>();
            var lstId2 = new List<Guid>();

            if (criteria.isAcc)
            {
                query = query.And(x => x.BillingNo != null);
            }
            if (isRefNo)
            {
                switch (criteria.ReferenceType)
                {
                    case ReferenceType.MasterBill:
                        if (opsTranRepo.Any(x => lstRefNo.Contains(x.Mblno)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.Mblno));
                            if (jobOps.Count > 0)
                            {
                                lstId1 = jobOps.Select(x => x.Id).ToList();
                                query = query.And(x => lstId1.Contains((Guid)x.JobId));
                            }

                        }
                        if (csTranRepo.Any(x => lstRefNo.Contains(x.Mawb)))
                        {
                            jobCs = await csTranRepo.WhereAsync(x => lstRefNo.Contains(x.Mawb));
                            if (jobCs.Count > 0)
                            {
                                lstId2 = jobCs.Select(x => x.Id).ToList();
                                query = query.And(x => lstId2.Contains((Guid)x.JobId));
                            }
                        }
                        if (!(opsTranRepo.Any(x => lstRefNo.Contains(x.Mblno))) && !csTranRepo.Any(x => lstRefNo.Contains(x.Mawb)))
                        {
                            query = q => false;
                        }
                        break;
                    case ReferenceType.HouseBill:
                        if (opsTranRepo.Any(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));
                            if (jobOps.Count > 0)
                            {
                                lstId1 = jobOps.Select(x => x.Id).ToList();
                                query = query.And(x => lstId1.Contains((Guid)x.JobId));
                            }
                        }
                        if (csTranDetailRepo.Any(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            jobCsde = await csTranDetailRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));

                            if (jobCsde.Count > 0)
                            {
                                lstId2 = jobCsde.Select(x => x.JobId).ToList();
                                query = query.And(x => lstId2.Contains((Guid)x.JobId));
                            }
                        }
                        if (!opsTranRepo.Any(x => lstRefNo.Contains(x.Hwbno)) && !csTranDetailRepo.Any(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            query = q => false;
                        }
                        break;
                    case ReferenceType.JobId:
                        if (opsTranRepo.Any(x => lstRefNo.Contains(x.JobNo)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.JobNo));
                            if (jobOps.Count > 0)
                            {
                                lstId1 = jobOps.Select(x => x.Id).ToList();
                                query = query.And(x => lstId1.Contains((Guid)x.JobId));
                            }
                        }
                        if (csTranRepo.Any(x => lstRefNo.Contains(x.JobNo)))
                        {
                            jobCs = await csTranRepo.WhereAsync(x => lstRefNo.Contains(x.JobNo));
                            if (jobCs.Count > 0)
                            {
                                lstId2 = jobCs.Select(x => x.Id).ToList();
                                query = query.And(x => lstId2.Contains((Guid)x.JobId));
                            }
                        }
                        if (!opsTranRepo.Any(x => lstRefNo.Contains(x.JobNo)) && !csTranRepo.Any(x => lstRefNo.Contains(x.JobNo)))
                        {
                            query = q => false;
                        }
                        break;
                    case ReferenceType.AccountantNo:
                        var accManage = await accManageRepo.GetAsync(x => lstRefNo.Contains(x.VoucherId));
                        var accManageNo = accManage.Select(x => x.AttachDocInfo);
                        var edocs = await edocRepo.GetAsync(x => accManageNo.Contains(x.BillingNo));
                        lstId1 = edocs.Select(x => x.Id).ToList();
                        query = query.And(x => lstId1.Contains((Guid)x.Id));
                        break;
                    case ReferenceType.InvoiceNo:
                        var accManageIV = await accManageRepo.GetAsync(x => lstRefNo.Contains(x.InvoiceNoReal));
                        var accManageNoIV = accManageIV.Select(x => x.AttachDocInfo);
                        var edocIVs = await edocRepo.GetAsync(x => accManageNoIV.Contains(x.BillingNo));
                        lstId1 = edocIVs.Select(x => x.Id).ToList();
                        query = query.And(x => lstId1.Contains((Guid)x.Id));
                        break;
                }
            }

            if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
            {
                switch (criteria.DateMode)
                {
                    case DateMode.CreateDate:
                        query = query.And(x => x.DatetimeCreated >= criteria.FromDate.Value && x.DatetimeCreated <= criteria.ToDate.Value.AddDays(1));
                        break;
                    case DateMode.AccountingDate:
                        var accManage = await accManageRepo.GetAsync(x => x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1));
                        var billingNo = accManage.Select(x => x.AttachDocInfo);
                        query = query.And(x => billingNo.Contains(x.BillingNo));
                        break;
                }
            }
            if (criteria.AccountantTypes != null && criteria.AccountantTypes.Count() > 0 && criteria.ReferenceType!=ReferenceType.InvoiceNo)
            {
                var accManage = new List<AccAccountingManagement>();
                if (!isRefNo)
                {
                    if (criteria.DateMode == DateMode.CreateDate)
                    {
                        accManage = await accManageRepo.GetAsync((x => x.DatetimeCreated >= criteria.FromDate.Value && x.DatetimeCreated <= criteria.ToDate.Value.AddDays(1)&&x.VoucherType!=null));
                    }
                    else
                    {
                        accManage = await accManageRepo.GetAsync(x => x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value.AddDays(1)&&x.VoucherType != null);
                    }
                    
                }
                else
                {
                    accManage = await accManageRepo.GetAsync(x => lstRefNo.Contains(x.VoucherId)&&x.VoucherType != null);
                }
                var query3 = await QueryAccType(criteria.AccountantTypes, accManage);
                query = query.And(query3);
            }

            return query;
        }

        private async Task<bool> isOther(Guid objectId)
        {
            var images = await DataContext.GetAsync(x => x.Id == objectId);
            var imageId = images.FirstOrDefault()?.Id;
            if (imageId == null)
            {
                return false;
            }
            return edocRepo.Any(x => x.SysImageId == imageId);
        }

        private List<string> GetListOtherFile(List<string> accNo)
        {
            var accManage = accManageRepo.Get(x => accNo.Contains(x.VoucherId) && x.Type == "Voucher");
            var billingNos = accManage.Select(x => x.AttachDocInfo);
            var result = new List<string>();
            if (billingNos.ToList().Count > 0)
            {
                billingNos.ToList().ForEach(async item =>
                    {
                        if (item.Contains("SM"))
                        {

                            var settle = await acctSettleRepo.GetAsync(x => x.SettlementNo == item);
                            var smId = settle.FirstOrDefault()?.Id;
                            if (smId != null)
                            {
                                result.Add(smId.ToString());
                            }
                        }
                        else if (item.Contains("CN"))
                        {
                            var job = await cdNoteRepo.GetAsync(x => x.Code == item);
                            var jobIds = job.Select(x => x.JobId.ToString());
                            if (jobIds.Count() > 0)
                            {
                                jobIds.ToList().ForEach(x =>
                                {
                                        result.AddRange(jobIds);
                                });
                            }
                            
                        }
                        else
                        {
                            var soa = await acctSOARepo.GetAsync(x => x.Soano == item);
                            var soaId = soa.FirstOrDefault()?.Id;
                            if (soaId != null && soaId != string.Empty)
                            {
                                Guid soaIdConverted;
                                if (Guid.TryParse(soaId, out soaIdConverted))
                                {
                                        result.Add(soaId);
                                }
                            }
                        }
                    });
            }
            return result;
        }
    }
}
