using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public abstract class BaseJsonConverterFactory : JsonConverterFactory
    {
        private readonly ServiceProvider _serviceProvider;

        protected BaseJsonConverterFactory(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            => CreateConverter(_serviceProvider, typeToConvert, options);

        protected abstract JsonConverter CreateConverter(ServiceProvider serviceProvider, Type typeToConvert,
            JsonSerializerOptions options);
    }

    public abstract class BaseJsonConverter<T> : JsonConverter<T>
    {
        private readonly ServiceProvider _serviceProvider;

        protected BaseJsonConverter(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected TService GetService<TService>() => _serviceProvider.GetService<TService>();
    }

    public abstract class BaseFilterJsonConverter<T> : BaseJsonConverter<T>
    {
        protected BaseFilterJsonConverter(ServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected FilterType GetFilterType(ref Utf8JsonReader reader)
        {
            var enumValue = reader.TokenType == JsonTokenType.String
                ? reader.GetString()
                : $"{reader.GetInt32()}";
            return Enum.Parse<FilterType>(enumValue);
        }

        protected object GetValue(JsonElement elm)
        {
            object value = null;
            switch (elm.ValueKind)
            {
                case JsonValueKind.String:
                    value = elm.GetString();
                    break;

                case JsonValueKind.Number:
                    value = elm.GetInt32();
                    break;

                case JsonValueKind.True:
                    value = elm.GetBoolean();
                    break;
                case JsonValueKind.Array:
                    var values = new List<object>();
                    var enumerateArray = elm.EnumerateArray();
                    while (enumerateArray.MoveNext())
                    {
                        values.Add(GetValue(enumerateArray.Current));
                    }

                    value = values;
                    break;
            }

            return value;
        }
    }
}