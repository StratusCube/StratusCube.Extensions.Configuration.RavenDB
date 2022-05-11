using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System;
using Initializer = Tests.RavenTestInitializer;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Tests;

[TestClass]
public class AddRavenCollectionTests : RavenConfigurationTests {

    static Action<HostBuilderContext , IServiceCollection> 
        configureServices(bool useCollectionName) =>
            (context , services) => {
                IConfiguration config =
                useCollectionName ?
                    context.Configuration.GetSection(Initializer.DEFAULT_COLLECTION)
                        : context.Configuration;

                services.Configure<HttpClientConfigs>(config);
            };
    static Action<IConfigurationBuilder> 
        configureAppConfiguration(bool reloadOnChange , bool useCollectionPrefix) =>
            configuration =>
                configuration.AddRavenDbCollection(
                        documentStore: Initializer.DocumentStore ,
                        collectionName: Initializer.DEFAULT_COLLECTION ,
                        reloadOnChange: reloadOnChange ,
                        useCollectionPrefix: useCollectionPrefix ,
                        config => config.SetMinimumLevel(LogLevel.Trace).AddConsole()
                    );

    public AddRavenCollectionTests() 
        : base(
            "StratusCube.Extensions.Configuration.RavenCollectionConfigurationProvider",
            () => configureAppConfiguration(true , true) , 
            () => configureAppConfiguration(false, false) , 
            () => configureServices(true) , 
            () => configureServices(false)
    ) { }

    [TestMethod]
    public void TestCollectionNamePrefix() {

        List<(IHost host, Func<KeyValuePair<string , string> , bool> func)> setups = new() {
            (_hostWith, _ => _.Value is not null && _.Key.Contains($"{Initializer.DEFAULT_COLLECTION}")) ,
            (_hostWithout, _ => _.Value is not null && 
                Regex.IsMatch(_.Key , Initializer.KEY_REGEX)
            )
        };

        var expectedCount = 4;

        Assert.IsTrue(
            setups.TrueForAll(i =>
                expectedCount == i.host.Services.GetRequiredService<IConfiguration>()
                    .AsEnumerable().Count(i.func)
            )
        );
    }

    
}

