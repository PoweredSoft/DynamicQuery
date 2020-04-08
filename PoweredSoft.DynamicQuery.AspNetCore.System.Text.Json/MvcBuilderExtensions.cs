using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.System.Text.Json;

namespace PoweredSoft.DynamicQuery.AspNetCore.System.Text.Json
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddPoweredSoftJsonNetDynamicQuery(this IMvcBuilder mvcBuilder,
            bool enableStringEnumConverter = true)
        {
            mvcBuilder.AddPoweredSoftDynamicQuery();
            var serviceProvider = mvcBuilder.Services.BuildServiceProvider();
            mvcBuilder.AddJsonOptions(cfg =>
            {
                if (enableStringEnumConverter)
                    cfg.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                cfg.JsonSerializerOptions.Converters.Add(new DynamicQuerySimpleFilterConverter(serviceProvider));
                cfg.JsonSerializerOptions.Converters.Add(new DynamicQuerySortConverter(serviceProvider));
                cfg.JsonSerializerOptions.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            });
            return mvcBuilder;
        }
    }
}