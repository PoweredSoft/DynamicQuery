namespace PoweredSoft.DynamicQuery.Core
{
    public class QueryExecutionOptions : IQueryExecutionOptions
    {
        public bool GroupByInMemory { get; set; } = false;
        public bool GroupByInMemoryNullCheck { get; set; } = false;
    }
}
