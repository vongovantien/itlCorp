using Amazon.Auth.AccessControlPolicy;
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
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Options;
using MongoDB.Driver.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using static eFMS.API.Common.Helpers.FileHelper;
using static System.Net.Mime.MediaTypeNames;

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
        private IContextBase<AcctSoa> _soaRepo;
        private IContextBase<SysUser> _userRepo;
        private IContextBase<CsShipmentSurcharge> _surRepo;
        private IContextBase<SysOffice> _officeRepo;
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
            IContextBase<SysUser> userRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<CsShipmentSurcharge> surRepo,
            IContextBase<SysOffice> officeRepo,
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
            _userRepo = userRepo;
            _soaRepo = soaRepo;
            _surRepo = surRepo;
            _officeRepo = officeRepo;
            PreviewTemplateCodeMappingAttachTemplateCode.Add("HBL", "HBL");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("MBL", "MBL");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("DEBIT", "INV");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("CREDIT", "CN");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("INVOICE", "INV");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("MAWB", "MAWB");
            PreviewTemplateCodeMappingAttachTemplateCode.Add("MNF", "MNF");
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
                    DocumentId = x.DocumentId,
                    AccountingType = x.AccountingType
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
                    List<SysImageDetail> listRepDetail = new List<SysImageDetail>();

                    string fileName = RenameFileS3(Path.GetFileNameWithoutExtension(BeforeExtention(edoc.File.FileName)));

                    string extension = Path.GetExtension(edoc.File.FileName);
                    key = model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;

                    var putRequest = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = key,
                        InputStream = edoc.File.OpenReadStream(),
                    };

                    PutObjectResponse putObjectResponse = await _client.PutObjectAsync(putRequest);
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
                            var attachTemplateSrc = new List<SysAttachFileTemplate>();
                            if (edoc.DocumentId == 0)
                            {
                                attachTemplateSrc = await _attachFileTemplateRepo.GetAsync(x => x.Code == edoc.Code && x.TransactionType == edoc.TransactionType);
                            }
                            else
                            {
                                attachTemplateSrc = await _attachFileTemplateRepo.GetAsync(x => x.Id == edoc.DocumentId);
                            }

                            var attachTemplate = attachTemplateSrc.FirstOrDefault();

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
                                SystemFileName = attachTemplate.Code + clearPrefix(null, edoc.AliasName),
                                UserCreated = currentUser.UserName,
                                UserFileName = fileName,
                                UserModified = currentUser.UserName,
                                DocumentTypeId = attachTemplate.Id,
                                SysImageId = imageID,
                                Source = model.FolderName,
                                Note = edoc.Note
                            };
                            listDetail.Add(sysImageDetail);
                            listRepDetail.Add(sysImageDetail);
                            var imageDetailClone = sysImageDetail;
                            if (_opsTranRepo.Any(x => x.Id == edoc.JobId && x.CurrentStatus != "Canceled"))
                            {
                                var imageDetailClone1 = sysImageDetail;
                                var mapRepToITL1 = await MapToJobITL(imageDetailClone1);
                                await _sysImageDetailRepo.AddAsync(mapRepToITL1, false);
                            }

                            await _sysImageDetailRepo.AddAsync(sysImageDetail, false);
                        }
                        else if (type == "Settlement")
                        {
                            string bilingNo = string.Empty;
                            var models = new List<TransctionTypeJobModel>();
                            var tranType = _attachFileTemplateRepo.Get(x => x.Id == edoc.DocumentId).FirstOrDefault()?.TransactionType;
                            if (edoc.JobId != null && edoc.JobId != Guid.Empty)
                            {
                                models = GetTransactionTypeJobBillingModel(type, edoc.BillingId).Where(x => x.JobId == edoc.JobId).ToList();
                            }
                            else
                            {
                                models = GetTransactionTypeJobBillingModel(type, edoc.BillingId);
                            }
                            foreach (var item in models)
                            {
                                var attachTemplate = GetAttTepmlateByJob(edoc.Code, edoc.DocumentId, item.TransactionType, edoc.AccountingType);
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
                                    SystemFileName = attachTemplate.Code + clearPrefix(null, edoc.AliasName),
                                    UserFileName = sysImage.Name,
                                    UserModified = sysImage.UserCreated,
                                    Source = type,
                                    ExpiredDate = attachTemplate.StorageTime == null ? null : ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                                    DocumentTypeId = attachTemplate.Id,
                                    Note = edoc.Note,
                                    GroupId = currentUser.GroupId,
                                    Hblid = item.HBLId,
                                };

                                await _sysImageDetailRepo.AddAsync(imageDetail, false);
                                if (_opsTranRepo.Any(x => x.Id == item.JobId && x.CurrentStatus != "Canceled"))
                                {
                                    var imageDetailClone = imageDetail;
                                    var mapRepToITL = await MapToJobITL(imageDetailClone);
                                    await _sysImageDetailRepo.AddAsync(mapRepToITL, false);
                                }
                            }
                        }
                        else
                        {
                            string bilingNo = string.Empty;
                            var models = new List<TransctionTypeJobModel>();
                            if (edoc.JobId != null && edoc.JobId != Guid.Empty)
                            {
                                models = GetTransactionTypeJobBillingModel(type, edoc.BillingId).Where(x => x.JobId == edoc.JobId).ToList();
                            }
                            else
                            {
                                models = GetTransactionTypeJobBillingModel(type, edoc.BillingId);
                            }

                            if (models.Count > 0)
                            {
                                foreach (var item in models)
                                {
                                    var attachTemplateSrc = await _attachFileTemplateRepo.GetAsync(x => x.TransactionType == item.TransactionType && x.Code == edoc.Code);
                                    var attachTemplate = attachTemplateSrc.FirstOrDefault();
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
                                        SystemFileName = attachTemplate.Code + clearPrefix(null, edoc.AliasName),
                                        UserFileName = sysImage.Name,
                                        UserModified = sysImage.UserCreated,
                                        Source = type,
                                        ExpiredDate = attachTemplate.StorageTime == null ? null : ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                                        DocumentTypeId = attachTemplate.Id,
                                        Note = edoc.Note,
                                        GroupId = currentUser.GroupId,
                                    };

                                    await _sysImageDetailRepo.AddAsync(imageDetail, false);
                                    if (_opsTranRepo.Any(x => x.Id == item.JobId && x.CurrentStatus != "Canceled"))
                                    {
                                        var imageDetailClone = imageDetail;
                                        var mapRepToITL = await MapToJobITL(imageDetailClone);
                                        await _sysImageDetailRepo.AddAsync(mapRepToITL, false);
                                    }
                                }
                            }
                        }
                    }
                    await _sysImageRepo.AddAsync(list, false);
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
                return (new HandleState(false, ex.Message));
                throw ex;
            }
        }

        private async Task<SysImageDetail> MapToJobITL(SysImageDetail edoc)
        {
            var isRep = await _opsTranRepo.AnyAsync(x => x.ReplicatedId == edoc.JobId && x.CurrentStatus != "Canceled");
            var jobITL = _opsTranRepo.WhereAsync(x => x.ReplicatedId == edoc.JobId).Result;
            if (isRep && jobITL != null)
            {
                return new SysImageDetail()
                {
                    BillingNo = edoc.BillingNo,
                    BillingType = edoc.BillingType,
                    JobId = jobITL.FirstOrDefault().Id,
                    Id = Guid.NewGuid(),
                    DatetimeCreated = edoc.DatetimeCreated,
                    DatetimeModified = edoc.DatetimeModified,
                    DepartmentId = edoc.DepartmentId,
                    DocumentTypeId = edoc.DocumentTypeId,
                    ExpiredDate = edoc.ExpiredDate,
                    GenEdocId = edoc.GenEdocId,
                    GroupId = edoc.GroupId,
                    Hblid = edoc.Hblid,
                    Note = edoc.Note,
                    OfficeId = edoc.OfficeId,
                    Source = edoc.Source,
                    SysImageId = edoc.SysImageId,
                    SystemFileName = edoc.SystemFileName,
                    UserCreated = edoc.UserCreated,
                    UserFileName = edoc.UserFileName,
                    UserModified = edoc.UserModified,
                };
            }
            return null;
        }

        private List<SysImageDetailModel> getEDocSM(string settleNo, string transactionType)
        {
            var edosExisted = new List<IGrouping<Guid?, SysImageDetail>>();
            if (_officeRepo.Any(x => x.Id == currentUser.OfficeID && x.OfficeType == "OutSource"))
            {
                var edocList = _sysImageDetailRepo.Get(x => x.BillingNo == settleNo);
                var edocData = new List<SysImageDetail>();
                edocList.ToList().ForEach(item =>
                {
                    if (_opsTranRepo.Any(x => x.Id == item.JobId && x.JobNo.Contains("R")))
                    {
                        edocData.Add(item);
                    }
                });
                edosExisted = edocData.OrderBy(x => x.DatetimeCreated).GroupBy(x => x.SysImageId).ToList();
            }
            else
            {
                edosExisted = _sysImageDetailRepo.Get(x => x.BillingNo == settleNo).OrderBy(x => x.DatetimeCreated).GroupBy(x => x.SysImageId).ToList();
            }
            var result = new List<SysImageDetailModel>();
            foreach (var x in edosExisted)
            {
                var image = _sysImageRepo.Get(z => z.Id == x.FirstOrDefault().SysImageId).FirstOrDefault();
                var jobDetail = GetJobDetail(x.FirstOrDefault().JobId, x.FirstOrDefault().Hblid);
                var countItem = x.GroupBy(z => z.JobId).Count();
                var edoc = new SysImageDetailModel()
                {
                    Id = x.FirstOrDefault().Id,
                    BillingNo = settleNo,
                    SystemFileName = x.FirstOrDefault().SystemFileName,
                    ImageUrl = image == null ? null : image.Url,
                    DatetimeCreated = x.FirstOrDefault().DatetimeCreated,
                    BillingType = transactionType,
                    DatetimeModified = x.FirstOrDefault().DatetimeModified,
                    DepartmentId = currentUser.DepartmentId,
                    DocumentTypeId = x?.FirstOrDefault().DocumentTypeId,
                    Source = SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT,
                    SysImageId = image.Id,
                    UserCreated = x.FirstOrDefault().UserCreated,
                    UserFileName = x.FirstOrDefault().UserFileName,
                    UserModified = x.FirstOrDefault().UserModified,
                    Note = x.FirstOrDefault().Note,
                    HBLNo = countItem > 1 ? null : jobDetail.HBLNo,
                    JobNo = countItem > 1 ? null : jobDetail.JobNo,
                    Hblid = countItem > 1 ? Guid.Empty : jobDetail.HBLId,
                    JobId = countItem > 1 ? Guid.Empty : jobDetail.JobId,
                    DocumentTypeName = _attachFileTemplateRepo.Get(y => y.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault().NameEn,
                    TransactionType = jobDetail?.TransactionType
                };
                result.Add(edoc);
            }
            return result;
        }


        public SysAttachFileTemplate GetAttTepmlateByJob(string Code, int docId, string transationType, string accountingType)
        {
            return _attachFileTemplateRepo.Get(x => x.Code == Code && (x.AccountingType == "Settlement" || x.AccountingType == "ADV-Settlement") && x.TransactionType == transationType && x.AccountingType == accountingType).FirstOrDefault();
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
                var template = _attachFileTemplateRepo.Get(z => z.Id == x.DocumentTypeId)?.FirstOrDefault();
                string _hblNo = string.Empty;
                if (x.Hblid != null && x.Hblid != Guid.Empty && template != null)
                {
                    if (template.TransactionType == "CL")
                    {
                        _hblNo = _opsTranRepo.Get(y => y.Hblid == x.Hblid)?.FirstOrDefault()?.Hwbno;
                    }
                    else
                    {
                        _hblNo = _tranDeRepo.Get(y => y.Id == x.Hblid)?.FirstOrDefault()?.Hwbno;
                    }
                }
                var image = _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault();
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
                    ImageUrl = _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault() != null ? image.Url : null,
                    HBLNo = _hblNo,
                    Note = x.Note,
                    TransactionType = transactionType,
                    DocumentCode = template?.Code
                };
                lstImageMD.Add(imageModel);
            });
            var newImageIds = _sysImageDetailRepo.Get(x => x.JobId == jobID).Select(x => x.SysImageId).ToList();
            var imageExist = _sysImageRepo.Get(x => x.ObjectId == jobID.ToString()).ToList();
            var imageMap = new List<SysImageDetailModel>();
            foreach (var image in imageExist)
            {
                if (!newImageIds.Any(x => x == image.Id))
                {
                    var imageModel = new SysImageDetailModel()
                    {
                        BillingNo = null,
                        BillingType = "Shipment",
                        DatetimeCreated = image.DateTimeCreated,
                        DatetimeModified = image.DatetimeModified,
                        DepartmentId = null,
                        DocumentTypeId = GetDocumentType("Shipment", Guid.Parse(image.ObjectId)),
                        ExpiredDate = null,
                        GroupId = null,
                        Hblid = null,
                        Id = image.Id,
                        JobId = Guid.Parse(image.ObjectId),
                        OfficeId = null,
                        Source = "Shipment",
                        SysImageId = image.Id,
                        SystemFileName = "OTH_" + image.Name,
                        UserCreated = _userRepo.Get(z => z.Id == image.UserCreated).FirstOrDefault() != null ? _userRepo.Get(z => z.Id == image.UserCreated).FirstOrDefault().Username : image.UserCreated,
                        UserFileName = image.Name,
                        UserModified = _userRepo.Get(z => z.Id == image.UserModified).FirstOrDefault() != null ? _userRepo.Get(z => z.Id == image.UserModified).FirstOrDefault().Username : image.UserModified,
                        ImageUrl = image.Url,
                        HBLNo = null,
                        Note = null,
                        TransactionType = _attachFileTemplateRepo.Get(x => x.Id == GetDocumentType("Shipment", Guid.Parse(image.ObjectId))).FirstOrDefault()?.TransactionType,
                        DocumentCode = "OTH"
                    };
                    imageMap.Add(imageModel);
                }
            }
            var otherId = lstTran.Where(x => x.Code == "OTH").FirstOrDefault() != null ? lstTran.Where(x => x.Code == "OTH").FirstOrDefault().Id : 0;
            var listOther = new List<SysImageDetailModel>();
            if (otherId != 0)
            {
                imageMap.AddRange(lstImageMD.Where(x => x.DocumentTypeId == otherId));
            }
            if (imageMap.Count > 0)
            {
                imageMap.ForEach(x =>
                {
                    var template = _attachFileTemplateRepo.Get(z => z.Id == x.DocumentTypeId)?.FirstOrDefault();
                    string _hblNo = string.Empty;
                    if (x.Hblid != null && x.Hblid != Guid.Empty && template != null)
                    {
                        if (template.TransactionType == "CL")
                        {
                            _hblNo = _opsTranRepo.Get(y => y.Hblid == x.Hblid)?.FirstOrDefault()?.Hwbno;
                        }
                        else
                        {
                            _hblNo = _tranDeRepo.Get(y => y.Id == x.Hblid)?.FirstOrDefault()?.Hwbno;
                        }
                    }
                    var jobNo = transactionType != "CL" ? _cstranRepo.Get(y => y.Id == jobID).FirstOrDefault().JobNo : _opsTranRepo.Get(z => z.Id == jobID).FirstOrDefault().JobNo;
                    var tra = GetDocumentType(transactionType, null);
                    var imagedetail = new SysImageDetailModel
                    {
                        BillingNo = null,
                        BillingType = "Other",
                        DatetimeCreated = x.DatetimeCreated,
                        DatetimeModified = x.DatetimeModified,
                        DepartmentId = currentUser.DepartmentId,
                        DocumentTypeId = tra,
                        ImageUrl = x.ImageUrl,
                        GroupId = currentUser.GroupId,
                        UserCreated = x.UserCreated,
                        UserModified = x.UserModified,
                        SystemFileName = x.SystemFileName.Contains("OTH") ? Path.GetFileNameWithoutExtension(x.SystemFileName) : "OTH" + Path.GetFileNameWithoutExtension(clearPrefix(null, x.SystemFileName)),
                        JobNo = jobNo,
                        UserFileName = x.UserFileName,
                        Id = x.Id,
                        TransactionType = transactionType,
                        Note = x.Note,
                        HBLNo = _hblNo,
                        Hblid = x.Hblid,
                        DocumentCode = x.DocumentCode,
                        SysImageId = x.SysImageId
                    };
                    listOther.Add(imagedetail);
                });
            }
            lstImageMD.GroupBy(x => x.DocumentTypeId).ToList().ForEach(x =>
            {
                if (result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault() != null)
                {
                    result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault().EDocs = x.OrderBy(z => z.DatetimeCreated).ToList();
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

        private async Task<string> getBillingNo(Guid billingId, string tranType)
        {
            switch (tranType)
            {
                case "Settlement":
                    var settle = await _setleRepo.GetAsync(z => z.Id == billingId);
                    return settle.FirstOrDefault().SettlementNo;
                case "Advance":
                    var adv = await _advRepo.GetAsync(z => z.Id == billingId);
                    return adv.FirstOrDefault().AdvanceNo;
                case "SOA":
                    var soa = await _soaRepo.GetAsync(x => x.Id == billingId.ToString());
                    return soa.FirstOrDefault().Soano;
                default: return null;
            }
        }

        private SysImageDetailModel MapEdocModelOTExist(SysImage x, string tranType, string billingNo, int? docTypeId)
        {
            string sysFileName = "";
            string source = "";
            string docTypeName = "";
            switch (tranType)
            {
                case "Settlement":
                    sysFileName = "OT" + '_' + x.Name;
                    source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_SETTLEMENT;
                    //docTypeName = "Other";
                    break;
                case "Advance":
                    sysFileName = "AD" + '_' + x.Name;
                    source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE;
                    //docTypeName = "AD";
                    break;
                case "SOA":
                    sysFileName = "SOA" + '_' + x.Name;
                    source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_SOA;
                    //docTypeName = "SOA";
                    break;
            }

            var edoc = new SysImageDetailModel()
            {
                Id = x.Id,
                BillingNo = billingNo,
                SystemFileName = sysFileName,
                ImageUrl = x.Url,
                DatetimeCreated = x.DateTimeCreated,
                BillingType = tranType,
                DatetimeModified = x.DatetimeModified,
                DepartmentId = docTypeId != null ? docTypeId : currentUser.DepartmentId,
                DocumentTypeId = 0, // TODO hoac gan cho no 1 cai bo other.
                Source = source,
                SysImageId = x.Id,
                UserCreated = x.UserCreated,
                UserFileName = x.Name,
                UserModified = x.UserModified,
                DocumentTypeName = "Other"
            };
            return edoc;
        }

        private SysImageDetailModel MapEdocModelSM(IGrouping<Guid?, SysImageDetailModel> x)
        {
            var edoc = new SysImageDetailModel()
            {
                JobNo = null,
                HBLNo = null,
                JobId = Guid.Empty,
                Hblid = Guid.Empty,
                SystemFileName = x.FirstOrDefault()?.SystemFileName,
                UserFileName = x.FirstOrDefault()?.UserFileName,
                DocumentTypeName = x.FirstOrDefault()?.DocumentTypeName,
                DocumentTypeId = x.FirstOrDefault()?.DocumentTypeId,
                TransactionType = x.FirstOrDefault()?.TransactionType,
                ImageUrl = x.FirstOrDefault()?.ImageUrl,
                SysImageId = x.FirstOrDefault()?.SysImageId,
                UserCreated = x.FirstOrDefault()?.UserCreated,
                Source = x.FirstOrDefault()?.Source,
                AccountingType = x.FirstOrDefault()?.AccountingType,
                DatetimeCreated = x.FirstOrDefault()?.DatetimeCreated,
                Note = x.FirstOrDefault()?.Note,
                Id = x.FirstOrDefault().Id,
            };
            return edoc;
        }

        private SysImageDetailModel MapEdocModelAcc(IGrouping<Guid?, SysImageDetail> x, string billingNo, string tranType, SysAttachFileTemplate att, SysImage im, string HblNo, string jobNo)
        {
            var edoc = new SysImageDetailModel()
            {
                Id = x.FirstOrDefault().Id,
                BillingNo = billingNo,
                SystemFileName = x.FirstOrDefault().SystemFileName,
                ImageUrl = im.Url,
                DatetimeCreated = x.FirstOrDefault().DatetimeCreated,
                BillingType = tranType,
                DatetimeModified = x.FirstOrDefault().DatetimeModified,
                DepartmentId = currentUser.DepartmentId,
                DocumentTypeId = att.Id,
                DocumentTypeName = att.NameEn,
                Source = SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE,
                SysImageId = im.Id,
                UserCreated = x.FirstOrDefault().UserCreated,
                UserFileName = x.FirstOrDefault().UserFileName,
                UserModified = x.FirstOrDefault().UserModified,
                Note = x.FirstOrDefault().Note,
                HBLNo = HblNo,
                JobNo = jobNo,
            };
            return edoc;
        }

        private List<SysImageDetailModel> MapOTToEDoc(List<SysImageDetailModel> lstEdocOT)
        {
            var result=new List<SysImageDetailModel>();
            lstEdocOT.ForEach(x =>
            {
                var edoc = new SysImageDetailModel()
                {
                    JobNo = null,
                    HBLNo = null,
                    JobId = Guid.Empty,
                    Hblid = Guid.Empty,
                    SystemFileName = x.SystemFileName,
                    UserFileName = x.UserFileName,
                    DocumentTypeName = x.DocumentTypeName,
                    DocumentTypeId = x.DocumentTypeId,
                    TransactionType = x.TransactionType,
                    ImageUrl = x.ImageUrl,
                    SysImageId = x.SysImageId,
                    UserCreated = x.UserCreated,
                    Source = x.Source,
                    AccountingType = x.AccountingType,
                    DatetimeCreated = x.DatetimeCreated,
                    Note = x.Note,
                    Id = x.Id,
                };
                result.Add(edoc);
            });

            return result;
        }

        private List<SysImageDetailModel> MapRepITLEDoc(List<IGrouping<Guid?,SysImageDetailModel>> edocLst)
        {
            var result=new List<SysImageDetailModel>();
            edocLst.ForEach(x =>
            {
                if (x.Count() == 1)
                {
                    result.Add(x.FirstOrDefault());
                }
                else if (x.Count() > 1)
                {
                    var edoc = MapEdocModelSM(x);
                    result.Add(edoc);
                }
            });
            return result;
        }

        private EdocJobInfo getEdocJobInfo(IGrouping<Guid?,SysImageDetail> x)
        {
            var fromRep = _officeRepo.Any(z => z.Id == currentUser.OfficeID && z.OfficeType == "OutSource");
            //var jobDetail = GetJobDetail(x.FirstOrDefault().JobId, x.FirstOrDefault().Hblid);
            var countItem = x.GroupBy(z => z.JobId).Count();
            if (fromRep)
            {
                var jobIds = x.Select(z => z.JobId).ToList();
                var repId = _opsTranRepo.Get(g => jobIds.Contains(g.Id) && g.LinkSource == "Replicate").FirstOrDefault().Id;
                var jobDetailRep = GetJobDetail(repId, x.FirstOrDefault().Hblid);
                return new EdocJobInfo
                {
                    HBLNo = countItem >= 3 ? null : jobDetailRep != null ? jobDetailRep.HBLNo : null,
                    JobNo = countItem >= 3 ? null : jobDetailRep != null ? jobDetailRep.JobNo : null,
                };
            }
            var jobDetail = GetJobDetail(x.FirstOrDefault().JobId, x.FirstOrDefault().Hblid);
            return new EdocJobInfo
            {
                HBLNo = countItem > 1 ? null : jobDetail != null ? jobDetail.HBLNo : null,
                JobNo = countItem > 1 ? null : jobDetail != null ? jobDetail.JobNo : null,
            };
        }

        public async Task<EDocGroupByType> GetEDocByAccountant(Guid billingId, string transactionType)
        {
            var result = new EDocGroupByType();
            var lstEdoc = new List<SysImageDetailModel>();
            var imgExist = await _sysImageRepo.GetAsync(x => x.Folder == transactionType && x.ObjectId == billingId.ToString()); // file goc co tren SM
            var imageExist = imgExist.ToList();
            var lstEdocOT = new List<SysImageDetailModel>();
            var settle = new AcctSettlementPayment();
            var advance = new AcctAdvancePayment();
            var soa = new AcctSoa();
            var attachTemplate = new SysAttachFileTemplate();
            var attachTemplateIds = new List<int>();
            var EdocIamgeIds = new List<Guid?>();
            var billingNo = await getBillingNo(billingId, transactionType);
            switch (transactionType)
            {
                case "Settlement":
                    attachTemplate = _attachFileTemplateRepo.Get(y => y.Code == "SM" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT || y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_ADV_SETTLE).FirstOrDefault();
                    attachTemplateIds = _attachFileTemplateRepo.Get(y => y.Code == "SM" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_SETTLEMENT || y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_ACCOUNTING_TYPE_ADV_SETTLE).ToList().Select(x => x.Id).ToList();
                    result.documentType = attachTemplate;
                    break;
                case "Advance":
                    attachTemplate = _attachFileTemplateRepo.Get(y => y.Code == "AD" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE).FirstOrDefault();
                    attachTemplateIds = _attachFileTemplateRepo.Get(y => y.Code == "AD" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_ADVANCE).ToList().Select(x => x.Id).ToList();
                    result.documentType = attachTemplate;
                    break;
                case "SOA":
                    attachTemplate = _attachFileTemplateRepo.Get(y => y.Code == "SOA" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_SOA).FirstOrDefault();
                    attachTemplateIds = _attachFileTemplateRepo.Get(y => y.Code == "SOA" && y.AccountingType == SystemFileManagementConstants.ATTACH_TEMPLATE_SOURCE_SOA).ToList().Select(x => x.Id).ToList();
                    break;
            }
            if (transactionType == "Settlement")
            {
                EdocIamgeIds = _sysImageDetailRepo.Get(x => x.BillingNo == billingNo).Select(x => x.SysImageId).ToList(); //ID file goc tren SM da dc generate detail.;
                imageExist.Where(x => !EdocIamgeIds.Contains(x.Id)).ToList().ForEach(x =>
                {
                    var edoc = MapEdocModelOTExist(x, transactionType, billingNo, null);
                    lstEdocOT.Add(edoc);
                });
                lstEdoc = getEDocSM(billingNo, transactionType);
                var lstEdocModel = new List<SysImageDetailModel>();
                var edocLst = lstEdoc.OrderBy(x => x.DatetimeCreated).GroupBy(x => x.SysImageId).ToList();
                lstEdocModel=MapRepITLEDoc(edocLst);
                var lstEdocOTModel = MapOTToEDoc(lstEdocOT);
                result.EDocs = lstEdocModel.Union(lstEdocOTModel).ToList();
                return result;
            }
            else if (transactionType == "Advance")
            {
                var advEDoc = _sysImageDetailRepo.Get(x => attachTemplateIds.Contains((int)x.DocumentTypeId) && x.BillingNo == billingNo).GroupBy(x => x.SysImageId).ToList();
                imageExist.Where(x => !advEDoc.Any(z => z.FirstOrDefault().SysImageId == x.Id)).ToList().ForEach(x =>
                {
                    var edoc = MapEdocModelOTExist(x, transactionType, advance.AdvanceNo, attachTemplate.Id);
                    lstEdocOT.Add(edoc);
                });
                foreach (var x in advEDoc)
                {
                    var image = _sysImageRepo.Get(z => z.Id == x.FirstOrDefault().SysImageId).FirstOrDefault();
                    var edocJobInfo = getEdocJobInfo(x);
                    var edoc = MapEdocModelAcc(x, advance.AdvanceNo, transactionType, attachTemplate, image, edocJobInfo.HBLNo, edocJobInfo.JobNo);
                    lstEdoc.Add(edoc);
                }
                var lstEdocOTModel = MapOTToEDoc(lstEdocOT);
                lstEdoc = lstEdocOTModel.Union(lstEdoc).ToList();
                result.EDocs = lstEdoc.OrderBy(x => x.DatetimeCreated).ToList();
            }
            else if (transactionType == "SOA")
            {
                var soaEDoc = _sysImageDetailRepo.Get(x => attachTemplateIds.Contains((int)x.DocumentTypeId) && x.BillingNo == billingNo).GroupBy(x => x.SysImageId).ToList();
                imageExist.Where(x => !soaEDoc.Any(z => z.FirstOrDefault().SysImageId == x.Id)).ToList().ForEach(x =>
                {
                    var edoc = MapEdocModelOTExist(x, transactionType, soa.Soano, attachTemplate.Id);
                    lstEdocOT.Add(edoc);
                });
                foreach (var x in soaEDoc)
                {
                    var image = _sysImageRepo.Get(z => z.Id == x.FirstOrDefault().SysImageId).FirstOrDefault();
                    var edocJobInfo = getEdocJobInfo(x);
                    var edoc = MapEdocModelAcc(x, soa.Soano, transactionType, attachTemplate, image, edocJobInfo.HBLNo, edocJobInfo.JobNo);
                    lstEdoc.Add(edoc);
                }
                var lstEdocOTModel = MapOTToEDoc(lstEdocOT);
                lstEdoc=lstEdoc.Union(lstEdocOTModel).ToList();
                result.EDocs = lstEdoc.OrderBy(x => x.DatetimeCreated).ToList();
            }
            return result;
        }
        
        private TransctionTypeJobModel GetJobDetail(Guid? jobId, Guid? hblId)
        {
            var JobOPS = hblId == Guid.Empty || hblId == null ? _opsTranRepo.Get(x => x.Id == jobId) : _opsTranRepo.Get(x => x.Hblid == hblId);
            if (JobOPS.Count() == 0)
            {
                var JobCS = hblId == Guid.Empty || hblId == null ? _tranDeRepo.Get(x => x.JobId == jobId) : _tranDeRepo.Get(x => x.Id == hblId);
                var csId = JobCS.FirstOrDefault().JobId;
                var cs = _cstranRepo.Get(x => x.Id == csId).FirstOrDefault();
                return new TransctionTypeJobModel() { HBLNo = JobCS.FirstOrDefault().Hwbno, JobNo = cs.JobNo, JobId = cs.Id, HBLId = JobCS.FirstOrDefault().Id, TransactionType = cs.TransactionType };
            }
            return new TransctionTypeJobModel() { HBLNo = JobOPS.FirstOrDefault().Hwbno, JobNo = JobOPS.FirstOrDefault().JobNo, JobId = JobOPS.FirstOrDefault().Id, HBLId = JobOPS.FirstOrDefault().Hblid, TransactionType = "CL" };
        }

        private int GetDocumentType(string transationType, Guid? jobId)
        {
            if (jobId != null)
            {
                if (_opsTranRepo.Get(x => x.Id == jobId).FirstOrDefault() != null)
                {
                    return _attachFileTemplateRepo.Get(x => x.Code == "OTH" && x.TransactionType == "CL").FirstOrDefault().Id;
                }
                var cstranType = _cstranRepo.Get(x => x.Id == jobId).FirstOrDefault()?.TransactionType;
                if (cstranType != null)
                {
                    return _attachFileTemplateRepo.Get(x => x.Code == "OTH" && x.TransactionType == cstranType).FirstOrDefault().Id;
                }
                return 0;
            }
            return _attachFileTemplateRepo.Get(x => x.TransactionType == transationType && x.Code == "OTH").FirstOrDefault().Id;
        }

        private async Task<DeleteObjectResponse> deleteFile(string keyS3)
        {
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = keyS3
            };

            DeleteObjectResponse rsDelete = await _client.DeleteObjectAsync(request);
            return rsDelete;
        }

        private bool checkHaveGenEdoc(Guid genId)
        {
            return _sysImageDetailRepo.Any(x => x.GenEdocId == genId);
        }

        public async Task<HandleState> DeleteEdocAcc(string billingNo)
        {
            try
            {
                var edocs = _sysImageDetailRepo.Get(x => x.BillingNo == billingNo).ToList();
                var imageIds = edocs.GroupBy(x => x.SysImageId).Select(x => x.FirstOrDefault().SysImageId).ToList();
                var images = await _sysImageRepo.GetAsync(x => imageIds.Contains(x.Id));
                var delImgDetail = await _sysImageDetailRepo.DeleteAsync(x => x.BillingNo == billingNo);
                if (delImgDetail.Success)
                {
                    images.ForEach(img =>
                    {
                        if (img != null)
                        {
                            var delImg = _sysImageRepo.Delete(x => x.Id == img.Id);
                            if (delImg.Success)
                            {
                                deleteFile(img.KeyS3);
                            }
                        }
                    });
                }
                return new HandleState(true, "Delete Edoc Success!");
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        public async Task<HandleState> DeleteEdoc(Guid edocId, Guid jobId)
        {
            HandleState result = new HandleState();
            try
            {
                var edocSrc = await _sysImageDetailRepo.GetAsync(x => x.Id == edocId);
                var edoc = edocSrc.FirstOrDefault();
                if (jobId == Guid.Empty)
                {
                    if (edoc == null)
                    {
                        var imageOtherSrc = await _sysImageRepo.GetAsync(x => x.Id == edocId);
                        var imageOther = imageOtherSrc.FirstOrDefault();
                        if (imageOther != null)
                        {
                            var rsDelete = deleteFile(imageOther.KeyS3);
                            if (rsDelete != null)
                            {
                                result = await _sysImageRepo.DeleteAsync(x => x.Id == edocId);
                                if (result.Success)
                                {
                                    await _sysImageDetailRepo.DeleteAsync(x => x.SysImageId == imageOther.Id);
                                }
                            }
                        }
                    }
                    if (edoc != null)
                    {
                        if (edoc.Source == "Shipment")
                        {
                            if (checkHaveGenEdoc(edocId))
                            {
                                await _sysImageDetailRepo.DeleteAsync(x => x.Id == edoc.Id);
                                await _sysImageDetailRepo.DeleteAsync(x => x.GenEdocId == edoc.Id);
                                var image = _sysImageRepo.Get(x => x.Id == edoc.SysImageId).FirstOrDefault();
                                var rsDelete = deleteFile(image.KeyS3);
                                if (rsDelete != null)
                                    if (edoc.Id != Guid.Empty)
                                    {
                                        result = await _sysImageRepo.DeleteAsync(x => x.Id == edoc.SysImageId);
                                    }
                                    else
                                    {
                                        result = await _sysImageRepo.DeleteAsync(x => x.Id == edocId);
                                    }
                            }
                            else
                            {
                                if (_opsTranRepo.Any(x => x.ReplicatedId == edoc.JobId && x.CurrentStatus != "Canceled"))
                                {
                                    var itlJob = _opsTranRepo.Get(x => x.ReplicatedId == edoc.JobId).FirstOrDefault();
                                    await _sysImageDetailRepo.DeleteAsync(x => x.SystemFileName == edoc.SystemFileName && x.SysImageId == edoc.SysImageId && x.JobId == itlJob.Id);
                                }
                                var image = _sysImageRepo.Get(x => x.Id == edoc.SysImageId).FirstOrDefault();
                                var rsDelete = deleteFile(image.KeyS3);
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

                        }
                        else if (edoc.GenEdocId != null)
                        {
                            var images = _sysImageDetailRepo.Get(x => x.BillingNo == edoc.BillingNo && x.GenEdocId != null && x.SysImageId == edoc.SysImageId).ToList();
                            var edocIds = images.Select(x => x.Id).ToList();
                            var delEdoc = await _sysImageDetailRepo.DeleteAsync(x => edocIds.Contains(x.Id));
                        }
                        else
                        {
                            var image = _sysImageRepo.Get(x => x.Id == edoc.SysImageId).FirstOrDefault();
                            var edocShipment = _sysImageDetailRepo.Get(x => x.SysImageId == image.Id).ToList();
                            var edocIds = edocShipment.Select(x => x.Id);
                            var rsDelete = deleteFile(image.KeyS3);
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
                                await _sysImageDetailRepo.DeleteAsync(x => edocIds.Contains(x.Id));
                            }
                        }
                    }
                }
                else
                {
                    var edocExist = _sysImageDetailRepo.Get(x => x.SysImageId == edoc.SysImageId).ToList();
                    if (edocExist != null)
                    {
                        if (edocExist.Count == 1)
                        {
                            await _sysImageRepo.DeleteAsync(x => x.Id == edoc.SysImageId);
                        }
                    }
                    var edocDelId = edocExist.Where(x => x.JobId == jobId).FirstOrDefault().Id;
                    result = await _sysImageDetailRepo.DeleteAsync(x => x.Id == edocDelId);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        private int GetDocTypeIdByJob(string transactionType, int docId, string AccountingType)
        {
            var doctype = _attachFileTemplateRepo.Get(x => x.Id == docId).FirstOrDefault();
            if (doctype.TransactionType == transactionType) return docId;
            if (AccountingType == "Shipment")
            {
                return _attachFileTemplateRepo.Get(x => x.NameEn == doctype.NameEn && x.TransactionType == transactionType && x.AccountingType == null).FirstOrDefault().Id;
            }
            return _attachFileTemplateRepo.Get(x => x.NameEn == doctype.NameEn && x.TransactionType == transactionType && (x.AccountingType == "Settlement" || x.AccountingType == "ADV-Settlement")).FirstOrDefault().Id;
        }

        public async Task<HandleState> UpdateEDoc(SysImageDetailModel edocUpdate)
        {
            var edoc = _sysImageDetailRepo.Get(x => x.Id == edocUpdate.Id).FirstOrDefault();
            try
            {
                if (edoc != null)
                {
                    if (edoc.DocumentTypeId != edocUpdate.DocumentTypeId)
                    {
                        await _sysImageDetailRepo.DeleteAsync(x => x.GenEdocId == edoc.Id);
                    }
                    else if (edoc.SystemFileName != edocUpdate.SystemFileName)
                    {
                        var edocgen = _sysImageDetailRepo.Get(x => x.GenEdocId == edoc.Id).FirstOrDefault();
                        if (edocgen != null)
                        {
                            edocgen.SystemFileName = edocUpdate.SystemFileName;
                            await _sysImageDetailRepo.UpdateAsync(edocgen, x => x.GenEdocId == edoc.Id);
                        }
                    }
                    var attachCode = _attachFileTemplateRepo.Get(x => x.Id == edocUpdate.DocumentTypeId).FirstOrDefault().Code;
                    edoc.SystemFileName = attachCode + clearPrefix(null, edocUpdate.SystemFileName);
                    edoc.Hblid = edocUpdate.Hblid;
                    edoc.Note = edocUpdate.Note;
                    edoc.DocumentTypeId = GetDocTypeIdByJob(edocUpdate.TransactionType, (int)edocUpdate.DocumentTypeId, edocUpdate.AccountingType);
                    edoc.JobId = edocUpdate.JobId;
                    var hs = await _sysImageDetailRepo.UpdateAsync(edoc, x => x.Id == edoc.Id, false);
                    if (_opsTranRepo.Any(x => edocUpdate.JobId == x.ReplicatedId))
                    {
                        var ops = await _opsTranRepo.GetAsync(x => x.ReplicatedId == edocUpdate.JobId);
                        var edocRep = await _sysImageDetailRepo.GetAsync(x => x.JobId == ops.FirstOrDefault().Id && x.SysImageId == edoc.SysImageId);
                        var edocClone = edocRep.FirstOrDefault();
                        edocClone.SystemFileName = edoc.SystemFileName;
                        edocClone.Note = edoc.Note;
                        edocClone.DocumentTypeId = edoc.DocumentTypeId;
                        _sysImageDetailRepo.Update(edocClone, x => x.Id == edocClone.Id, true);
                    }

                }
                if (edoc == null)
                {
                    var attachCode = _attachFileTemplateRepo.Get(x => x.Id == edocUpdate.DocumentTypeId).FirstOrDefault().Code;
                    var image = _sysImageRepo.Get(x => x.Id == edocUpdate.Id).FirstOrDefault();
                    var edocGenAdd = new SysImageDetail()
                    {
                        DocumentTypeId = edocUpdate.DocumentTypeId,
                        SystemFileName = attachCode + clearPrefix(null, edocUpdate.SystemFileName),
                        UserFileName = image?.Name,
                        Hblid = edocUpdate.Hblid,
                        Note = edocUpdate.Note,
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

        private string clearPrefix(string transactionType, string fileName)
        {
            var prefixs = new List<string>();
            if (transactionType == null)
            {
                prefixs = _attachFileTemplateRepo.Get().Select(x => x.Code).OrderBy(x => x.Length).ToList();
            }
            else
            {
                prefixs = _attachFileTemplateRepo.Get(x => x.TransactionType == transactionType).Select(x => x.Code).OrderBy(x => x.Length).ToList(); prefixs = _attachFileTemplateRepo.Get(x => x.TransactionType == transactionType).Select(x => x.Code).OrderBy(x => x.Length).ToList();
            }
            string code = null;
            bool clearLastChar = false;
            if (fileName.Split('_').Count() > 0)
            {
                if (fileName.ToList().Last() == '_')
                {
                    fileName = fileName.Remove(fileName.Length - 1);
                    clearLastChar = true;
                }
                var fileNameSplit = fileName.Split('_').ToList().Last();
                var preFixFileName = fileName.Replace(fileNameSplit, "");
                for (int i = 0; i < prefixs.Count; i++)
                {
                    if (!string.IsNullOrEmpty(prefixs[i]) && preFixFileName.Contains(prefixs[i]))
                    {
                        code = prefixs[i];
                    }
                }
            }
            else
            {
                return '_' + fileName;
            }
            if (clearLastChar)
            {
                fileName = fileName + '_';
            }
            if (code != null)
            {
                return '_' + fileName.Remove(0, code.Length + 1).ToString();
            }
            return '_' + fileName;
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
                            if (item.Contains("LOG") || item.Contains("TKI"))
                            {
                                var opsJob = DC.OpsTransaction.Where(x => x.CurrentStatus != "Canceled").FirstOrDefault(x => x.JobNo == item);
                                if (opsJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = item.Contains("TKI")?"TK": "CL", BillingNo = bilingNo, Code = "AD", HBLId = opsJob.Hblid });
                                }
                            } 
                            else
                            {
                                var csJob = DC.CsTransaction.Where(x => x.CurrentStatus != "Canceled").FirstOrDefault(x => x.JobNo == item);
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
                    var grpJobsSm = DC.CsShipmentSurcharge.Where(x => x.SettlementCode == bilingNo).GroupBy(x => new { x.JobNo }).Select(x => new { x.FirstOrDefault().JobNo, x.FirstOrDefault().Hblid });
                    foreach (var item in grpJobsSm)
                    {
                        if (item.JobNo.Contains("LOG")|| item.JobNo.Contains("TKI"))
                        {
                            var opsJob = DC.OpsTransaction.Where(x => x.CurrentStatus != "Canceled").FirstOrDefault(x => x.JobNo == item.JobNo);
                            if (opsJob != null)
                            {
                                transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = item.JobNo.Contains("TKI")?"TK": "CL", BillingNo = bilingNo, Code = "SM", HBLId = null });
                            }
                        }
                        else
                        {
                            var csJob = DC.CsTransaction.Where(x => x.CurrentStatus != "Canceled").FirstOrDefault(x => x.JobNo == item.JobNo);
                            if (csJob != null)
                            {
                                var hblId = _tranDeRepo.Get(x => x.JobId == csJob.Id).FirstOrDefault().Id;
                                transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType, BillingNo = bilingNo, Code = "SM", HBLId = null });
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
                            if (item.Contains("LOG") || item.Contains("TKI"))
                            {
                                var opsJob = DC.OpsTransaction.Where(x => x.CurrentStatus != "Canceled").FirstOrDefault(x => x.JobNo == item);
                                if (opsJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = item.Contains("TKI")?"TK": "CL", BillingNo = bilingNo, Code = "SOA" });
                                }
                            }
                            else
                            {
                                var csJob = DC.CsTransaction.Where(x => x.CurrentStatus != "Canceled").FirstOrDefault(x => x.JobNo == item);
                                if (csJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType, BillingNo = bilingNo, Code = "SOA" });
                                }
                            }
                        }
                    }
                    break;
                case "Shipment":
                    var csjobdetail = _tranDeRepo.Get(x => x.Id.ToString() == billingId).FirstOrDefault();
                    if (csjobdetail != null)
                    {
                        bilingNo = csjobdetail.Hwbno;
                        var csjob = _cstranRepo.Get(x => x.Id == csjobdetail.JobId && x.CurrentStatus != "Canceled").FirstOrDefault();
                        transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csjobdetail.JobId, TransactionType = csjob.TransactionType, BillingNo = bilingNo, Code = "OTH" });
                    }
                    else
                    {
                        var opsjob = _opsTranRepo.Get(x => x.Hblid.ToString() == billingId && x.CurrentStatus != "Canceled").FirstOrDefault();
                        if (opsjob != null)
                        {
                            transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsjob.Id, TransactionType = "CL", BillingNo = opsjob.Hwbno, Code = "OTH" });
                        }
                    }
                    break;
                default:
                    break;
            }
            return transctionTypeJobModels;
        }

        private bool SMhaveADV(string smId)
        {
            var smNo = _setleRepo.Get(x => x.Id.ToString() == smId).FirstOrDefault().SettlementNo;
            return _surRepo.Any(x => x.SettlementCode == smNo && x.AdvanceNo != null);
        }

        public async Task<HandleState> MappingeDocToShipment(Guid imageId, string billingId, string billingType)
        {
            HandleState result = new HandleState();
            string bilingNo = string.Empty;
            bool haveADV = false;
            if (billingType == "Settlement")
            {
                if (SMhaveADV(billingId))
                {
                    haveADV = true;
                }
             }
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
                        SystemFileName = Path.GetFileNameWithoutExtension(sysImage.Name.Contains(item.Code) ? sysImage.Name : item.Code + "_" + sysImage.Name),
                        UserFileName = sysImage.Name,
                        UserModified = sysImage.UserCreated,
                        Source = billingType,
                        DocumentTypeId = GetDocumentTypeWithTypeAttachTemplate(type, item.TransactionType, item.Code, billingType, haveADV)?.FirstOrDefault()?.Id,
                        Hblid = billingType == "Shipment" ? Guid.Parse(billingId) : Guid.Empty,
                    };

                    await _sysImageDetailRepo.AddAsync(imageDetail);
                }

                result = _sysImageDetailRepo.SubmitChanges();
            }
            return result;
        }
        private IQueryable<SysAttachFileTemplate> GetDocumentTypeWithTypeAttachTemplate(string type, string transactionType, string code, string accountingType, bool haveEdoc)
        {
            var tranType=transactionType=="TK"||transactionType=="CL"?"CL":transactionType;
            Expression<Func<SysAttachFileTemplate, bool>> queryAttachTemplate = x => x.Type == type && x.TransactionType == tranType;
            if (!string.IsNullOrEmpty(code))
            {
                queryAttachTemplate = queryAttachTemplate.And(x => x.Code == code);
            }
            if (!string.IsNullOrEmpty(accountingType) && accountingType != "Shipment")
            {
                if (accountingType == "Settlement" && haveEdoc)
                {
                    queryAttachTemplate = queryAttachTemplate.And(x => x.AccountingType == "ADV-Settlement");
                }
                else
                {
                    queryAttachTemplate = queryAttachTemplate.And(x => x.AccountingType == accountingType);
                }
            }
            var template = _attachFileTemplateRepo.Get(queryAttachTemplate);

            return template;
        }

        private async Task<List<SysImage>> UpLoadS3(FileUploadModel model, bool isSync)
        {
            var urlImage = "";
            List<SysImage> list = new List<SysImage>();
            if(model.Files != null){
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
                            UserCreated = !string.IsNullOrEmpty(model.UserCreated) ?model.UserCreated: "",
                            UserModified = !string.IsNullOrEmpty(model.UserCreated) ? model.UserCreated : "",
                            DateTimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            ChildId = model.Child,
                            KeyS3 = key,
                            SyncStatus = isSync == true ? "Synced" : ""
                        };
                        list.Add(sysImage);
                    }
                }
            }

            return list;
        }

        public async Task<HandleState> PostFileAttacheDoc(FileUploadModel model)
        {
            try
            {
                List<SysImage> imageList = await UpLoadS3(model, false);
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
        public async Task<string> PostAttachFileTemplateToEDoc(FileUploadAttachTemplateModel req)
        {
            var stream = new MemoryStream(req.File.FileContent);
            var fFile = new FormFile(stream, 0, stream.Length, null, req.File.FileName);
            var fFiles = new List<IFormFile>() { fFile };
            var model = new FileUploadModel
        {
                Child = null,
                Files = fFiles,
                FolderName = req.FolderName,
                ModuleName = req.ModuleName,
                Id = req.Id,
                UserCreated=req.UserCreated
            };
            var urlImage = "";
            List<SysImage> imageList = await UpLoadS3(model, true);
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
                    List<SysImage> imageList = await UpLoadS3(UploadModel, false);

                    if (imageList.Count > 0)
                    {
                        HandleState hsAddImage = await _sysImageRepo.AddAsync(imageList);

                        if (hsAddImage.Success)
                        {
                            foreach (var image in imageList)
                            {
                                model.TransactionType = convertTransactionType(model.TransactionType);
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

        private string convertTransactionType(string tranType)
        {
            string result = null;
            if (CustomData.Services.Any(x => x.DisplayName.Replace(" ", "") == tranType))
            {
                CustomData.Services.ForEach(x =>
                {
                    if (x.DisplayName.Replace(" ", "") == tranType)
                    {
                        result = x.Value.ToString();
                    }
                });
            }
            else
            {
                result = tranType;
            }

            return result;
        }

        private string GetAliasNameForPreviewTemplate(string name)
        {
            return Path.GetFileNameWithoutExtension(name).TrimEnd('_').Remove(name.LastIndexOf('_'));
        }

        private async Task<HandleState> MappingPreviewTemplateToShipment(EDocAttachPreviewTemplateUploadModel model, SysImage image)
        {
            HandleState result = new HandleState();
            try
            {
                bool isExsitedCode = PreviewTemplateCodeMappingAttachTemplateCode.ContainsKey(model.TemplateCode);
                string code = !isExsitedCode ? "OTH" : PreviewTemplateCodeMappingAttachTemplateCode[model.TemplateCode];
                int? _docTypeId = -1;
                if (model.TransactionType == null)
                {
                    model.TransactionType = _cstranRepo.Get(x => x.Id == model.ObjectId).FirstOrDefault()?.TransactionType;
                }
                var docTypeTemplate = _attachFileTemplateRepo.Get(x => x.Code == code
                                                                && x.TransactionType == model.TransactionType
                                                                && x.Type == SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_GENERAL)?.FirstOrDefault();
                if (docTypeTemplate == null)
                {
                    docTypeTemplate = _attachFileTemplateRepo.Get(x => x.Code == "OTH"
                                                        && x.TransactionType == model.TransactionType
                                                        && x.Type == SystemFileManagementConstants.ATTACH_TEMPLATE_TYPE_GENERAL)?.FirstOrDefault();
                    _docTypeId = docTypeTemplate.Id;
                }
                else
                {
                    _docTypeId = docTypeTemplate.Id;
                }
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
                    SystemFileName = docTypeTemplate.Code + "_" + GetAliasNameForPreviewTemplate(image.Name),
                    DocumentTypeId = _docTypeId
                };
                result = await _sysImageDetailRepo.AddAsync(imageDetail);

                return result;
            }
            catch (Exception ex)
            {
                return new HandleState((object)ex.ToString());
            }
        }

        public async Task<HandleState> OpenFile(Guid Id)
        {
            try
            {
                var image = await _sysImageRepo.GetAsync(x => x.Id == Id);
                if (image == null)
                {
                    return new HandleState("Not found file");
                }
                string key = image.FirstOrDefault().KeyS3;
                if (string.IsNullOrEmpty(key))
                {
                    return new HandleState("Not found key");
                }
                var request = new GetObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = key
                };
                new LogHelper("Request Open File", JsonConvert.SerializeObject(request));
                GetObjectResponse response = await _client.GetObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK) { return new HandleState("Stream file error"); }
                return new HandleState(true, response.ResponseStream);
            }
            catch (Exception ex)
            {
                new LogHelper("Ex Open File", JsonConvert.SerializeObject(ex));
                return new HandleState(ex.ToString());
            }
        }

        public async Task<HandleState> CreateEDocZip(FileDowloadZipModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var lst = new List<SysImage>();

                var edocs = new List<SysImageDetail>();
                if (model.FolderName == "Shipment")
                {
                    edocs = await _sysImageDetailRepo.GetAsync(x => (x.JobId.ToString() == model.ObjectId));
                }
                else
                {
                    string billingNo = null;
                    switch (model.FolderName)
                    {
                        case "SOA":
                            var soa = await _soaRepo.GetAsync(x => x.Id == model.ObjectId);
                            billingNo = soa.FirstOrDefault().Soano;
                            break;
                        case "Settlement":
                            var settle = await _setleRepo.GetAsync(x => x.Id.ToString() == model.ObjectId);
                            billingNo = settle.FirstOrDefault().SettlementNo;
                            break;
                        case "Advance":
                            var adv = await _advRepo.GetAsync(x => x.Id.ToString() == model.ObjectId);
                            billingNo = adv.FirstOrDefault().AdvanceNo;
                            break;
                    }
                    edocs = await _sysImageDetailRepo.GetAsync(x => (x.BillingNo == billingNo));
                }
                var imageExist = _sysImageRepo.Get(x => x.ObjectId == model.ObjectId && !edocs.Select(z => z.SysImageId).Contains(x.Id)).Select(x => x.Id).Distinct();
                lst = await _sysImageRepo.GetAsync(x => edocs.Select(y => y.SysImageId).Distinct().Contains(x.Id) || imageExist.Contains(x.Id));

                if (lst == null) { return new HandleState("Not found data"); }
                var files = new List<InMemoryFile>();
                foreach (var it in lst)
                {
                    var request = new GetObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = it.KeyS3
                    };
                    GetObjectResponse response = await _client.GetObjectAsync(request);
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        var f = new InMemoryFile();
                        f = new InMemoryFile() { Content = streamToByteArray(response.ResponseStream), FileName = GetAliasName(it) };
                        files.Add(f);
                    }
                }

                return new HandleState(true, GetZipArchive(files));
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        private string GetAliasName(SysImage image)
        {
            var name = _sysImageDetailRepo.Get(x => x.SysImageId == image.Id).FirstOrDefault()?.SystemFileName;
            if (name == null)
            {
                return "OT_" + image.Name;
            }
            return _sysImageDetailRepo.Get(x => x.SysImageId == image.Id).FirstOrDefault()?.SystemFileName + Path.GetExtension(image.Url);
        }

        public async Task<HandleState> GenEdocByBilling(string billingNo, string billingType)
        {
            HandleState result = new HandleState();
            try
            {
                var edocs = new List<SysImageDetail>();
                var settleId = _setleRepo.Get(x => x.SettlementNo == billingNo).FirstOrDefault().Id.ToString();
                var haveEdoc = SMhaveADV(settleId);
                var advSM = _surRepo.Get(x => x.SettlementCode == billingNo && x.Mblno != null && x.AdvanceNo != null).FirstOrDefault();
                switch (billingType)
                {
                    case "Advance":
                        var chargeSMAD = _surRepo.Get(x => x.SettlementCode == billingNo && x.AdvanceNo != null);
                        var jobSettleAD = chargeSMAD.GroupBy(x => new { x.JobNo, x.AdvanceNo }).Select(x => new { jobNo = x.FirstOrDefault().JobNo, tranType = x.FirstOrDefault().TransactionType, advNo = x.FirstOrDefault().AdvanceNo });
                        jobSettleAD.ToList().ForEach(x =>
                        {
                            var adv = _advRepo.Get(z => z.AdvanceNo == x.advNo && z.SyncStatus != "Rejected").FirstOrDefault();
                            if (adv != null)
                            {
                                var advId = adv.Id;
                                var image = _sysImageRepo.Get(z => advId.ToString() == z.ObjectId && z.SyncStatus == "Synced" && z.Folder == "Advance").OrderByDescending(z => z.DateTimeCreated).FirstOrDefault();
                                if (image != null)
                                {
                                    var edocExist = _sysImageDetailRepo.Get(z => z.SysImageId == image.Id && z.BillingNo == billingNo && z.Source == "Settlement");
                                    if (edocExist.Count() == 0)
                                    {
                                        var tranType = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && z.Code == "AD-SM").FirstOrDefault();
                                        var edocFrom = _sysImageDetailRepo.Get(z => z.SysImageId == image.Id).FirstOrDefault();
                                        var edoc = new SysImageDetail()
                                        {
                                            Id = Guid.NewGuid(),
                                            BillingNo = billingNo,
                                            BillingType = "Settlement",
                                            DatetimeCreated = DateTime.Now,
                                            DatetimeModified = DateTime.Now,
                                            DepartmentId = currentUser.DepartmentId,
                                            ExpiredDate = null,
                                            GroupId = currentUser.GroupId,
                                            DocumentTypeId = tranType.Id,
                                            JobId = getJobId(x.jobNo, x.tranType),
                                            SysImageId = image.Id,
                                            SystemFileName = edocFrom != null ? edocFrom.SystemFileName : image.Name,
                                            UserFileName = image.Name,
                                            UserCreated = currentUser.UserName,
                                            UserModified = currentUser.UserName,
                                            OfficeId = currentUser.OfficeID,
                                            Source = "Settlement",
                                            Hblid = null,
                                            Note = null,
                                            GenEdocId = edocFrom != null ? edocFrom.Id : Guid.Empty
                                        };
                                        edocs.Add(edoc);

                                    }
                                }
                            }
                        });
                        break;
                    case "MBL":
                        var chargeSMMBL = _surRepo.Get(x => x.SettlementCode == billingNo && x.Mblno != null);
                        var jobSettleMBL = chargeSMMBL.GroupBy(x => x.JobNo).Select(x => new { jobNo = x.FirstOrDefault().JobNo, tranType = x.FirstOrDefault().TransactionType });
                        jobSettleMBL.ToList().ForEach(x =>
                        {
                            var jobId = getJobId(x.jobNo, x.tranType);
                            var images = _sysImageRepo.Get(z => jobId.ToString() == z.ObjectId && z.Folder == "Shipment").OrderByDescending(z => z.DateTimeCreated);
                            images.ToList().ForEach(img =>
                            {
                                if (img != null)
                                {
                                    var edocExist = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id && z.BillingNo == billingNo && z.Source == "Settlement");
                                    var MBLCode = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && ((z.Code == "BL" || z.Code == "MB") && z.Type == "General")).FirstOrDefault();
                                    if (edocExist.Count() == 0)
                                    {
                                        int docTypeId = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && (z.Code == "BL" && z.AccountingType == "Settlement")).FirstOrDefault().Id;
                                        var tranType = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && (z.Code == "BL" && z.Type == "Accountant")).FirstOrDefault();
                                        if (MBLCode != null)
                                        {
                                            if (checEdocType(img.Id, MBLCode.Id))
                                            {
                                                var edocFrom = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id).FirstOrDefault();
                                                var edoc = new SysImageDetail()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    BillingNo = billingNo,
                                                    BillingType = "Settlement",
                                                    DatetimeCreated = DateTime.Now,
                                                    DatetimeModified = DateTime.Now,
                                                    DepartmentId = currentUser.DepartmentId,
                                                    ExpiredDate = null,
                                                    GroupId = currentUser.GroupId,
                                                    DocumentTypeId = docTypeId,
                                                    JobId = getJobId(x.jobNo, x.tranType),
                                                    SysImageId = img.Id,
                                                    SystemFileName = edocFrom.SystemFileName,
                                                    UserFileName = img.Name,
                                                    UserCreated = currentUser.UserName,
                                                    UserModified = currentUser.UserName,
                                                    OfficeId = currentUser.OfficeID,
                                                    Source = "Settlement",
                                                    Hblid = null,
                                                    Note = null,
                                                    GenEdocId = edocFrom.Id
                                                };
                                                edocs.Add(edoc);
                                            }
                                        }
                                    }
                                }
                            });
                        });
                        break;
                    case "HBL":
                        var chargeSMHBL = _surRepo.Get(x => x.SettlementCode == billingNo && x.Hblno != null);
                        advSM = _surRepo.Get(x => x.SettlementCode == billingNo && x.Mblno != null && x.AdvanceNo != null).FirstOrDefault();
                        var jobSettleHBL = chargeSMHBL.GroupBy(x => x.JobNo).Select(x => new { jobNo = x.FirstOrDefault().JobNo, tranType = x.FirstOrDefault().TransactionType });
                        jobSettleHBL.ToList().ForEach(x =>
                        {
                            var jobId = getJobId(x.jobNo, x.tranType);
                            var images = _sysImageRepo.Get(z => jobId.ToString() == z.ObjectId && z.Folder == "Shipment").OrderByDescending(z => z.DateTimeCreated);
                            images.ToList().ForEach(img =>
                            {
                                if (img != null)
                                {
                                    var edocExist = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id && z.BillingNo == billingNo && z.Source == "Settlement");
                                    if (edocExist.Count() == 0)
                                    {
                                        int docTypeId = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && (z.Code == "BL" && z.AccountingType == "Settlement")).FirstOrDefault().Id;
                                        var HBLCodes = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && ((z.Code == "HB" || z.Code == "BL") && z.Type == "General")).ToList();
                                        HBLCodes.ToList().ForEach(HBLCode =>
                                        {
                                            if (checEdocType(img.Id, HBLCode.Id))
                                            {
                                                var edocFrom = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id).FirstOrDefault();
                                                var edoc = new SysImageDetail()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    BillingNo = billingNo,
                                                    BillingType = "Settlement",
                                                    DatetimeCreated = DateTime.Now,
                                                    DatetimeModified = DateTime.Now,
                                                    DepartmentId = currentUser.DepartmentId,
                                                    ExpiredDate = null,
                                                    GroupId = currentUser.GroupId,
                                                    DocumentTypeId = docTypeId,
                                                    JobId = getJobId(x.jobNo, x.tranType),
                                                    SysImageId = img.Id,
                                                    SystemFileName = edocFrom.SystemFileName,
                                                    UserFileName = img.Name,
                                                    UserCreated = currentUser.UserName,
                                                    UserModified = currentUser.UserName,
                                                    OfficeId = currentUser.OfficeID,
                                                    Source = "Settlement",
                                                    Hblid = null,
                                                    Note = null,
                                                    GenEdocId = edocFrom.Id
                                                };
                                                edocs.Add(edoc);
                                            }
                                        });
                                    }
                                }
                            });
                        });
                        break;
                    case "INV":
                        var chargeSMINV = _surRepo.Get(x => x.SettlementCode == billingNo && x.InvoiceNo != null);
                        var jobSettleINV = chargeSMINV.GroupBy(x => x.JobNo).Select(x => new { jobNo = x.FirstOrDefault().JobNo, tranType = x.FirstOrDefault().TransactionType });
                        jobSettleINV.ToList().ForEach(x =>
                        {
                            var jobId = getJobId(x.jobNo, x.tranType);
                            var images = _sysImageRepo.Get(z => jobId.ToString() == z.ObjectId && z.Folder == "Shipment").OrderByDescending(z => z.DateTimeCreated);
                            images.ToList().ForEach(img =>
                            {
                                if (img != null)
                                {
                                    var edocExist = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id && z.BillingNo == billingNo && z.Source == "Settlement");
                                    if (edocExist.Count() == 0)
                                    {
                                        int docTypeId = 0;
                                        if (advSM != null)
                                        {
                                            docTypeId = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && (z.Code == "INV" && z.AccountingType == "ADV-Settlement")).FirstOrDefault().Id;
                                        }
                                        else
                                        {
                                            docTypeId = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && (z.Code == "INV" && z.AccountingType == "Settlement" && z.PartnerType == "Supplier")).FirstOrDefault().Id;
                                        }
                                        var tranType = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && z.Code == "INV" && z.Type == "Accountant" && z.PartnerType == "Supplier" && z.AccountingType == "Settlement").FirstOrDefault();
                                        var INVCodes = _attachFileTemplateRepo.Get(z => z.TransactionType == x.tranType && z.Code.Contains("INV") && z.Type == "General").ToList();
                                        INVCodes.ToList().ForEach(INVCode =>
                                        {
                                            if (INVCode != null)
                                            {
                                                if (checEdocType(img.Id, INVCode.Id))
                                                {
                                                    var edocFrom = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id).FirstOrDefault();
                                                    var edoc = new SysImageDetail()
                                                    {
                                                        Id = Guid.NewGuid(),
                                                        BillingNo = billingNo,
                                                        BillingType = "Settlement",
                                                        DatetimeCreated = DateTime.Now,
                                                        DatetimeModified = DateTime.Now,
                                                        DepartmentId = currentUser.DepartmentId,
                                                        ExpiredDate = null,
                                                        GroupId = currentUser.GroupId,
                                                        DocumentTypeId = docTypeId,
                                                        JobId = getJobId(x.jobNo, x.tranType),
                                                        SysImageId = img.Id,
                                                        SystemFileName = edocFrom.SystemFileName,
                                                        UserFileName = img.Name,
                                                        UserCreated = currentUser.UserName,
                                                        UserModified = currentUser.UserName,
                                                        OfficeId = currentUser.OfficeID,
                                                        Source = "Settlement",
                                                        Hblid = null,
                                                        Note = null,
                                                        GenEdocId = edocFrom.Id
                                                    };
                                                    edocs.Add(edoc);
                                                }
                                            }
                                        });
                                    }
                                }
                            });
                        });
                        break;
                    case "SOA":
                        var jobSMSOA = _surRepo.Get(x => x.SettlementCode == billingNo);
                        var jobDetailSOA = jobSMSOA.GroupBy(x => new { x.JobNo }).Select(x => x.FirstOrDefault().JobNo).ToList();
                        var charges = _surRepo.Get(x => jobDetailSOA.Contains(x.JobNo)).Select(x => new { x.PaySoano, x.Soano, x.Type, x.TransactionType, x.JobNo });
                        charges.ToList().ForEach(x =>
                        {
                            var soa = new List<AcctSoa>();
                            if (x.TransactionType == "BUY")
                            {
                                soa = _soaRepo.Get(z => z.Soano == x.PaySoano).ToList();
                            }
                            else if (x.TransactionType == "SELL")
                            {
                                soa = _soaRepo.Get(z => z.Soano == x.Soano).ToList();
                            }
                            else
                            {
                                soa = _soaRepo.Get(z => z.Soano == x.Soano || z.Soano == x.PaySoano).ToList();
                            }
                            if (soa.Count() > 0)
                            {
                                soa.ForEach(s =>
                                {
                                    var images = _sysImageRepo.Get(z => z.ObjectId == s.Id && z.Folder == "SOA");
                                    images.ToList().ForEach(img =>
                                    {
                                        if (img != null)
                                        {
                                            var edocExist = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id && z.BillingNo == billingNo && z.Source == "Settlement").FirstOrDefault();
                                            if (edocExist == null)
                                            {
                                                int docTypeId = 0;
                                                if (advSM != null)
                                                {
                                                    docTypeId = _attachFileTemplateRepo.Get(z => z.TransactionType == x.TransactionType && (z.Code == "SM_SOA" && z.AccountingType == "ADV-Settlement")).FirstOrDefault().Id;
                                                }
                                                else
                                                {
                                                    docTypeId = _attachFileTemplateRepo.Get(z => z.TransactionType == x.TransactionType && (z.Code == "SM_SOA" && z.AccountingType == "Settlement")).FirstOrDefault().Id;
                                                }
                                                var edocFrom = _sysImageDetailRepo.Get(z => z.SysImageId == img.Id).FirstOrDefault();
                                                var edoc = new SysImageDetail()
                                                {
                                                    Id = Guid.NewGuid(),
                                                    BillingNo = billingNo,
                                                    BillingType = "Settlement",
                                                    DatetimeCreated = DateTime.Now,
                                                    DatetimeModified = DateTime.Now,
                                                    DepartmentId = currentUser.DepartmentId,
                                                    ExpiredDate = null,
                                                    GroupId = currentUser.GroupId,
                                                    DocumentTypeId = docTypeId,
                                                    JobId = getJobId(x.JobNo, x.TransactionType),
                                                    SysImageId = img.Id,
                                                    SystemFileName = edocFrom != null ? edocFrom.SystemFileName : img.Name,
                                                    UserFileName = img.Name,
                                                    UserCreated = currentUser.UserName,
                                                    UserModified = currentUser.UserName,
                                                    OfficeId = currentUser.OfficeID,
                                                    Source = "Settlement",
                                                    Hblid = null,
                                                    Note = null,
                                                    GenEdocId = edocFrom != null ? edocFrom.Id : Guid.Empty
                                                };
                                                edocs.Add(edoc);
                                            }
                                        }
                                    });
                                });
                            }
                        });
                        break;
                    default: break;
                }
                if (edocs.Count == 0)
                {
                    return new HandleState("Not found file");
                }
                var hs = _sysImageDetailRepo.Add(edocs, false);
                if (hs.Success)
                {
                    _sysImageDetailRepo.SubmitChanges();
                    return new HandleState(hs.Success, hs.Message);
                }
                return new HandleState("Something Wrong");
            }
            catch (Exception ex)
            {
                return new HandleState(ex.Message);
            }
        }

        private Guid getJobId(string jobNo, string trantype)
        {
            if (trantype == "CL"|| trantype == "TK")
            {
                var ops = _opsTranRepo.Get(x => x.JobNo == jobNo);
                return ops.FirstOrDefault().Id;
            }
            var cs = _cstranRepo.Get(x => x.JobNo == jobNo);
            return cs.FirstOrDefault().Id;
        }

        private bool checEdocType(Guid imageId, int docType)
        {
            var edoc = _sysImageDetailRepo.Get(x => x.SysImageId == imageId).FirstOrDefault();
            if (edoc != null)
            {
                if (edoc.DocumentTypeId == docType)
                {
                    return true;
                }
            }
            return false;
        }
        public async Task<bool> CheckAllowSettleEdocSendRequest(Guid settleId)
        {
            var settle = await _setleRepo.GetAsync(x => settleId == x.Id);
            var docType = await _sysImageDetailRepo.GetAsync(x => x.BillingNo == settle.FirstOrDefault().SettlementNo && x.BillingType == "Settlement");
            var docTypeId = docType.GroupBy(x => x.DocumentTypeId).Select(x => x.FirstOrDefault().DocumentTypeId).ToList();
            var attAdv = await _attachFileTemplateRepo.GetAsync(x => x.Type == "Accountant" && (x.AccountingType == "ADV-Settlement"));
            var attAdvSm = attAdv.Select(x => (int?)x.Id).ToList();
            var diff1 = docTypeId.Except(attAdvSm);
            if (diff1.Count() > 0)
            {
                var attSmSrc = await _attachFileTemplateRepo.GetAsync(x => x.Type == "Accountant" && (x.AccountingType == "Settlement"));
                var attSm = attSmSrc.Select(x => (int?)x.Id).ToList();
                var diff2 = docTypeId.Except(attSm);
                if (diff2.Count() > 0)
                {
                    return false;
                }
            }
            return true;
        }

        private List<Guid?> FilterEdocForJob(string billingNo, bool fromRep)
        {
            //var currenUser = currentUser;
            var result = new List<Guid?>();
            var imageForJob = _sysImageDetailRepo.Get(x => x.BillingNo == billingNo).GroupBy(x => x.SysImageId);
            if (fromRep)
            {
                imageForJob.ToList().ForEach(x =>
                {
                    if (x.Count() > 2)
                    {
                        result.Add(x.FirstOrDefault().SysImageId);
                    }
                });
            }
            else
            {
                imageForJob.ToList().ForEach(x =>
                {
                    if (x.Count() > 0)
                    {
                        result.Add(x.FirstOrDefault().SysImageId);
                    }
                });
            }
            return result;
        }

        private string getBillingId(string billingType, string billingNo)
        {
            switch (billingType)
            {
                case "Settlement":
                    return _setleRepo.Get(x => x.SettlementNo == billingNo).FirstOrDefault().Id.ToString();
                case "Advance":
                    return _advRepo.Get(x => x.AdvanceNo == billingNo).FirstOrDefault().Id.ToString();
                case "SOA":
                    return _soaRepo.Get(x => x.Soano == billingNo).FirstOrDefault().Id;
                default: return null;
            }
        }


        private List<SysImageDetail> GetListImageByAcc(string billingType, string billingNo, bool fromRep)
        {
            var imgIds = FilterEdocForJob(billingNo, fromRep);
            var images = new List<Guid>();
            var billingId = getBillingId(billingType, billingNo);
            switch (billingType)
            {
                case "Settlement":
                    images = _sysImageRepo.Get(x => x.ObjectId == billingId && imgIds.Contains(x.Id)).Select(x => x.Id).ToList();
                    break;
                case "Advance":
                    images = _sysImageRepo.Get(x => x.ObjectId == billingId && imgIds.Contains(x.Id)).Select(x => x.Id).ToList();
                    break;
                case "SOA":
                    images = _sysImageRepo.Get(x => x.ObjectId == billingId && imgIds.Contains(x.Id)).Select(x => x.Id).ToList();
                    break;
                default: return null;
            }
            var result = _sysImageDetailRepo.Get(x=> images.Contains((Guid)x.SysImageId)).GroupBy(x => x.SysImageId).Select(x => x.FirstOrDefault()).ToList();
            return _sysImageDetailRepo.Get(x => images.Contains((Guid)x.SysImageId)).GroupBy(x => x.SysImageId).Select(x => x.FirstOrDefault()).ToList();
        }

        public async Task<HandleState> UpdateEdocByAcc(EdocAccUpdateModel model)
        {
            var hsDel = new HandleState();
            var result=  new HandleState(true,"Update EDoc Success");
            if (hsDel.Success)
            {
                var listEdoc = new List<SysImageDetail>();
                var listImage = GetListImageByAcc(model.BillingType, model.BillingNo, model.FromRep);
                listImage.ForEach(img =>
                {
                    model.ListAdd.ForEach(x =>
                    {
                        var edoc = new SysImageDetail()
                        {
                            Id = Guid.NewGuid(),
                            BillingNo = model.BillingNo,
                            BillingType = model.BillingType,
                            DatetimeCreated = DateTime.Now,
                            DocumentTypeId = img.DocumentTypeId,
                            JobId = x,
                            Source = model.BillingType,
                            SystemFileName = img.SystemFileName,
                            SysImageId = img.SysImageId,
                            UserCreated = img.UserCreated,
                            UserFileName = img.UserFileName,
                        };
                        listEdoc.Add(edoc);
                        if (model.FromRep)
                        {
                            var itl = _opsTranRepo.Get(g => g.ReplicatedId == edoc.JobId);
                            var itlId = itl.FirstOrDefault().Id;
                            var edocClone = new SysImageDetail
                            {
                                Id = Guid.NewGuid(),
                                BillingNo = model.BillingNo,
                                BillingType = model.BillingType,
                                DatetimeCreated = DateTime.Now,
                                DocumentTypeId = img.DocumentTypeId,
                                JobId = itlId,
                                Source = model.BillingType,
                                SystemFileName = img.SystemFileName,
                                SysImageId = img.SysImageId,
                                UserCreated = img.UserCreated,
                                UserFileName = img.UserFileName,
                            };
                            listEdoc.Add(edocClone);
                        }
                    });
                });
                result = await _sysImageDetailRepo.AddAsync(listEdoc);
                if (!result.Success)
                {
                    result = new HandleState("Add Edoc Wrrong");
                }
                else
                {
                    if (model.ListDel.Count == 0)
                    {
                        hsDel = new HandleState(true, "Don't have Edoc to Delete");
                    }
                    else
                    {
                        hsDel = await _sysImageDetailRepo.DeleteAsync(x => x.BillingNo == model.BillingNo && model.ListDel.Contains((Guid)x.JobId));
                    }
                }
            }
            return result;
        }

        public async Task<List<SysImageDetailModel>> GetProofOfDeliveryAttachedFiles(string transactionType, Guid jobId, Guid? hblId)
        {
            var docType = await _attachFileTemplateRepo.Get(x => x.Code == "POD" && x.TransactionType == transactionType).FirstOrDefaultAsync();
            var listEdocFile = await _sysImageDetailRepo.GetAsync(x => x.JobId == jobId && x.DocumentTypeId == docType.Id);
            var listFileReturn = listEdocFile.Where(x => x.Hblid == hblId || x.Hblid == Guid.Empty);

            var result = (from file in listFileReturn
                          join image in _sysImageRepo.Get() on file.SysImageId equals image.Id
                          join template in _attachFileTemplateRepo.Get() on file.DocumentTypeId equals template.Id into gjTemplate
                          from template in gjTemplate.DefaultIfEmpty()
                          select new SysImageDetailModel
                          {
                              BillingNo = file.BillingNo,
                              BillingType = file.BillingNo,
                              DatetimeCreated = file.DatetimeCreated,
                              DatetimeModified = file.DatetimeModified,
                              DepartmentId = file.DepartmentId,
                              DocumentTypeId = file.DocumentTypeId,
                              ExpiredDate = file.ExpiredDate,
                              GroupId = file.GroupId,
                              Hblid = file.Hblid,
                              Id = file.Id,
                              JobId = file.JobId,
                              OfficeId = file.OfficeId,
                              Source = file.Source,
                              SysImageId = file.SysImageId,
                              SystemFileName = file.SystemFileName,
                              UserCreated = file.UserCreated,
                              UserFileName = file.UserFileName,
                              UserModified = file.UserModified,
                              ImageUrl = image != null ? image.Url : null,
                              Note = file.Note,
                              TransactionType = transactionType,
                              DocumentCode = template?.Code
                          });

            return result.OrderByDescending(x => x.DatetimeCreated).ToList();
        }
    }
}
