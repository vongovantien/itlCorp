using ITL.NetCore.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace eFMS.API.Common.Helpers
{
    public class CriteriaBuilder<T>
    {
        private readonly List<Expression<Func<T, bool>>> _expressions;

        public CriteriaBuilder()
        {
            _expressions = new List<Expression<Func<T, bool>>>();
        }

        public CriteriaBuilder<T> Where(Expression<Func<T, bool>> expression)
        {
            _expressions.Add(expression);
            return this;
        }

        public CriteriaBuilder<T> WhereIf(bool condition, Expression<Func<T, bool>> expression)
        {
            if (condition)
            {
                _expressions.Add(expression);
            }
            return this;
        }

        public IQueryable<T> Apply(IQueryable<T> query)
        {
            if (_expressions.Count > 0)
            {
                var predicate = _expressions[0];
                for (int i = 1; i < _expressions.Count; i++)
                {
                    predicate = predicate.And(_expressions[i]);
                }
                return query.Where(predicate);
            }
            else
            {
                return query;
            }
        }
    }
}
