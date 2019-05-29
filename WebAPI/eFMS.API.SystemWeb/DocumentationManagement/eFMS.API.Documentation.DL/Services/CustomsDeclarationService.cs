using AutoMapper;
using eFMS.API.Documentation.DL.IService;
using eFMS.API.Documentation.DL.Models;
using eFMS.API.Documentation.Service.Models;
using ITL.NetCore.Connection.BL;
using ITL.NetCore.Connection.EF;
using System;
using System.Collections.Generic;
using System.Text;

namespace eFMS.API.Documentation.DL.Services
{
    public class CustomsDeclarationService : RepositoryBase<CustomsDeclaration, CustomsDeclarationModel>, ICustomsDeclarationService
    {
        public CustomsDeclarationService(IContextBase<CustomsDeclaration> repository, IMapper mapper) : base(repository, mapper)
        {
        }

        public List<CustomsDeclarationModel> GetByJobId(Guid jobId)
        {
            List<CustomsDeclarationModel> results = null;
            var data = DataContext.Get(x => x.Mblid == jobId);
            foreach(var item in data)
            {
                var clearance = mapper.Map<CustomsDeclarationModel>(item);
                results.Add(clearance);
            }
            return results;
        }
    }
}
