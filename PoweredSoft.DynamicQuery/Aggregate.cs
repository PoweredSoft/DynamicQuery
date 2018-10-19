using System;
using System.Collections.Generic;
using System.Text;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    public class Aggregate : IAggregate
    {
        public string Path { get; set; }
        public AggregateType Type { get; set; }
    }
}
