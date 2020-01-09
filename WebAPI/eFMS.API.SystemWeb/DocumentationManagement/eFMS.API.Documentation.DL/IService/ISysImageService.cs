using eFMS.API.Common;
using eFMS.API.Documentation.DL.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Documentation.DL.IService
{
    public interface ISysImageService
    {
        Task<ResultHandle> UploadDocumentationFiles(DocumentFileUploadModel model);
    }
}
