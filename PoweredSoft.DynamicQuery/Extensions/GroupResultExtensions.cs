using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.Extensions
{
    public static class GroupResultExtensions
    {
        public static IQueryExecutionGroupResult<TRecord> GroupedResult<TRecord>(this IQueryExecutionResult<TRecord> source)
        {
            if (source is IQueryExecutionGroupResult<TRecord> ret)
                return ret;

            throw new Exception("this result is not a grouped result");                
        }
    }
}
