using Amazon.S3.Model;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.IService
{
    public interface IS3Service
    {
        Task<DeleteObjectResponse> DeleteObjectAsync(DeleteObjectRequest request);
        Task<PutObjectResponse> PutObjectAsync(PutObjectRequest request);
        Task<GetObjectResponse> GetObjectAsync(GetObjectRequest request);
        Task<CopyObjectResponse> CopyObjectAsync(CopyObjectRequest request);
        Task<ListObjectsResponse> GetListObjectAsync(ListObjectsRequest request);
    }
}
