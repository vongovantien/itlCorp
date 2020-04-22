using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.System.DL.Infrastructure
{
    public static class ExpressionExtension
    {
        public static Expression<Func<TModel, TProperty>> CreateExpression<TModel, TProperty>(
        string propertyName)
        {
            var param = Expression.Parameter(typeof(TModel), "x");
            return Expression.Lambda<Func<TModel, TProperty>>(
                Expression.PropertyOrField(param, propertyName), param);
        }
    }
}
