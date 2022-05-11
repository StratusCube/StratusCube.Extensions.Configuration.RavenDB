using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Raven.Client.Documents;
using StratusCube.Extensions.Configuration;

namespace StratusCube.Extensions.Configuration.RavenDB;

public static class ConfigurationBuilderExtensions {

    /// <summary>
    /// Adds an entire database a a configuration. 
    /// Effectively all documents in
    /// the database will be added as configutations.
    /// A specific collection can be used by using <seealso cref="AddRavenDbCollection(IConfigurationBuilder, IDocumentStore, string, bool)"/>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="documentStore">The document store to use for raven</param>
    /// <param name="reloadOnChange">
    ///     Should reload when any document in the databse changes
    /// </param>
    /// <param name="loggerConfig">
    ///     Optional action to build out a logger config for the provider to use
    /// </param>
    /// <returns></returns>
    public static IConfigurationBuilder AddRavenDb(
        this IConfigurationBuilder builder ,
        IDocumentStore documentStore ,
        bool reloadOnChange = default ,
        Action<ILoggingBuilder>? loggerConfig = default
    ) {
        return builder.Add(
            new RavenDatabaseConfigurationSource(
                documentStore , reloadOnChange ,
                buildSubstription: ds => ds.Changes().ForAllDocuments() ,
                loggerFactory: LoggerFactory.Create(loggerConfig)
            )
        );
    }

    /// <summary>
    /// Added a specific collection from a RavenDb database as configurations.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="documentStore">The document store to use for RavenDb</param>
    /// <param name="collectionName">The collection to use as configuratins</param>
    /// <param name="reloadOnChange">
    ///     Should reload when any document in the databse changes 
    /// </param>
    /// <param name="loggerConfig">
    ///     Optional action to build out a logger config for the provider to use
    /// </param>
    /// <returns></returns>
    public static IConfigurationBuilder AddRavenDbCollection(
        this IConfigurationBuilder builder ,
        IDocumentStore documentStore ,
        string collectionName ,
        bool reloadOnChange = false ,
        bool useCollectionPrefix = false ,
        Action<ILoggingBuilder>? loggerConfig = null
    ) {
        var logger = LoggerFactory.Create(loggerConfig);
        var configuration = new RavenCollectionConfigurationSource(
            documentStore ,
            collectionName ,
            reloadOnChange ,
            useCollectionPrefix ,
            ds => ds.Changes().ForDocumentsInCollection(collectionName) ,
            logger
        );

        return builder.Add(configuration);
    }

    /// <summary>
    /// Adds a single raven document as configurations
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="documentStore">The document store to use for RavenDb</param>
    /// <param name="documentID">
    /// The id of the documnet to use as a configuration
    /// <code>MyCollection/A1</code>
    /// </param>
    /// <param name="reloadOnChange"></param>
    /// <param name="loggerConfig"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static IConfigurationBuilder AddRavenDocumnet(
        this IConfigurationBuilder builder ,
        IDocumentStore documentStore ,
        string documentID ,
        bool reloadOnChange = default ,
        Action<ILoggingBuilder>? loggerConfig = default
    ) {
        var logger = LoggerFactory.Create(loggerConfig);
        throw new NotImplementedException();
        var configuration = new RavenCollectionConfigurationSource(
            documentStore ,
            string.Empty ,
            reloadOnChange ,
            false ,
            ds => ds.Changes().ForDocument(documentID) ,
            logger
        );
    }
}
