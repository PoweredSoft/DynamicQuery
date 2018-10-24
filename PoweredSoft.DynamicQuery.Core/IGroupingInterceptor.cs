namespace PoweredSoft.DynamicQuery.Core
{
    public interface IGroupInterceptor : IQueryInterceptor
    {
        IGroup InterceptGroup(IGroup group);
    }
}
