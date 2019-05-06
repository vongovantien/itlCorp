using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace ITL.NetCore.Common
{
    public static class TUtility
    {
        public static object ExpressionToFunc(Expression pre)
        {
            return ObjectUtility.ExecMethod(pre, "Compile", null);
        }

        public static Expression<Func<T, bool>> GetLambda<T>(T dr, string FieldName)
            where T : class, new()
        {
            return GetLambda2<T>(ObjectUtility.GetValue(dr, FieldName), FieldName);
        }

        public static Expression<Func<T, bool>> GetLambda2<T>(object val, string FieldName)
            where T : class, new()
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "t");
            var p = Expression.Property(param, FieldName);
            var KeyValue = Expression.Constant(val, p.Type);
            var makeExp = Expression.MakeBinary(ExpressionType.Equal, p, KeyValue);

            return Expression.Lambda<Func<T, bool>>(makeExp, param);
        }
        public static Expression<Func<T, bool>> NotEquals<T>(T val, string FieldName)
           where T : class, new()
        {
           var value= val.GetType().GetProperty(FieldName).GetValue(val, null);
            ParameterExpression param = Expression.Parameter(typeof(T), "t");
            var p = Expression.Property(param, FieldName);
            var constant = Expression.Constant(value);
            var notEqual = Expression.NotEqual(p, constant);
            //var expression = Expression.Not(Expression.Equal(p, constant));
            return Expression.Lambda<Func<T, bool>>(notEqual, param);
        }
        public static Expression<Func<T, bool>> Predicate<T>(string propertyName, object value, ExpressionType exType)
           where T : class, new()
        {
            ParameterExpression param = Expression.Parameter(typeof(T), "t");
            var p = Expression.Property(param, propertyName);
            var keyValue = value is DateTime ? p.Member.ToString().Contains("Nullable") ?
                Expression.Constant(value, typeof(DateTime?))
                : Expression.Constant(value, typeof(DateTime))
                : Expression.Constant(value);
            var makeExp = Expression.MakeBinary(exType, p, keyValue);

            return Expression.Lambda<Func<T, bool>>(makeExp, param);
        }

        #region PredicateBuilder

        public static Expression<Func<T, bool>> True<T>() { return f => true; }
        public static Expression<Func<T, bool>> False<T>() { return f => false; }

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
        }
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
                                                            Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.Or(expr1.Body, invokedExpr), expr1.Parameters);
        }
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
        }
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
                                                             Expression<Func<T, bool>> expr2)
        {
            var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
            return Expression.Lambda<Func<T, bool>>
                  (Expression.And(expr1.Body, invokedExpr), expr1.Parameters);
        }
        #endregion

        public static IQueryable<T> PredicateDateRange<T>(IQueryable<T> q, string fieldDate, DateTime? dateFrom, DateTime? dateTo)
            where T : class, new()
        {
            return PredicateDateRange<T>(q, fieldDate, dateFrom, dateTo, true);
        }

        public static IQueryable<T> PredicateDateRange<T>(IQueryable<T> q,
          string fieldDate, DateTime? dateFrom, DateTime? dateTo, bool allowEqual)
            where T : class, new()
        {
            Expression.Parameter(typeof(T), "t");
            if (dateFrom != null && dateFrom > DateTime.MinValue)
            {
                DateTime dateFromNew = (DateTime)dateFrom;
                q = q.Where(Predicate<T>(fieldDate, new DateTime(dateFromNew.Year, dateFromNew.Month, dateFromNew.Day, 0, 0, 0),
                    allowEqual ? ExpressionType.GreaterThanOrEqual : ExpressionType.GreaterThan));
            }
            if (dateTo != null && dateTo > DateTime.MinValue)
            {
                DateTime dateToNew = (DateTime)dateTo;
                q = q.Where(Predicate<T>(fieldDate, new DateTime(dateToNew.Year, dateToNew.Month, dateToNew.Day, 23, 59, 59),
                    allowEqual ? ExpressionType.LessThanOrEqual : ExpressionType.LessThan));
            }
            return q;
        }
    }
}