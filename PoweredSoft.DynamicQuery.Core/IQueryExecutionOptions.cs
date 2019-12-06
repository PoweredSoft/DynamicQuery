namespace PoweredSoft.DynamicQuery.Core
{
    public interface IQueryExecutionOptions
    {
        bool GroupByInMemory { get; set; }
        bool GroupByInMemoryNullCheck { get; set; }
    }
}
