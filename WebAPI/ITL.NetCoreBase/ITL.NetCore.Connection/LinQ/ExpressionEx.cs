using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace ITL.NetCore.Connection.LinQ
{
    public static class ExpressionEx
    {
        /// <summary>
        /// Convert from lambda expression of a entity to lambda expression of new entity
        /// </summary>      
        public static Expression<Func<TNewEntity, TResult>> ConvertExpression<TNewEntity, TOldEntity, TResult>(Expression<Func<TOldEntity, TResult>> expression)
            where TOldEntity : class, new()
            where TNewEntity : class, new()
        {
            var param = Expression.Parameter(typeof(TNewEntity));
            return Expression.Lambda<Func<TNewEntity, TResult>>(
                expression.Body.Replace(expression.Parameters[0], param)
                , param);
        }
        public static Expression Replace(this Expression expression, Expression searchEx, Expression replaceEx)
        {
            return new ReplaceVisitor(searchEx, replaceEx).Visit(expression);
        }
    }
    internal class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression from, to;
        public ReplaceVisitor(Expression from, Expression to)
        {
            this.from = from;
            this.to = to;
        }
        public override Expression Visit(Expression node)
        {
            return node == from ? to : base.Visit(node);
        }
    }
}
