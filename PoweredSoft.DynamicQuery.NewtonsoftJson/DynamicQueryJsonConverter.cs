using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PoweredSoft.DynamicQuery.AspNetCore.Json
{
    public class DynamicQueryJsonConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => false;

        private Type[] DynamicQueryTypes { get; } = new Type[]
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

        public IServiceProvider ServiceProvider { get; }

        public DynamicQueryJsonConverter(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public override bool CanConvert(Type objectType) => objectType.IsInterface && DynamicQueryTypes.Contains(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return (object)null;

            if (objectType == typeof(IFilter))
            {
                var jo = JObject.Load(reader);

                bool isComposite = false;
                if (jo.ContainsKey("type"))
                {
                    isComposite = jo.GetValue("type").Value<string>()
                        .Equals("composite", StringComparison.OrdinalIgnoreCase);
                }
                else if (jo.ContainsKey("Type"))
                {
                    isComposite = jo.GetValue("Type").Value<string>()
                        .Equals("composite", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    throw new Exception("IFilter should have a type property..");
                }

                var filterObj = ServiceProvider.GetService(isComposite ? typeof(ICompositeFilter) : typeof(ISimpleFilter));
                var filterType = filterObj.GetType();
                filterObj = jo.ToObject(filterType, serializer);
                return filterObj;
            }

            var obj = ServiceProvider.GetService(objectType);
            if (obj == null)
                throw new JsonSerializationException("No object created.");

            serializer.Populate(reader, obj);
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
