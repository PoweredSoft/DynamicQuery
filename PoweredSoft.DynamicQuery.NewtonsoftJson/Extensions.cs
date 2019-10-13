using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoweredSoft.DynamicQuery.AspNetCore.Json;
using System;

namespace PoweredSoft.DynamicQuery.NewtonsoftJson
{
    public static class JsonNetSerializationSettingsExtensions
    {
        public static JsonSerializerSettings AddPoweredSoftDynamicQueryNewtonsoftJson(this JsonSerializerSettings settings, IServiceProvider serviceProvider, bool enableStringEnumConverter = true)
        {
            if (enableStringEnumConverter)
                settings.Converters.Add(new StringEnumConverter());

            settings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            return settings;
        }
    }
}
