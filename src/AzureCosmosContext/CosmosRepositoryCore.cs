using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading.Tasks;
using AzureCosmosContext.Extensions;
using Newtonsoft.Json;

namespace AzureCosmosContext
{
    /// <summary>
    /// 
    /// </summary>
    ///<remarks>
    /// CosmosRepositoryCore クラスでは1 Database, N Container を管理し、
    /// 継承するクラスでは、1 repository に 1 Container のみ利用する想定。
    /// CRUD 処理のメソッド引数で PartitionKey の省略が可能だが、省略するとCosmosDB の通信が一度走るため、PartitionKey省略のメソッドは現時点では実装しない方針。
    /// https://github.com/Azure/azure-cosmos-dotnet-v3/blob/master/Microsoft.Azure.Cosmos/src/Resource/Container/ContainerCore.Items.cs

    ///</remarks>
    public abstract class CosmosRepositoryCore
    {
        #region Variables/Constructors

        private readonly CosmosContext _context;
        private readonly ILogger _logger;

        private Container _containerInstance;
        private Container Container => _containerInstance ?? (_containerInstance = _context.Containers[ContainerId]);

        public abstract string ContainerId { get; }


        protected CosmosRepositoryCore(CosmosContext context, ILogger logger)
        {
            _context = context;
            _logger = logger;
        }

        #endregion

        #region Create

        /// <summary>
        /// Crate a Item.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task CreateItemAsync<TItem>(object partitionKey, TItem item)
        {
            try
            {
                var response = await Container.CreateItemAsync(item, new PartitionKey(partitionKey));
                LogRequestCharge(response);
            }
            catch (CosmosException e)
            {
                if (e.StatusCode == HttpStatusCode.Conflict)
                {
                    _logger.LogError(e.ActivityId, e, e.Message);
                }
                throw;
            }
        }

        #endregion

        #region Read/Query


        /// <summary>
        /// Get a Item by Id
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        protected virtual async Task<TItem> GetItemByIdAsync<TItem>(object partitionKey, string itemId)
        {
            var response = await Container.ReadItemAsync<TItem>(itemId, new PartitionKey(partitionKey));
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
        /// <param name="maxBufferedItemCount"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        protected virtual async Task<IEnumerable<TItem>> GetItemsAsync<TItem>(QueryDefinition query, int maxConcurrency = 1, int maxItemCount = 10, int maxBufferedItemCount = 10, object partitionKey = null)
        {
            // TODO: method argument's default value's definition....move to appsettings?

            var queryRequestOptions = new QueryRequestOptions()
            {
                MaxConcurrency = maxConcurrency,
                MaxBufferedItemCount = maxBufferedItemCount,
                MaxItemCount = maxItemCount
            }.AddPartitionKey(partitionKey);

            var set = Container.GetItemQueryIterator<TItem>(query, requestOptions: queryRequestOptions);

            var items = new List<TItem>();
            var activeIds = new List<string>();
            var requestCharges = new List<double>();

            while (set.HasMoreResults)
            {
                var feedResponse = await set.ReadNextAsync();
                items.AddRange(feedResponse);

                activeIds.Add(feedResponse.ActivityId);
                requestCharges.Add(feedResponse.RequestCharge);
            }

            LogRequestCharge(activeIds, requestCharges);

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

        #endregion

        #region update/upsert

        /// <summary>
        /// Update a Item.
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        protected virtual async Task UpdateItemAsync<TItem>(string partitionKey, string itemId, TItem item)
        {
            var response = await Container.ReplaceItemAsync(item, itemId, new PartitionKey(partitionKey));
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
            var response = await Container.UpsertItemAsync(item, new PartitionKey(partitionKey));
            LogRequestCharge(response);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Delete a Item
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="partitionKey"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        protected virtual async Task DeleteItemAsync<TItem>(object partitionKey, string itemId)
        {
            var response = await Container.DeleteItemAsync<TItem>(itemId, new PartitionKey(partitionKey));
            LogRequestCharge(response);
        }

        #endregion

        #region RequetCharge logging


        private void LogRequestCharge<TItem>(ItemResponse<TItem> response)
        {
            // 暫定で、とりあえずログ出してるのみ。
            // TODO: App Insights の custom metric に組み込むとか、呼び出しのメソッド名出すとか？もしくはメソッドコール時にログ出して App insights のタイムラインで確認する？
            _logger.LogInformation("Request Charge log: {DatabaseId}.{ContainerId};  ActivityId:{ActivityId}; RC: {RequestCharge};", _context.Database.Id, ContainerId, response.ActivityId, response.RequestCharge);
        }

        private void LogRequestCharge(IEnumerable<string> activeIds, IEnumerable<double> requestCharges)
        {
            // QueryDefinition から queryText が取得できない！なんで "Query" は private readonly Property なんだ？
            _logger.LogInformation("Request Charge log: {DatabaseId}.{ContainerId};  Total RC: {RequestCharge};", _context.Database.Id, ContainerId, requestCharges.Sum());
            _logger.LogInformation("Request Charge log - detail: ActiveIds: {activeIds}", JsonConvert.SerializeObject(activeIds));
            _logger.LogInformation("Request Charge log - detail: requestCharges: {activeIds}", JsonConvert.SerializeObject(requestCharges));
        }

        protected async Task UpdateDatabaseRequestCharge(int throughput)
        {
            // TODO
            await _context.Database.ReplaceThroughputAsync(throughput);
        }

        #endregion
    }
}