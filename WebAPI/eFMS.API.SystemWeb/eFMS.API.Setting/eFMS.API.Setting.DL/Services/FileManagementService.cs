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
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using eFMS.API.Common.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.JsonPatch.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Security.AccessControl;
using static System.Net.Mime.MediaTypeNames;

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
        private readonly IContextBase<AccAccountPayable> accPayableRepo;

        public FileManagementService(IStringLocalizer<LanguageSub> localizer, IContextBase<AccAccountPayable> accPayableRepository, IContextBase<SysAttachFileTemplate> docTypeRepository, IContextBase<CsShipmentSurcharge> surchargeRepository, IContextBase<AccAccountingManagement> accManageRepository, IContextBase<CsTransactionDetail> csTranDetailRepoitory, IContextBase<OpsTransaction> opsTranRepository, IContextBase<SysImage> repository, IContextBase<CsTransaction> csTran, IContextBase<AcctSoa> acctSOA, IContextBase<SysImageDetail> EDoc, IContextBase<AcctSettlementPayment> accSettle, IContextBase<AcctAdvancePayment> acctAdvance, IMapper mapper, ICurrentUser user) : base(repository, mapper)
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
            accPayableRepo = accPayableRepository;
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

        public async Task<ResponsePagingModel<EDocFile>> GetEdocManagement(EDocManagementCriterial criterial)
        {
            var data = await edocRepo.WhereAsync(await ExpressionQuery(criterial));
            List<EDocFile> eDocFiles = new List<EDocFile>();
            var result = new List<SysImageDetail>();
            var other = new List<EDocFile>();
            if (criterial.isAcc)
            {
                
                other = GetOtherFile(criterial).Result;
                result = data.Concat(other).AsQueryable().Skip((criterial.Page - 1) * (criterial.Size + other.Count())).Take(criterial.Size).ToList();
            }
            else{
                result = data.AsQueryable().Skip((criterial.Page - 1) * criterial.Size).Take(criterial.Size).ToList();
            }
            bool isAcc = criterial.AccountantTypes != null;
            result.ToList().ForEach(x =>
            {
                var edocFile = MappingEDocFile(x, isAcc);
                eDocFiles.Add(edocFile.Result);
            });
            //if (other.Count > 0)
            //{
            //    other.ForEach(x =>
            //    {
            //        eDocFiles.Add(x);
            //    });
            //};
            return new ResponsePagingModel<EDocFile>()
            {
                Data = eDocFiles.AsQueryable(),
                Page = criterial.Page,
                Size = criterial.Size,
                TotalItems = other.Count()>0? data.Count()+other.Count():data.Count,
        };
        }

        private async Task<List<AccAccountingManagement>> GetByDate(EDocManagementCriterial criteria, bool isRef,string Type)
        {
            if (!isRef)
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    if (criteria.DateMode == DateMode.CreateDate)
                    {
                        return await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null && x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate.Value.AddDays(1));
                    }
                    else
                    {
                        return await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value);
                    }
                }
                else
                {
                    return await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null);
                }
            }
            else
            {
                if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
                {
                    criteria.ReferenceNo = criteria.ReferenceNo.Replace(" ", "").Replace("\t", ";").Replace("\n", ";");
                    var accPayable = await accPayableRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.VoucherNo));
                    var accManageNo = accPayable.Select(x => x.VoucherNo);
                    var accManage = await accManageRepo.GetAsync(x => accManageNo.Contains(x.VoucherId));
                    if (criteria.DateMode == DateMode.CreateDate)
                    {
                        return accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null && x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate.Value.AddDays(1)).ToList();
                    }
                    else
                    {
                        return accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null && x.Date >= criteria.FromDate && x.Date <= criteria.ToDate.Value).ToList();
                    }
                }
                else
                {
                    var accPayable = await accPayableRepo.GetAsync(x => criteria.ReferenceNo.Contains(x.VoucherNo));
                    var accManageNo = accPayable.Select(x => x.VoucherNo);
                    var accManage = await accManageRepo.GetAsync(x => accManageNo.Contains(x.VoucherId));
                    return accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains(Type) && x.VoucherId != null).ToList();
                }
                
            }
        }

        private async Task<List<EDocFile>> GetOtherFile(EDocManagementCriterial criteria)
        {
            try
            {
                var result = new List<EDocFile>();
                if (criteria.AccountantTypes != null && criteria.AccountantTypes.Count() > 0)
                {
                    var accManage = new List<AccAccountingManagement>();
                    var accPayable = new List<AccAccountPayable>();
                    var lstRefNo = new List<string>();
                    var isRefNo = criteria.ReferenceNo != null && criteria.ReferenceNo != "";
                    var edocOTs = new List<SysImageModel>();
                    if (isRefNo)
                    {
                        criteria.ReferenceNo = criteria.ReferenceNo.Replace(" ", "").Replace("\t", ";").Replace("\n", ";");
                        lstRefNo = criteria.ReferenceNo.Split(";").ToList();
                        lstRefNo.Remove("");
                    }
                    if (!isRefNo)
                    {
                        accPayable = null;
                        accManage = null;
                    }
                    else
                    {
                        accPayable = await accPayableRepo.GetAsync(x => lstRefNo.Contains(x.VoucherNo));
                        var accManageNo = accPayable.Select(x => x.VoucherNo);
                        accManage = await accManageRepo.GetAsync(x => accManageNo.Contains(x.VoucherId));

                    }
                    for (int i = 0; i < criteria.AccountantTypes.Count; i++)
                    {
                        switch (criteria.AccountantTypes[i])
                        {
                            case AccountantType.OtherEntry:
                                var voucherOTIds = GetByDate(criteria, isRefNo, "OTHERENTRY").Result;
                                if (!isRefNo)
                                {
                                    //var voucherOTIds = accManageRepo.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("OTHERENTRY") && x.VoucherId != null);
                                    //var voucherOTIds = GetByDate(criteria, isRefNo, "OTHERENTRY").Result;
                                    var voucherOTId = voucherOTIds.Select(x => x.VoucherId).ToList();
                                    var accOTPayables = accPayableRepo.Where(x => voucherOTId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accOTPayableNo = accOTPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accOTPayableNo.ToList()));
                                }
                                else
                                {
                                    //var voucherOTIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("OTHERENTRY") && x.VoucherId != null);
                                    //var voucherOTIds = GetByDate(criteria, isRefNo, "OTHERENTRY").Result;
                                    var voucherOTId = voucherOTIds.Select(x => x.VoucherId).ToList();
                                    var accOTPayables = accPayable.Where(x => voucherOTId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accOTPayableNo = accOTPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accOTPayableNo.ToList()));
                                }
                                break;
                            case AccountantType.CashPayment:
                                var voucherCPIds = GetByDate(criteria, isRefNo, "CASHPAYMENT").Result;
                                if (!isRefNo)
                                {
                                    //var voucherCPIds = accManageRepo.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHPAYMENT") && x.VoucherId != null); ;
                                    var voucherCPId = voucherCPIds.Select(x => x.VoucherId).ToList();
                                    var accCPPayables = accPayableRepo.Where(x => voucherCPId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accCPPayableNo = accCPPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accCPPayableNo.ToList()));
                                }
                                else
                                {
                                    //var voucherCPIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHPAYMENT") && x.VoucherId != null);
                                    var voucherCPId = voucherCPIds.Select(x => x.VoucherId).ToList();
                                    var accCPPayables = accPayable.Where(x => voucherCPId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accCPPayableNo = accCPPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accCPPayableNo.ToList()));
                                }
                                break;
                            case AccountantType.PurchasingNote:
                                var voucherPNIds = GetByDate(criteria, isRefNo, "PURCHASINGNOTE").Result;
                                if (!isRefNo)
                                {
                                    //var voucherPNIds = accManageRepo.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("PURCHASINGNOTE") && x.VoucherId != null); ;
                                    var voucherPNId = voucherPNIds.Select(x => x.VoucherId).ToList();
                                    var accPNPayables = accPayableRepo.Where(x => voucherPNId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accPNPayableNo = accPNPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accPNPayableNo.ToList()));
                                }
                                else
                                {
                                    //var voucherPNIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("PURCHASINGNOTE") && x.VoucherId != null);
                                    var voucherPNId = voucherPNIds.Select(x => x.VoucherId).ToList();
                                    var accPNPayables = accPayable.Where(x => voucherPNId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accPNPayableNo = accPNPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accPNPayableNo.ToList()));
                                }
                                break;
                            case AccountantType.CreditSlip:
                                var voucherCLIds = GetByDate(criteria, isRefNo, "CREDITSLIP").Result;
                                if (!isRefNo)
                                {
                                    //var voucherCLIds = accManageRepo.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CREDITSLIP") && x.VoucherId != null);
                                    var voucherCLId = voucherCLIds.Select(x => x.VoucherId).ToList();
                                    var accCLPayables = accPayableRepo.Where(x => voucherCLId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accCLPayableNo = accCLPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accCLPayableNo.ToList()));
                                }
                                else
                                {
                                    //var voucherCLIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CREDITSLIP") && x.VoucherId != null);
                                    var voucherCLId = voucherCLIds.Select(x => x.VoucherId).ToList();
                                    var accCLPayables = accPayable.Where(x => voucherCLId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accCLPayableNo = accCLPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accCLPayableNo.ToList()));
                                }
                                break;
                            case AccountantType.CashReceipt:
                                var voucherCRIds = GetByDate(criteria, isRefNo, "CASHRECEIPT").Result;
                                if (!isRefNo)
                                {
                                    //var voucherCRIds = accManageRepo.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHRECEIPT") && x.VoucherId != null); ;
                                    var voucherCRId = voucherCRIds.Select(x => x.VoucherId).ToList();
                                    var accCRPayables = accPayableRepo.Where(x => voucherCRId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accCRPayableNo = accCRPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accCRPayableNo.ToList()));
                                }
                                else
                                {
                                    //var voucherCRIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHRECEIPT") && x.VoucherId != null);
                                    var voucherCRId = voucherCRIds.Select(x => x.VoucherId).ToList();
                                    var accCRPayables = accPayable.Where(x => voucherCRId.Contains(x.VoucherNo) && x.BillingNo != null);
                                    var accCRPayableNo = accCRPayables.Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType });
                                    edocOTs.AddRange(GetListOtherFile(accCRPayableNo.ToList()));
                                }
                                break;
                        }
                    };
                    edocOTs.ForEach(x =>
                    {
                        //int docId=x.folderName=="SOA"?docTypeRepo.Get(z=>z.Code=="OT"&&z.Type=="SOA").FirstOrDefault().Id: docTypeRepo.Get(z => z.Code == "OT" && z.Type == "SOA").FirstOrDefault().Id
                        var detail = new EDocFile
                        {
                            UserCreated = x.UserCreated,
                            DatetimeCreated = x.DateTimeCreated,
                            SystemFileName = "OTH_" + x.Name,
                            UserFileName = x.Name,
                            Id = x.Id,
                            SysImageId = x.Id,
                            UserModified = x.UserModified,
                            BillingNo = x.BillingNo,
                            BillingType = x.BillingType,
                            Source = x.folderName,
                            DatetimeModified = x.DatetimeModified,
                            DocumentType = "Accountant",
                        };
                        result.Add(detail);
                    });
                }
                 return result.ToList();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
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
            if(docType.Count()>0)
            {
                edocFile.DocumentType = docType.FirstOrDefault().NameEn;
            }
            else
            {
                edocFile.DocumentType = null;
            }
            if (docType.Count == 0)
            {
                edocFile.Type = "Accountant";
                edocFile.ImageUrl = DataContext.Get(x => x.Id == imageDetail.SysImageId).FirstOrDefault()?.Url;
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId, true);
                edocFile.DocumentType = "Other";
                edocFile.Source=edocFile.BillingType;
                   
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

            if (isAcc)
            {
                var acc = await accPayableRepo.GetAsync(x => x.BillingNo == edocFile.BillingNo);
                edocFile.AcRef = acc.Select(x => x.VoucherNo).ToList();
            }

            return edocFile;
        }
        private async Task<Expression<Func<SysImageDetail, bool>>> QueryAccType(List<AccountantType> accTypes, List<AccAccountingManagement> accManage,
        List<AccAccountPayable> accPayable)
        {
            Expression<Func<SysImageDetail, bool>> query1 = q => true;
            var Acctype = new List<Guid>();
            bool haveQuery = false;
            for (int i = 0; i < accTypes.Count; i++)
            {
                switch (accTypes[i])
                {
                    case AccountantType.OtherEntry:
                        if (accManage == null && accPayable == null)
                        {
                            var voucherOTIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("OTHERENTRY") && x.VoucherId != null); ;
                            var voucherOTId = voucherOTIds.Select(x => x.VoucherId);
                            var accOTPayables = await accPayableRepo.WhereAsync(x => voucherOTId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accOTPayableNo = accOTPayables.Select(x => new { billingNo = x.BillingNo, billingType = x.BillingType });
                            var edocOTs = await edocRepo.GetAsync(x => accOTPayableNo.Select(z => z.billingNo).Contains(x.BillingNo));
                            Acctype.AddRange(edocOTs.Select(x => x.Id).ToList());
                        }
                        else
                        {
                            var voucherOTIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("OTHERENTRY") && x.VoucherId != null);
                            var voucherOTId = voucherOTIds.Select(x => x.VoucherId);
                            var accOTPayables = accPayable.Where(x => voucherOTId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accOTPayableNo = accOTPayables.Select(x => x.BillingNo);
                            var edocOTs = edocRepo.Get(x => accOTPayableNo.Contains(x.BillingNo));
                            Acctype.AddRange(edocOTs.Select(x => x.Id).ToList());
                        }
                        //var voucherOTId = accManage.Where(x => x.VoucherType.Replace(" ", "").ToUpper() == "OTHERENTRY").Select(x => x.VoucherId);
                        //var accOTPayables = accPayable.Where(x => voucherOTId.Contains(x.VoucherNo));
                        //var accOTPayableNo = accOTPayables.Select(x => x.BillingNo);
                        //var edocOTs = edocRepo.Get(x => accOTPayableNo.Contains(x.BillingNo));
                        //Acctype = edocOTs.Select(x => x.Id).ToList();
                        break;
                    case AccountantType.CashPayment:
                        if (accManage == null && accPayable == null)
                        {
                            var voucherCPIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHPAYMENT") && x.VoucherId != null);
                            var voucherCPId = voucherCPIds.Select(x => x.VoucherId);
                            var accCPPayables = await accPayableRepo.WhereAsync(x => voucherCPId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accCPPayableNo = accCPPayables.Select(x => new { billingNo = x.BillingNo, billingType = x.BillingType });
                            var edocCPs = await edocRepo.GetAsync(x => accCPPayableNo.Select(z => z.billingNo).Contains(x.BillingNo));
                            Acctype.AddRange( edocCPs.Select(x => x.Id).ToList());
                        }
                        else
                        {
                            var voucherCPIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHPAYMENT") && x.VoucherId != null);
                            var voucherCPId = voucherCPIds.Select(x => x.VoucherId);
                            var accCPPayables = accPayable.Where(x => voucherCPId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accCPPayableNo = accCPPayables.Select(x => x.BillingNo);
                            var edocCPs = edocRepo.Get(x => accCPPayableNo.Contains(x.BillingNo));
                            Acctype.AddRange(edocCPs.Select(x => x.Id).ToList());
                        }
                        //var voucherCPId = accManage.Where(x => x.VoucherType.Replace(" ", "").ToUpper() == "CASHPAYMENT").Select(x => x.VoucherId);
                        //var accCPPayables = accPayable.Where(x => voucherCPId.Contains(x.VoucherNo));
                        //var accCPPayableNo = accCPPayables.Select(x => x.BillingNo);
                        //var edocCPs = edocRepo.Get(x => accCPPayableNo.Contains(x.BillingNo));
                        //Acctype = edocCPs.Select(x => x.Id).ToList();
                        break;
                    case AccountantType.PurchasingNote:
                        if (accManage == null && accPayable == null)
                        {
                            var voucherPNIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("PURCHASINGNOTE") && x.VoucherId != null);
                            var voucherPNId = voucherPNIds.Select(x => x.VoucherId).Distinct();
                            var accPNPayables = await accPayableRepo.WhereAsync(x => voucherPNId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accPNPayableNo = accPNPayables.ToList().Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType }).Distinct();
                            var edocPNs = await edocRepo.GetAsync(x => accPNPayableNo.Select(z => z.billingNo).Contains(x.BillingNo));
                            //var edocOT = accPNPayableNo.Where(x => !edocPNs.Select(y => y.BillingNo).Contains(x.billingNo)).Distinct();
                            //var AccOT = GetListOtherFile(edocOT.ToList());
                            Acctype.AddRange(edocPNs.Select(x => x.Id).ToList());
                        }
                        else
                        {
                            var voucherPNIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("PURCHASINGNOTE") && x.VoucherId != null);
                            var voucherPNId = voucherPNIds.Select(x => x.VoucherId);
                            var accPNPayables = accPayable.Where(x => voucherPNId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accPNPayableNo = accPNPayables.Select(x => x.BillingNo);
                            var edocPNs = edocRepo.Get(x => accPNPayableNo.Contains(x.BillingNo));
                            Acctype.AddRange(edocPNs.Select(x => x.Id).ToList());
                        }
                        break;
                    case AccountantType.CreditSlip:
                        //var voucherCLId = accManage.Where(x => x.VoucherType.Replace(" ", "").ToUpper() == "CREDITSLIP").Select(x => x.VoucherId);
                        //var accCLPayables = accPayable.Where(x => voucherCLId.Contains(x.VoucherNo) && x.BillingNo != null);
                        //var accCLPayableNo = accCLPayables.Select(x => x.BillingNo);
                        //var edocCLs = edocRepo.Get(x => accCLPayableNo.Contains(x.BillingNo));
                        //Acctype = edocCLs.Select(x => x.Id).ToList();
                        //break;
                        if (accManage == null && accPayable == null)
                        {
                            var voucherCLIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CREDITSLIP") && x.VoucherId != null);
                            var voucherCLId = voucherCLIds.Select(x => x.VoucherId).Distinct();
                            var accCLPayables = await accPayableRepo.WhereAsync(x => voucherCLId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accCLPayableNo = accCLPayables.ToList().Select(x => new accType { billingNo = x.BillingNo, billingType = x.BillingType }).Distinct();
                            var edocCLs = await edocRepo.GetAsync(x => accCLPayableNo.Select(z => z.billingNo).Contains(x.BillingNo));
                            //var edocOT = accPNPayableNo.Where(x => !edocPNs.Select(y => y.BillingNo).Contains(x.billingNo)).Distinct();
                            //var AccOT = GetListOtherFile(edocOT.ToList());
                            Acctype.AddRange(edocCLs.Select(x => x.Id).ToList());
                        }
                        else
                        {
                            var voucherCLIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CREDITSLIP") && x.VoucherId != null);
                            var voucherCLId = voucherCLIds.Select(x => x.VoucherId);
                            var accCLPayables = accPayable.Where(x => voucherCLId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accCLPayableNo = accCLPayables.Select(x => x.BillingNo);
                            var edocCLs = edocRepo.Get(x => accCLPayableNo.Contains(x.BillingNo));
                            Acctype.AddRange(edocCLs.Select(x => x.Id).ToList());
                        }
                        break;
                    case AccountantType.CashReceipt:
                        if (accManage == null && accPayable == null)
                        {
                            var voucherRCIds = await accManageRepo.WhereAsync(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHRECEIPT") && x.VoucherId != null);
                            var voucherRCId = voucherRCIds.Select(x => x.VoucherId);
                            var accRCPayables = await accPayableRepo.WhereAsync(x => voucherRCId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accRCPayableNo = accRCPayables.Select(x => new { billingNo = x.BillingNo, billingType = x.BillingType });
                            var edocRCs = await edocRepo.GetAsync(x => accRCPayableNo.Select(z => z.billingNo).Contains(x.BillingNo));
                            Acctype.AddRange(edocRCs.Select(x => x.Id).ToList());
                        }
                        else
                        {
                            var voucherRCIds = accManage.Where(x => x.VoucherType.ToUpper().Replace(" ", "").Contains("CASHRECEIPT") && x.VoucherId != null);
                            var voucherRCId = voucherRCIds.Select(x => x.VoucherId);
                            var accRCPayables = accPayable.Where(x => voucherRCId.Contains(x.VoucherNo) && x.BillingNo != null);
                            var accRCPayableNo = accRCPayables.Select(x => x.BillingNo);
                            var edocRCs = edocRepo.Get(x => accRCPayableNo.Contains(x.BillingNo));
                            Acctype.AddRange(edocRCs.Select(x => x.Id).ToList());
                        }
                        //var voucherRCId = accManage.Where(x => x.VoucherType.Replace(" ", "").ToUpper() == "CASHRECEIPT").Select(x => x.VoucherId);
                        //var accRCPayables = accPayable.Where(x => voucherRCId.Contains(x.VoucherNo));
                        //var accRCPayableNo = accRCPayables.Select(x => x.BillingNo);
                        //var edocRCs = edocRepo.Get(x => accRCPayableNo.Contains(x.BillingNo));
                        //Acctype = edocRCs.Select(x => x.Id).ToList();
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
                criteria.ReferenceNo = criteria.ReferenceNo.Replace(" ", "").Replace("\t", ";").Replace("\n", ";");
                lstRefNo = criteria.ReferenceNo.Split(";").ToList();
                lstRefNo.Remove("");
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
                        //if (!isRefNo)
                        //{
                        //    jobOps = await opsTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        //    jobCs = await csTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        //    lstId1 = jobOps.Select(x => x.Id).ToList();
                        //    lstId2 = jobCs.Select(x => x.Id).ToList();
                        //}
                        //else
                        //{
                        if (opsTranRepo.Any(x => lstRefNo.Contains(x.Mblno)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.Mblno));
                            lstId1 = jobOps.Select(x => x.Id).ToList();
                        }
                        if (csTranRepo.Any(x => lstRefNo.Contains(x.Mawb)))
                        {
                            jobCs = await csTranRepo.WhereAsync(x => lstRefNo.Contains(x.Mawb));
                            lstId2 = jobCs.Select(x => x.Id).ToList();
                        }
                        //}
                        query = query.And(x => lstId1.Concat(lstId2).Contains((Guid)x.JobId));
                        break;
                    case ReferenceType.HouseBill:
                        //if (!isRefNo)
                        //{
                        //    jobOps = await opsTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate);
                        //    jobCsde = await csTranDetailRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate);
                        //    lstId1 = jobOps.Select(x => x.Id).ToList();
                        //    lstId2 = jobCsde.Select(x => x.Id).ToList();
                        //    query = query.And(x => lstId1.Concat(lstId2).Contains((Guid)x.Hblid));
                        //}
                        //else
                        //{
                        if (opsTranRepo.Any(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));
                            lstId1 = jobOps.Select(x => x.Id).ToList();
                            query = query.And(x => lstId1.Contains((Guid)x.JobId));
                        }
                        if (csTranDetailRepo.Any(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            jobCsde = await csTranDetailRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));
                            lstId2 = jobCsde.Select(x => x.JobId).ToList();
                            var hblIds = jobCsde.Select(x => x.Id).ToList();
                            query = query.And(x => lstId2.Contains((Guid)x.JobId) && (x.Hblid == null || hblIds.Contains(x.Id)));

                            //jobCsde = await csTranDetailRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));
                            //jobCsde.GroupBy(x => x.JobId).ToList().ForEach(async x =>
                            //{
                            //    if (x.Select(z => z.Id).Count() == 1)
                            //    {
                            //        query = query.And(z => z.JobId==x.FirstOrDefault().JobId);

                            //        jobCsde.Remove(jobCsde.Where(y => y.JobId == x.FirstOrDefault().JobId).FirstOrDefault());
                            //    }
                            //});
                            //lstId2 = jobCsde.Select(x => x.Id).ToList();
                            //query = query.And(x => lstId2.Contains((Guid)x.Hblid));
                        }
                        //}
                        break;
                    case ReferenceType.JobId:
                        //if (!isRefNo)
                        //{
                        //    jobOps = await opsTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate);
                        //    jobCs = await csTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate);
                        //    lstId1 = jobOps.Select(x => x.Id).ToList();
                        //    lstId2 = jobCs.Select(x => x.Id).ToList();
                        //}
                        //else
                        //{
                        if (opsTranRepo.Any(x => lstRefNo.Contains(x.JobNo)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.JobNo));
                            lstId1 = jobOps.Select(x => x.Id).ToList();
                        }
                        if (csTranRepo.Any(x => lstRefNo.Contains(x.JobNo)))
                        {
                            jobCs = await csTranRepo.WhereAsync(x => lstRefNo.Contains(x.JobNo));
                            lstId2 = jobCs.Select(x => x.Id).ToList();
                        }
                        //}
                        query = query.And(x => lstId1.Concat(lstId2).Contains((Guid)x.JobId));
                        break;
                    case ReferenceType.AccountantNo:
                        //if (!isRefNo)
                        //{
                        //    var accPayable = await accPayableRepo.GetAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate);
                        //    var accPayableNo = accPayable.Select(x => x.BillingNo);
                        //    var edocs = await edocRepo.GetAsync(x => accPayableNo.Contains(x.BillingNo));
                        //    lstId1 = edocs.Select(x => x.Id).ToList();
                        //}
                        //else
                        //{
                        var accPayable = await accPayableRepo.GetAsync(x => lstRefNo.Contains(x.VoucherNo));
                        var accPayableNo = accPayable.Select(x => x.BillingNo);
                        var edocs = await edocRepo.GetAsync(x => accPayableNo.Contains(x.BillingNo));
                        lstId1 = edocs.Select(x => x.Id).ToList();
                        //}
                        query = query.And(x => lstId1.Contains((Guid)x.Id));
                        break;
                    case ReferenceType.InvoiceNo:
                        //if (!isRefNo)
                        //{
                        //    var accPayable = await accPayableRepo.GetAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated <= criteria.ToDate);
                        //    var accPayableNo = accPayable.Select(x => x.BillingNo);
                        //    var edocs = await edocRepo.GetAsync(x => accPayableNo.Contains(x.BillingNo));
                        //    lstId1 = edocs.Select(x => x.Id).ToList();
                        //}
                        //else
                        //{
                        var accPayableIV = await accPayableRepo.GetAsync(x => lstRefNo.Contains(x.InvoiceNo));
                        var accPayableIVNo = accPayableIV.Select(x => x.BillingNo);
                        var edocIVs = await edocRepo.GetAsync(x => accPayableIVNo.Contains(x.BillingNo));
                        lstId1 = edocIVs.Select(x => x.Id).ToList();
                        //}
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
                        var accPayable = await accPayableRepo.GetAsync(x => x.VoucherDate >= criteria.FromDate && x.VoucherDate <= criteria.ToDate.Value.AddDays(1));
                        var billingNo = accPayable.Select(x => x.BillingNo);
                        query = query.And(x => billingNo.Contains(x.BillingNo));
                        break;
                }
            }
            if (criteria.AccountantTypes != null && criteria.AccountantTypes.Count() > 0)
            {
                var accManage = new List<AccAccountingManagement>();
                var accPayable = new List<AccAccountPayable>();
                if (!isRefNo)
                {
                    //accPayable = await accPayableRepo.GetAsync();
                    //var accManageNo = accPayable.Select(x => x.VoucherNo);
                    accPayable = null;
                    accManage = null;
                }
                else
                {
                    if (criteria.ReferenceType == ReferenceType.AccountantNo)
                    {
                        accPayable = await accPayableRepo.GetAsync(x => lstRefNo.Contains(x.VoucherNo));
                    }
                    else if(criteria.ReferenceType == ReferenceType.InvoiceNo)
                    {
                        accPayable = await accPayableRepo.GetAsync(x => lstRefNo.Contains(x.InvoiceNo));
                    }
                    var accManageNo = accPayable.Select(x => x.VoucherNo);
                    accManage = await accManageRepo.GetAsync(x => accManageNo.Contains(x.VoucherId));
                }
                var query1 = await QueryAccType(criteria.AccountantTypes, accManage, accPayable);
                query = query.And(query1);
            }

            return query;
        }
        private class accType
        {
            public string billingType { get; set; }
            public string billingNo { get; set; }
        }

        private List<SysImageModel> GetListOtherFile(List<accType> accType)
        {
            var result = new List<SysImageModel>();
            if (accType.ToList().Count > 0)
            {
                accType.ForEach(item =>
                    {
                        if (item.billingType == "SOA")
                        {
                            var soa = acctSOARepo.Get(x => x.Soano == item.billingNo);
                            var soaId = soa.FirstOrDefault().Id;
                            if (soaId != null)
                            {
                                var image = DataContext.Get(x => x.ObjectId == soaId);
                                var imageMD = _mapper.Map<List<SysImageModel>>(image);
                                imageMD.ForEach(x =>
                                {
                                    x.BillingNo = soa.FirstOrDefault()?.Soano;
                                    x.BillingType = "SOA";
                                });
                                result.AddRange(imageMD);
                            }
                        }
                        if (item.billingType == "SETTLEMENT")
                        {
                            var settle = acctSettleRepo.Get(x => x.SettlementNo == item.billingNo);
                            var smId = settle.FirstOrDefault()?.Id;
                            if (smId != null)
                            {
                                var image = DataContext.Get(x => x.ObjectId == smId.ToString());
                                var imageMD = _mapper.Map<List<SysImageModel>>(image);
                                imageMD.ForEach(x =>
                                {
                                    x.BillingNo = settle.FirstOrDefault()?.SettlementNo;
                                    x.BillingType = "Settlement";
                                });
                                result.AddRange(imageMD);
                            }
                        }
                    });
            }
            return result;
        }
    }
}
