using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using Raven.Client.Documents.Changes;

namespace StratusCube.Extensions.Configuration {
    internal class RavenDatabaseConfigurationProvider : RavenConfigurationProvider {

        readonly bool _useDbNamePrefix;

        public RavenDatabaseConfigurationProvider(
            IDocumentStore documentStore ,
            bool reloadOnChange = false ,
            bool useDatabaseNamePrefix = false ,
            IObservable<DocumentChange>? observable = null ,
            ILogger<RavenConfigurationProvider>? logger = null
        ) : base(documentStore , reloadOnChange , observable , logger) {
            _useDbNamePrefix = useDatabaseNamePrefix;
        }

        public override void Load() {
            using var session = Session;
            Data = session.Advanced.RawQuery<object>("from '@all_docs'")
                .ToArray()
                .Select(o =>
                    o.ToJObject()
                    .ToDictionary(_useDbNamePrefix ? _documentStore.Database : null)
                ).ToFlatDictionary();
        }
    }
}
