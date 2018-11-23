using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Converters;
using PoweredSoft.DynamicQuery.AspNetCore.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.AspNetCore
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddPoweredSoftDynamicQuery(this IMvcBuilder builder)
        {
            builder.Services.AddPoweredSoftDynamicQuery();
            var serviceProvider = builder.Services.BuildServiceProvider();
            builder.AddJsonOptions(o =>
            {
                o.SerializerSettings.Converters.Add(new StringEnumConverter());
                o.SerializerSettings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            });
            return builder;
        }
    }
}
