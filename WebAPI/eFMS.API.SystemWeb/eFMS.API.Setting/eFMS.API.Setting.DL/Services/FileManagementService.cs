﻿using AutoMapper;
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

        public FileManagementService(IStringLocalizer<LanguageSub> localizer, IContextBase<SysAttachFileTemplate> docTypeRepository, IContextBase<CsShipmentSurcharge> surchargeRepository, IContextBase<AccAccountingManagement> accManageRepository, IContextBase<CsTransactionDetail> csTranDetailRepoitory, IContextBase<OpsTransaction> opsTranRepository, IContextBase<SysImage> repository, IContextBase<CsTransaction> csTran, IContextBase<AcctSoa> acctSOA, IContextBase<SysImageDetail> EDoc, IContextBase<AcctSettlementPayment> accSettle, IContextBase<AcctAdvancePayment> acctAdvance, IMapper mapper, ICurrentUser user) : base(repository, mapper)
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
                                           FolderName = "SOA"+soa.Soano,
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

        public async Task<List<EDocFile>> GetEdocManagement(EDocManagementCriterial criterial)
        {
            var data = await edocRepo.WhereAsync(await ExpressionQuery(criterial));
            List<EDocFile> eDocFiles = new List<EDocFile>();
            data.ForEach(x =>
            {
                var edocFile = MappingEDocFile(x);
                eDocFiles.Add(edocFile.Result);
            });
            var result = eDocFiles.Take(criterial.Size).Skip(criterial.Page * criterial.Size).ToList();
            return result;
        }

        private string getFileNameWithWxtention(Guid? sysImageId)
        {
            return "OTH" + DataContext.Get(x => x.Id == sysImageId).FirstOrDefault().Name;
        }

        private async Task<EDocFile> MappingEDocFile(SysImageDetail imageDetail)
        {
            var docType = await docTypeRepo.GetAsync(x => x.Id == imageDetail.DocumentTypeId);
            var edocFile = _mapper.Map<EDocFile>(imageDetail);
            edocFile.DocumentType = docType.FirstOrDefault().NameEn;
            if (docType.FirstOrDefault().TransactionType == "CL")
            {
                var jobOps = await opsTranRepo.GetAsync(x => x.Id == edocFile.JobId);
                edocFile.JobRef = jobOps.FirstOrDefault().JobNo;
                edocFile.HBLNo = imageDetail.Hblid!=null&&imageDetail.Hblid!=Guid.Empty? jobOps.Where(x => x.Hblid == imageDetail.Hblid).FirstOrDefault().Hwbno:null;
                edocFile.Type = docType.FirstOrDefault().Type;
                if (imageDetail.SystemFileName.Substring(0, 3) == "OTH")
                {
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId);
                }
            }
            else
            {
                var jobCS = await csTranRepo.GetAsync(x => x.Id == edocFile.JobId);
                edocFile.JobRef = jobCS.FirstOrDefault().JobNo;
                var detailCs = await csTranDetailRepo.GetAsync(x => x.Id == edocFile.Hblid);
                edocFile.HBLNo = imageDetail.Hblid != null && imageDetail.Hblid != Guid.Empty ? detailCs.FirstOrDefault().Hwbno:null;
                edocFile.Type = docType.FirstOrDefault().Type;
                if (imageDetail.SystemFileName.Substring(0, 3) == "OTH")
                {
                    edocFile.UserFileName = getFileNameWithWxtention(edocFile.SysImageId);
                }
            }

            return edocFile;
        }
        private async Task<Expression<Func<SysImageDetail, bool>>> ExpressionQuery(EDocManagementCriterial criteria)
        {
            Expression<Func<SysImageDetail, bool>> query = q => true;
            var lstRefNo = new List<string>();
            var isRefNo = criteria.ReferenceNo != null;
            if (isRefNo)
            {
                lstRefNo = criteria.ReferenceNo.Split("\n").ToList();
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
                query = query.And(x => x.Source != "Shipment");
            }
            switch (criteria.ReferenceType)
            {
                case ReferenceType.MasterBill:
                    if (!isRefNo)
                    {
                        jobOps = await opsTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        jobCs = await csTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        lstId1 = jobOps.Select(x => x.Id).ToList();
                        lstId2 = jobCs.Select(x => x.Id).ToList();
                    }
                    else
                    {
                        if (await opsTranRepo.AnyAsync(x => lstRefNo.Contains(x.Mblno)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.Mblno));
                            lstId1 = jobOps.Select(x => x.Id).ToList();
                        }
                        if (await csTranRepo.AnyAsync(x => lstRefNo.Contains(x.Mawb)))
                        {
                            jobCs = await csTranRepo.WhereAsync(x => lstRefNo.Contains(x.Mawb));
                            lstId2 = jobCs.Select(x => x.Id).ToList();
                        }
                    }
                    query = query.And(x => lstId1.Concat(lstId2).Contains((Guid)x.JobId));
                    break;
                case ReferenceType.HouseBill:
                    if (!isRefNo)
                    {
                        jobOps = await opsTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        jobCsde = await csTranDetailRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        lstId1 = jobOps.Select(x => x.Id).ToList();
                        lstId2 = jobCsde.Select(x => x.Id).ToList();
                    }
                    else
                    {
                        if (await opsTranRepo.AnyAsync(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            jobOps = await opsTranRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));
                            lstId1 = jobOps.Select(x => x.Hblid).ToList();
                        }
                        if (await csTranDetailRepo.AnyAsync(x => lstRefNo.Contains(x.Hwbno)))
                        {
                            jobCsde = await csTranDetailRepo.WhereAsync(x => lstRefNo.Contains(x.Hwbno));
                            lstId2 = jobCsde.Select(x => x.Id).ToList();
                        }
                    }
                    query = query.And(x => lstId1.Concat(lstId2).Contains((Guid)x.Hblid));
                    break;
                case ReferenceType.JobId:
                    if (!isRefNo)
                    {
                        jobOps = await opsTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        jobCs = await csTranRepo.WhereAsync(x => x.DatetimeCreated >= criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        lstId1 = jobOps.Select(x => x.Id).ToList();
                        lstId2 = jobCs.Select(x => x.Id).ToList();
                    }
                    else
                    {
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
                    }
                    query = query.And(x => lstId1.Concat(lstId2).Contains((Guid)x.JobId));
                    break;
                case ReferenceType.AccountantNo:
                    if (!isRefNo)
                    {
                        accManages = await accManageRepo.GetAsync(x => x.DatetimeCreated>=criteria.FromDate && x.DatetimeCreated<=criteria.ToDate);
                        surCharges = await surchargeRepo.GetAsync(x => x.AcctManagementId == accManages.FirstOrDefault().Id);
                        lstId1 = surCharges.Select(x => x.Hblid).ToList();
                    }
                    else
                    {
                        accManages = await accManageRepo.GetAsync(x => lstRefNo.Contains(x.VoucherId));
                        surCharges = await surchargeRepo.GetAsync(x => x.AcctManagementId == accManages.FirstOrDefault().Id);
                        lstId1 = surCharges.Select(x => x.Hblid).ToList();
                    }
                    query = query.And(x => lstId1.Contains((Guid)x.Hblid));
                    break;
            }
            

            if (criteria.FromDate.HasValue && criteria.ToDate.HasValue)
            {
                switch (criteria.DateMode)
                {
                    case DateMode.CreateDate:
                        query=query.And(x=>x.DatetimeCreated>=criteria.FromDate.Value&&x.DatetimeCreated<=criteria.ToDate);
                        break;
                    case DateMode.AccountingDate:
                        var accManage = await accManageRepo.GetAsync(x => lstRefNo.Contains(x.VoucherId));
                        query = query.And(x => accManage.FirstOrDefault().Date >= criteria.FromDate.Value && accManage.FirstOrDefault().Date <= criteria.ToDate);
                        break;
                }
            }
            if (criteria.AccountantTypes != null&&criteria.AccountantTypes.Count()>0)
            {
                var accManage = await accManageRepo.GetAsync(x => lstRefNo.Contains(x.VoucherId));
                //var accType = accManage.Select(x => x.VoucherType);
                criteria.AccountantTypes.ForEach(z =>
                {
                    switch (z)
                    {
                        case AccountantType.OtherEntry:
                            var accOEId = accManage.Where(x => x.VoucherType.Trim().ToUpper() == "OTHERENTRY").Select(x => x.Id);
                            query = query.And(x => accOEId.Contains((Guid)x.JobId));
                            break;
                        case AccountantType.CashPayment:
                            var accCPId = accManage.Where(x => x.VoucherType.Trim().ToUpper() == "CASHPAYMENT").Select(x => x.Id);
                            query = query.And(x => accCPId.Contains((Guid)x.JobId));
                            break;
                        case AccountantType.PurchasingNote:
                            var accPNId = accManage.Where(x => x.VoucherType.Trim().ToUpper() == "PURCHASINGNOTE").Select(x => x.Id);
                            query = query.And(x => accPNId.Contains((Guid)x.JobId));
                            break;
                        case AccountantType.CreditSlip:
                            var accCLId = accManage.Where(x => x.VoucherType.Trim().ToUpper() == "CREDITSLIP").Select(x => x.Id);
                            query = query.And(x => accCLId.Contains((Guid)x.JobId));
                            break;
                        case AccountantType.CashReceipt:
                            var accRCId = accManage.Where(x => x.VoucherType.Trim().ToUpper() == "CASHRECEIPT").Select(x => x.Id);
                            query = query.And(x => accRCId.Contains((Guid)x.JobId));
                            break;
                    }
                });
            }
          
            return query;
        }
    }
}
