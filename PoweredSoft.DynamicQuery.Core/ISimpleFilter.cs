namespace PoweredSoft.DynamicQuery.Core
{
    public interface ISimpleFilter : IFilter
    {
        string Path { get; set; }
        object Value { get; set; }
        bool? Not { get; set; }
    }
}
