using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.TypeConverters;
using PoweredSoft.Types;

namespace PoweredSoft.DynamicQuery
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPoweredSoftDynamicQuery(this IServiceCollection services)
        {
            Converter.RegisterConverter(new ToEnumConverter());
            Converter.RegisterConverter(new StringToLongConverter());
            Converter.RegisterConverter(new StringToTimeSpanConverter());

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