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
        private IContextBase<CsShipmentSurcharge> _surchargeRepo;
        private IContextBase<AcctSettlementPayment> _settleRepo;
        public AttachFilteTemplateService(IContextBase<SysAttachFileTemplate> repository, IMapper mapper, IContextBase<CsShipmentSurcharge> surchargeRepo, IContextBase<AcctSettlementPayment> settleRepo) : base(repository, mapper)
        {
            _surchargeRepo = surchargeRepo;
            _settleRepo = settleRepo;
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

        public async Task<List<DocumentTypeModel>> GetDocumentType(string transactionType, string billingId)
        {
            switch (transactionType)
            {
                case "SOA":
                    var soas = await DataContext.GetAsync(x => x.Type == "Accountant" && x.AccountingType == "SOA" && x.Code != "OTH");
                    return soas.GroupBy(x => x.Code).Select(x => new DocumentTypeModel()
                    {
                        Id = x.FirstOrDefault().Id,
                        Code = x.FirstOrDefault().Code,
                        NameEn = x.FirstOrDefault().NameEn,
                    }).ToList();
                case "Settlement":
                    var settleNo = _settleRepo.Get(x => x.Id.ToString() == billingId).FirstOrDefault().SettlementNo;
                    var transType = _surchargeRepo.Get(x => x.SettlementCode == settleNo).Select(x => x.TransactionType).ToList();
                    var SMCode = await DataContext.GetAsync(x => x.Type == "Accountant" && x.Code != "OTH" && (x.AccountingType == "Settlement" || x.AccountingType == "ADV-Settlement") && transType.Contains(x.TransactionType));
                    return SMCode.GroupBy(x => new { x.Code }).Select(x => new DocumentTypeModel()
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
                    var jobs = await DataContext.GetAsync(x => x.Type != "Accountant" && x.TransactionType == transactionType && x.Code != "OTH");
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
