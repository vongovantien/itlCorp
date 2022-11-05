using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.IService
{
    public interface IAttachFileTemplateService : IRepositoryBase<SysAttachFileTemplate, SysAttachFileTemplateModel>
    {
        Task<HandleState> Import(List<SysAttachFileTemplate> list);
        Task<List<DocumentTypeModel>> GetDocumentType(string transactionType,string billingId);
    }
}
