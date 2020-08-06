using eFMS.API.Common.Globals;
using eFMS.IdentityServer.DL.UserManager;
using System;
using System.Linq;

namespace eFMS.API.Provider.Services.IService
{
    public interface IPermissionBaseService<TContext, TModel> 
        where TContext : class, new()
        where TModel : class, new()
    {
        IQueryable<TModel> QueryByPermission(IQueryable<TModel> data, PermissionRange range, ICurrentUser currentUser);
        bool CheckAllowPermissionAction(Guid id, PermissionRange range);
    }
}
