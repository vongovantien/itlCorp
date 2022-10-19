using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using eFMS.API.SystemFileManagement.DL.IService;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class S3Service : IS3Service
    {
        private AmazonS3Client _client;
        private readonly string _awsAccessKeyId;
        private readonly string _bucketName;
        private readonly string _awsSecretAccessKey;
        public S3Service()
        {
            _awsAccessKeyId = DbHelper.DbHelper.AWSS3AccessKeyId;
            _bucketName = DbHelper.DbHelper.AWSS3BucketName;
            _awsSecretAccessKey = DbHelper.DbHelper.AWSS3SecretAccessKey;

            var credentials = new BasicAWSCredentials(_awsAccessKeyId, _awsSecretAccessKey);
            _client = new AmazonS3Client(credentials, RegionEndpoint.USEast1);
        }

        public async Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request)
        {
            return await _client.DeleteObjectAsync(request);
        }

        public async Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request)
        {
            return await _client.PutObjectAsync(request);
        }

        public async Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request)
        {
            return await _client.GetObjectAsync(request);
        }

        public async Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request)
        {
            return await _client.CopyObjectAsync(request);
        }

        public async Task<ListObjectsResponse> GetListObjectAsync(ListObjectsRequest request)
        {
            return await _client.ListObjectsAsync(request);
        }
    }
}
