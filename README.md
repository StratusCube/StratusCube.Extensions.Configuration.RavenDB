# StratusCube.Extensions.Configuration.RavenDB

Use RavenDB as a configuration provider in dotnet.

The package will let you use RavenDB as a configuration provider. Either a collection or an entire database may be used as a provider. The provider can also make use of Collection or Database subscriptions to reload the configuration changes.

## Getting Started

Add the package to your project from [nuget](https://www.nuget.org/packages/StratusCube.Extensions.Configuration.RavenDB).

```bash
dotnet add package StratusCube.Extensions.Configuration.RavenDB
```

The `RavenDBConfigurationExtensions` class resides in the namespace `Microsoft.Extensions.Configuration` so, in most new web projects the namespace will not need to be specified in the `Program.cs` file.

## Usage

The following is an example implementation. In the `Program.cs` you can add the following:

```csharp
using Microsoft.Extensions.Configuration;

//. Document store to use for the config
IDocumentStore configStore = new DocumentStore {
    Database = "MyDatabase" ,
    Urls = new[] { "http://localhost:8080" } ,
};

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddRavenDbCollection(
    documentStore: configStore ,
    //. the collection to use for the configurations
    collectionName: "MyCollection" ,
    //. if true a subscriber will be used to notify when the collection has changed
    //. and if so load the new configuration values
    reloadOnChange: true ,
    //. if true then the collection will be prefixed to the configuration keys
    //. true: [MyCollection:Foo:Field]
    //. false: [Foo:Field]
    useCollectionPrefix: true ,
    //. optionally add a logger, in this case the logger will grab the
    //. configuration section of logging and add a console provider
    loggerConfig: config => config
        .AddConfiguration(builder.Configuration.GetSection("Logging"))
        .AddConsole()
);
```

---

## Additional Resources

- [C# Options Pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)
