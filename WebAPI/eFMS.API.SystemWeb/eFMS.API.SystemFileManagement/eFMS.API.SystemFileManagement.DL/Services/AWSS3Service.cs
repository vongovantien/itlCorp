﻿using Amazon.S3.Model;
using eFMS.API.Common;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static eFMS.API.Common.Helpers.FileHelper;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class AWSS3Service : IAWSS3Service
    {
        private ICurrentUser currentUser;
        private IContextBase<SysImage> _sysImageRepo;
        private IContextBase<SysImageDetail> _sysImageDetailRepo;
        private readonly string _bucketName;
        private readonly string _domainTest;
        private readonly IOptions<ApiUrl> _apiUrl;
        private IEDocService edocService;
        private IS3Service _client;
        private readonly IContextBase<AcctSoa> _soaRepo;
        private readonly IContextBase<AcctSettlementPayment> _settleRepo;
        private readonly IContextBase<AcctAdvancePayment> _advRepo;

        public AWSS3Service(IContextBase<SysImage> SysImageRepo,
            IContextBase<SysImageDetail> sysImageDetailRepo,
            IContextBase<AcctSoa> soaRepo,
            IContextBase<AcctSettlementPayment> settleRepo,
            IContextBase<AcctAdvancePayment> advRepo,
            ICurrentUser currentUser,
            IOptions<ApiUrl> apiUrl,
            IS3Service s3,
            IEDocService edoc)
        {
            this.currentUser = currentUser;
            _client = s3;
            _bucketName = DbHelper.DbHelper.AWSS3BucketName;
            _domainTest = DbHelper.DbHelper.AWSS3DomainApi;
            _sysImageRepo = SysImageRepo;
            _apiUrl = apiUrl;
            edocService = edoc;
            _sysImageDetailRepo = sysImageDetailRepo;
            _settleRepo = settleRepo;
            _advRepo = advRepo;
            _soaRepo = soaRepo;
        }

        public async Task<HandleState> DeleteFile(string moduleName, string folder, Guid id)
        {
            HandleState result = new HandleState();
            try
            {
                var lst = await _sysImageRepo.GetAsync(x => x.Id == id);
                var it = lst.Where(x => x.Id == id).FirstOrDefault();
                if (it == null) { return new HandleState("Not found data"); }
                var key = moduleName + "/" + folder + "/" + it.ObjectId + "/" + it.Name;

                DeleteObjectRequest request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = it.KeyS3,
                };

                DeleteObjectResponse rsDelete = await _client.DeleteObjectAsync(request);
                if (rsDelete != null)
                    result = await _sysImageRepo.DeleteAsync(x => x.Id == id);
                if (result.Success)
                {
                    var imageDetail = await _sysImageDetailRepo.DeleteAsync(x => x.SysImageId == id);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        public async Task<HandleState> DeleteFileFolder(string moduleName, string folderName, Guid objectId)
        {
            HandleState deleteResult = new HandleState();
            try
            {
                var listRequest = new ListObjectsRequest
                {
                    BucketName = _bucketName,
                    Prefix = $"{moduleName}/{folderName}/{objectId}"
                };
                var listObject = await _client.GetListObjectAsync(listRequest);
                if (!listObject.S3Objects.Any()) return deleteResult;
                var listFile = listObject.S3Objects.Select(x => x.Key).ToList();

                var deleteRequest = new DeleteObjectsRequest
                {
                    BucketName = _bucketName,
                    Objects = listFile.Select(key => new KeyVersion { Key = key }).ToList()
                };
                var deleteResponse = await _client.DeleteObjectsAsync(deleteRequest);

                if (deleteResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    deleteResult = _sysImageRepo.Delete(x => x.ObjectId == objectId.ToString());
                }

                return deleteResult;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        public async Task<HandleState> DeleteFileS3(string moduleName, string folder, Guid id, Guid objId)
        {
            HandleState result = new HandleState();
            try
            {
                var lst = await _sysImageRepo.GetAsync(x => x.Id == id);
                var it = lst.Where(x => x.Id == id).FirstOrDefault();
                if (it == null) { return new HandleState("Not found data"); }
                var key = moduleName + "/" + folder + "/" + objId + "/" + it.Name;

                DeleteObjectRequest request = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = key,
                };

                DeleteObjectResponse rsDelete = await _client.DeleteObjectAsync(request);
                if (rsDelete == null)
                {
                    return new HandleState(true, "Delete File S3 Wrong");
                }
                return new HandleState(true, "Delete File S3 Success");
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }

        public async Task<List<SysImage>> GetFileSysImage(string moduleName, string folder, Guid id, string child = null)
        {
            var res = await _sysImageRepo.GetAsync(x => x.Folder == folder
            && x.ObjectId == id.ToString() && x.ChildId == child);

            return res.OrderByDescending(x => x.DateTimeCreated).ToList();
        }
        public async Task<HandleState> PostObjectAsync(FileUploadModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var key = "";
                List<SysImage> list = new List<SysImage>();
                foreach (var file in model.Files)
                {
                    string fileName = RenameFileS3(Path.GetFileNameWithoutExtension(BeforeExtention(file.FileName)));

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
                        string urlImage = _domainTest + "/OpenFile/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
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
                if (list.Count > 0)
                {
                    result = await _sysImageRepo.AddAsync(list);
                }
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }



        public async Task<string> PostFileReportAsync(FileUploadModel model)
        {
            var result = string.Empty;
            try
            {
                var key = "";
                List<SysImage> list = new List<SysImage>();
                foreach (var file in model.Files)
                {
                    string fileName = Path.GetFileNameWithoutExtension(BeforeExtention(file.FileName));
                    fileName = RenameFileS3(fileName);

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
                        result = _domainTest + "/OpenFile/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
                    }
                    if (extension == ".doc")
                    {
                        result = _domainTest + "/DownloadFileAsync/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + fileName + extension;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public async Task<HandleState> OpenFile(string moduleName, string folder, Guid objId, string fileName)
        {
            try
            {
                var lst = await _sysImageRepo.GetAsync(x => x.ObjectId == objId.ToString());
                if (lst == null) { return new HandleState("Not found data"); }

                var key = moduleName + "/" + folder + "/" + objId + "/" + fileName;

                var request = new GetObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = key
                };
                GetObjectResponse response = await _client.GetObjectAsync(request);
                if (response.HttpStatusCode != HttpStatusCode.OK) { return new HandleState("Stream file error"); }
                else if (Path.GetExtension(fileName) == ".txt")
                {
                    var data = new StreamReader(response.ResponseStream, Encoding.UTF8);
                    var obj = new object();
                    obj = await data.ReadToEndAsync();
                    return new HandleState(true, obj);
                }
                return new HandleState(true, response.ResponseStream);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        public async Task<byte[]> DownloadFileAsync(string moduleName, string folder, Guid objId, string fileName)
        {
            MemoryStream ms = null;

            try
            {
                var key = moduleName + "/" + folder + "/" + objId + "/" + fileName;
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = moduleName + "/" + folder + "/" + objId + "/" + fileName
                };

                using (var response = await _client.GetObjectAsync(request))
                {
                    if (response.HttpStatusCode == HttpStatusCode.OK)
                    {
                        using (ms = new MemoryStream())
                        {
                            await response.ResponseStream.CopyToAsync(ms);
                        }
                    }
                }

                if (ms is null || ms.ToArray().Length < 1)
                    throw new FileNotFoundException(string.Format("The document '{0}' is not found", fileName));

                return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<HandleState> CreateFileZip(FileDowloadZipModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var lst = new List<SysImage>();
                if (model.FolderName.Contains("Edoc"))
                {
                    var edocs = new List<SysImageDetail>();
                    if (model.FolderName == "EdocShipment")
                    {
                        edocs = await _sysImageDetailRepo.GetAsync(x => (x.JobId.ToString() == model.ObjectId));
                    }
                    else
                    {
                        string billingNo = null;
                        switch (model.FolderName)
                        {
                            case "EdocSOA":
                                var soa = await _soaRepo.GetAsync(x => x.Id == model.ObjectId);
                                billingNo = soa.FirstOrDefault().Soano;
                                break;
                            case "EdocSettlement":
                                var settle = await _settleRepo.GetAsync(x => x.Id.ToString() == model.ObjectId);
                                billingNo = settle.FirstOrDefault().SettlementNo;
                                break;
                            case "EdocAdvance":
                                var adv = await _advRepo.GetAsync(x => x.Id.ToString() == model.ObjectId);
                                billingNo = adv.FirstOrDefault().AdvanceNo;
                                break;
                        }
                        edocs = await _sysImageDetailRepo.GetAsync(x => (x.BillingNo == billingNo));
                    }
                    var imageExist = _sysImageRepo.Get(x => x.ObjectId == model.ObjectId && !edocs.Select(z => z.SysImageId).Contains(x.Id)).Select(x => x.Id).Distinct();
                    lst = await _sysImageRepo.GetAsync(x => edocs.Select(y => y.SysImageId).Distinct().Contains(x.Id) || imageExist.Contains(x.Id));
                }
                else if (model.ChillId == null)
                {
                    lst = await _sysImageRepo.GetAsync(x => x.ObjectId == model.ObjectId);
                }
                else
                {
                    lst = await _sysImageRepo.GetAsync(x => x.ObjectId == model.ObjectId && x.ChildId == model.ChillId);
                }

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
                        var f = new InMemoryFile() { Content = streamToByteArray(response.ResponseStream), FileName = it.Name };
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

        public async Task<HandleState> CoppyObjectAsync(FileCoppyModel filecCoppyModel)
        {
            try
            {
                CopyObjectRequest request = new CopyObjectRequest
                {
                    SourceBucket = _bucketName,
                    SourceKey = filecCoppyModel.srcKey,
                    DestinationBucket = _bucketName,
                    DestinationKey = filecCoppyModel.destKey
                };
                CopyObjectResponse response = await _client.CopyObjectAsync(request);

                return new HandleState(true, response);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        private FileCoppyModel ReDirectFolder(FileCoppyModel filecCoppyModel)
        {
            if (filecCoppyModel.Type != null)
            {
                switch (filecCoppyModel.Type)
                {
                    case 1:
                        return new FileCoppyModel()
                        {
                            srcKey = "Accounting/SOA/" + filecCoppyModel.srcKey.ToLower(),
                            destKey = "Accounting/SOA/" + filecCoppyModel.destKey + "/",
                        };
                    case 2:
                        return new FileCoppyModel()
                        {
                            srcKey = "Accounting/Settlement/" + filecCoppyModel.srcKey.ToLower(),
                            destKey = "Accounting/Settlement/" + filecCoppyModel.destKey + "/",
                        };
                    case 3:
                        return new FileCoppyModel()
                        {
                            srcKey = "Accounting/Advance/" + filecCoppyModel.srcKey.ToLower(),
                            destKey = "Accounting/Advance/" + filecCoppyModel.destKey + "/",
                        };
                    default:
                        break;
                }
            }
            return filecCoppyModel;
        }

        public async Task<HandleState> MoveObjectAsync(FileCoppyModel filecCoppyModel)
        {
            try
            {
                var filecCoppyConvert = ReDirectFolder(filecCoppyModel);
                ListObjectsRequest request = new ListObjectsRequest { BucketName = _bucketName, Prefix = filecCoppyConvert.srcKey };
                var listObject = await _client.GetListObjectAsync(request);
                var listFile = listObject.S3Objects.Select(x => x.Key).ToList();
                foreach (var item in listFile)
                {
                    FileCoppyModel filecCoppy = new FileCoppyModel()
                    {
                        destKey = filecCoppyConvert.destKey + item.Split("/").Last(),
                        srcKey = item,
                    };
                    var coppied = CoppyObjectAsync(filecCoppy);
                    var images = _sysImageRepo.Get(x => x.KeyS3 == item).ToList();
                    foreach (var image in images)
                    {
                        image.KeyS3 = filecCoppyConvert.destKey + image.Name;
                        image.ObjectId = filecCoppyModel.destKey.ToLower();
                        image.Url = _apiUrl.Value.Url.ToString() + "/file/api/v1/en-Us/AWSS3/OpenFile/" + filecCoppyConvert.destKey + image.Name;
                        var updateImg = await _sysImageRepo.UpdateAsync(image, x => x.Id == image.Id);
                        if (!updateImg.Success)
                        {
                            return new HandleState(false, "Update Image Error");
                        }
                    }
                }
                return new HandleState(true, listFile);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        public async Task<string> PostFileAttacheDoc(FileUploadModel model)
        {
            try
            {
                var urlImage = "";
                List<SysImage> list = new List<SysImage>();
                foreach (var file in model.Files)
                {
                    string fileName = RenameFileS3(Path.GetFileNameWithoutExtension(BeforeExtention(file.FileName)));
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
                if (list.Count > 0)
                {
                    HandleState result = await _sysImageRepo.AddAsync(list);

                    if (result.Success)
                    {
                        foreach (var image in list)
                        {
                            var hsEdoc = edocService.MappingeDocToShipment(image.Id, image.ObjectId, image.Folder);
                        }
                    }
                }
                return urlImage;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
