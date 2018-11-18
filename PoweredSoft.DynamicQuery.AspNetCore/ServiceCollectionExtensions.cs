using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static void AddDynamicQueryDefaultMappings(this IServiceCollection services)
        {
            services.AddTransient<ISort, Sort>();
            services.AddTransient<IAggregate, Aggregate>();
            services.AddTransient<ISimpleFilter, SimpleFilter>();
            services.AddTransient<ICompositeFilter, CompositeFilter>();
            services.AddTransient<IGroup, Group>();
            services.AddTransient<IQueryCriteria, QueryCriteria>();
            services.AddTransient<IQueryHandler, QueryHandler>();
        }
    }
}
