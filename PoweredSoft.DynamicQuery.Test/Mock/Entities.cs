using System;
using System.Collections.Generic;
using Bogus.DataSets;

namespace PoweredSoft.DynamicQuery.Test.Mock
{
    public class Order
    {
        public long Id { get; set; }
        public long OrderNum { get; set; }
        public DateTime Date { get; set; }
        public long CustomerId { get; set; }

        public virtual Customer Customer { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }

    public class Customer
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public ICollection<Order> Orders { get; set; } = new HashSet<Order>();

        public DateTime RegisterTime { get; set; }

        public TimeSpan BirthTime { get; set; }
    }

    public class Item
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new HashSet<OrderItem>();
    }

    public class OrderItem
    {
        public long Id { get; set; }
        public long Quantity { get; set; }
        public decimal PriceAtTheTime { get; set; }
        public long ItemId { get; set; }
        public long OrderId { get; set; }

        public virtual Item Item { get; set; }
        public virtual Order Order { get; set; }
    }
}