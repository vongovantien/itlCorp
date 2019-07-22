using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Contexts;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;

namespace eFMS.API.Documentation.DL.Services
{
    public class AcctSOAService : RepositoryBase<AcctSoa, AcctSoaModel>, IAcctSOAService
    {
        public AcctSOAService(IContextBase<AcctSoa> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public object AddSOA(AcctSoaModel model)
        {
            eFMSDataContext dc = (eFMSDataContext)DataContext.DC;
            var soa = mapper.Map<AcctSoa>(model);
            var hsTrans = dc.AcctSoa.Add(soa);
            dc.SaveChanges();
            var result = new HandleState();
            return new { model = soa, result };
        }
    }
}
