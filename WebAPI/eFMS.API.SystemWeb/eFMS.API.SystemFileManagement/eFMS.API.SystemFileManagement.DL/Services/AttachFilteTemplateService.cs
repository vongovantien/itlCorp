using AutoMapper;
using eFMS.API.SystemFileManagement.DL.IService;
using eFMS.API.SystemFileManagement.DL.Models;
using eFMS.API.SystemFileManagement.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace eFMS.API.SystemFileManagement.DL.Services
{
    public class AttachFilteTemplateService : RepositoryBase<SysAttachFileTemplate, SysAttachFileTemplateModel>, IAttachFileTemplateService
    {
        public AttachFilteTemplateService(IContextBase<SysAttachFileTemplate> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public async Task<HandleState> Import(List<SysAttachFileTemplate> list)
        {
            if (list.Count == 0)
            {
                return new HandleState();
            }

            var hs = await DataContext.AddAsync(list);

            return hs;
        }

        public async Task<List<SysAttachFileTemplate>> GetDocumentType(string transactionType, string billingNo)
        {
            switch (transactionType)
            {
                case "SOA":
                    return await DataContext.GetAsync(x => x.Type == "Accountant" && x.AccountingType == "SOA" && x.Code != "OTH");
                case "Settlement":
                    var SMCode = await DataContext.GetAsync(x => x.Type == "Accountant" && x.Code != "OTH" && (x.AccountingType == "Settlement" || x.AccountingType == "ADV-Settlement"));
                    return SMCode.GroupBy(x => x.Code).Select(x => x.FirstOrDefault()).ToList();
                case "Advace":
                    return await DataContext.GetAsync(x => x.Type == "Accountant" && x.AccountingType == "Advance" && x.Code != "OTH");
                default:
                    return await DataContext.GetAsync(x => x.Type != "Accountant" && x.TransactionType == transactionType && x.Code != "OTH");
            }
        }
    }
}
