using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;
internal abstract class RavenConfigurationSource {
    internal readonly IDocumentStore _documentStore;
    internal readonly bool _reloadOnChange;
    internal readonly ILoggerFactory? _loggerFactory;
    internal readonly IObservable<DocumentChange>? _observable;
   

    public RavenConfigurationSource(
        IDocumentStore documentStore ,
        bool reloadOnChange = false ,
        Func<IDocumentStore , IObservable<DocumentChange>>? buildSubstription = null ,
        ILoggerFactory? loggerFactory = null
    ) {
        ArgumentNullException.ThrowIfNull(documentStore , nameof(documentStore));

        (_documentStore, _reloadOnChange, _loggerFactory) =
            (documentStore, reloadOnChange, loggerFactory);
        _observable = buildSubstription?.Invoke(_documentStore);
    }
}
