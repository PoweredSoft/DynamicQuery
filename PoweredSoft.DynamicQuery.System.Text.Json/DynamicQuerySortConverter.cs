using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public class DynamicQuerySortConverter : JsonConverterFactory
    {
        private readonly ServiceProvider _serviceProvider;

        public DynamicQuerySortConverter(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && typeToConvert == typeof(ISort);

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            return Activator.CreateInstance(typeof(SortConverter), args: new object[] {_serviceProvider}) as
                SortConverter;
        }

        class SortConverter : JsonConverter<ISort>
        {
            private readonly ServiceProvider _serviceProvider;

            public SortConverter(ServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override ISort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                var sort = _serviceProvider.GetService(typeof(ISort)) as ISort;
                if (sort == null)
                    throw new Exception("ISort service not found");

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return sort;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    var propertyName = reader.GetString().ToLower();
                    reader.Read();

                    switch (propertyName)
                    {
                        case "path":
                            sort.Path = reader.GetString();
                            break;

                        case "ascending":
                            sort.Ascending = reader.GetBoolean();
                            break;
                    }
                }

                throw new JsonException();
            }

            public override void Write(Utf8JsonWriter writer, ISort value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}
