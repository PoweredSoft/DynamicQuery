using Microsoft.Extensions.DependencyInjection;
using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using PoweredSoft.DynamicQuery.System.Text.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
// using PoweredSoft.DynamicQuery.NewtonsoftJson;
// using Newtonsoft.Json;
// using Newtonsoft.Json.Converters;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class SerializationTests
    {
        [Fact]
        public void SimpleFilter()
        {
            var json = @"{""path"":""Title"",""value"":true,""type"":""In"",""and"":false}";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPoweredSoftDynamicQuery();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // var settings = new JsonSerializerSettings();
            //
            // settings.Converters.Add(new StringEnumConverter());
            // settings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            //
            // var data = JsonConvert.DeserializeObject<IQueryCriteria>(json, settings);

            var opts = new JsonSerializerOptions();
            // opts.PropertyNameCaseInsensitive = true;
            // opts.PropertyNamingPolicy=JsonNamingPolicy.CamelCase;
            // opts.WriteIndented = true;
            opts.Converters.Add(new DynamicQuerySimpleFilterConverter(serviceProvider));
            // opts.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));

            var data = JsonSerializer.Deserialize<ISimpleFilter>(json, opts);
            Assert.NotNull(data);
            Assert.Equal(data.Value, true);
        }



        [Fact]
        public void QueryCriteria()
        {
            // var json = @"{""page"":1,""pageSize"":20,""filters"":[{""type"":""composite"",""filters"":[{""path"":""title"",""value"":""Qui"",""type"":""StartsWith"",""and"":false}]}]}";
            var json =
                @"{""page"":1,""PageSize"":20,""filters"":[{""path"":""title"",""value"":""Qui"",""type"":""StartsWith"",""and"":false}],""sorts"":[{""path"":""aaaa"",""ascending"":true}]}";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPoweredSoftDynamicQuery();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // var settings = new JsonSerializerSettings();
            //
            // settings.Converters.Add(new StringEnumConverter());
            // settings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            //
            // var data = JsonConvert.DeserializeObject<IQueryCriteria>(json, settings);

            var opts = new JsonSerializerOptions();
            // opts.PropertyNameCaseInsensitive = true;
            opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opts.WriteIndented = true;

            opts.Converters.Add(new JsonStringEnumConverter());
            opts.Converters.Add(new DynamicQuerySimpleFilterConverter(serviceProvider));
            opts.Converters.Add(new DynamicQuerySortConverter(serviceProvider));
            opts.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));

            var data = JsonSerializer.Deserialize<IQueryCriteria>(json, opts);
            Assert.NotNull(data);
            Assert.Equal(1, data.Page);
            Assert.Equal(20, data.PageSize);
            Assert.NotEmpty(data.Filters);
            Assert.NotEmpty(data.Sorts);
        }
    }
}
