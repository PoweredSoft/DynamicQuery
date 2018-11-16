using System;
using System.Collections.Generic;

namespace PoweredSoft.DynamicQuery.Test.Mock
{
    public static class TestSeeders
    {
        public static void SimpleSeedScenario(string testName)
        {
            MockContextFactory.TestContextFor(testName, ctx =>
            {
                ctx.AddRange(new Customer[]
                {
                    new Customer() { Id = 1, FirstName = "David", LastName = "Lebee" },
                    new Customer() { Id = 2, FirstName = "John", LastName = "Doe" },
                    new Customer() { Id = 3, FirstName = "Chuck", LastName = "Norris" },
                    new Customer() { Id = 4, FirstName = "Nelson", LastName = "Mendela" },
                    new Customer() { Id = 5, FirstName = "Jimi", LastName = "Hendrix" },
                    new Customer() { Id = 6, FirstName = "Axel", LastName = "Rose" },
                    new Customer() { Id = 7, FirstName = "John", LastName = "Frusciante" },
                    new Customer() { Id = 8, FirstName = "Michael", LastName = "Jackson" },
                    new Customer() { Id = 9, FirstName = "Anita", LastName = "Franklin" },
                });

                ctx.AddRange(new Item[]
                {
                    new Item { Id = 1, Name = "Computer", Price = 1000M },
                    new Item { Id = 2, Name = "Mice", Price = 25.99M },
                    new Item { Id = 3, Name = "Keyboard", Price = 100M },
                    new Item { Id = 4, Name = "Screen", Price = 499.98M },
                    new Item { Id = 5, Name = "Printer", Price = 230.95M },
                    new Item { Id = 6, Name = "HDMI Cables", Price = 20M },
                    new Item { Id = 7, Name = "Power Cables", Price = 5.99M }
                });

                ctx.Orders.AddRange(new Order[]
                {
                    new Order()
                    {
                        Id = 1,
                        OrderNum = 1000,
                        CustomerId = 1,
                        Date = new DateTime(2018, 1, 1),
                        OrderItems = new List<OrderItem>()
                        {
                            new OrderItem() { Id = 1, ItemId = 1, PriceAtTheTime = 1000M, Quantity = 1 },
                            new OrderItem() { Id = 2, ItemId = 2, PriceAtTheTime =  30M, Quantity = 1 },
                            new OrderItem() { Id = 3, ItemId = 4, PriceAtTheTime =  399.99M, Quantity = 2 },
                            new OrderItem() { Id = 4, ItemId = 6, PriceAtTheTime = 20, Quantity = 2 },
                            new OrderItem() { Id = 8, ItemId = 6, PriceAtTheTime = 3.99M, Quantity = 3 }
                        } 
                    },
                    new Order()
                    {
                        Id = 2,
                        OrderNum = 1001,
                        CustomerId = 2,
                        Date = new DateTime(2018, 2, 1),
                        OrderItems = new List<OrderItem>()
                        {
                            new OrderItem() { Id = 9, ItemId = 6, PriceAtTheTime = 20, Quantity = 2 },
                            new OrderItem() { Id = 10, ItemId = 6, PriceAtTheTime = 3.99M, Quantity = 3 }
                        }
                    },
                    new Order()
                    {
                        Id = 3,
                        OrderNum = 1002,
                        CustomerId = 3,
                        Date = new DateTime(2018, 2, 1),
                        OrderItems = new List<OrderItem>()
                        {
                            new OrderItem() { Id = 11, ItemId = 5, PriceAtTheTime = 499.99M, Quantity = 1 },
                            new OrderItem() { Id = 12, ItemId = 6, PriceAtTheTime = 20, Quantity = 1 },
                            new OrderItem() { Id = 13, ItemId = 7, PriceAtTheTime = 3.99M, Quantity = 1 }
                        }
                    },
                    new Order()
                    {
                        Id = 4,
                        OrderNum = 1003,
                        CustomerId = 1,
                        Date = new DateTime(2018, 3, 1),
                        OrderItems = new List<OrderItem>()
                        {
                            new OrderItem() { Id = 14, ItemId = 2, PriceAtTheTime = 50M, Quantity = 1 },
                            new OrderItem() { Id = 15, ItemId = 3, PriceAtTheTime = 75.50M, Quantity = 1 },
                        }
                    }
                });

                ctx.SaveChanges();
            });
        }

        internal static void SeedTicketScenario(string testName)
        {
            MockContextFactory.TestContextFor(testName, ctx =>
            {
                var faker = new Bogus.Faker<Ticket>()
                    .RuleFor(t => t.TicketType, (f, u) => f.PickRandom("new", "open", "refused", "closed"))
                    .RuleFor(t => t.Title, (f, u) => f.Lorem.Sentence())
                    .RuleFor(t => t.Details, (f, u) => f.Lorem.Paragraph())
                    .RuleFor(t => t.IsHtml, (f, u) => false)
                    .RuleFor(t => t.TagList, (f, u) => string.Join(",", f.Commerce.Categories(3)))
                    .RuleFor(t => t.CreatedDate, (f, u) => f.Date.Recent(100))
                    .RuleFor(t => t.Owner, (f, u) => f.Person.FullName)
                    .RuleFor(t => t.AssignedTo, (f, u) => f.Person.FullName)
                    .RuleFor(t => t.TicketStatus, (f, u) => f.PickRandom(1, 2, 3))
                    .RuleFor(t => t.LastUpdateBy, (f, u) => f.Person.FullName)
                    .RuleFor(t => t.LastUpdateDate, (f, u) => f.Date.Soon(5))
                    .RuleFor(t => t.Priority, (f, u) => f.PickRandom("low", "medium", "high", "critical"))
                    .RuleFor(t => t.AffectedCustomer, (f, u) => f.PickRandom(true, false))
                    .RuleFor(t => t.Version, (f, u) => f.PickRandom("1.0.0", "1.1.0", "2.0.0"))
                    .RuleFor(t => t.ProjectId, (f, u) => f.Random.Number(100))
                    .RuleFor(t => t.DueDate, (f, u) => f.Date.Soon(5))
                    .RuleFor(t => t.EstimatedDuration, (f, u) => f.Random.Number(20))
                    .RuleFor(t => t.ActualDuration, (f, u) => f.Random.Number(20))
                    .RuleFor(t => t.TargetDate, (f, u) => f.Date.Soon(5))
                    .RuleFor(t => t.ResolutionDate, (f, u) => f.Date.Soon(5))
                    .RuleFor(t => t.Type, (f, u) => f.PickRandom(1, 2, 3))
                    .RuleFor(t => t.ParentId, () => 0)
                    .RuleFor(t => t.PreferredLanguage, (f, u) => f.PickRandom("fr", "en", "es"))
                ;

                var fakeModels = new List<Ticket>();
                for (var i = 0; i < 500; i++)
                {
                    var t = faker.Generate();
                    t.TicketId = i + 1;
                    fakeModels.Add(t);
                }

                ctx.AddRange(fakeModels);
                ctx.SaveChanges();
            });
        }
    }
}
