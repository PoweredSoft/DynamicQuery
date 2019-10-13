# Dynamic Query

It's a library that allows you to easily query a queryable using a criteria object.

It also offers, to intercept the query using **IQueryInterceptor** implementations.

## Breaking Changes

If you are moving up from v1, the breaking changes details are lower.


## Getting Started

> Install nuget package to your awesome project.

Full Version                  | NuGet                                                                                                                                                                                                                                                                 |                                           NuGet Install
------------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|-------------------------------------------------------:
PoweredSoft.DynamicQuery      | <a href="https://www.nuget.org/packages/PoweredSoft.DynamicQuery/" target="_blank">[![NuGet](https://img.shields.io/nuget/v/PoweredSoft.DynamicQuery.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/PoweredSoft.DynamicQuery/)</a>                |      ```PM> Install-Package PoweredSoft.DynamicQuery```
PoweredSoft.DynamicQuery.Core | <a href="https://www.nuget.org/packages/PoweredSoft.DynamicQuery.Core/" target="_blank">[![NuGet](https://img.shields.io/nuget/v/PoweredSoft.DynamicQuery.Core.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/PoweredSoft.DynamicQuery.Core/)</a> | ```PM> Install-Package PoweredSoft.DynamicQuery.Core```
PoweredSoft.DynamicQuery.AspNetCore | <a href="https://www.nuget.org/packages/PoweredSoft.DynamicQuery.AspNetCore/" target="_blank">[![NuGet](https://img.shields.io/nuget/v/PoweredSoft.DynamicQuery.AspNetCore.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/PoweredSoft.DynamicQuery.AspNetCore/)</a> | ```PM> Install-Package PoweredSoft.DynamicQuery.AspNetCore```
PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson | <a href="https://www.nuget.org/packages/PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson/" target="_blank">[![NuGet](https://img.shields.io/nuget/v/PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson.svg?style=flat-square&label=nuget)](https://www.nuget.org/packages/PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson/)</a> | ```PM> Install-Package PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson```

## Using in ASP.NET Core

The package Asp.net core of dynamic query will help you start to use Dynamic Query faster in your web project.

> For NET CORE 2.x look at v2.0 branch.

### How to configure during startup (NET Core 3)

```csharp
using PoweredSoft.DynamicQuery.AspNetCore.NewtonsoftJson;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services
            .AddMvc()
            .AddPoweredSoftJsonNetDynamicQuery();
    }
}

```

> How to use in a controller

```csharp

[HttpGet]
public IQueryExecutionResult<OfSomething> Get(
            [FromServices]YourContext context, 
            [FromServices]IQueryHandler handler, 
            [FromServices]IQueryCriteria criteria,
            int? page = null,
            int? pageSize = null)
{
    criteria.Page = page;
    criteria.PageSize = pageSize;
    IQueryable<OfSomething> query = context.Somethings;
    var result = handler.Execute(query, criteria);
    return result;
}

[HttpPost]
public IQueryExecutionResult<OfSomething> Read(
    [FromServices]YourContext context, 
    [FromServices]IQueryHandler handler,
    [FromBody]IQueryCriteria criteria)
{
    IQueryable<OfSomething> query = context.Somethings;
    var result = handler.Execute(query, criteria);
    return result;
}
```

> New support for async

```csharp
[HttpPost]
public async Task<IQueryExecutionResult<OfSomething>> Read(
    [FromServices]YourContext context, 
    [FromServices]IQueryHandlerAsync handler,
    [FromBody]IQueryCriteria criteria)
{
    IQueryable<OfSomething> query = context.Somethings;
    var result = await handler.ExecuteAsync(query, criteria);
    return result;
}
```

### Sample Web Project - ASP.NET CORE + EF Core

Visit: https://github.com/PoweredSoft/DynamicQueryAspNetCoreSample

### Breaking Changes if you are migrating from 1.x

Response interface, is now generic ```IQueryResult<T>``` which impacts the way to execute the handler.

#### Grouping results

Since the results are now generic, it's no longer a List<object> in the response so that changes the result if grouping is requested.

You have now a property Groups, and HasSubGroups, and SubGroups.

#### QueryConvertTo Interceptor

If you are using IQueryConvertTo interceptors, it's new that you must specify the type you are converting to
Ex:
```csharp
IQueryable<OfSomething> query = context.Somethings;
var result = handler.Execute<OfSomething, OfSomethingElse>(query, criteria);
```

## Criteria

Criteria must implement the following interfaces

Object           | Interface                                                                | Implementation                                                                | Example                                                              | Description
-----------------|--------------------------------------------------------------------------|-------------------------------------------------------------------------------|----------------------------------------------------------------------|--------------------------------------------
Query Criteria   | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryCriteria.cs)   | [default implementation](../master/PoweredSoft.DynamicQuery/QueryCriteria.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/CriteriaTests.cs#L13) | Wraps the query parameters
Paging           | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryCriteria.cs)   | [default implementation](../master/PoweredSoft.DynamicQuery/QueryCriteria.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/CriteriaTests.cs#L29) | Paging support
Filter           | [interface](../master/PoweredSoft.DynamicQuery.Core/IFilter.cs)          | [default implementation](../master/PoweredSoft.DynamicQuery/Filter.cs)        | [test](../master/PoweredSoft.DynamicQuery.Test/FilterTests.cs#L22)   | Represent a filter to be executed
Simple Filter    | [interface](../master/PoweredSoft.DynamicQuery.Core/ISimpleFilter.cs)    | [default implementation](../master/PoweredSoft.DynamicQuery/Filter.cs)        | [test](../master/PoweredSoft.DynamicQuery.Test/FilterTests.cs#L40)   | Represent a simple filter to be executed
Composite Filter | [interface](../master/PoweredSoft.DynamicQuery.Core/ICompositeFilter.cs) | [default implementation](../master/PoweredSoft.DynamicQuery/Filter.cs)        | [test](../master/PoweredSoft.DynamicQuery.Test/FilterTests.cs#L68)   | Represent a composite filter to be executed
Sort             | [interface](../master/PoweredSoft.DynamicQuery.Core/ISort.cs)            | [default implementation](../master/PoweredSoft.DynamicQuery/Sort.cs)          | [test](../master/PoweredSoft.DynamicQuery.Test/SortTests.cs#L15)     | Represent a sort to be executed
Group            | [interface](../master/PoweredSoft.DynamicQuery.Core/IGroup.cs)           | [default implementation](../master/PoweredSoft.DynamicQuery/Group.cs)         | [test](../master/PoweredSoft.DynamicQuery.Test/GroupTests.cs)        | Represent a group to be executed
Aggregate        | [interface](../master/PoweredSoft.DynamicQuery.Core/IAggregate.cs)       | [default implementation](../master/PoweredSoft.DynamicQuery/Aggregate.cs)     | [test](../master/PoweredSoft.DynamicQuery.Test/AggregateTests.cs)    | Represent an aggregate to be executed

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
IQueryExecutionResult<OfSomeQueryableType> result = queryHandler.Execute(someQueryable, criteria);
```

## Query Result

Here is the interfaces that represent the result of query handling execution.

> Changed in 2.x

```csharp
public interface IAggregateResult
{
    string Path { get; set; }
    AggregateType Type { get; set; }
    object Value { get; set; }
}

public interface IQueryResult<TRecord>
{
    List<IAggregateResult> Aggregates { get; }
    List<TRecord> Data { get; }
}

public interface IGroupQueryResult<TRecord> : IQueryResult<TRecord>
{
    string GroupPath { get; set; }
    object GroupValue { get; set; }
    bool HasSubGroups { get; }
    List<IGroupQueryResult<TRecord>> SubGroups { get; set; }
}

public interface IQueryExecutionResultPaging
{
    long TotalRecords { get; set; }
    long? NumberOfPages { get; set; }
}

public interface IQueryExecutionResult<TRecord> : IQueryResult<TRecord>, IQueryExecutionResultPaging
{

}

public interface IQueryExecutionGroupResult<TRecord> : IQueryExecutionResult<TRecord>
{
    List<IGroupQueryResult<TRecord>> Groups { get; set; }
}
```


## Interceptors

Interceptors are meant to add hooks at certain part of the query handling to allow alteration of the criterias or the queryable it self.

The following is documented in the order of what they are called by the **default** query handler implementation.

> Before the expression is being built

Interceptor                            | Interface                                                                                  | Example                                                     | Description
---------------------------------------|--------------------------------------------------------------------------------------------|-------------------------------------------------------------|------------------------------------------------------------------------------------------------------------
IIncludeStrategyInterceptor            | [interface](../master/PoweredSoft.DynamicQuery.Core/IIncludeStrategyInterceptor.cs)   | [test](../master/PoweredSoft.DynamicQuery.Test/IncludeStrategyTests.cs) | This is to allow you to specify include paths for the queryable
IIncludeStrategyInterceptor&lt;T&gt;   | [interface](../master/PoweredSoft.DynamicQuery.Core/IIncludeStrategyInterceptor.cs)   | [test](../master/PoweredSoft.DynamicQuery.Test/IncludeStrategyTests.cs#L65) | This is to allow you to specify include paths for the queryable
IBeforeQueryFilterInterceptor          | [interface](../master/PoweredSoft.DynamicQuery.Core/IBeforeQueryFilterInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/BeforeFilterTests.cs) | Before adding the filters to the expression
IBeforeQueryFilterInterceptor&lt;T&gt; | [interface](../master/PoweredSoft.DynamicQuery.Core/IBeforeQueryFilterInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/BeforeFilterTests.cs#L64) | Before adding the filters to the expression
INoSortInterceptor                     | [interface](../master/PoweredSoft.DynamicQuery.Core/INoSortInterceptor.cs)            | [test](../master/PoweredSoft.DynamicQuery.Test/NoSortTests.cs) | This is called to allow you to specify an OrderBy in case none is specified, to avoid paging crash with EF6
INoSortInterceptor&lt;T&gt;            | [interface](../master/PoweredSoft.DynamicQuery.Core/INoSortInterceptor.cs)            | [test](../master/PoweredSoft.DynamicQuery.Test/NoSortTests.cs#L65) | This is called to allow you to specify an OrderBy in case none is specified, to avoid paging crash with EF6

> After/During expression building before query execution

Interceptor           | Interface                                                                          | Example                                                     | Description
----------------------|------------------------------------------------------------------------------------|-------------------------------------------------------------|---------------------------------------------------------------------------------------------------
IFilterInterceptor    | [interface](../master/PoweredSoft.DynamicQuery.Core/IFilterInterceptor.cs)    | [test](../master/PoweredSoft.DynamicQuery.Test/FilterInterceptorTests.cs) | This interceptor allows you to change the behavior of a IFilter being applied to the queryable
ISortInterceptor      | [interface](../master/PoweredSoft.DynamicQuery.Core/ISortInterceptor.cs)      | [test](../master/PoweredSoft.DynamicQuery.Test/SortInterceptorTests.cs) | This interceptor allows you to change the behavior of a ISort being applied to the queryable
IGroupInterceptor     | [interface](../master/PoweredSoft.DynamicQuery.Core/IGroupInterceptor.cs)     | [test](../master/PoweredSoft.DynamicQuery.Test/GroupInterceptorTests.cs) | This interceptor allows you to change the behavior of a IGroup being applied to the queryable
IAggregateInterceptor | [interface](../master/PoweredSoft.DynamicQuery.Core/IAggregateInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/AggregateInterceptorTests.cs) | This interceptor allows you to change the behavior of a IAggregate being applied to the queryable

> Post Query execution

Interceptor                       | Interface                                                                             | Example                                                     | Description
----------------------------------|---------------------------------------------------------------------------------------|-------------------------------------------------------------|------------------------------------------------------------------------------------------------
IQueryConvertInterceptor          | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryConvertInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/ConvertibleInterceptorTests.cs) | This interceptor allows you to replace the object that is being returned by the query, by another object instance
IQueryConvertInterceptor&lt;T, T2&gt; | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryConvertInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/ConvertibleInterceptorTests.cs#L72) | This interceptor allows you to replace the object that is being returned by the query, by another object instance **(restricts the source)**
IQueryConvertInterceptor&lt;T, T2&gt; | [interface](../master/PoweredSoft.DynamicQuery.Core/IQueryConvertInterceptor.cs) | [test](../master/PoweredSoft.DynamicQuery.Test/ConvertibleInterceptorTests.cs#L101) | This interceptor allows you to replace the object that is being returned by the query, by another object instance **(restricts the source & output)**