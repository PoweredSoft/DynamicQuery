﻿using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace PoweredSoft.DynamicQuery.Cli
{
    public class PersonQueryInterceptor : IQueryInterceptor
        , IAggregateInterceptor
        , IQueryConvertInterceptor<Person>
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

        public IAggregate InterceptAggregate(IAggregate aggregate)
        {
            if (aggregate.Path == nameof(PersonModel.AgeStr))
                return new Aggregate {Type = aggregate.Type, Path = nameof(Person.Age)};
            return aggregate;
        }

        public object InterceptResultTo(Person entity)
        {
            var personModel = new PersonModel();
            personModel.Id = entity.Id;
            personModel.FirstName = entity.FirstName;
            personModel.LastName = entity.LastName;
            personModel.Age = entity.Age;
            personModel.Sex = entity.Sex;
            return personModel;
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Sex { get; set; }
    }

    public class PersonModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int Age { get; set; }
        public string Sex { get; set; }
        public string AgeStr => $"{Age} years old";
        public string FullName => $"{FirstName} {LastName}";
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
                new Person{ Id = 1, FirstName = "David", LastName = "Lebee", Sex = "Male", Age = 29 },
                new Person{ Id = 2, FirstName = "Michaela", LastName = "Lebee", Sex = "Female", Age = 29},
                new Person{ Id = 3, FirstName = "Zohra", LastName = "Lebee", Sex = "Female", Age = 20},
                new Person{ Id = 4, FirstName = "Eric", LastName = "Vickar", Sex = "Male", Age = 30},
                new Person{ Id = 5, FirstName = "Susan", LastName = "Vickar", Sex = "Female", Age = 30},
            };

            var queryable = list.AsQueryable();
            var criteria = new QueryCriteria();
            criteria.Page = 1;
            criteria.PageSize = 10;

            criteria.Groups = new List<IGroup>()
            {
                new Group { Path = "LastName" },
                new Group { Path = "Sexe" }
            };

            criteria.Aggregates = new List<IAggregate>()
            {
                new Aggregate { Type = AggregateType.Count },
                new Aggregate { Path = "AgeStr", Type = AggregateType.Avg }
            };;

            var handler = new QueryHandler(Enumerable.Empty<IQueryInterceptorProvider>());
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
