using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class SerializationTests
    {
        [Fact]
        public void QueryCriteria()
        {
            var json = @"{""page"":1,""pageSize"":20,""filters"":[{""type"":""composite"",""filters"":[{""path"":""title"",""value"":""Qui"",""type"":""StartsWith"",""and"":false}]}]}";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPoweredSoftDynamicQuery();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var settings = new JsonSerializerSettings();

            settings.Converters.Add(new StringEnumConverter());
            settings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
       
            var data = JsonConvert.DeserializeObject<IQueryCriteria>(json, settings);
            Assert.NotNull(data);
        }

        
    }
}
