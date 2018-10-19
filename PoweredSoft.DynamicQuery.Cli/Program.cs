using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PoweredSoft.DynamicQuery.Cli
{
    public class PersonQueryInterceptor : IQueryInterceptor
        //, IBeforeQueryAlteredInterceptor<Person>
        //, IFilterInterceptor
    {
        public IQueryable<Person> InterceptQueryBeforeAltered(IQueryCriteria criteria, IQueryable<Person> queryable) 
            => queryable.Where(t => t.FirstName.StartsWith("Da"));

        public IFilter InterceptFilter(IFilter filter)
        {
            if (filter is SimpleFilter)
            {
                var simpleFilter = filter as ISimpleFilter;
                if (simpleFilter.Path == "FirstName" && simpleFilter.Value is string && ((string)simpleFilter.Value).Contains(","))
                {
                    var firstNames = ((string) simpleFilter.Value).Split(',');
                    var filters = firstNames.Select(firstName => new SimpleFilter
                    {
                        Path = "FirstName",
                        Type = FilterType.Equal,
                        Value = firstName
                    }).Cast<IFilter>().ToList();

                    return new CompositeFilter
                    {
                        Type = FilterType.Composite,
                        Filters = filters,
                        And = true
                    };
                }
            }

            return filter;
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class OtherClass
    {
        
    }

    class Program
    {
        static void Main(string[] args)
        {
            Play1();

            
        }

        private static void Play1()
        {
            var list = new List<Person>()
            {
                new Person{ Id = 1, FirstName = "David", LastName = "Lebee"},
                new Person{ Id = 2, FirstName = "Michaela", LastName = "Lebee"},
                new Person{ Id = 3, FirstName = "Zohra", LastName = "Lebee"},
                new Person{ Id = 4, FirstName = "Eric", LastName = "Vickar"},
                new Person{ Id = 5, FirstName = "Susan", LastName = "Vickar"},
            };

            var queryable = list.AsQueryable();
            var criteria = new QueryCriteria();
            criteria.Page = 1;
            criteria.PageSize = 10;

            criteria.Filters = new List<IFilter>
            {
                new SimpleFilter() {Path = nameof(Person.LastName), Value = "Lebee", Type = FilterType.Equal},
                new CompositeFilter()
                {
                    Type = FilterType.Composite,
                    And = true,
                    Filters = new List<IFilter>
                    {
                        new SimpleFilter() {Path = nameof(Person.FirstName), Value = "David", Type = FilterType.Equal},
                        new SimpleFilter() {Path = nameof(Person.FirstName), Value = "Zohra", Type = FilterType.Equal},
                    }
                }
            };

            var handler = new QueryHandler();
            handler.AddInterceptor(new PersonQueryInterceptor());
            var result = handler.Execute(queryable, criteria);

            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            jsonSettings.Converters.Add(new StringEnumConverter { AllowIntegerValues = false });

            Console.WriteLine("Request:\n");
            Console.WriteLine(JsonConvert.SerializeObject(criteria, Formatting.Indented, jsonSettings));
            Console.WriteLine("");
            Console.WriteLine("Response:\n");
            Console.WriteLine(JsonConvert.SerializeObject(result, Formatting.Indented, jsonSettings));
            Console.ReadKey();
        }
    }
}
