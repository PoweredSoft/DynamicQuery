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
            opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            // opts.WriteIndented = true;
            // opts.Converters.Add(new DynamicQuerySimpleFilterConverter(serviceProvider));
            opts.Converters.Add(new DynamicQueryFilterConverter(serviceProvider));

            var data = JsonSerializer.Deserialize<ISimpleFilter>(json, opts);
            Assert.NotNull(data);
            Assert.Equal(data.Value, true);
        }


        [Fact]
        public void QueryCriteria()
        {
            var json = @"
                        {
                            ""page"":1,
                            ""pageSize"":20,
                            ""filters"":[
                                {""path"":""title"",""value"":""Qui"",""type"":""StartsWith"",""and"":true},
                                {""path"":""date"",""value"":""2020-04-01"",""type"":""Equal"",""and"":false},
                               
                                {""type"":""Composite"",""and"":false,""filters"":[
                                    {""path"":""date1"",""type"":""GreaterThan"",""value"":""2020-04-01""},
                                    {""path"":""date1"",""type"":""LessThan"",""value"":""2020-04-02""},
                                    {""type"":""Composite"",""and"":false,""filters"":[
                                        {""path"":""date2"",""type"":""GreaterThan"",""value"":""2020-05-01""},
                                        {""path"":""date2"",""type"":""LessThan"",""value"":""2020-05-02""}
                                    ]}
                                ]}
                            ],
                            ""sorts"":[{""path"":""title"",""ascending"":true}]
                        }
                      ";

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddPoweredSoftDynamicQuery();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            #region Newtonsoft.Json

            // var settings = new JsonSerializerSettings();
            //
            // settings.Converters.Add(new StringEnumConverter());
            // settings.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));
            //
            // var data = JsonConvert.DeserializeObject<IQueryCriteria>(json, settings);

            #endregion

            #region Text.Json

            var opts = new JsonSerializerOptions();
            // opts.PropertyNameCaseInsensitive = true;
            opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            opts.WriteIndented = true;

            opts.Converters.Add(new JsonStringEnumConverter());

            opts.Converters.Add(new DynamicQueryFilterConverter(serviceProvider));

            opts.Converters.Add(new DynamicQuerySortConverter(serviceProvider));
            opts.Converters.Add(new DynamicQueryJsonConverter(serviceProvider));

            #endregion

            var data = JsonSerializer.Deserialize<IQueryCriteria>(json, opts);
            Assert.NotNull(data);
            Assert.Equal(1, data.Page);
            Assert.Equal(20, data.PageSize);
            Assert.Equal(typeof(ICompositeFilter), data.Filters[2].GetType().GetInterface("ICompositeFilter"));
            Assert.NotEmpty(data.Filters);
            Assert.NotEmpty(data.Sorts);
        }
    }
}