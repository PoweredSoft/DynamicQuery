using PoweredSoft.DynamicQuery.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PoweredSoft.DynamicQuery.Cli
{
    public class PersonQueryInterceptor : IBeforeQueryAlteredInterceptor<Person>
    {
        public IQueryable<Person> InterceptQueryBeforeAltered(IQueryCriteria criteria, IQueryable<Person> queryable) => queryable.Where(t => t.FirstName == "David");
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
                new Person{ Id = 1, FirstName = "David", LastName = "Lebee "},
                new Person{ Id = 2, FirstName = "Michaela", LastName = "Lebee "},
                new Person{ Id = 3, FirstName = "Zohra", LastName = "Lebee "},
                new Person{ Id = 4, FirstName = "Eric", LastName = "Vickar "},
                new Person{ Id = 5, FirstName = "Susan", LastName = "Vickar "},
            };

            var queryable = list.AsQueryable();

            var qb = new QueryBuilder();
            qb.AddInterceptor(new PersonQueryInterceptor());
            qb.Execute(queryable, null);
        }
    }
}
