using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration;

internal class RavenDocConfigurationSource : RavenConfigurationSource , IConfigurationSource {

    readonly string _documentId;

    public RavenDocConfigurationSource(
        IDocumentStore documentStore ,
        string documentId ,
        bool reloadOnChange = false ,
        Func<IDocumentStore , IObservable<DocumentChange>>? buildSubstription = null ,
        ILoggerFactory? loggerFactory = null
    ) : base(documentStore , reloadOnChange , buildSubstription , loggerFactory) {
        ArgumentNullException.ThrowIfNull(documentId , nameof(documentId));
        _documentId = documentId;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder) =>
        new RavenDocConfigurationProvider(
            _documentStore ,
            _documentId ,
            _reloadOnChange ,
            _observable ,
            _loggerFactory?.CreateLogger<RavenDocConfigurationProvider>()
        );
}
