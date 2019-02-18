# AzureCosmosContext

This is a sample using [Azure/azure-cosmos-dotnet-v3](https://github.com/Azure/azure-cosmos-dotnet-v3) SDK.



## appsettings.json settings

This library need following appsettings.json.  
(The following values are sample values)

```json
{
  "cosmosOptions": {
    "currentRegion": "",
    "databaseId": "SampleDb",
    "defaultThroughput": 400,
    "needCreateIfExist": true,
    "throttlingRetryOptions": {
      "maxRetryWaitTimeInSeconds": 1,
      "numberOfRetries": 3
    },
    "cosmosContainerOptions": [
      {
        "containerId": "itemContainer",
        "partitionKey": "/itemCategory"
      },
      {
        "containerId": "carContainer",
        "partitionKey": "/carCategory"
      }
    ]
  }
}
```


## ConnectionString settings

if you use [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) in local debug,
set connectionString to `cosmosDbConnectionString` section in appsettings.Development.json .

if you use Cosmos DB in Azure, should set connectionString to `cosmosDbConnectionString` in environment variables.


## How to use

show AzureCosmosContext.ConsoleAppSample.
