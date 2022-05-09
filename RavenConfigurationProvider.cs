using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenConfigurationProvider : ConfigurationProvider {

    private readonly IDocumentStore _documentStore;
    private readonly ILogger<RavenConfigurationProvider>? _logger;
    private readonly IObservable<DocumentChange>? _observable;
    private readonly string _collection;
    private readonly bool _useCollectionPrefix;
    private readonly bool _reloadOnChange;

    public RavenConfigurationProvider(
        IDocumentStore documentStore ,
        bool reloadOnChange = default ,
        string collection = "@all_docs",
        bool useCollectionPrefix = default ,
        IObservable<DocumentChange>? observable = null ,
        ILogger<RavenConfigurationProvider>? logger = null
    ) {
        _logger = logger;
        _documentStore = documentStore;
        _observable = observable;
        _collection = collection;
        _useCollectionPrefix = useCollectionPrefix;
        _reloadOnChange = reloadOnChange;
        try {
            if (_reloadOnChange) {
                _observable?.Subscribe(_ => {
                    Load();
                    OnReload();
                });
            }
        }
        catch (Exception ex) {
            _logger?.LogError(
                ex , 
                "Encountered issue when attaching to raven query. See exception for details."
            );
        }
    }

    private IEnumerable<IDictionary<string , string>>? MapDictionaries() {
        
        using var session =
            _documentStore.OpenSession();

        var results =
            session.Advanced
            .RawQuery<object>($"from {_collection}")
            .ToArray();

        return
            results
                ?.Select(o => JsonConvert.SerializeObject(o))
                ?.Select(r => {
                    var o = JObject.Parse(r ?? "{}");
                    o.Remove("@metadata");
                    o.Remove("Id");
                    return o.Descendants()
                        .OfType<JValue>()
                        .ToDictionary(
                            jv => @$"{(_collection == "@all_docs" && !_useCollectionPrefix
                                ? "" : $"{_collection}:")}{jv.Path.Replace('.' , ':')}" , 
                            jv => $"{jv}");
                });
    }

    public override void Load() {
        try {
            _logger?.LogDebug(
                "Loading configurations from raven database {{{0}}} using collection {{{1}}}" 
                , _documentStore.Database , _collection
            );
            Data = MapDictionaries()?.Aggregate((a , b) =>
                a.Concat(b).ToDictionary(d => d.Key , d => d.Value)
            );
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

