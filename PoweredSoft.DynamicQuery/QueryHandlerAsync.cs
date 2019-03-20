using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PoweredSoft.Data.Core;
using PoweredSoft.DynamicLinq;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{
    /*
    public class QueryHandlerAsync : QueryHandlerBase, IQueryHandlerAsync
    {
        internal MethodInfo ExecuteAsyncGeneric = typeof(QueryHandlerAsync).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic).First(t => t.Name == "ExecuteAsync" && t.IsGenericMethod);

        public IAsyncQueryableService AsyncQueryableService { get; }

        internal Task<IQueryExecutionResult> ExecuteAsyncReflected(CancellationToken cancellationToken) => (Task<IQueryExecutionResult>)ExecuteAsyncGeneric.MakeGenericMethod(QueryableUnderlyingType).Invoke(this, new object[] { cancellationToken });

        public QueryHandlerAsync(IAsyncQueryableService asyncQueryableService)
        {
            AsyncQueryableService = asyncQueryableService;
        }

        protected virtual Task<IQueryExecutionResult> ExecuteAsync<T>(CancellationToken cancellationToken = default(CancellationToken))
        {
            CommonBeforeExecute<T>();
            return HasGrouping ? ExecuteAsyncGrouping<T>(cancellationToken) : ExecuteAsyncNoGrouping<T>(cancellationToken);
        }

        public Task<IQueryExecutionResult> ExecuteAsync(IQueryable queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            Reset(queryable, criteria);
            return ExecuteAsyncReflected(cancellationToken);
        }

        protected virtual async Task<IQueryExecutionResult> ExecuteAsyncGrouping<T>(CancellationToken cancellationToken)
        {
            var result = new QueryExecutionResult();

            // preserve queryable.
            var queryableAfterFilters = CurrentQueryable;

            // async.
            result.TotalRecords = await this.AsyncQueryableService.LongCountAsync((IQueryable<T>)queryableAfterFilters, cancellationToken);
            CalculatePageCount(result);

            // intercept groups in advance to avoid doing it more than once :)
            var finalGroups = Criteria.Groups.Select(g => InterceptGroup<T>(g)).ToList();

            // get the aggregates.
            var aggregateResults = await FetchAggregatesAsync<T>(finalGroups, cancellationToken);

            // sorting.
            finalGroups.ForEach(fg => Criteria.Sorts.Insert(0, new Sort(fg.Path, fg.Ascending)));

            // apply sorting and paging.
            ApplySorting<T>();
            ApplyPaging<T>();

            // create group & select expression.
            CurrentQueryable = CurrentQueryable.GroupBy(QueryableUnderlyingType, gb => finalGroups.ForEach((fg, index) => gb.Path(fg.Path, $"Key_{index}")));
            CurrentQueryable = CurrentQueryable.Select(sb =>
            {
                finalGroups.ForEach((fg, index) => sb.Key($"Key_{index}", $"Key_{index}"));
                sb.ToList("Records");
            });

            // loop through the grouped records.
            var groupRecords = await AsyncQueryableService.ToListAsync(CurrentQueryable.Cast<DynamicClass>(), cancellationToken);

            // now join them into logical collections
            var lastLists = new List<List<object>>();
            result.Data = RecursiveRegroup<T>(groupRecords, aggregateResults, Criteria.Groups.First(), lastLists);

            // converted to grouped by.
            await QueryInterceptToGrouped<T>(lastLists);

            result.Aggregates = await CalculateTotalAggregateAsync<T>(queryableAfterFilters, cancellationToken);
            return result;
        }

        protected async Task<IQueryExecutionResult> ExecuteAsyncNoGrouping<T>(CancellationToken cancellationToken)
        {
            var result = new QueryExecutionResult();

            // after filter queryable
            IQueryable<T> afterFilterQueryable = (IQueryable<T>)CurrentQueryable;

            // total records.
            result.TotalRecords = await AsyncQueryableService.LongCountAsync(afterFilterQueryable, cancellationToken);
            CalculatePageCount(result);

            // sorts and paging.
            ApplySorting<T>();
            ApplyPaging<T>();

            // data.
            var entities = await AsyncQueryableService.ToListAsync(((IQueryable<T>)CurrentQueryable), cancellationToken);
            var records = await InterceptConvertTo<T>(entities);
            result.Data = records;

            // aggregates.
            result.Aggregates = await CalculateTotalAggregateAsync<T>(afterFilterQueryable, cancellationToken);
            return result;
        }

        protected virtual async Task<List<IAggregateResult>> CalculateTotalAggregateAsync<T>(IQueryable queryableAfterFilters, CancellationToken cancellationToken)
        {
            if (!Criteria.Aggregates.Any())
                return null;

            IQueryable selectExpression = CreateTotalAggregateSelectExpression<T>(queryableAfterFilters);
            var aggregateResult = await AsyncQueryableService.FirstOrDefaultAsync(selectExpression.Cast<DynamicClass>());
            return MaterializeCalculateTotalAggregateResult(aggregateResult);
        }

        protected async virtual Task<List<List<DynamicClass>>> FetchAggregatesAsync<T>(List<IGroup> finalGroups, CancellationToken cancellationToken)
        {
            if (!Criteria.Aggregates.Any())
                return null;

            var previousGroups = new List<IGroup>();

            var whenAllResult = await Task.WhenAll(finalGroups.Select(fg =>
            {
                IQueryable selectExpression = CreateFetchAggregateSelectExpression<T>(fg, previousGroups);
                var selectExpressionCasted = selectExpression.Cast<DynamicClass>();
                var aggregateResult = AsyncQueryableService.ToListAsync(selectExpressionCasted, cancellationToken);
                previousGroups.Add(fg);
                return aggregateResult;
            }));

            var finalResult = whenAllResult.ToList();
            return finalResult;
        }
    }*/
}
