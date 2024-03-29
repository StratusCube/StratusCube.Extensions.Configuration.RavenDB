# StratusCube.Extensions.Configuration.RavenDB

[![.NET](https://github.com/StratusCube/StratusCube.Extensions.Configuration.RavenDB/actions/workflows/dotnet.yml/badge.svg)](https://github.com/StratusCube/StratusCube.Extensions.Configuration.RavenDB/actions/workflows/dotnet.yml) 
[![CodeQL](https://github.com/StratusCube/StratusCube.Extensions.Configuration.RavenDB/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/StratusCube/StratusCube.Extensions.Configuration.RavenDB/actions/workflows/codeql-analysis.yml)

Use RavenDB as a configuration provider in dotnet.

The package will let you use RavenDB as a configuration provider. Either a collection or an entire database may be used as a provider. The provider can also make use of Collection or Database subscriptions to reload the configuration changes.

## Getting Started

Add the package to your project from [nuget](https://www.nuget.org/packages/StratusCube.Extensions.Configuration.RavenDB).

```bash
dotnet add package StratusCube.Extensions.Configuration.RavenDB
```

The `RavenDBConfigurationExtensions` class resides in the namespace `Microsoft.Extensions.Configuration`. In most new web projects the namespace will not need to be specified in the `Program.cs` file.

## Usage

The following is an example implementation. In the `Program.cs` you can add the following:

### Program.cs Boilerplate

```csharp
//. Document store to use for the config
using Microsoft.Extensions.Configuration;

IDocumentStore configStore = new DocumentStore {
    Database = "MyDatabase" ,
    Urls = new[] { "http://localhost:8080" } ,
};

var builder = WebApplication.CreateBuilder(args);
```

### Add Single Raven Document

```csharp
builder.Configuration.AddRavenDocument(
    documentStore: configStore ,
    //. the document ID to use for the configurations
    documentId: "MyDocumentID" ,
    //. if true a subscriber will be used to notify when the collection has changed
    //. and if so load the new configuration values
    reloadOnChange: true ,
    //. optionally add a logger, in this case the logger will grab the
    //. configuration section of logging and add a console provider
    config => config
        .AddConfiguration(builder.Configuration.GetSection("Logging"))
        .AddConsole()
);
```

### Add Raven Collection Source

```csharp
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

### Add Raven Database

```csharp
builder.Configuration.AddRavenDb(
    //. The database specified in the IDocumentStore will be used
    //. for the configurations
    documentStore: configStore ,
    //. if true a subscriber will be used to notify when the collection has changed
    //. and if so load the new configuration values
    reloadOnChange: true ,
    //. optionally add a logger, in this case the logger will grab the
    //. configuration section of logging and add a console provider
    config => config
        .AddConfiguration(builder.Configuration.GetSection("Logging"))
        .AddConsole()
);
```

---

## Additional Resources

- [C# Options Pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)
