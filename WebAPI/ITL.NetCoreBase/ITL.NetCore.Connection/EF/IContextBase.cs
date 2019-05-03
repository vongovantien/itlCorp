using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ITL.NetCore.Common;
using ITL.NetCore.Connection.Utility;
using Microsoft.EntityFrameworkCore;

namespace ITL.NetCore.Connection.EF
{
    public interface IContextBase<T> : IDisposable
         where T : class, new()
    {
        DbContext DC { get; }
        //void ConfigDataContext<D>(string connectionString);

        void SetChildren<TChild>(string MasterField, string ChildField, bool AllowDeleteChild)
        where TChild : class, new();

        void SetParent<TParent>(string ChildrentField, string ParentField)
        where TParent : class, new();

        void SetUnique(string[] keyfields);
       
        bool HaveParent(T entitty);
        void AddAutoTrimColumns(string[] columns);
        void AddValidation(DataColumnError validation);

        void AddAutoValue(DataFieldAutoValue dataFieldAutoValue);

        void SetAllowEditColumn(string[] columns, bool isAllowEdit, string strError);

        IQueryable<T> Where(Expression<Func<T, bool>> exp);
        Task<List<T>> WhereAsync(Expression<Func<T, bool>> exp);

        IQueryable<T> Get();
        Task<List<T>> GetAsync();

        IQueryable<T> Get(Expression<Func<T, bool>> query);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> query);

        IQueryable<T> Get(Expression<Func<T, bool>> query, int from, int count);
        Task<List<T>> GetAsync(Expression<Func<T, bool>> query, int from, int count);

        T First(Expression<Func<T, bool>> query);
        Task<T> FirstAsync(Expression<Func<T, bool>> query);

        bool Any(Expression<Func<T, bool>> query);
        Task<bool> AnyAsync(Expression<Func<T, bool>> query);

        int Count(Expression<Func<T, bool>> query);
        Task<int> CountAsync(Expression<Func<T, bool>> query);

        IQueryable<T> Paging(Expression<Func<T, bool>> query, int page, int size, out int rowsCount);
        Task<(List<T> result, int rowsCount)> PagingAsync(Expression<Func<T, bool>> query, int page, int size);

        IQueryable<T> Paging(int page, int size, out int rowsCount);
        Task<(List<T> result, int rowsCount)> PagingAsync(int page, int size);
        IQueryable<T> Paging(int page, int size, Expression<Func<T, object>> orderByProperty, bool isAscendingOrder, out int rowsCount);
        IQueryable<T> Paging(Expression<Func<T, bool>> query, int page, int size, Expression<Func<T, object>> orderByProperty, bool isAscendingOrder, out int rowsCount);
        Task<(List<T> result, int rowsCount)> PagingAsync(Expression<Func<T, bool>> query, int page, int size, Expression<Func<T, object>> orderByProperty, bool isAscendingOrder);
        HandleState Add(T entity);
        Task<HandleState> AddAsync(T entity);
        HandleState Add(T entity, bool allowSubmitChange);

        Task<HandleState> AddAsync(T entity, bool allowSubmitChange);

        HandleState Add(IEnumerable<T> entities);
        Task<HandleState> AddAsync(IEnumerable<T> entities);

        HandleState Add(IEnumerable<T> entities, bool allowSubmitChange);
        Task<HandleState> AddAsync(IEnumerable<T> entities, bool allowSubmitChange);

        HandleState Update(T entity, Expression<Func<T, bool>> query);
        Task<HandleState> UpdateAsync(T entity, Expression<Func<T, bool>> query);

        HandleState Update(T entity, Expression<Func<T, bool>> query, bool allowSubmitChange);
        Task<HandleState> UpdateAsync(T entity, Expression<Func<T, bool>> query, bool allowSubmitChange);
        //HandleState Update(T entity, Expression<Func<T, bool>> query, IEnumerable<string> propertyNames, bool reverse, bool allowSubmitChange);
        HandleState Delete(Expression<Func<T, bool>> query);
        Task<HandleState> DeleteAsync(Expression<Func<T, bool>> query);
        HandleState Delete(Expression<Func<T, bool>> query, bool allowSubmitChange);
        Task<HandleState> DeleteAsync(Expression<Func<T, bool>> query, bool allowSubmitChange);
        HandleState SubmitChanges();
    }
}