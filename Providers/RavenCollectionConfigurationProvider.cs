using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenCollectionConfigurationProvider : RavenConfigurationProvider {

    readonly string _collectionName;
    readonly bool _useCollectionPrefix;

    public RavenCollectionConfigurationProvider(
        IDocumentStore documentStore ,
        string collectionName ,
        bool reloadOnChange = default ,
        bool useCollectionPrefix = default ,
        IObservable<DocumentChange>? observable = default ,
        ILogger<RavenCollectionConfigurationProvider>? logger = default
    ) : base(documentStore , reloadOnChange , observable , logger) {
        ArgumentNullException.ThrowIfNull(collectionName , nameof(collectionName));
        (_collectionName, _useCollectionPrefix) =
            (collectionName, useCollectionPrefix);
    }

    private IEnumerable<IDictionary<string , string>>? MapDictionaries() {

        using var session = Session;

        var results = session.Query<object>(collectionName: _collectionName)
            .ToArray();

        return results?.Select(o =>
                o.ToJObject()
                .ToDictionary(_useCollectionPrefix ? _collectionName : null)
            );
    }

    public override void Load() {
        base.Load();
        try {
            _logger?.LogDebug(
                "Loading configurations from raven database {{{0}}} using collection {{{1}}}"
                , _documentStore.Database , _collectionName
            );
            Data = MapDictionaries()?.ToFlatDictionary();
        }
        catch (Exception ex) {
            _logger?.LogWarning(
                ex ,
                "Could not load configuations from {0}" ,
                _documentStore?.Database
            );
        }
    }
}

