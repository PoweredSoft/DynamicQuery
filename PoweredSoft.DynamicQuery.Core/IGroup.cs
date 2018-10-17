namespace PoweredSoft.DynamicQuery.Core
{
    public interface IGroup
    {
        string Path { get; set; }
        bool? Ascending { get; set; }
    }
}
