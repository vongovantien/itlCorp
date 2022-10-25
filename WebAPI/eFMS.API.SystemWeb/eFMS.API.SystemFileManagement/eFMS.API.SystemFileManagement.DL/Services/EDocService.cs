using Amazon.S3.Model;
using eFMS.API.Common;
using eFMS.API.Common.Globals;
using eFMS.API.Common.Helpers;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
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
        private eFMSDataContextDefault DC => (eFMSDataContextDefault)_sysImageDetailRepo.DC;

        public EDocService(IContextBase<SysImage> SysImageRepo,
            IContextBase<SysAttachFileTemplate> attachFileTemplateRepo,
            IS3Service client,
            ICurrentUser currentUser, 
            IOptions<ApiUrl> apiUrl, 
            IContextBase<SysImageDetail> sysImageDetailRepo,
            IContextBase<OpsTransaction> opsTranRepo,
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
        public async Task<HandleState> PostEDocAsync(EDocUploadModel model, List<IFormFile> files,string type)
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
                        var attachTemplate = _attachFileTemplateRepo.Get(x => x.Code == edoc.Code && x.TransactionType == edoc.TransactionType).FirstOrDefault();
                        if (type == "Job")
                        {
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
                            await _sysImageDetailRepo.AddAsync(sysImageDetail, false);
                        }
                        else
                        {
                            string bilingNo = string.Empty;
                            var jobIds = GetJobFromAccountant(type, edoc.BillingId);

                            switch (type)
                            {
                                case "Advance":
                                    var adv = DC.AcctAdvancePayment.FirstOrDefault(x => x.Id.ToString() == edoc.BillingId);
                                    bilingNo = adv.AdvanceNo;
                                    break;
                                case "Settlement":
                                    bilingNo = DC.AcctSettlementPayment.FirstOrDefault(x => x.Id.ToString() == edoc.BillingId).SettlementNo;
                                    break;
                                case "SOA":
                                    var soa = DC.AcctSoa.FirstOrDefault(x => x.Id.ToString() == edoc.BillingId);
                                    if (soa != null)
                                    {
                                        bilingNo = soa.Soano;
                                    }
                                    break;
                                default: break;
                            };
                            if (jobIds.Count > 0)
                            {
                                foreach (var Id in jobIds)
                                {
                                    var imageDetail = new SysImageDetail
                                    {
                                        SysImageId = imageID,
                                        BillingType = type,
                                        BillingNo = bilingNo,
                                        DatetimeCreated = DateTime.Now,
                                        DatetimeModified = DateTime.Now,
                                        Id = Guid.NewGuid(),
                                        JobId = (Guid)edoc.JobId,
                                        UserCreated = sysImage.UserCreated,
                                        SystemFileName = sysImage.Name,
                                        UserFileName = sysImage.Name,
                                        UserModified = sysImage.UserCreated,
                                        Source = type,
                                        ExpiredDate = attachTemplate.StorageTime == null ? null : ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                                        DocumentTypeId = attachTemplate.Id,
                                        Note = edoc.Note,
                                        GroupId = currentUser.GroupId,
                                    };

                                    await _sysImageDetailRepo.AddAsync(imageDetail, false);
                                }
                            }
                        }
                        hs = _sysImageDetailRepo.SubmitChanges();
                    }   
                    if (hs.Success)
                    {
                        result = await _sysImageRepo.AddAsync(list);
                        _sysImageRepo.SubmitChanges();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        public async Task<List<EDocGroupByType>> GetEDocByAccountant(string billingNo, string transactionType)
        {
            var lstTran = await _attachFileTemplateRepo.GetAsync(x => x.TransactionType == transactionType || x.Type == "Accountant");
            var lst = await _sysImageDetailRepo.GetAsync(x => x.BillingNo == billingNo&&x.BillingType==transactionType);
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
            lst.ForEach(x =>
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
                    ImageUrl = _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault().Url,
                    HBLNo = x.Hblid != null ? _attachFileTemplateRepo.Get(z => z.Id == x.DocumentTypeId).FirstOrDefault().TransactionType == "CL" ? _opsTranRepo.Get(y => y.Id == x.Hblid).FirstOrDefault().Hwbno :
                    _tranDeRepo.Get(y => y.Id == x.Hblid).FirstOrDefault().Hwbno : null,
                };
                lstImageMD.Add(imageModel);
            });
            lstImageMD.GroupBy(x => x.DocumentTypeId).ToList().ForEach(x =>
            {
                if (result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault() != null)
                {
                    result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault().EDocs = x.ToList();
                };
            });
            return result.Where(x => x.documentType.Type == "Accountant" && x.EDocs != null).ToList();
        }
        public async Task<List<EDocGroupByType>> GetEDocByJob(Guid jobID, string transactionType)
        {
            var lstTran = await _attachFileTemplateRepo.GetAsync(x => x.TransactionType == transactionType||x.Type== "Accountant");
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
                    ImageUrl = _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault()!=null ? _sysImageRepo.Get(z => z.Id == x.SysImageId).FirstOrDefault().Url:null,
                    HBLNo = x.Hblid!=null?_attachFileTemplateRepo.Get(z => z.Id == x.DocumentTypeId).FirstOrDefault().TransactionType == "CL" ? _opsTranRepo.Get(y => y.Id == x.Hblid).FirstOrDefault()!=null?
                    _opsTranRepo.Get(y => y.Id == x.Hblid).FirstOrDefault().Hwbno:null :
                    _tranDeRepo.Get(y => y.Id == x.Hblid).FirstOrDefault()!=null? _tranDeRepo.Get(y => y.Id == x.Hblid).FirstOrDefault().Hwbno:null : null,
                };
                lstImageMD.Add(imageModel);
            });
            lstImageMD.GroupBy(x => x.DocumentTypeId).ToList().ForEach(x =>
            {
                if(result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault() != null)
                {
                    result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault().EDocs = x.ToList();
                };
            });
            var resultAcc = result.Where(x => x.documentType.Type == "Accountant" && x.EDocs != null).ToList();
            var resultGen = result.Where(x => x.documentType.Type != "Accountant").ToList();
            result= resultGen.Concat(resultAcc).ToList();
            return result;
        }

        public async Task<HandleState> DeleteEdoc(Guid edocId)
        {
            HandleState result = new HandleState();
            try
            {
                var edoc = _sysImageDetailRepo.Get(x => x.Id == edocId).FirstOrDefault();
                var lst = await _sysImageRepo.GetAsync(x => x.Id == edoc.SysImageId);
                if (edoc.Source == "Job")
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
                        result = await _sysImageRepo.DeleteAsync(x => x.Id == edoc.SysImageId);
                    if (result.Success)
                    {
                        await _sysImageDetailRepo.DeleteAsync(x => x.Id == edocId);
                    }
                }
                else
                {
                    await _sysImageDetailRepo.DeleteAsync(x => x.Id == edocId);
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
                    var hs = _sysImageDetailRepo.UpdateAsync(edoc, x => x.Id == edoc.Id, false);
                }
                var result = _sysImageDetailRepo.SubmitChanges();
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        private List<TransctionTypeJobModel> GetJobFromAccountant(string billingType, string billingId)
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
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = "CL" });
                                }
                            }
                            else
                            {
                                var csJob = DC.CsTransaction.FirstOrDefault(x => x.JobNo == item);
                                if (csJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType });
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
                                transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = "CL" });
                            }
                        }
                        else
                        {
                            var csJob = DC.CsTransaction.FirstOrDefault(x => x.JobNo == item);
                            if (csJob != null)
                            {
                                transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType });
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
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = opsJob.Id, TransactionType = "CL" });
                                }
                            }
                            else
                            {
                                var csJob = DC.CsTransaction.FirstOrDefault(x => x.JobNo == item);
                                if (csJob != null)
                                {
                                    transctionTypeJobModels.Add(new TransctionTypeJobModel { JobId = csJob.Id, TransactionType = csJob.TransactionType });
                                }
                            }
                        }
                    }
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
            var jobIds = GetJobFromAccountant(billingType, billingId);
            string _codeAttachFile = string.Empty;

            switch (billingType)
            {
                case "Advance":
                    var adv = DC.AcctAdvancePayment.FirstOrDefault(x => x.Id.ToString() == billingId);
                    bilingNo = adv.AdvanceNo;
                    _codeAttachFile = "AD";
                    break;
                case "Settlement":
                    bilingNo = DC.AcctSettlementPayment.FirstOrDefault(x => x.Id.ToString() == billingId).SettlementNo;
                    _codeAttachFile = "SM";
                    break;
                case "SOA":
                    var soa = DC.AcctSoa.FirstOrDefault(x => x.Id.ToString() == billingId);
                    if (soa != null)
                    {
                        bilingNo = soa.Soano;
                    }
                    _codeAttachFile = "SOA";
                    break;
                default: break;
            };
            if (jobIds.Count > 0)
            {
                var sysImage = _sysImageRepo.Get(x => x.Id == imageId)?.FirstOrDefault();
                foreach (var item in jobIds)
                {
                    var imageDetail = new SysImageDetail
                    {
                        SysImageId = imageId,
                        BillingType = billingType,
                        BillingNo = bilingNo,
                        DatetimeCreated = DateTime.Now,
                        DatetimeModified = DateTime.Now,
                        Id = Guid.NewGuid(),
                        JobId = item.JobId,
                        UserCreated = sysImage.UserCreated,
                        SystemFileName = sysImage.Name,
                        UserFileName = sysImage.Name,
                        UserModified = sysImage.UserCreated,
                        Source = billingType,
                        DocumentTypeId = GetDocumentTypeWithTypeAttachTemplate("Accountant", item.TransactionType, _codeAttachFile, billingType)?.FirstOrDefault()?.Id
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
    }
}
