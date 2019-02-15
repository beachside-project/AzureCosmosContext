using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AzureCosmosContext
{
    public abstract class CosmosRepositoryCore
    {
        private readonly CosmosContext _context;
        private readonly ILogger _logger;

        public abstract string ContainerId { get; }

        protected CosmosRepositoryCore(CosmosContext context, ILogger<CosmosRepositoryCore> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Itemの作成
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task CreateItemAsync<TItem>(object partitionKey, TItem item)
        {
            var response = await _context.Containers[ContainerId].Items.CreateItemAsync(partitionKey, item);
            LogRequestCharge(response.RequestCharge);
        }

        /// <summary>
        /// Itemの更新
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task UpdateItemAsync<TItem>(object partitionKey, string itemId, TItem item)
        {
            var response = await _context.Containers[ContainerId].Items.ReplaceItemAsync(partitionKey, itemId, item);
            LogRequestCharge(response.RequestCharge);
        }

        /// <summary>
        /// ItemのUpsert
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task UpsertItemAsync<TItem>(object partitionKey, TItem item)
        {
            var response = await _context.Containers[ContainerId].Items.UpsertItemAsync(partitionKey, item);
            LogRequestCharge(response.RequestCharge);
        }

        /// <summary>
        /// Itemの削除
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        protected virtual async Task DeleteItemAsync<TItem>(object partitionKey, string itemId)
        {
            var response = await _context.Containers[ContainerId].Items.DeleteItemAsync<TItem>(partitionKey, itemId);
            LogRequestCharge(response.RequestCharge);
        }

        /// <summary>
        /// Idを検索キーにして単一のItemを取得
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        protected virtual async Task<TItem> GetItemByIdAsync<TItem>( object partitionKey, string itemId)
        {
            var response = await _context.Containers[ContainerId].Items.ReadItemAsync<TItem>(partitionKey, itemId);
            LogRequestCharge(response.RequestCharge);

            return response.Resource;
        }

        /// <summary>
        /// Sql Queryで複数のItemを取得
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
            // TODO: total RC logging

            return items;
        }

        /// <summary>
        /// Predicateで複数のItemを取得
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="predicate"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<TItem>> GetItemsAsync<TItem>(Expression<Func<TItem, bool>> predicate)
        {
            await Task.CompletedTask;
            throw new NotImplementedException();
        }

        protected void ValidateContainerId(string containerId)
        {
            if (!_context.ContainerIds.Contains(containerId))
            {
                _logger.LogCritical("Repository Container definition invalid({containerId})", containerId);
                throw new ArgumentException("Repository Container definition invalid({containerId})", containerId);
            }
        }

        private void LogRequestCharge(double rc)
        {
            // TODO: log RC
        }

        protected async Task UpdateDatabaseRequestCharge(int throughput)
        {
            await _context.Database.ReplaceProvisionedThroughputAsync(throughput);
        }
    }
}