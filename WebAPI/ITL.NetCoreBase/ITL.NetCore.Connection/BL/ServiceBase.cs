using AutoMapper;
using ITL.NetCore.Connection.EF;

namespace ITL.NetCore.Connection.BL
{
    public class ServiceBase<TContext, TModel>: RepositoryBase<TContext, TModel> where TContext : class, new()
        where TModel : class, new()
    {
        public ServiceBase(IContextBase<TContext> repository, IMapper mapper) : base(repository, mapper)
        {
        }
    }
}
