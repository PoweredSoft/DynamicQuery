using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoweredSoft.DynamicQuery.Cli
{
    public class PersonQueryInterceptor : IQueryInterceptor
        //, IBeforeQueryAlteredInterceptor<Person>
        //, IFilterInterceptor
        , IFilterInterceptor<Person>
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

        public IFilter InterceptFilter<T>(IFilter filter)
        {
            return InterceptFilter(filter);
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
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

            criteria.Filters.Add(new SimpleFilter
            {
                Path = "LastName",
                Value = "Lebee",
                Type = FilterType.Equal,
            });

            criteria.Filters.Add(new SimpleFilter
            {
                Path = "FirstName",
                Value = "David,Michaela",
                Type = FilterType.Equal,
            });

            var handler = new QueryHandler();
            handler.AddInterceptor(new PersonQueryInterceptor());
            handler.Execute(queryable, criteria);
        }
    }
}
