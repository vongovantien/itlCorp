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

        public async Task<List<DocumentTypeModel>> GetDocumentType(string transactionType)
        {
            switch (transactionType)
            {
                case "SOA":
                    var soas = await DataContext.GetAsync(x => x.Type == "Accountant" && x.AccountingType == "SOA" && x.Code != "OTH");
                    return soas.GroupBy(x => x.Code).Select(x => new DocumentTypeModel()
                    {
                        Id= x.FirstOrDefault().Id,
                        Code=x.FirstOrDefault().Code,
                        NameEn=x.FirstOrDefault().NameEn,
                    }).ToList();
                case "Settlement":
                    var SMCode = await DataContext.GetAsync(x => x.Type == "Accountant" && x.Code != "OTH" && (x.AccountingType == "Settlement" || x.AccountingType == "ADV-Settlement"));
                    return SMCode.GroupBy(x => new { x.Code,x.AccountingType }).Select(x=>new DocumentTypeModel()
                    {
                        Id = x.FirstOrDefault().Id,
                        Code = x.FirstOrDefault().Code,
                        NameEn = x.FirstOrDefault().NameEn,
                    }).ToList();
                case "Advance":
                    var advs = await DataContext.GetAsync(x => x.Type == "Accountant" && x.AccountingType == "Advance" && x.Code != "OTH");
                    return advs.GroupBy(x => x.Code).Select(x => new DocumentTypeModel()
                    {
                        Id = x.FirstOrDefault().Id,
                        Code = x.FirstOrDefault().Code,
                        NameEn = x.FirstOrDefault().NameEn,
                    }).ToList();
                default:
                    var jobs= await DataContext.GetAsync(x => x.Type != "Accountant" && x.TransactionType == transactionType && x.Code != "OTH");
                    var result = new List<DocumentTypeModel>();
                    jobs.ToList().ForEach(x =>
                     {
                         var type = new DocumentTypeModel()
                         {
                             Id = x.Id,
                             Code = x.Code,
                             NameEn = x.NameEn,
                         };
                         result.Add(type);
                     });
                    return result;
            }
        }
    }
}
