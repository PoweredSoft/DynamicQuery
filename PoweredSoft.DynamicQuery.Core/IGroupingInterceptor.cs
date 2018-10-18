namespace PoweredSoft.DynamicQuery.Core
{
    public interface IGroupingInterceptor : IQueryInterceptor
    {
        IGroup InterceptGroup(IGroup group);
    }
}
