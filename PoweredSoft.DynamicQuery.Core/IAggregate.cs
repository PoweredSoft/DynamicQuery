namespace PoweredSoft.DynamicQuery.Core
{
    public interface IAggregate
    {
        string Path { get; set; }
        AggregateType Type { get; set; }
    }
}