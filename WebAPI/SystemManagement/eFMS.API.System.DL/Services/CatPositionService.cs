using AutoMapper;
using ITL.NetCore.Connection.EF;
using SystemManagement.DL.IService;
using SystemManagement.DL.Models;
using ITL.NetCore.Connection.BL;
using SystemManagementAPI.Service.Models;
namespace SystemManagement.DL.Services
{
    public class CatPositionService : RepositoryBase<CatPosition, CatPositionModel>, ICatPositionService
    {
        private readonly IContextBase<CatPosition> _repository;
        private readonly IMapper _mapper;
        public CatPositionService(IContextBase<CatPosition> repository, IMapper mapper) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }
    }
}
