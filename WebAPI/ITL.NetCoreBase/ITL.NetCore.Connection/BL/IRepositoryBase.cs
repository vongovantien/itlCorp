using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.Utility;

namespace ITL.NetCore.Connection.BL
{
    //public interface IRepositoryBase<TContext, TModel>: IDisposable
    public interface IRepositoryBase<TContext, TModel>
        where TContext : class, new()
        where TModel : class, new()
    {
        DataModeType DataMode { get; }
        List<DataColumnError> DataColumnErrors { get; }
        void SetChildren<TChild>(string MasterField, string ChildField, bool AllowDeleteChild) where TChild : class, new();
        void SetChildren<TChild>(string MasterField, string ChildField) where TChild : class, new();
        void SetParent<TParent>(string ChildrentField, string ParentField) where TParent : class, new();
        void SetUnique(string[] keyfields);
        void SetAllowEditColumn(string[] columns, bool isAllowEdit, string strError);
        void AddAutoTrimColumns(string[] columns);
        void AddValidation(DataColumnError validation);
        void AddAutoValue(DataFieldAutoValue dataFieldAutoValue);

        IQueryable<TModel> Get();
        Task<List<TModel>> GetAsync();

        IQueryable<TModel> Get(Expression<Func<TModel, bool>> predicate);
        Task<List<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate);

        IQueryable<TModel> Get(Expression<Func<TModel, bool>> predicate, int from, int count);
        Task<List<TModel>> GetAsync(Expression<Func<TModel, bool>> predicate, int from, int count);

        TModel First(Expression<Func<TModel, bool>> predicate);

        bool Any(Expression<Func<TModel, bool>> query);
        Task<bool> AnyAsync(Expression<Func<TModel, bool>> query);

        int Count(Expression<Func<TModel, bool>> query);

        IQueryable<TModel> Paging(Expression<Func<TModel, bool>> predicate, int page, int size);
        IQueryable<TModel> Paging(Expression<Func<TModel, bool>> predicate, int page, int size, out int rowsCount);
        Task<(List<TModel> result, int rowsCount)> PagingAsync(Expression<Func<TModel, bool>> predicate, int page, int size);

        IQueryable<TModel> Paging(int page, int size, out int rowsCount);
        Task<(List<TModel> result, int rowsCount)> PagingAsync(int page, int size);
        IQueryable<TModel> Paging(Expression<Func<TModel, bool>> predicate, int page, int size, Expression<Func<TModel, object>> orderByProperty, bool isAscendingOrder, out int rowsCount);
        Task<(List<TModel> result, int rowsCount)> PagingAsync(Expression<Func<TModel, bool>> predicate, int page, int size, Expression<Func<TModel, object>> orderByProperty, bool isAscendingOrder);
        HandleState Add(TModel entity, bool allowSubmitChange);
        HandleState Add(ref TModel entity, bool allowSubmitChange);
        Task<HandleState> AddAsync(TModel entity, bool allowSubmitChange);
        HandleState Add(IEnumerable<TModel> entities, bool allowSubmitChange);
        Task<HandleState> AddAsync(IEnumerable<TModel> entities, bool allowSubmitChange);
        HandleState Update(TModel entity, Expression<Func<TModel, bool>> predicate, bool allowSubmitChange);
        Task<HandleState> UpdateAsync(TModel entity, Expression<Func<TModel, bool>> query, bool allowSubmitChange);
        HandleState Delete(Expression<Func<TModel, bool>> predicate, bool allowSubmitChange);
        Task<HandleState> DeleteAsync(Expression<Func<TModel, bool>> query, bool allowSubmitChange);
        HandleState Add(TModel entity);
        HandleState Add(ref TModel entity);
        Task<HandleState> AddAsync(TModel entity);
        HandleState Add(IEnumerable<TModel> entities);
        Task<HandleState> AddAsync(IEnumerable<TModel> entities);
        HandleState Update(TModel entity, Expression<Func<TModel, bool>> predicate);
        Task<HandleState> UpdateAsync(TModel entity, Expression<Func<TModel, bool>> query);
        HandleState Delete(Expression<Func<TModel, bool>> predicate);
        Task<HandleState> DeleteAsync(Expression<Func<TModel, bool>> query);
    }
}