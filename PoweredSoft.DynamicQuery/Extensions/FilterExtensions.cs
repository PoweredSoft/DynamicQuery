using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace PoweredSoft.DynamicQuery.Extensions
{
    public static class FilterExtensions
    {
        public static bool IsSimpleFilter(this IFilter filter) => filter is ISimpleFilter;
        public static bool IsCompositeFilter(this IFilter filter) => filter is ICompositeFilter;

        public static bool IsSimpleFilterOn(this IFilter filter, string path)
        {
            var simpleFilter = filter as ISimpleFilter;
            if (simpleFilter == null)
                return false;

            var result = simpleFilter.Path?.Equals(path, StringComparison.InvariantCultureIgnoreCase) == true;
            return result;
        }

        public static bool IsSimpleFilterOn<T>(this IFilter filter, Expression<Func<T, object>> expr)
        {
            var resolved = GetPropertySymbol(expr);
            return filter.IsSimpleFilterOn(resolved);
        }

        public static ISimpleFilter ReplaceByOn(this IFilter filter, string path)
        {
            var simpleFilter = filter as ISimpleFilter;
            if (simpleFilter == null)
                throw new Exception("Must be a simple filter");

            var ret = new SimpleFilter();
            ret.And = filter.And;
            ret.Type = filter.Type;
            ret.Value = simpleFilter.Value;
            ret.Path = path;
            return ret;
        }

        public static ISimpleFilter ReplaceByOn<T>(this IFilter filter, Expression<Func<T, object>> expr)
        {
            var resolved = GetPropertySymbol(expr);
            return filter.ReplaceByOn(resolved);
        }

        public static ICompositeFilter ReplaceByCompositeOn(this IFilter filter, params string[] paths)
        {
            var simpleFilter = filter as ISimpleFilter;
            if (simpleFilter == null)
                throw new Exception("Must be a simple filter");

            var compositeFilter = new CompositeFilter();
            compositeFilter.And = filter.And;
            compositeFilter.Type = FilterType.Composite;
            compositeFilter.Filters = paths
                .Select(t => new SimpleFilter
                {
                    Type = filter.Type,
                    Path = t,
                    And = false,
                    Value = simpleFilter.Value
                })
                .AsEnumerable<IFilter>()
                .ToList();
            return compositeFilter;
        }

        public static ICompositeFilter ReplaceByCompositeOn<T>(this IFilter filter, params Expression<Func<T, object>>[] exprs)
        {
            var paths = exprs.Select(expr => GetPropertySymbol(expr)).ToArray();
            return ReplaceByCompositeOn(filter, paths);
        }

        internal static string GetPropertySymbol<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            return string.Join(".",
                GetMembersOnPath(expression.Body as MemberExpression)
                    .Select(m => m.Member.Name)
                    .Reverse());
        }

        internal static IEnumerable<MemberExpression> GetMembersOnPath(MemberExpression expression)
        {
            while (expression != null)
            {
                yield return expression;
                expression = expression.Expression as MemberExpression;
            }
        }
    }
}
