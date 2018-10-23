# Dynamic Query

It's a library that allows you to easily query a queryable using a criteria object.

It also offers, to intercept the query using **IQueryInterceptor** implementations.

## Getting Started

> Install nuget package to your awesome project.

Full Version                  | NuGet                                                                                                                                                                                                                                                                 |                                           NuGet Install
------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------:
PoweredSoft.DynamicQuery      | <a href="https://www.nuget.org/packages/PoweredSoft.DynamicQuery/" target="_blank">[![NuGet](https://img.shields.io/nuget/v/PoweredSoft.DynamicQuery.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/PoweredSoft.DynamicQuery/)</a>                |      ```PM> Install-Package PoweredSoft.DynamicQuery```
PoweredSoft.DynamicQuery.Core | <a href="https://www.nuget.org/packages/PoweredSoft.DynamicQuery.Core/" target="_blank">[![NuGet](https://img.shields.io/nuget/v/PoweredSoft.DynamicQuery.Core.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/PoweredSoft.DynamicQuery.Core/)</a> | ```PM> Install-Package PoweredSoft.DynamicQuery.Core```

## Criteria

Criteria must implement the following interfaces

Object           | Interface                                                                | Implementation                                                                | Example                                                | Description
-----------------|--------------------------------------------------------------------------|-------------------------------------------------------------------------------|--------------------------------------------------------|--------------------------------------------
Query Criteria   | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryCriteria.cs)   | [default implementation](../master/PoweredSoft.DynamicQuery/QueryCriteria.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/CriteriaTests.cs) | Wraps the query parameters
Filter           | [interface](../master/PoweredSoft.DynamicQuery.Core/IFilter.cs)          | [default implementation](../master/PoweredSoft.DynamicQuery/Filter.cs)        | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Represent a filter to be executed
Simple Filter    | [interface](../master/PoweredSoft.DynamicQuery.Core/ISimpleFilter.cs)    | [default implementation](../master/PoweredSoft.DynamicQuery/Filter.cs)        | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Represent a simple filter to be executed
Composite Filter | [interface](../master/PoweredSoft.DynamicQuery.Core/ICompositeFilter.cs) | [default implementation](../master/PoweredSoft.DynamicQuery/Filter.cs)        | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Represent a composite filter to be executed
Sort             | [interface](../master/PoweredSoft.DynamicQuery.Core/ISort.cs)            | [default implementation](../master/PoweredSoft.DynamicQuery/Sort.cs)          | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Represent a sort to be executed
Group            | [interface](../master/PoweredSoft.DynamicQuery.Core/IGroup.cs)           | [default implementation](../master/PoweredSoft.DynamicQuery/Group.cs)         | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Represent a group to be executed
Aggregate        | [interface](../master/PoweredSoft.DynamicQuery.Core/IAggregate.cs)       | [default implementation](../master/PoweredSoft.DynamicQuery/Aggregate.cs)     | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Represent an aggregate to be executed

### Sample

```csharp
var criteria = new QueryCriteria
{
    Page = 1,
    PageSize = 12,
    Filters = new List<IFilter>
    {
        new SimpleFilter { Path = "FirstName", Type = FilterType.Equal, Value = "John" }
    }
};

var queryHandler = new QueryHandler();
IQueryExecutionResult result = queryHandler.Execute(someQueryable, criteria);
```

## Query Result

Here is the interfaces that represent the result of query handling execution.

```csharp
public interface IAggregateResult
{
    string Path { get; set; }
    AggregateType Type { get; set; }
    object Value { get; set; }
}

public interface IQueryResult
{
    List<IAggregateResult> Aggregates { get; }
    List<object> Data { get; }
}

public interface IGroupQueryResult : IQueryResult
{
    string GroupPath { get; set; }
    object GroupValue { get; set; }
}

public interface IQueryExecutionResult : IQueryResult
{
    long TotalRecords { get; set; }
    long? NumberOfPages { get; set; }
}
```


## Interceptors

Interceptors are meant to add hooks at certain part of the query handling to allow alteration of the criterias or the queryable it self.\
The following is documented in the order of what they are called by the **default** query handler implementation.

> Before the expression is being built

Interceptor                            | Interface                                                                                  | Example                                                     | Description
---------------------------------------|--------------------------------------------------------------------------------------------|-------------------------------------------------------------|------------------------------------------------------------------------------------------------------------
IIncludeStrategyInterceptor            | [interface](../master/PoweredSoft.DynamicQuery.Core/IIncludeStrategyInterceptor.cs)   | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This is to allow you to specify include paths for the queryable
IIncludeStrategyInterceptor&lt;T&gt;   | [interface](../master/PoweredSoft.DynamicQuery.Core/IIncludeStrategyInterceptor.cs)   | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This is to allow you to specify include paths for the queryable
IBeforeQueryFilterInterceptor          | [interface](../master/PoweredSoft.DynamicQuery.Core/IBeforeQueryFilterInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Before adding the filters to the expression
IBeforeQueryFilterInterceptor&lt;T&gt; | [interface](../master/PoweredSoft.DynamicQuery.Core/IBeforeQueryFilterInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | Before adding the filters to the expression
INoSortInterceptor                     | [interface](../master/PoweredSoft.DynamicQuery.Core/INoSortInterceptor.cs)            | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This is called to allow you to specify an OrderBy in case none is specified, to avoid paging crash with EF6
INoSortInterceptor&lt;T&gt;            | [interface](../master/PoweredSoft.DynamicQuery.Core/INoSortInterceptor.cs)            | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This is called to allow you to specify an OrderBy in case none is specified, to avoid paging crash with EF6

> After/During expression building before query execution

Interceptor           | Interface                                                                          | Example                                                     | Description
----------------------|------------------------------------------------------------------------------------|-------------------------------------------------------------|---------------------------------------------------------------------------------------------------
IFilterInterceptor    | [interface](../master/PoweredSoft.DynamicQuery.Core/IFilterInterceptor.cs)    | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This interceptor allows you to change the behavior of a IFilter being applied to the queryable
ISortInterceptor      | [interface](../master/PoweredSoft.DynamicQuery.Core/ISortInterceptor.cs)      | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This interceptor allows you to change the behavior of a ISort being applied to the queryable
IGroupInterceptor     | [interface](../master/PoweredSoft.DynamicQuery.Core/IGroupInterceptor.cs)     | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This interceptor allows you to change the behavior of a IGroup being applied to the queryable
IAggregateInterceptor | [interface](../master/PoweredSoft.DynamicQuery.Core/IAggregateInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This interceptor allows you to change the behavior of a IAggregate being applied to the queryable

> Post Query execution

Interceptor                       | Interface                                                                             | Example                                                     | Description
----------------------------------|---------------------------------------------------------------------------------------|-------------------------------------------------------------|------------------------------------------------------------------------------------------------
IQueryConvertInterceptor          | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryConvertInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This interceptor allows you to replace the object that is being returned by the query, by another object instance
IQueryConvertInterceptor&lt;T&gt; | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryConvertInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/TBT.md) | This interceptor allows you to replace the object that is being returned by the query, by another object instance
