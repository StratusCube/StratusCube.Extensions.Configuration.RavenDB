
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenConfigurationSource : IConfigurationSource {
    readonly IDocumentStore _configStore;
    readonly bool _reloadOnChange;
    readonly IObservable<DocumentChange>? _observable;
    readonly string _collection;
    readonly bool _useCollectionPrefix;
    readonly ILoggerFactory? _loggerFactory;

    public RavenConfigurationSource(
        IDocumentStore configStore ,
        bool reloadOnChange = false ,
        string collection = "@all_docs" ,
        bool useCollecttionPrefix = default ,
        Func<IDocumentStore , IObservable<DocumentChange>>? buildSubstription = default,
        ILoggerFactory? loggerFactory = default
    ) {
        (_configStore, _reloadOnChange, _collection, _useCollectionPrefix, _loggerFactory) =
            (configStore.Initialize(), reloadOnChange, collection, useCollecttionPrefix, loggerFactory);
        _observable = buildSubstription?.Invoke(_configStore);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new RavenConfigurationProvider(
            _configStore , _reloadOnChange , _collection ,
            _useCollectionPrefix , _observable  , 
            _loggerFactory?.CreateLogger<RavenConfigurationProvider>()
        );
}
