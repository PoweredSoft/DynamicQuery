using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public class DynamicQuerySimpleFilterConverter : JsonConverterFactory
    {
        private readonly ServiceProvider _serviceProvider;

        public DynamicQuerySimpleFilterConverter(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && typeToConvert == typeof(ISimpleFilter);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return Activator.CreateInstance(typeof(SimpleFilterConverter),
                args: new object[] {_serviceProvider}) as SimpleFilterConverter;
        }


        class SimpleFilterConverter : JsonConverter<ISimpleFilter>
        {
            private readonly ServiceProvider _serviceProvider;

            public SimpleFilterConverter(ServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override ISimpleFilter Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                var filter = _serviceProvider.GetService(typeof(ISimpleFilter)) as ISimpleFilter;
                
                if (filter == null)
                    throw new Exception("IFilter service not found");
                
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return filter;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    var propertyName = reader.GetString().ToLower();
                    reader.Read();

                    var tokenType = reader.TokenType;
                    switch (propertyName)
                    {
                        case "type":
                            var enumValue = tokenType == JsonTokenType.String
                                ? reader.GetString()
                                : $"{reader.GetInt32()}";
                            filter.Type = Enum.Parse<FilterType>(enumValue);
                            break;
                        case "path":
                            filter.Path = reader.GetString();
                            break;
                        case "and":
                            filter.And = reader.GetBoolean();
                            break;
                        case "value":
                            var elm = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                            filter.Value = getValue(elm);
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, ISimpleFilter value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            private object getValue(JsonElement elm)
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
                            values.Add(getValue(enumerateArray.Current));
                        }

                        value = values;
                        break;
                }

                return value;
            }
        }
    }
}