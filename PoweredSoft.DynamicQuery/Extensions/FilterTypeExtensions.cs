using System;
using System.Collections.Generic;
using System.Text;
using PoweredSoft.DynamicLinq;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.Extensions
{
    public static class FilterTypeExtensions
    {
        public static ConditionOperators? ConditionOperator(this FilterType filterType)
        {
            if (filterType == FilterType.Equal)
                return ConditionOperators.Equal;
            if (filterType == FilterType.NotEqual)
                return ConditionOperators.NotEqual;
            if (filterType == FilterType.GreaterThan)
                return ConditionOperators.GreaterThan;
            if (filterType == FilterType.GreaterThanOrEqual)
                return ConditionOperators.GreaterThanOrEqual;
            if (filterType == FilterType.LessThan)
                return ConditionOperators.LessThan;
            if (filterType == FilterType.LessThanOrEqual)
                return ConditionOperators.LessThanOrEqual;
            if (filterType == FilterType.StartsWith)
                return ConditionOperators.StartsWith;
            if (filterType == FilterType.EndsWith)
                return ConditionOperators.EndsWith;
            if (filterType == FilterType.Contains)
                return ConditionOperators.Contains;
            if (filterType == FilterType.In)
                return ConditionOperators.In;
            if (filterType == FilterType.NotIn)
                return ConditionOperators.NotIn;

            return null;
        }
    }
}
