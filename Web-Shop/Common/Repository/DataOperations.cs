using Azure.Data.Tables;

namespace Common.Repository
{
    public class DataOperations<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _tableClient;

        public DataOperations(TableClient tableClient)
        {
            _tableClient = tableClient;
        }

        public async IAsyncEnumerable<T> RetrieveAllAsync(string partitionKey)
        {
            await foreach (var entity in _tableClient.QueryAsync<T>(e => e.PartitionKey == partitionKey))
            {
                yield return entity;
            }
        }

        public async Task AddAsync(T entity)
        {
            await _tableClient.AddEntityAsync((ITableEntity)entity);
        }

        public async Task Modify(T entity)
        {
            await _tableClient.UpdateEntityAsync((ITableEntity)entity, Azure.ETag.All);
        }

        public async Task<bool> IsThere(string rowKey, string partitionKey)
        {
            var entities = _tableClient.QueryAsync<T>(e => e.RowKey == rowKey && e.PartitionKey == partitionKey);

            await foreach (var entity in entities)
            {
                if (entity.RowKey == rowKey && entity.PartitionKey == partitionKey)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
