using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenDocConfigurationProvider : RavenConfigurationProvider {

    readonly string _documentId;

    public RavenDocConfigurationProvider(
        IDocumentStore documentStore ,
        string documentId ,
        bool reloadOnChange = false ,
        IObservable<DocumentChange>? observable = null ,
        ILogger<RavenDocConfigurationProvider>? logger = null
    ) : base(documentStore , reloadOnChange , observable , logger) {
        ArgumentNullException.ThrowIfNull(documentId , nameof(documentId));
        _documentId = documentId;
    }

    public override void Load() {
        try {
            using var session = Session;
            var doc = session.Load<object>(_documentId);
            Data = doc.ToJObject().ToDictionary();
        }
        catch (Exception ex) {
            _logger?.LogWarning(
                ex ,
                "Could not load configuations from {0} with id {{{1}}}" ,
                _documentStore?.Database , _documentId
            );
        }
    }
}
