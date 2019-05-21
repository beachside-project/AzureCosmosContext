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
    "needCreateIfExist": false,
    "throttlingRetryOptions": {
      "maxRetryWaitTimeInSeconds": 1,
      "numberOfRetries": 3
    },
    "cosmosContainerOptions": [
      {
        "containerId": "items",
        "partitionKey": "/division",
        "uniqueKeys": [ "/companyId", "/employeeId" ]
      }
    ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  }
}

```


## ConnectionString settings for Azure Functions V2

if you use [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) in local debug,
set connectionString to `cosmosDbConnectionString` section in **local.settings.json**.
And On Azure Functions, set connectionString to `cosmosDbConnectionString` key on ApplicationSettings.


## ConnectionString settings other than Azure Functions V2

if you use [Azure Cosmos DB Emulator](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator) in local debug,
set connectionString to `cosmosDbConnectionString` section in **appsettings.Development.json** .

On Azure, should set connectionString to `cosmosDbConnectionString` key on ApplicationSettings.


### When load values from appsettings.Development.json

When using ASP.NET Core, the environment name is set by default.  
if use ConsoleApp, you need to set environment name value.  
please watch ConsoleAppSample project > Program.cs > `ConfigureHostConfiguration` section.


## How to use

### Azure Functions V2 hosting

Please see `Startup.cs` in FunctionsV2Sample project.

### ASP.NET Core (other than Azure Functions V2 hosting)

Please see `Startup.cs` and `CarRepository.cs` in AspnetCoreSample project.

### ConsoleApp

Please see ConsoleAppSample project. This is Generic Host sample.
