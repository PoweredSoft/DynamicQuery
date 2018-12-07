using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPoweredSoftDynamicQuery(this IServiceCollection services)
        {
            services.TryAddTransient<ISort, Sort>();
            services.TryAddTransient<IAggregate, Aggregate>();
            services.TryAddTransient<ISimpleFilter, SimpleFilter>();
            services.TryAddTransient<ICompositeFilter, CompositeFilter>();
            services.TryAddTransient<IGroup, Group>();
            services.TryAddTransient<IQueryCriteria, QueryCriteria>();
            services.TryAddTransient<IQueryHandler, QueryHandler>();
            services.TryAddTransient<IQueryHandlerAsync, QueryHandlerAsync>();
            return services;
        }
    }
}
