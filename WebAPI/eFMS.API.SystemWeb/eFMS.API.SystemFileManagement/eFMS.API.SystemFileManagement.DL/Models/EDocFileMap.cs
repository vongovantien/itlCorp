using Microsoft.AspNetCore.Http;

namespace eFMS.API.SystemFileManagement.DL.Models
{
    public class EDocFileMap : EDocFile
    {
        public IFormFile File { get; set; }
    }
}
