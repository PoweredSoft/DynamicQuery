using PoweredSoft.DynamicQuery.Core;
using PoweredSoft.DynamicQuery.Test.Mock;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace PoweredSoft.DynamicQuery.Test
{
    public class QueryConvertInterceptorTests
    {
        private class CustomerModel
        {
            public long Id { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string FullName => $"{FirstName} {LastName}";
        }

        private class MockQueryConvertInterceptor : IQueryConvertInterceptor
        {
            public object InterceptResultTo(object entity)
            {
                var customer = entity as Customer;
                var personModel = new CustomerModel
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName
                };
                return personModel;
            }
        }

        private class MockQueryConvertGenericInterceptor : 
            IQueryConvertInterceptor<Customer>,
            IQueryConvertInterceptor<Order>
        {
            public object InterceptResultTo(Customer entity)
            {
                var customer = entity;
                var personModel = new CustomerModel
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName
                };
                return personModel;
            }

            public object InterceptResultTo(Order entity)
            {
                // leave the throw, its on purpose to match the type testing.
                throw new NotImplementedException();
            }
        }

        private class MockQueryConvertGenericInterceptor2 :
            IQueryConvertInterceptor<Customer, CustomerModel>
        {
            public CustomerModel InterceptResultTo(Customer entity)
            {
                var customer = entity;
                var personModel = new CustomerModel
                {
                    Id = customer.Id,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName
                };
                return personModel;
            }
        }

        [Fact]
        public void NonGeneric()
        {
            MockContextFactory.SeedAndTestContextFor("QueryConvertInterceptorTests_NonGeneric", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(new MockQueryConvertInterceptor());
                var result = queryHandler.Execute<Customer, CustomerModel>(ctx.Customers, criteria);
                Assert.All(result.Data, t => Assert.IsType<CustomerModel>(t));
            });
        }

        [Fact]
        public void Generic()
        {
            MockContextFactory.SeedAndTestContextFor("ConvertibleIntereceptorTests_Generic", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(new MockQueryConvertGenericInterceptor());
                var result = queryHandler.Execute<Customer, CustomerModel>(ctx.Customers, criteria);
                Assert.All(result.Data, t => Assert.IsType<CustomerModel>(t));
            });
        }

        [Fact]
        public void Generic2()
        {
            MockContextFactory.SeedAndTestContextFor("ConvertibleIntereceptorTests_Generic2", TestSeeders.SimpleSeedScenario, ctx =>
            {
                var criteria = new QueryCriteria();
                var queryHandler = new QueryHandler();
                queryHandler.AddInterceptor(new MockQueryConvertGenericInterceptor2());
                var result = queryHandler.Execute<Customer, CustomerModel>(ctx.Customers, criteria);
                Assert.All(result.Data, t => Assert.IsType<CustomerModel>(t));
            });
        }
    }
}
