using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.Common;
using eFMS.API.Common.Helpers;
using eFMS.IdentityServer.DL.UserManager;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Net;
using eFMS.API.SystemFileManagement.Service.Models;
using eFMS.API.SystemFileManagement.Service.Common;
using Amazon.Runtime;
using Amazon.S3;
using Amazon;
using Amazon.S3.Model;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class AWSS3Service : IAWSS3Service
    {
        private ICurrentUser currentUser;
        private AmazonS3Client _client;
        private IContextBase<SysImage> _sysImageRepo;
        private readonly string _awsAccessKeyId;
        private readonly string _bucketName;
        private readonly string _awsSecretAccessKey;
        private readonly string _domainTest;

        public AWSS3Service(IContextBase<SysImage> SysImageRepo, ICurrentUser currentUser)
        {
            this.currentUser = currentUser;

            _awsAccessKeyId = DbHelper.DbHelper.AWSS3AccessKeyId;
            _bucketName = DbHelper.DbHelper.AWSS3BucketName;
            _awsSecretAccessKey = DbHelper.DbHelper.AWSS3SecretAccessKey;
            _domainTest = DbHelper.DbHelper.AWSS3DomainApi;

            var credentials = new BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);
            _client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
            _sysImageRepo = SysImageRepo;
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
                    Key = key,
                };

                DeleteObjectResponse rsDelete = _client.DeleteObjectAsync(request).Result;
                if (rsDelete != null)
                    result = await _sysImageRepo.DeleteAsync(x => x.Id == id);
                return result;
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        public async Task<List<SysImage>> GetFileSysImage(string moduleName, string folder, Guid id, string child = null)
        {
            var res = await _sysImageRepo.GetAsync(x => x.Folder == moduleName + "/" + folder
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
                    string fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    key = model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + file.FileName;

                    var putRequest = new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = key,
                        InputStream = file.OpenReadStream(),
                    };

                    PutObjectResponse putObjectResponse = _client.PutObjectAsync(putRequest).Result;
                    if (putObjectResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        string urlImage = _domainTest + "/" + model.ModuleName + "/" + model.FolderName + "/" + model.Id + "/" + file.FileName;
                        var sysImage = new SysImage
                        {
                            Id = Guid.NewGuid(),
                            Url = urlImage,
                            Name = file.FileName,
                            Folder = model.ModuleName + "/" + model.FolderName,
                            ObjectId = model.Id.ToString(),
                            UserCreated = currentUser.UserName,
                            UserModified = currentUser.UserName,
                            DateTimeCreated = DateTime.Now,
                            DatetimeModified = DateTime.Now,
                            ChildId = model.Child
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
        public async Task<HandleState> OpenFile(string moduleName, string folder, Guid objId, string fileName)
        {
            try
            {
                var lst = await _sysImageRepo.GetAsync(x => x.ObjectId == objId.ToString());
                //var it = lst.Where(x => x.Id == id).FirstOrDefault();
                if (lst == null) { return new HandleState("Not found data"); }

                var key = moduleName + "/" + folder + "/" + objId + "/" + fileName;

                var request = new GetObjectRequest()
                {
                    BucketName = _bucketName,
                    Key = key
                };
                GetObjectResponse response = _client.GetObjectAsync(request).Result;
                if (response.HttpStatusCode != HttpStatusCode.OK) { return new HandleState("Stream file error"); }
                return new HandleState(true, response.ResponseStream);
            }
            catch (Exception ex)
            {
                return new HandleState(ex.ToString());
            }
        }
        public async Task<HandleState> CreateFileZip(FileDowloadZipModel model)
        {
            HandleState result = new HandleState();
            try
            {
                var lst = await _sysImageRepo.GetAsync(x => x.ObjectId == model.ObjectId);
                if (lst == null) { return new HandleState("Not found data"); }
                var files = new List<InMemoryFile>();
                foreach (var it in lst)
                {
                    var key = it.Folder + "/" + model.ObjectId + "/" + it.Name;
                    var request = new GetObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = key
                    };
                    GetObjectResponse response = _client.GetObjectAsync(request).Result;
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


        byte[] GetZipArchive(List<InMemoryFile> files)
        {
            byte[] archiveFile;
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);

                        using (var zipStream = zipArchiveEntry.Open()) 
                        {
                            zipStream.Write(file.Content, 0, file.Content.Length);
                        } 
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return archiveFile;
        }
        class InMemoryFile
        {
            public string FileName { get; set; }

            public byte[] Content { get; set; }
        }
        byte[] streamToByteArray(Stream input)
        {
            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return ms.ToArray();
        }
    }
}
