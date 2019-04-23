using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using PoweredSoft.Data;
using PoweredSoft.Data.Core;
using PoweredSoft.DynamicLinq;
using PoweredSoft.DynamicQuery.Core;

namespace PoweredSoft.DynamicQuery
{

    public class QueryHandlerAsync : QueryHandlerBase, IQueryHandlerAsync
    {
        public IAsyncQueryableService AsyncQueryableService { get; }

        public QueryHandlerAsync(IAsyncQueryableService asyncQueryableService)
        {
            AsyncQueryableService = asyncQueryableService;
        }

        protected virtual Task<IQueryExecutionResult<TRecord>> FinalExecuteAsync<TSource, TRecord>(CancellationToken cancellationToken = default(CancellationToken))
        {
            CommonBeforeExecute<TSource>();
            return HasGrouping ? ExecuteAsyncGrouping<TSource, TRecord>(cancellationToken) : ExecuteAsyncNoGrouping<TSource, TRecord>(cancellationToken);
        }

        protected virtual async Task<IQueryExecutionResult<TRecord>> ExecuteAsyncGrouping<TSource, TRecord>(CancellationToken cancellationToken)
        {
            var result = new QueryExecutionGroupResult<TRecord>();

            // preserve queryable.
            var queryableAfterFilters = CurrentQueryable;

            // async.
            result.TotalRecords = await this.AsyncQueryableService.LongCountAsync((IQueryable<TSource>)queryableAfterFilters, cancellationToken);
            CalculatePageCount(result);

            // intercept groups in advance to avoid doing it more than once :)
            var finalGroups = Criteria.Groups.Select(g => InterceptGroup<TSource>(g)).ToList();

            // get the aggregates.
            var aggregateResults = await FetchAggregatesAsync<TSource>(finalGroups, cancellationToken);

            // sorting.
            finalGroups.ReversedForEach(fg => Criteria.Sorts.Insert(0, new Sort(fg.Path, fg.Ascending)));

            // apply sorting and paging.
            ApplySorting<TSource>();
            ApplyPaging<TSource>();

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
            var lastLists = new List<(List<TSource> entities, IGroupQueryResult<TRecord> group)>();
            result.Groups = RecursiveRegroup<TSource, TRecord>(groupRecords, aggregateResults, Criteria.Groups.First(), lastLists);

            // converted to grouped by.
            await QueryInterceptToGrouped<TSource, TRecord>(lastLists);

            result.Aggregates = await CalculateTotalAggregateAsync<TSource>(queryableAfterFilters, cancellationToken);
            return result;
        }

        protected async Task<IQueryExecutionResult<TRecord>> ExecuteAsyncNoGrouping<TSource, TRecord>(CancellationToken cancellationToken)
        {
            var result = new QueryExecutionResult<TRecord>();

            // after filter queryable
            IQueryable<TSource> afterFilterQueryable = (IQueryable<TSource>)CurrentQueryable;

            // total records.
            result.TotalRecords = await AsyncQueryableService.LongCountAsync(afterFilterQueryable, cancellationToken);
            CalculatePageCount(result);

            // sorts and paging.
            ApplySorting<TSource>();
            ApplyPaging<TSource>();

            // data.
            var entities = await AsyncQueryableService.ToListAsync(((IQueryable<TSource>)CurrentQueryable), cancellationToken);
            var records = await InterceptConvertTo<TSource, TRecord>(entities);
            result.Data = records;

            // aggregates.
            result.Aggregates = await CalculateTotalAggregateAsync<TSource>(afterFilterQueryable, cancellationToken);
            return result;
        }

        protected virtual async Task<List<IAggregateResult>> CalculateTotalAggregateAsync<TSource>(IQueryable queryableAfterFilters, CancellationToken cancellationToken)
        {
            if (!Criteria.Aggregates.Any())
                return null;

            IQueryable selectExpression = CreateTotalAggregateSelectExpression<TSource>(queryableAfterFilters);
            var aggregateResult = await AsyncQueryableService.FirstOrDefaultAsync(selectExpression.Cast<DynamicClass>());
            return MaterializeCalculateTotalAggregateResult(aggregateResult);
        }

        protected async virtual Task<List<List<DynamicClass>>> FetchAggregatesAsync<TSource>(List<IGroup> finalGroups, CancellationToken cancellationToken)
        {
            if (!Criteria.Aggregates.Any())
                return null;

            var previousGroups = new List<IGroup>();

            var whenAllResult = await Task.WhenAll(finalGroups.Select(fg =>
            {
                IQueryable selectExpression = CreateFetchAggregateSelectExpression<TSource>(fg, previousGroups);
                var selectExpressionCasted = selectExpression.Cast<DynamicClass>();
                var aggregateResult = AsyncQueryableService.ToListAsync(selectExpressionCasted, cancellationToken);
                previousGroups.Add(fg);
                return aggregateResult;
            }));

            var finalResult = whenAllResult.ToList();
            return finalResult;
        }

        public Task<IQueryExecutionResult<TSource>> ExecuteAsync<TSource>(IQueryable<TSource> queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            Reset(queryable, criteria);
            return FinalExecuteAsync<TSource, TSource>(cancellationToken);
        }

        public Task<IQueryExecutionResult<TRecord>> ExecuteAsync<TSource, TRecord>(IQueryable<TSource> queryable, IQueryCriteria criteria, CancellationToken cancellationToken = default(CancellationToken))
        {
            Reset(queryable, criteria);
            return FinalExecuteAsync<TSource, TRecord>(cancellationToken);
        }
    }
}
