using System;
using System.Collections.Generic;
using System.Linq;
//using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;
using SystemManagement.DL.Helpers.PagingPrams;

namespace SystemManagement.DL.Helpers.PageList
{
    public class PagedList<T>
    {
        public PagedList(IQueryable<T> source, int pageNumber, int pageSize)
        {
            this.TotalItems = source.Count();
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
            this.List = source
                            .Skip(pageSize * (pageNumber - 1))
                            .Take(pageSize)
                            .ToList();
        }

        public int TotalItems { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public List<T> List { get; }
        public int TotalPages =>
              (int)Math.Ceiling(this.TotalItems / (double)this.PageSize);
        public bool HasPreviousPage => this.PageNumber > 1;
        public bool HasNextPage => this.PageNumber < this.TotalPages;
       
        public bool HasLastPage => this.PageNumber < this.TotalPages;
        public int LastPage =>
            this.HasLastPage ? this.TotalPages : 1;
       
        public int NextPageNumber =>
               this.HasNextPage ? this.PageNumber + 1 : this.TotalPages;
        public int PreviousPageNumber =>
               this.HasPreviousPage ? this.PageNumber - 1 : 1;

        public PagingHeader GetHeader()
        {
            return new PagingHeader(
                 this.TotalItems, this.PageNumber,
                 this.PageSize, this.TotalPages);
        }
    }
    //public static class Dlinq
    //{
    //    public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string ordering)
    //    {
    //        var type = typeof(T);
    //        var property = type.GetProperty(ordering);
    //        var parameter = Expression.Parameter(type, "p");
    //        var propertyAccess = Expression.MakeMemberAccess(parameter, property);
    //        var orderByExp = Expression.Lambda(propertyAccess, parameter);
    //        MethodCallExpression resultExp = Expression.Call(typeof(IQueryable), "OrderBy", new Type[] { type, property.PropertyType }, source.Expression, Expression.Quote(orderByExp));
    //        return source.Provider.CreateQuery<T>(resultExp);
    //    }
    //}
}
