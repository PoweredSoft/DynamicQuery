using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public class DynamicQueryFilterConverter : BaseJsonConverterFactory
    {
        public DynamicQueryFilterConverter(ServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && typeToConvert == typeof(IFilter);

        protected override JsonConverter CreateConverter(ServiceProvider serviceProvider, Type typeToConvert,
            JsonSerializerOptions options) =>
            Activator.CreateInstance(typeof(FilterConverter),
                args: new object[] {serviceProvider}) as FilterConverter;
    }

    class FilterConverter : BaseFilterJsonConverter<IFilter>
    {
        public FilterConverter(ServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override IFilter Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException();

            FilterType? filterType = null;
            bool and = false;
            string path = null;
            object value = null;
            List<IFilter> filters = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType != JsonTokenType.PropertyName)
                    continue;

                var propertyName = reader.GetString().ToLower();
                reader.Read();

                switch (propertyName)
                {
                    case "path":
                        path = reader.GetString();
                        break;

                    case "type":
                        filterType = GetFilterType(ref reader);
                        break;

                    case "and":
                        and = reader.GetBoolean();
                        break;

                    case "value":
                        value = GetValue(JsonSerializer.Deserialize<JsonElement>(ref reader, options));
                        break;

                    case "filters":
                        var elm = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                        filters = JsonSerializer.Deserialize<List<IFilter>>(elm.GetRawText(), options);
                        break;
                }
            }

            if (filterType == null)
                throw new JsonException("filterType error");

            // ICompositeFilter
            if (filterType == FilterType.Composite)
            {
                var compositeFilter = GetService<ICompositeFilter>();
                compositeFilter.And = and;
                compositeFilter.Type = filterType.Value;
                compositeFilter.Filters = filters;
                return compositeFilter;
            }

            if (string.IsNullOrEmpty(path)) throw new JsonException("path error");

            // ISimpleFilter
            var simpleFilter = GetService<ISimpleFilter>();
            simpleFilter.And = and;
            simpleFilter.Type = filterType.Value;
            simpleFilter.Path = path;
            simpleFilter.Value = value;
            return simpleFilter;
        }

        public override void Write(Utf8JsonWriter writer, IFilter value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}