using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenDatabaseConfigurationSource : RavenConfigurationSource , IConfigurationSource {

    readonly bool _useDatabaseNamePrefix;

    public RavenDatabaseConfigurationSource(
        IDocumentStore documentStore ,
        bool reloadOnChange = false ,
        bool useDatabaseNamePrefix = false ,
        Func<IDocumentStore , IObservable<DocumentChange>>? buildSubstription = null ,
        ILoggerFactory? loggerFactory = null
    ) : base(documentStore , reloadOnChange , buildSubstription , loggerFactory) {
        _useDatabaseNamePrefix = useDatabaseNamePrefix;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new RavenDatabaseConfigurationProvider(
            _documentStore ,
            _reloadOnChange ,
            _useDatabaseNamePrefix ,
            _observable ,
            _loggerFactory?.CreateLogger<RavenDatabaseConfigurationProvider>()
        );
}
