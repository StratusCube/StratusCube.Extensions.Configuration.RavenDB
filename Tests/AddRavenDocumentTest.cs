using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Initializer = Tests.RavenTestInitializer;

namespace Tests;

[TestClass]
public class AddRavenDocumentTest : RavenConfigurationTests {
    public AddRavenDocumentTest() 
        : base (
            "StratusCube.Extensions.Configuration.RavenDocConfigurationProvider" ,
            () => configuration => {
                configuration.AddRavenDocument(
                    documentStore: Initializer.DocumentStore ,
                    documentId: Initializer.DEFAULT_DOC_ID ,
                    reloadOnChange: true ,
                    loggerConfig: config => 
                        config.SetMinimumLevel(LogLevel.Trace).AddConsole()
                );
            } ,
            () => configuration => {
                configuration.AddRavenDocument(
                    documentStore: Initializer.DocumentStore ,
                    documentId: Initializer.DEFAULT_DOC_ID ,
                    reloadOnChange: false ,
                    loggerConfig: config =>
                        config.SetMinimumLevel(LogLevel.Trace).AddConsole()
                );
            }
        ) {}
}
