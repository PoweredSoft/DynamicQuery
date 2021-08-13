using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.NewtonsoftJson;

namespace PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson
{
    public static class MvcBuilderExtensions
    {
        public static  IMvcBuilder AddPoweredSoftJsonNetDynamicQuery(this IMvcBuilder mvcBuilder, bool enableStringEnumConverter = true)
        {
            mvcBuilder.AddPoweredSoftDynamicQuery();
            var serviceProvider = mvcBuilder.Services.BuildServiceProvider();

            mvcBuilder.AddNewtonsoftJson(o =>
            {
                o.SerializerSettings.AddPoweredSoftDynamicQueryNewtonsoftJson(serviceProvider, enableStringEnumConverter: enableStringEnumConverter);
            });

            return mvcBuilder;
        }
    }
}
