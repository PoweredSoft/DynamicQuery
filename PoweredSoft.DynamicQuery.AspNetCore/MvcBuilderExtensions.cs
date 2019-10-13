using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoweredSoft.DynamicQuery.AspNetCore
{
    public static class MvcBuilderExtensions
    {
        public static IMvcBuilder AddPoweredSoftDynamicQuery(this IMvcBuilder builder)
        {
            builder.Services.AddPoweredSoftDataServices();
            builder.Services.AddPoweredSoftDynamicQuery();
            return builder;
        }
    }
}
