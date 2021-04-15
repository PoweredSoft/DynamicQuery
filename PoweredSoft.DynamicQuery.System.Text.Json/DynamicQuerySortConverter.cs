using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public class DynamicQuerySortConverter : BaseJsonConverterFactory
    {
        public DynamicQuerySortConverter(ServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && typeToConvert == typeof(ISort);

        protected override JsonConverter CreateConverter(ServiceProvider serviceProvider, Type typeToConvert,
            JsonSerializerOptions options) =>
            Activator.CreateInstance(typeof(SortConverter), args: new object[] {serviceProvider}) as
                SortConverter;

        class SortConverter : BaseJsonConverter<ISort>
        {
            public SortConverter(ServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            public override ISort Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                var sort = GetService<ISort>();
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