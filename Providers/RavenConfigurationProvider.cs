using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;
using Raven.Client.Documents.Session;

namespace StratusCube.Extensions.Configuration;

internal abstract class RavenConfigurationProvider : ConfigurationProvider {

    internal IDocumentStore _documentStore;
    internal readonly bool _relaodOnChange;
    internal readonly IObservable<DocumentChange>? _observable;
    internal readonly ILogger<RavenConfigurationProvider>? _logger;
    internal IDocumentSession Session =>
        _documentStore.OpenSession();

    public RavenConfigurationProvider(
        IDocumentStore documentStore ,
        bool reloadOnChange = default ,
        IObservable<DocumentChange>? observable = default ,
        ILogger<RavenConfigurationProvider>? logger = default
    ) {
        (_documentStore, _relaodOnChange, _observable, _logger) = (
            documentStore, reloadOnChange, observable, logger
        );

        try {
            if (_relaodOnChange)
                _observable?.Subscribe(_ => {
                    Load();
                    OnReload();

                });
        }
        catch (Exception ex) {
            _logger?.LogError(
               ex ,
               "Encountered issue when attaching subscription. See exception for details."
           );
        }
    }
}
