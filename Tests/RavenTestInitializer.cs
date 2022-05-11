using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Client.Documents;
using Raven.Client.Documents.Conventions;
using Raven.Client.ServerWide;
using Raven.Embedded;
using System;

namespace Tests;

[TestClass]
public class RavenTestInitializer {

    public static IDocumentStore DocumentStore { get; set; } =
        new DocumentStore();

    public const string DEFAULT_COLLECTION = "@empty";
    public static readonly string DEFAULT_DOC_ID =
        Guid.NewGuid().ToString();
    public static readonly string KEY_REGEX =
        $"^({nameof(HttpClientConfigs.GitHubApi)}|{nameof(HttpClientConfigs.TimeApi)}):";

    [AssemblyInitialize]
    public static void Initialize(TestContext context) {
        EmbeddedServer.Instance.StartServer(new ServerOptions {
            AcceptEula = true ,
            CommandLineArgs = { "--RunInMemory=True" }
        });

        DocumentStore = EmbeddedServer.Instance
            .GetDocumentStore(new DatabaseOptions(new DatabaseRecord(Guid.NewGuid().ToString()) {
                Settings = {
                        ["RunInMemory"] = "True"
                }
            }) {
                Conventions = new DocumentConventions {
                    FindCollectionNameForDynamic = type =>
                        DEFAULT_COLLECTION ,
                    FindCollectionName = type =>
                        type == typeof(HttpClientConfig) || type == typeof(HttpClientConfig) || type == typeof(HttpClientConfigs) ?
                            DEFAULT_COLLECTION : type.FullName

                }
            });

        using var session = DocumentStore.OpenSession();

        var httpConfigs = new HttpClientConfigs {
            GitHubApi = new HttpClientConfig {
                BaseAddress = new Uri("https://api.github.com/zen") ,
                Timeout = TimeSpan.FromSeconds(5)
            } ,
            TimeApi = new HttpClientConfig {
                BaseAddress = new Uri("https://www.timeapi.io/api/Time") ,
                Timeout = TimeSpan.FromSeconds(5)
            }
        };

        session.Store(httpConfigs, DEFAULT_DOC_ID);
        session.SaveChanges();

        //EmbeddedServer.Instance.OpenStudioInBrowser();
    }

    [AssemblyCleanup]
    public static void Cleanup() {
        DocumentStore.Dispose();
        EmbeddedServer.Instance.Dispose();
    }
}