using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Model.Internal.MarshallTransformations;
using eFMS.API.Common;
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
using System.Net;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class EDocService : IEDocService
    {
        private ICurrentUser currentUser;
        private AmazonS3Client _client;
        private IContextBase<SysImage> _sysImageRepo;
        private IContextBase<SysImageDetail> _sysImageDetailRepo;
        private readonly string _awsAccessKeyId;
        private readonly string _bucketName;
        private readonly string _awsSecretAccessKey;
        private readonly string _domainTest;
        private readonly IOptions<ApiUrl> _apiUrl;
        private IContextBase<SysAttachFileTemplate> _attachFileTemplateRepo;

        public EDocService(IContextBase<SysImage> SysImageRepo, IContextBase<SysAttachFileTemplate> attachFileTemplateRepo, ICurrentUser currentUser, IOptions<ApiUrl> apiUrl, IContextBase<SysImageDetail> sysImageDetailRepo)
        {
            this.currentUser = currentUser;

            _awsAccessKeyId = DbHelper.DbHelper.AWSS3AccessKeyId;
            _bucketName = DbHelper.DbHelper.AWSS3BucketName;
            _awsSecretAccessKey = DbHelper.DbHelper.AWSS3SecretAccessKey;
            _domainTest = DbHelper.DbHelper.AWSS3DomainApi;
            _attachFileTemplateRepo = attachFileTemplateRepo;
            var credentials = new BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);
            _client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
            _sysImageRepo = SysImageRepo;
            _apiUrl = apiUrl;
            _sysImageDetailRepo = sysImageDetailRepo;
        }

        public async Task<HandleState> GetDocumentType(string transactionType)
        {
            try
            {
                var lst = await _attachFileTemplateRepo.GetAsync(x => x.TransactionType == transactionType);
                if (lst == null) { return new HandleState("Not found data"); }
                return new HandleState(true, lst);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        private DateTime ConvertExpiredDate(int TimeStorage, string TypeStorage)
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
        public async Task<HandleState> PostEDocAsync(EDocUploadModel model, List<IFormFile> files)
        {
            HandleState result = new HandleState();
            try
            {
                var key = "";
                List<SysImage> list = new List<SysImage>();
                List<SysImageDetail> listDetail = new List<SysImageDetail>();
                var edocMapModel = MapEDocUploadModel(model, files);
                foreach (var edoc in edocMapModel.EDocFilesMap)
                {
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
                        var attachTemplate = _attachFileTemplateRepo.Get(x => x.Code == edoc.Code && x.TransactionType == edoc.TransactionType).FirstOrDefault();
                        var sysImageDetail = new SysImageDetail
                        {
                            Id = Guid.NewGuid(),
                            BillingNo = edoc.BillingNo,
                            BillingType = edoc.BillingType,
                            DatetimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            DepartmentId = currentUser.DepartmentId,
                            ExpiredDate = ConvertExpiredDate((int)attachTemplate.StorageTime, attachTemplate.StorageType),
                            GroupId = currentUser.GroupId,
                            Hblid = edoc.HBL,
                            JobId = edoc.JobId,
                            OfficeId = currentUser.OfficeID,
                            SystemFileName = edoc.AliasName,
                            UserCreated = currentUser.UserName,
                            UserFileName = fileName,
                            UserModified = currentUser.UserName,
                            DocumentTypeId = attachTemplate.Id,
                            SysImageId = imageID,
                            Source=model.FolderName
                        };
                        list.Add(sysImage);
                        listDetail.Add(sysImageDetail);
                    }
                }
                if (list.Count > 0)
                {
                    var hs = await _sysImageDetailRepo.AddAsync(listDetail);
                    if (hs.Success)
                    {
                        result = await _sysImageRepo.AddAsync(list);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        public async Task<HandleState> GetEDocByJob(Guid jobID, string transactionType)
        {
            try
            {
                var lstTran = await _attachFileTemplateRepo.GetAsync(x => x.TransactionType == transactionType);
                var lst = await _sysImageDetailRepo.GetAsync(x => x.JobId == jobID);
                if (lst == null) { return new HandleState("Not found data"); }
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
                    };
                    lstImageMD.Add(imageModel);
                });
                lstImageMD.GroupBy(x => x.DocumentTypeId).ToList().ForEach(x =>
                {
                    result.Where(y => y.documentType.Id == x.FirstOrDefault().DocumentTypeId).FirstOrDefault().EDocs = x.ToList();
                });
                return new HandleState(true, result);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
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
    }
}
