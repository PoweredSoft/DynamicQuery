namespace PoweredSoft.DynamicQuery.Core
{
    public interface ISort
    {
        string Path { get; set; }
        bool? Ascending { get; set; }
    }
}
