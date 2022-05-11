
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenCollectionConfigurationSource : RavenConfigurationSource, IConfigurationSource {

    readonly string _collectionName;
    readonly bool _useCollectionPrefix;

    public RavenCollectionConfigurationSource(
        IDocumentStore documentStore ,
        string collectionName ,
        bool useCollectionPrefix ,
        bool reloadOnChange = false ,
        Func<IDocumentStore , IObservable<DocumentChange>>? buildSubstription = null ,
        ILoggerFactory? loggerFactory = null
    ) : base(documentStore , reloadOnChange , buildSubstription , loggerFactory) {
        (_collectionName, _useCollectionPrefix) =
            (collectionName, useCollectionPrefix);
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new RavenCollectionConfigurationProvider(
            _documentStore , _collectionName , _reloadOnChange ,
            _useCollectionPrefix , _observable ,
            _loggerFactory?.CreateLogger<RavenCollectionConfigurationProvider>()
        );
}
