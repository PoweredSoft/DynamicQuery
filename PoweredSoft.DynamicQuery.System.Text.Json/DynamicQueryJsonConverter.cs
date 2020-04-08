using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public class DynamicQueryJsonConverter : JsonConverterFactory
    {
        private Type[] DynamicQueryTypes { get; } =
        {
            typeof(IFilter),
            typeof(ISimpleFilter),
            typeof(ICompositeFilter),
            typeof(IAggregate),
            typeof(ISort),
            typeof(IGroup),
            typeof(IQueryCriteria),
            typeof(IQueryHandler)
        };

        private IServiceProvider _serviceProvider;

        public DynamicQueryJsonConverter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && DynamicQueryTypes.Contains(typeToConvert);

        public override JsonConverter CreateConverter(Type objectType, JsonSerializerOptions options)
        {
            return (JsonConverter) Activator.CreateInstance(typeof(QueryCriteriaConverter),
                args: new object[] {_serviceProvider});
        }

        public class QueryCriteriaConverter : JsonConverter<IQueryCriteria>
        {
            private readonly IServiceProvider _serviceProvider;

            public QueryCriteriaConverter(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override IQueryCriteria Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options)
            {
                var queryCriteria = _serviceProvider.GetService(typeof(IQueryCriteria)) as IQueryCriteria;
                if (queryCriteria == null)
                    throw new Exception("IQueryCriteria service not found");

                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException();

                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        return queryCriteria;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        continue;

                    var propertyName = reader.GetString().ToLower();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "page":
                        case "pagesize":
                            if (propertyName == "page")
                                queryCriteria.Page = reader.GetInt32();
                            else
                                queryCriteria.PageSize = reader.GetInt32();
                            break;
                        case "filters":
                        case "sorts":
                            var jsonElm = JsonSerializer.Deserialize<JsonElement>(ref reader);
                            if (propertyName == "filters")
                            {
                                var filters =
                                    JsonSerializer.Deserialize<List<ISimpleFilter>>(jsonElm.GetRawText(), options);
                                queryCriteria.Filters = new List<IFilter>();
                                queryCriteria.Filters.AddRange(filters);
                            }
                            else
                            {
                                var sorts =
                                    JsonSerializer.Deserialize<List<ISort>>(jsonElm.GetRawText(), options);
                                queryCriteria.Sorts = new List<ISort>();
                                queryCriteria.Sorts.AddRange(sorts);
                            }

                            break;
                    }
                }

                return queryCriteria;
            }

            public override void Write(Utf8JsonWriter writer, IQueryCriteria value, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}