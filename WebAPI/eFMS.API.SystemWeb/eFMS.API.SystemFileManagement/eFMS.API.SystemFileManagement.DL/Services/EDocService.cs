using Amazon.S3.Model;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.API.SystemFileManagement.DL.Common;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class EDocService : IEDocService
    {
        private ICurrentUser currentUser;
        private IS3Service _client;
        private IContextBase<SysImage> _sysImageRepo;
        private IContextBase<SysImageDetail> _sysImageDetailRepo;
        private readonly string _bucketName;
        private readonly IOptions<ApiUrl> _apiUrl;
        private readonly string _domainTest;
        private IContextBase<SysAttachFileTemplate> _attachFileTemplateRepo;
        private IContextBase<OpsTransaction> _opsTranRepo;
        private IContextBase<CsTransactionDetail> _tranDeRepo;
        private IContextBase<CsTransaction> _cstranRepo;
        private IContextBase<AcctSettlementPayment> _setleRepo;
        private IContextBase<AcctAdvancePayment> _advRepo;
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)_sysImageDetailRepo.DC;
        private readonly Dictionary<string, string> PreviewTemplateCodeMappingAttachTemplateCode = new Dictionary<string, string>();
        public EDocService(IContextBase<SysImage> SysImageRepo,
            IContextBase<SysAttachFileTemplate> attachFileTemplateRepo,
            IS3Service client,
            ICurrentUser currentUser,
            IOptions<ApiUrl> apiUrl,
            IContextBase<SysImageDetail> sysImageDetailRepo,
            IContextBase<OpsTransaction> opsTranRepo,
            IContextBase<CsTransaction> cstranRepo,
            IContextBase<AcctSettlementPayment> setleRepo,
            IContextBase<AcctAdvancePayment> advRepo,
        IContextBase<CsTransactionDetail> tranDeRepo)
        {
            this.currentUser = currentUser;
            _domainTest = DbHelper.DbHelper.AWSS3DomainApi;
            _bucketName = DbHelper.DbHelper.AWSS3BucketName;
            _sysImageRepo = SysImageRepo;
            _apiUrl = apiUrl;
            _sysImageDetailRepo = sysImageDetailRepo;
            _client = client;
            _attachFileTemplateRepo = attachFileTemplateRepo;
            _tranDeRepo = tranDeRepo;
            _opsTranRepo = opsTranRepo;
            _cstranRepo = cstranRepo;
            _setleRepo = setleRepo;
            _advRepo = advRepo;
            PreviewTemplateCodeMappingAttachTemplateCode.Add("HBL", "HBL");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("MBL", "MBL");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("DEBIT", "INV");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("CREDIT", "CN");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("INVOICE", "INV");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("MAWB", "MAWB");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("MNF", "MNF");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("POD", "POD");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("AT", "OTH"); // Attach List
            PreviewTemplateCodeMappingAttachTemplateCode.Add("CoverPage", "OTH"); // Coverpage
            PreviewTemplateCodeMappingAttachTemplateCode.Add("PLSheet", "OTH"); // PL
            PreviewTemplateCodeMappingAttachTemplateCode.Add("AN", "AN"); // PL
            PreviewTemplateCodeMappingAttachTemplateCode.Add("DO", "DO"); // PL
        }


        private DateTime? ConvertExpiredDate(int TimeStorage, string TypeStorage)
        {
            DateTime expiredDate = DateTime.Now;
            switch (TypeStorage)
            {
                case "Day":
                    return expiredDate.AddDays(TimeStorage);
                case "Month":
                    return expiredDate.AddMonths(TimeStorage);
                case "Year":
                    return expiredDate.AddYears(TimeStorage);
                default: break;
            }
            return expiredDate;
        }

        private EDocUploadMapModel MapEDocUploadModel(EDocUploadModel model, List<IFormFile> files)
        {
            EDocUploadMapModel edocUploadMapModel = new EDocUploadMapModel();
            edocUploadMapModel.Id = model.Id;
            edocUploadMapModel.ModuleName = model.ModuleName;
            edocUploadMapModel.FolderName = model.FolderName;

            var lstDocMap = new List<EDocFileMap>();

            model.EDocFiles.ForEach(x =>
            {
                var z = new EDocFileMap
                {
                    AliasName = x.AliasName,
                    BillingNo = x.BillingNo,
                    BillingType = x.BillingType,
                    Code = x.Code,
                    FileName = x.FileName,
                    HBL = x.HBL,
                    JobId = x.JobId,
                    TransactionType = x.TransactionType,
                    BillingId = x.BillingId,
                    Note = x.Note,
                    DocumentId = x.DocumentId
                };
                lstDocMap.Add(z);
            });
            lstDocMap.ForEach(x =>
            {
                x.File = files.Where(z => z.FileName == x.FileName).FirstOrDefault();
            });

            edocUploadMapModel.EDocFilesMap = lstDocMap;
            return edocUploadMapModel;
        }
        public async Task<HandleState> PostEDocAsync(EDocUploadModel model, List<IFormFile> files, string type)
        {
            HandleState result = new HandleState();
            try
            {
                var hs = new HandleState();
                var key = "";
                var edocMapModel = MapEDocUploadModel(model, files);
                foreach (var edoc in edocMapModel.EDocFilesMap)
                {
                    List<SysImage> list = new List<SysImage>();
                    List<SysImageDetail> listDetail = new List<SysImageDetail>();

                    string fileName = FileHelper.RenameFileS3(Path.GetFileNameWithoutExtension(FileHelper.BeforeExtention(edoc.File.FileName)));

                    string extension = Path.GetExtension(edoc.File.FileName);
                    key = model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;

                    var putRequest = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = key,
                        InputStream = edoc.File.OpenReadStream(),
                    };

                    PutObjectResponse putObjectResponse = _client.PutObjectAsync(putRequest).Result;
                    if (putObjectResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        string urlImage = _domainTest + "/OpenFile/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
                        if (extension == ".doc")
                        {
                            urlImage = _domainTest + "/DownloadFile/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
                        }
                        var imageID = Guid.NewGuid();
                        var sysImage = new SysImage
                        {
                            Id = imageID,
                            Url = urlImage,
                            Name = fileName + extension,
                            Folder = model.FolderName,
                            ObjectId = model.Id.ToString(),
                            UserCreated = currentUser.UserName,
                            UserModified = currentUser.UserName,
                            DateTimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            ChildId = null,
                            KeyS3 = key
                        };
                        list.Add(sysImage);
                        if (type == "Shipment")
                        {
                            var attachTemplate = _attachFileTemplateRepo.Get(x => x.Id == edoc.DocumentId).FirstOrDefault();
                            var sysImageDetail = new SysImageDetail
                            {
                                Id = Guid.NewGuid(),
                                BillingNo = edoc.BillingNo,
                                BillingType = edoc.BillingType,
                                DatetimeCreated = DateTime.Now,
                                DatetimeModified = DateTime.Now,
                                DepartmentId = currentUser.DepartmentId,
                                ExpiredDate = attachTemplate.StorageTime == null ? null : ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                                GroupId = currentUser.GroupId,
                                Hblid = edoc.HBL,
                                JobId = (Guid)edoc.JobId,
                                OfficeId = currentUser.OfficeID,
                                SystemFileName = edoc.AliasName,
                                UserCreated = currentUser.UserName,
                                UserFileName = fileName,
                                UserModified = currentUser.UserName,
                                DocumentTypeId = attachTemplate.Id,
                                SysImageId = imageID,
                                Source = model.FolderName,
                                Note = edoc.Note
                            };

                            listDetail.Add(sysImageDetail);
                            _sysImageDetailRepo.Add(sysImageDetail, false);
                        }
                        else if (type == "Settlement")
                        {
                            string bilingNo = string.Empty;
                            var models = GetTransactionTypeJobBillingModel(type, edoc.BillingId);

                            if (models.Count > 0)
                            {
                                foreach (var item in models)
                                {
                                    var attachTemplate = GetAttTepmlateByJob(item.TransactionType, item.Code);
                                    var imageDetail = new SysImageDetail
                                    {
                                        SysImageId = imageID,
                                        BillingType = type,
                                        BillingNo = item.BillingNo,
                                        DatetimeCreated = DateTime.Now,
                                        DatetimeModified = DateTime.Now,
                                        Id = Guid.NewGuid(),
                                        JobId = item.JobId,
                                        UserCreated = sysImage.UserCreated,
                                        SystemFileName = sysImage.Name,
                                        UserFileName = sysImage.Name,
                                        UserModified = sysImage.UserCreated,
                                        Source = type,
                                        ExpiredDate = attachTemplate.StorageTime == null ? null : ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                                        DocumentTypeId = attachTemplate.Id,
                                        Note = edoc.Note,
                                        GroupId = currentUser.GroupId,
                                        Hblid = edoc.HBL,
                                    };

                                    _sysImageDetailRepo.Add(imageDetail, false);
                                }
                            }
                        }
                        else
                        {
                            var attachTemplate = _attachFileTemplateRepo.Get(x => x.Id == edoc.DocumentId && x.TransactionType == edoc.TransactionType).FirstOrDefault();
                            string bilingNo = string.Empty;
                            var models = GetTransactionTypeJobBillingModel(type, edoc.BillingId);

                            if (models.Count > 0)
                            {
                                foreach (var item in models)
                                {
                                    var imageDetail = new SysImageDetail
                                    {
                                        SysImageId = imageID,
                                        BillingType = type,
                                        BillingNo = item.BillingNo,
                                        DatetimeCreated = DateTime.Now,
                                        DatetimeModified = DateTime.Now,
                                        Id = Guid.NewGuid(),
                                        JobId = item.JobId,
                                        UserCreated = sysImage.UserCreated,
                                        SystemFileName = "SM" + sysImage.Name,
                                        UserFileName = sysImage.Name,
                                        UserModified = sysImage.UserCreated,
                                        Source = type,
                                        ExpiredDate = attachTemplate.StorageTime == null ? null : ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                                        DocumentTypeId = attachTemplate.Id,
                                        Note = edoc.Note,
                                        GroupId = currentUser.GroupId,
                                    };

                                    _sysImageDetailRepo.Add(imageDetail, false);
                                }
                            }
                        }
                    }
                    _sysImageRepo.Add(list, false);
                    result = _sysImageRepo.SubmitChanges();
                    if (result.Success)
                    {
                        _sysImageDetailRepo.SubmitChanges();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        public SysAttachFileTemplate GetAttTepmlateByJob(string transactionType, string Code)
        {
            return _attachFileTemplateRepo.Get(x => x.Code == Code && x.TransactionType == transactionType).FirstOrDefault();
        }
        public async Task<List<EDocGroupByType>> GetEDocByJob(Guid jobID, string transactionType)
        {
            var lstTran = await _attachFileTemplateRepo.GetAsync(x => x.TransactionType == transactionType || x.Type == SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_ACCOUNTANT);
            var lst = await _sysImageDetailRepo.GetAsync(x => x.JobId == jobID);
            if (lst == null) { return null; }
            var result = new List<EDocGroupByType>();
            lstTran.ForEach(x =>
            {
                var data = new EDocGroupByType()
                {
                    documentType = x
                };
                result.Add(data);
            });
            var lstImageMD = new List<SysImageDetailModel>();
            lst.ToList().ForEach(x =>
            {
                var imageModel = new SysImageDetailModel()
                {
                    BillingNo = x.BillingNo,
                    BillingType = x.BillingNo,
                    DatetimeCreated = x.DatetimeCreated,
                    DatetimeModified = x.DatetimeModified,
                    DepartmentId = x.DepartmentId,
                    DocumentTypeId = x.DocumentTypeId,
                    ExpiredDate = x.ExpiredDate,
                    GroupId = x.GroupId,
                    Hblid = x.Hblid,
                    Id = x.Id,
                    JobId = x.JobId,
                    OfficeId = x.OfficeId,
                    Source = x.Source,
                    SysImageId = x.SysImageId,
                    SystemFileName = x.SystemFileName,
                    UserCreated = x.UserCreated,
                    UserFileName = x.UserFileName,
                    UserModified = x.UserModified,
                    ImageUrl = _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault() != null ? _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault().Url : null,
                    HBLNo = x.Hblid != null ? _attachFileTemplateRepo.Get(z => z.Id == x.DocumentTypeId).FirstOrDefault().TransactionType == "CL" ? _opsTranRepo.Get(y => y.Id == x.Hblid).FirstOrDefault() != null ?
                    _opsTranRepo.Get(y => y.Id == x.Hblid).FirstOrDefault().Hwbno : null :
                    _tranDeRepo.Get(y => y.Id == x.Hblid).FirstOrDefault() != null ? _tranDeRepo.Get(y => y.Id == x.Hblid).FirstOrDefault().Hwbno : null : null,
                    Note = x.Note
                };
                lstImageMD.Add(imageModel);
            });
            lstImageMD.OrderBy(x => x.DatetimeCreated);
            var newImageIds = _sysImageDetailRepo.Get(x => x.JobId == jobID).Select(x => x.SysImageId).ToList();
            var imageExist = _sysImageRepo.Get(x => x.ObjectId == jobID.ToString()).ToList();
            var imageMap = new List<SysImage>();
            foreach (var image in imageExist)
            {
                if (!newImageIds.Any(x => x == image.Id))
                {
                    imageMap.Add(image);
                }
            }
            var otherId = lstTran.Where(x => x.Code == "OTH").FirstOrDefault() != null ? lstTran.Where(x => x.Code == "OTH").FirstOrDefault().Id : 0;
            var listOther = new List<SysImageDetailModel>();
            if (otherId != 0)
            {
                listOther.AddRange(lstImageMD.Where(x => x.DocumentTypeId == otherId));
            }
            if (imageMap.Count > 0)
            {
                imageMap.ForEach(x =>
                {
                    var imagedetail = new SysImageDetailModel
                    {
                        BillingNo = null,
                        BillingType = "Other",
                        DatetimeCreated = x.DateTimeCreated,
                        DatetimeModified = x.DatetimeModified,
                        DepartmentId = currentUser.DepartmentId,
                        DocumentTypeId = GetDocumentType(transactionType),
                        ImageUrl = x.Url,
                        GroupId = currentUser.GroupId,
                        UserCreated = x.UserCreated,
                        UserModified = x.UserModified,
                        SystemFileName = "OTH" + x.Name,
                        JobNo = transactionType != "CL" ? _cstranRepo.Get(y => y.Id == jobID).FirstOrDefault().JobNo : _opsTranRepo.Get(z => z.Id == jobID).FirstOrDefault().JobNo,
                        UserFileName = x.Name,
                        Id = x.Id,
                    };
                    listOther.Add(imagedetail);
                });
            }
            listOther.OrderBy(x => x.DatetimeCreated);
            lstImageMD.GroupBy(x => x.DocumentTypeId).ToList().ForEach(x =>
            {
                if (result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault() != null)
                {
                    result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault().EDocs = x.ToList();
                };
            });
            var resultAcc = result.Where(x => x.documentType.Type == SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_ACCOUNTANT && x.EDocs != null).ToList();
            var resultGen = result.Where(x => x.documentType.Type != SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_ACCOUNTANT).ToList();
            var resultGenEdoc = resultGen.Where(x => x.EDocs != null).ToList().OrderBy(x => x.EDocs.Count());
            var resultGenNotEdoc = resultGen.Where(x => x.EDocs == null);
            resultGen = resultGenEdoc.Concat(resultGenNotEdoc).ToList();
            if (resultGen.Where(x => x.documentType.Code == "OTH").FirstOrDefault() != null)
            {
                resultGen.Where(x => x.documentType.Code == "OTH").FirstOrDefault().EDocs = listOther.Count > 0 ? listOther : new List<SysImageDetailModel>();
            }
            result = resultGen.Concat(resultAcc).ToList();
            return result;
        }
        public EDocGroupByType GetEDocByAccountant(Guid billingId, string transactionType)
        {
            var result = new EDocGroupByType();
            var lstEdoc = new List<SysImageDetailModel>();
            var imageExist = _sysImageRepo.Get(x => x.Folder == transactionType && x.ObjectId == billingId.ToString()).ToList();

            if (transactionType == "Settlement")
            {
                var settle = _setleRepo.Get(z => z.Id == billingId).FirstOrDefault();
                var attachTemplate = _attachFileTemplateRepo.Get(y => y.Code == "SM" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT || y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_ADV_SETTLE).FirstOrDefault();
                var attachTemplateIds = _attachFileTemplateRepo.Get(y => y.Code == "SM" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT || y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_ADV_SETTLE).ToList().Select(x => x.Id);
                result.documentType = attachTemplate;
                var settleEdoc = _sysImageDetailRepo.Get(x => attachTemplateIds.Contains((int)x.DocumentTypeId) && x.BillingNo == settle.SettlementNo).GroupBy(x => x.SysImageId).ToList();
                imageExist.Where(x => !settleEdoc.Any(z => z.FirstOrDefault().SysImageId == x.Id)).ToList().ForEach(x =>
                {
                    var edoc = new SysImageDetailModel()
                    {
                        Id = x.Id,
                        BillingNo = settle.SettlementNo,
                        SystemFileName = "SM" + '_' + x.Name,
                        ImageUrl = x.Url,
                        DatetimeCreated = x.DateTimeCreated,
                        BillingType = transactionType,
                        DatetimeModified = x.DatetimeModified,
                        DepartmentId = currentUser.DepartmentId,
                        DocumentTypeId = attachTemplate.Id,
                        Source = SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT,
                        SysImageId = x.Id,
                        UserCreated = x.UserCreated,
                        UserFileName = x.Name,
                        UserModified = x.UserModified,
                    };
                    lstEdoc.Add(edoc);
                });
                foreach (var x in settleEdoc)
                {
                    var image = _sysImageRepo.Get(z => z.Id == x.FirstOrDefault().SysImageId).FirstOrDefault();
                    var jobDetail = GetJobDetail(x.FirstOrDefault().JobId, x.FirstOrDefault().Hblid, x.FirstOrDefault().DocumentTypeId);
                    var edoc = new SysImageDetailModel()
                    {
                        Id = x.FirstOrDefault().Id,
                        BillingNo = settle.SettlementNo,
                        SystemFileName = x.FirstOrDefault().SystemFileName,
                        ImageUrl = image==null? null:image.Url,
                        DatetimeCreated = x.FirstOrDefault().DatetimeCreated,
                        BillingType = transactionType,
                        DatetimeModified = x.FirstOrDefault().DatetimeModified,
                        DepartmentId = currentUser.DepartmentId,
                        DocumentTypeId = attachTemplate.Id,
                        Source = SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT,
                        SysImageId = x.FirstOrDefault().Id,
                        UserCreated = x.FirstOrDefault().UserCreated,
                        UserFileName = x.FirstOrDefault().UserFileName,
                        UserModified = x.FirstOrDefault().UserModified,
                        Note = x.FirstOrDefault().Note,
                        HBLNo = jobDetail != null ? jobDetail.HBLNo : null,
                        JobNo = jobDetail != null ? jobDetail.JobNo : null,
                        Hblid= jobDetail != null ? jobDetail.HBLId : Guid.Empty,
                        JobId = jobDetail != null ? jobDetail.JobId : Guid.Empty,
                    };
                    lstEdoc.Add(edoc);
                }
                result.EDocs = lstEdoc.OrderBy(x=>x.DatetimeCreated).ToList();
                return result;
            }
            else if (transactionType == "Advance")
            {
                var advance = _advRepo.Get(z => z.Id == billingId).FirstOrDefault();
                var attachTemplate = _attachFileTemplateRepo.Get(y => y.Code == "AD" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE).FirstOrDefault();
                var attachTemplateIds = _attachFileTemplateRepo.Get(y => y.Code == "AD" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE).ToList().Select(x => x.Id);
                result.documentType = attachTemplate;
                var advEDoc = _sysImageDetailRepo.Get(x => attachTemplateIds.Contains((int)x.DocumentTypeId) && x.BillingNo == advance.AdvanceNo).GroupBy(x => x.SysImageId).ToList();
                imageExist.Where(x => !advEDoc.Any(z => z.FirstOrDefault().SysImageId == x.Id)).ToList().ForEach(x =>
                {
                    var edoc = new SysImageDetailModel()
                    {
                        Id = x.Id,
                        BillingNo = advance.AdvanceNo,
                        SystemFileName = "AD" + '_' + x.Name,
                        ImageUrl = x.Url,
                        DatetimeCreated = x.DateTimeCreated,
                        BillingType = transactionType,
                        DatetimeModified = x.DatetimeModified,
                        DepartmentId = currentUser.DepartmentId,
                        DocumentTypeId = attachTemplate.Id,
                        Source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE,
                        SysImageId = x.Id,
                        UserCreated = x.UserCreated,
                        UserFileName = x.Name,
                        UserModified = x.UserModified,
                    };
                    lstEdoc.Add(edoc);
                });
                foreach (var x in advEDoc)
                {
                    var image = _sysImageRepo.Get(z => z.Id == x.FirstOrDefault().SysImageId).FirstOrDefault();
                    var jobDetail = GetJobDetail(x.FirstOrDefault().JobId, x.FirstOrDefault().Hblid, x.FirstOrDefault().DocumentTypeId);
                    var edoc = new SysImageDetailModel()
                    {
                        Id = x.FirstOrDefault().Id,
                        BillingNo = advance.AdvanceNo,
                        SystemFileName = x.FirstOrDefault().SystemFileName,
                        ImageUrl = image.Url,
                        DatetimeCreated = x.FirstOrDefault().DatetimeCreated,
                        BillingType = transactionType,
                        DatetimeModified = x.FirstOrDefault().DatetimeModified,
                        DepartmentId = currentUser.DepartmentId,
                        DocumentTypeId = attachTemplate.Id,
                        Source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE,
                        SysImageId = x.FirstOrDefault().Id,
                        UserCreated = x.FirstOrDefault().UserCreated,
                        UserFileName = x.FirstOrDefault().UserFileName,
                        UserModified = x.FirstOrDefault().UserModified,
                        Note = x.FirstOrDefault().Note,
                        HBLNo = jobDetail != null ? jobDetail.HBLNo : null,
                        JobNo = jobDetail != null ? jobDetail.JobNo : null,
                    };
                    lstEdoc.Add(edoc);
                }
                result.EDocs = lstEdoc.OrderBy(x=>x.DatetimeCreated).ToList();
                return result;
            };
            return result;
        }

        private TransctionTypeJobModel GetJobDetail(Guid? jobId, Guid? hblId, int? documentId)
        {
            var typeJob = _attachFileTemplateRepo.Get(x => x.Id == documentId).FirstOrDefault();
            if (typeJob.TransactionType == "CL")
            {
                var JobOPS = hblId == Guid.Empty || hblId == null ? _opsTranRepo.Get(x => x.Id == jobId) : _opsTranRepo.Get(x => x.Hblid == hblId);
                if (JobOPS.Count() == 0)
                {
                    return null;
                }
                return new TransctionTypeJobModel() { HBLNo = JobOPS.FirstOrDefault().Hwbno, JobNo = JobOPS.FirstOrDefault().JobNo, JobId=JobOPS.FirstOrDefault().Id,HBLId=JobOPS.FirstOrDefault().Hblid };
            }
            else
            {
                var JobCS = hblId == Guid.Empty || hblId == null ? _tranDeRepo.Get(x => x.JobId == jobId) : _tranDeRepo.Get(x => x.Id == hblId);
                if (JobCS.Count() == 0)
                {
                    return null;
                }
                var cs = _cstranRepo.Get(x => x.Id == JobCS.FirstOrDefault().JobId);
                return new TransctionTypeJobModel() { HBLNo = JobCS.FirstOrDefault().Hwbno, JobNo = cs.FirstOrDefault().JobNo,JobId=cs.FirstOrDefault().Id,HBLId=JobCS.FirstOrDefault().Id };
            }
        }

        private int GetDocumentType(string transationType)
        {
            return _attachFileTemplateRepo.Get(x => x.TransactionType == transationType && x.Code == "OTH").FirstOrDefault().Id;
        }

        public async Task<HandleState> DeleteEdoc(Guid edocId)
        {
            HandleState result = new HandleState();
            try
            {
                var edoc = _sysImageDetailRepo.Get(x => x.Id == edocId).FirstOrDefault();
                var lst = new List<SysImage>();
                if (edoc != null)
                {
                    lst = await _sysImageRepo.GetAsync(x => x.Id == edoc.SysImageId);
                }
                else{
                    lst = await _sysImageRepo.GetAsync(x => x.Id == edocId);

                    edoc = new SysImageDetail()
                    {
                        Id = Guid.Empty,
                        Source = "Shipment"
                    };
                }

                if (edoc.Source == "Shipment")
                {
                    var it = lst.FirstOrDefault();
                    if (it == null) { return new HandleState("Not found data"); }
                    var key = "Document/Shipment/" + it.ObjectId + "/" + it.Name;
                    DeleteObjectRequest request = new DeleteObjectRequest
                    {
                        BucketName = _bucketName,
                        Key = it.KeyS3,
                    };

                    DeleteObjectResponse rsDelete = _client.DeleteObjectAsync(request).Result;
                    if (rsDelete != null)
                        if (edoc.Id != Guid.Empty)
                        {
                            result = await _sysImageRepo.DeleteAsync(x => x.Id == edoc.SysImageId);
                        }
                        else
                        {
                            result = await _sysImageRepo.DeleteAsync(x => x.Id == edocId);
                        }
                    if (result.Success && edoc.Id != Guid.Empty)
                    {
                        await _sysImageDetailRepo.DeleteAsync(x => x.Id == edocId);
                    }
                }
                else
                {
                    if (edoc != null)
                    {
                        await _sysImageDetailRepo.DeleteAsync(x => x.Id == edocId);
                    }
                    await _sysImageRepo.DeleteAsync(x => x.Folder == edoc.Source && x.Id == edoc.SysImageId);
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        public async Task<HandleState> UpdateEDoc(SysImageDetailModel edocUpdate)
        {
            var edoc = _sysImageDetailRepo.Get(x => x.Id == edocUpdate.Id).FirstOrDefault();
            try
            {
                if (edoc != null)
                {
                    edoc.SystemFileName = edocUpdate.SystemFileName;
                    edoc.Hblid = edocUpdate.Hblid;
                    edoc.Note = edocUpdate.Note;
                    edoc.DocumentTypeId = edocUpdate.DocumentTypeId;
                    edoc.JobId = edocUpdate.JobId;    
                    var hs = await _sysImageDetailRepo.UpdateAsync(edoc, x => x.Id == edoc.Id, false);
                }
                if (edoc == null)
                {
                    var image = _sysImageRepo.Get(x => x.Id == edocUpdate.Id).FirstOrDefault();
                    var edocGenAdd = new SysImageDetail()
                    {
                        DocumentTypeId = edocUpdate.DocumentTypeId,
                        SystemFileName = edocUpdate.SystemFileName,
                        Hblid = edocUpdate.Hblid,
                        Note = edocUpdate.Note,
                        UserFileName = image.Name,
                        UserCreated = currentUser.UserName,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        DepartmentId = currentUser.DepartmentId,
                        Id = edocUpdate.Id,
                        UserModified = currentUser.UserName,
                        GroupId = currentUser.GroupId,
                        OfficeId = currentUser.OfficeID,
                        SysImageId = image.Id,
                        JobId = Guid.Parse(image.ObjectId),
                        Source = "Shipment",
                    };
                    var hs = await _sysImageDetailRepo.AddAsync(edocGenAdd, false);
                }
                var result = _sysImageDetailRepo.SubmitChanges();
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        private List<TransctionTypeJobModel> GetTransactionTypeJobBillingModel(string billingType, string billingId)
        {
            string bilingNo = string.Empty;
            var transctionTypeJobModels = new List<TransctionTypeJobModel>();

            switch (billingType)
            {
                case "Advance":
                    var adv = DC.AcctAdvancePayment.FirstOrDefault(x => x.Id.ToString() == billingId);
                    bilingNo = adv.AdvanceNo;
                    if (adv != null)
                    {
                        var advRquest = DC.AcctAdvanceRequest.Where(x => x.AdvanceNo == bilingNo);
                        var grpJobsAdv = advRquest.GroupBy(x => x.JobId).Select(x => x.Key).ToList();
                        foreach (var item in grpJobsAdv)
                        {
                            if (item.Contains("LOG"))
                            {
                                var opsJob = DC.OpsTransaction.FirstOrDefault(x => x.JobNo == item);
                                if (opsJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = "CL", BillingNo = bilingNo, Code = "AD" });
                                }
                            }
                            else
                            {
                                var csJob = DC.CsTransaction.FirstOrDefault(x => x.JobNo == item);
                                if (csJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType, BillingNo = bilingNo, Code = "AD" });
                                }
                            }
                        }
                    }
                    break;
                case "Settlement":
                    bilingNo = DC.AcctSettlementPayment.FirstOrDefault(x => x.Id.ToString() == billingId).SettlementNo;
                    var grpJobsSm = DC.CsShipmentSurcharge.Where(x => x.SettlementCode == bilingNo).GroupBy(x => x.JobNo).Select(x => x.Key);
                    foreach (var item in grpJobsSm)
                    {
                        if (item.Contains("LOG"))
                        {
                            var opsJob = DC.OpsTransaction.FirstOrDefault(x => x.JobNo == item);
                            if (opsJob != null)
                            {
                                transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = "CL", BillingNo = bilingNo, Code = "SM" });
                            }
                        }
                        else
                        {
                            var csJob = DC.CsTransaction.FirstOrDefault(x => x.JobNo == item);
                            if (csJob != null)
                            {
                                transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType, BillingNo = bilingNo, Code = "SM" });
                            }
                        }
                    }
                    break;
                case "SOA":
                    var soa = DC.AcctSoa.FirstOrDefault(x => x.Id.ToString() == billingId);
                    if (soa != null)
                    {
                        bilingNo = soa.Soano;
                        var surchargeSoa = Enumerable.Empty<CsShipmentSurcharge>().AsQueryable();
                        if (soa.Type == "Debit")
                        {
                            surchargeSoa = DC.CsShipmentSurcharge.Where(x => x.Soano == bilingNo);
                        }
                        else
                        {
                            surchargeSoa = DC.CsShipmentSurcharge.Where(x => x.PaySoano == bilingNo);
                        }

                        var grpJobsSOA = surchargeSoa.GroupBy(x => x.JobNo).Select(x => x.Key);
                        foreach (var item in grpJobsSOA)
                        {
                            if (item.Contains("LOG"))
                            {
                                var opsJob = DC.OpsTransaction.FirstOrDefault(x => x.JobNo == item);
                                if (opsJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = "CL", BillingNo = bilingNo, Code = "SOA" });
                                }
                            }
                            else
                            {
                                var csJob = DC.CsTransaction.FirstOrDefault(x => x.JobNo == item);
                                if (csJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType, BillingNo = bilingNo, Code = "SOA" });
                                }
                            }
                        }
                    }
                    break;
                case "Shipment":
                    break;
                default:
                    break;
            }
            return transctionTypeJobModels;
        }

        public async Task<HandleState> MappingeDocToShipment(Guid imageId, string billingId, string billingType)
        {
            HandleState result = new HandleState();
            string bilingNo = string.Empty;

            var models = GetTransactionTypeJobBillingModel(billingType, billingId);
            var listAccountantTypes = new List<string> {
                SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_ADVANCE,
                SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT,
                SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SOA,
            };
            string type = string.Empty;
            if (listAccountantTypes.Contains(billingType))
            {
                type = SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_ACCOUNTANT;
            }
            else
            {
                type = SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_GENERAL;
            }
            if (models.Count > 0)
            {
                var sysImage = _sysImageRepo.Get(x => x.Id == imageId)?.FirstOrDefault();
                foreach (var item in models)
                {
                    var imageDetail = new SysImageDetail
                    {
                        SysImageId = imageId,
                        BillingType = billingType,
                        BillingNo = item.BillingNo,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        Id = Guid.NewGuid(),
                        JobId = item.JobId,
                        UserCreated = sysImage.UserCreated,
                        SystemFileName = sysImage.Name,
                        UserFileName = sysImage.Name,
                        UserModified = sysImage.UserCreated,
                        Source = billingType,
                        DocumentTypeId = GetDocumentTypeWithTypeAttachTemplate(type, item.TransactionType, item.Code, billingType)?.FirstOrDefault()?.Id
                    };

                    await _sysImageDetailRepo.AddAsync(imageDetail, false);
                }

                result = _sysImageDetailRepo.SubmitChanges();
            }
            return result;
        }
        private IQueryable<SysAttachFileTemplate> GetDocumentTypeWithTypeAttachTemplate(string type, string transactionType, string code, string accountingType)
        {
            Expression<Func<SysAttachFileTemplate, bool>> queryAttachTemplate = x => x.Type == type && x.TransactionType == transactionType;
            if (!string.IsNullOrEmpty(code))
            {
                queryAttachTemplate = queryAttachTemplate.And(x => x.Code == code);
            }
            if (!string.IsNullOrEmpty(accountingType))
            {
                queryAttachTemplate = queryAttachTemplate.And(x => x.AccountingType == accountingType);
            }
            var template = _attachFileTemplateRepo.Get(queryAttachTemplate);

            return template;
        }

        private async Task<List<SysImage>> UpLoadS3(FileUploadModel model)
        {
            var urlImage = "";
            List<SysImage> list = new List<SysImage>();
            foreach (var file in model.Files)
            {
                string fileName = FileHelper.RenameFileS3(Path.GetFileNameWithoutExtension(FileHelper.BeforeExtention(file.FileName)));
                var key = "";

                string extension = Path.GetExtension(file.FileName);
                key = model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;

                var putRequest = new PutObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = key,
                    InputStream = file.OpenReadStream(),
                };

                PutObjectResponse putObjectResponse = await _client.PutObjectAsync(putRequest);
                if (putObjectResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    urlImage = _domainTest + "/OpenFile/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
                    if (extension == ".doc")
                    {
                        urlImage = _domainTest + "/DownloadFile/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
                    }
                    var sysImage = new SysImage
                    {
                        Id = Guid.NewGuid(),
                        Url = urlImage,
                        Name = fileName + extension,
                        Folder = model.FolderName,
                        ObjectId = model.Id.ToString(),
                        UserCreated = currentUser.UserName,
                        UserModified = currentUser.UserName,
                        DateTimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        ChildId = model.Child,
                        KeyS3 = key
                    };
                    list.Add(sysImage);
                }
            }

            return list;
        }

        public async Task<HandleState> PostFileAttacheDoc(FileUploadModel model)
        {
            try
            {
                List<SysImage> imageList = await UpLoadS3(model);
                HandleState result = new HandleState();
                if (imageList.Count > 0)
                {
                    HandleState hsAddImage = await _sysImageRepo.AddAsync(imageList);

                    if (hsAddImage.Success)
                    {
                        foreach (var image in imageList)
                        {
                            result = await MappingeDocToShipment(image.Id, image.ObjectId, image.Folder);
                        }
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<string> PostAttachFileTemplateToEDoc(FileUploadModel model)
        {
            var urlImage = "";
            List<SysImage> imageList = await UpLoadS3(model);
            urlImage = imageList.FirstOrDefault()?.Url;
            if (imageList.Count > 0)
            {
                HandleState hsAddImage = await _sysImageRepo.AddAsync(imageList);
                if (hsAddImage.Success)
                {
                    foreach (var image in imageList)
                    {
                        HandleState result = await MappingeDocToShipment(image.Id, image.ObjectId, image.Folder);
                    }
                }
            }
            return urlImage;
        }

        public async Task<HandleState> AttachPreviewTemplate(List<EDocAttachPreviewTemplateUploadModel> models)
        {
            HandleState result = new HandleState();
            try
            {
                foreach (var model in models)
                {

                    byte[] filesArrayBuffer = await FileHelper.DownloadFile(model.Url);
                    if (filesArrayBuffer == null)
                    {
                        return new HandleState((object)"Not found files");
                    }
                    FileReportUpload fileUpload = new FileReportUpload
                    {
                        FileName = Path.GetFileName(model.Url),
                        FileContent = filesArrayBuffer
                    };

                    var stream = new MemoryStream(fileUpload.FileContent);
                    List<IFormFile> fFiles = new List<IFormFile>() { new FormFile(stream, 0, stream.Length, null, fileUpload.FileName) };
                    FileUploadModel UploadModel = new FileUploadModel
                    {
                        Files = fFiles,
                        FolderName = model.Folder,
                        Id = model.ObjectId,
                        ModuleName = model.Module
                    };
                    List<SysImage> imageList = await UpLoadS3(UploadModel);

                    if (imageList.Count > 0)
                    {
                        HandleState hsAddImage = await _sysImageRepo.AddAsync(imageList);

                        if (hsAddImage.Success)
                        {
                            foreach (var image in imageList)
                            {
                                result = await MappingPreviewTemplateToShipment(model, image);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new HandleState((object)ex.ToString());
            }
        }

        private async Task<HandleState> MappingPreviewTemplateToShipment(EDocAttachPreviewTemplateUploadModel model, SysImage image)
        {
            HandleState result = new HandleState();
            try
            {
                string codeMapping = PreviewTemplateCodeMappingAttachTemplateCode[model.TemplateCode];
                string code = string.IsNullOrEmpty(codeMapping) ? "OTH" : codeMapping;
                var imageDetail = new SysImageDetail
                {
                    Id = Guid.NewGuid(),
                    DatetimeCreated = DateTime.Now,
                    DatetimeModified = DateTime.Now,
                    UserCreated = currentUser.UserName,
                    UserModified = currentUser.UserName,
                    DepartmentId = currentUser.DepartmentId,
                    OfficeId = currentUser.OfficeID,
                    GroupId = currentUser.GroupId,
                    Hblid = model.HblId,
                    JobId = model.ObjectId,
                    Source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_SHIPMENT,
                    SysImageId = image.Id,
                    UserFileName = Path.GetFileNameWithoutExtension(image.Name),
                    SystemFileName = Path.GetFileNameWithoutExtension(image.Name),
                    DocumentTypeId = _attachFileTemplateRepo.Get(x => x.Code == code && x.TransactionType == model.TransactionType)?.FirstOrDefault().Id
                };
                result = await _sysImageDetailRepo.AddAsync(imageDetail);

                return result;
            }
            catch (Exception ex)
            {
                return new HandleState((object)ex.ToString());
            }
        }
    }
}
