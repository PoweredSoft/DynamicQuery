using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.AspNetCore.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.AspNetCore
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddDynamicQueryJsonConverter(this IMvcBuilder builder, IServiceProvider serviceProvider)
        {
            builder.AddJsonOptions(o =>
            {
                o.SerializerSettings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            });
            return builder;
        }
    }
}
