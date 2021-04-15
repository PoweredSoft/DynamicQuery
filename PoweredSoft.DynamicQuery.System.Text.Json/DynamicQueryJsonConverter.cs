using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery.System.Text.Json
{
    public class DynamicQueryJsonConverter : BaseJsonConverterFactory
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

        public DynamicQueryJsonConverter(ServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool CanConvert(Type typeToConvert) =>
            typeToConvert.IsInterface && DynamicQueryTypes.Contains(typeToConvert);


        protected override JsonConverter CreateConverter(ServiceProvider serviceProvider, Type typeToConvert,
            JsonSerializerOptions options) =>
            (JsonConverter) Activator.CreateInstance(typeof(QueryCriteriaConverter),
                args: new object[] {serviceProvider});


        class QueryCriteriaConverter : BaseJsonConverter<IQueryCriteria>
        {
            public QueryCriteriaConverter(ServiceProvider serviceProvider) : base(serviceProvider)
            {
            }

            public override IQueryCriteria Read(ref Utf8JsonReader reader, Type typeToConvert,
                JsonSerializerOptions options)
            {
                var queryCriteria = GetService<IQueryCriteria>();
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
                                    JsonSerializer.Deserialize<List<IFilter>>(jsonElm.GetRawText(), options);
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