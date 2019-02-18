using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureCosmosContext
{
    /// <summary>
    /// 
    /// </summary>
    ///<remarks>
    /// CosmosRepositoryCore クラスでは1 Database, N Container を管理し、
    /// 継承するクラスでは、1 repository に 1 Container のみ利用する想定。
    ///</remarks>
    public abstract class CosmosRepositoryCore
    {
        private readonly CosmosContext _context;
        private readonly ILogger _logger;

        public abstract string ContainerId { get; }

        protected CosmosRepositoryCore(CosmosContext context, ILogger<CosmosRepositoryCore> logger)
        {
            _context = context;
            _logger = logger;
            ValidateContainerId();
        }

        /// <summary>
        /// Crate a Item.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task CreateItemAsync<TItem>(object partitionKey, TItem item)
        {
            var response = await _context.Containers[ContainerId].Items.CreateItemAsync(partitionKey, item);
            LogRequestCharge(response);
        }

        /// <summary>
        /// Update a Item.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task UpdateItemAsync<TItem>(object partitionKey, string itemId, TItem item)
        {
            var response = await _context.Containers[ContainerId].Items.ReplaceItemAsync(partitionKey, itemId, item);
            LogRequestCharge(response);
        }

        /// <summary>
        /// Upsert a Item
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task UpsertItemAsync<TItem>(object partitionKey, TItem item)
        {
            var response = await _context.Containers[ContainerId].Items.UpsertItemAsync(partitionKey, item);
            LogRequestCharge(response);
        }

        /// <summary>
        /// Delete a Item
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        protected virtual async Task DeleteItemAsync<TItem>(object partitionKey, string itemId)
        {
            var response = await _context.Containers[ContainerId].Items.DeleteItemAsync<TItem>(partitionKey, itemId);
            LogRequestCharge(response);
        }

        /// <summary>
        /// Get a Item by Id
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        protected virtual async Task<TItem> GetItemByIdAsync<TItem>(object partitionKey, string itemId)
        {
            var response = await _context.Containers[ContainerId].Items.ReadItemAsync<TItem>(partitionKey, itemId);
            LogRequestCharge(response);

            return response.Resource;
        }

        /// <summary>
        /// Get Items by SqlQuery
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="query"></param>
        /// <param name="maxConcurrency"></param>
        /// <param name="maxItemCount"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<TItem>> GetItemsAsync<TItem>(CosmosSqlQueryDefinition query, int maxConcurrency = 1, int maxItemCount = 10)
        {
            // TODO: parameter's default value's definition....move to appsettings?
            var set = _context.Containers[ContainerId].Items.CreateItemQuery<TItem>(query, maxConcurrency, maxItemCount);
            var items = new List<TItem>();
            while (set.HasMoreResults)
            {
                var response = await set.FetchNextSetAsync();
                items.AddRange(response);
            }
            // TODO: logging total RC 

            return items;
        }

        /// <summary>
        /// Get Items by Predicate
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<TItem>> GetItemsAsync<TItem>(Expression<Func<TItem, bool>> predicate)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }


        private void ValidateContainerId()
        {
            if (string.IsNullOrWhiteSpace(ContainerId))
            {
                _logger.LogCritical("Repository's Container definition invalid(null or empty)");
                throw new ArgumentNullException(ContainerId);
            }

            if (!_context.ContainerIds.Contains(ContainerId))
            {
                _logger.LogCritical("Repository's Container definition invalid({containerId})", ContainerId);
                throw new ArgumentOutOfRangeException(ContainerId);
            }
        }

        private void LogRequestCharge<TItem>(CosmosItemResponse<TItem> response)
        {
            // 暫定で、とりあえずログ出してるのみ。
            // TODO: App Insights の custom metric に組み込むとか、呼び出しのメソッド名出すとか？もしくはメソッドコール時にログ出して App insights のタイムラインで確認する？
            _logger.LogInformation("Request Charge log: {DatabaseId}.{ContainerId};  ActivityId:{ActivityId}; RC: {RequestCharge};", _context.Database.Id, ContainerId, response.ActivityId, response.RequestCharge);
        }

        protected async Task UpdateDatabaseRequestCharge(int throughput)
        {
            await _context.Database.ReplaceProvisionedThroughputAsync(throughput);
        }
    }
}