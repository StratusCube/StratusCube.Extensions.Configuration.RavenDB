using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using System;
using Initializer = Tests.RavenTestInitializer;
using StratusCube.Extensions.Configuration.RavenDB;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace Tests;

[TestClass]
public class AddRavenCollectionTests : IDisposable {

    private IHost _hostWith;
    private IHost _hostWithout;
    const string PROVIDER_TYPE = "StratusCube.Extensions.Configuration.RavenCollectionConfigurationProvider";

    public AddRavenCollectionTests() {
        //. no real need to create extensions
        Action<HostBuilderContext , IServiceCollection> configureServices(bool useCollectionName) =>
            (context , services) => {
                IConfiguration config =
                useCollectionName ?
                    context.Configuration.GetSection(Initializer.DEFAULT_COLLECTION)
                     : context.Configuration;

                services.Configure<HttpClientConfigs>(config);
            };

        Action<IConfigurationBuilder> configureAppConfiguration(bool reloadOnChange , bool useCollectionPrefix) =>
            configuration =>
                configuration.AddRavenDbCollection(
                        documentStore: Initializer.DocumentStore ,
                        collectionName: Initializer.DEFAULT_COLLECTION ,
                        reloadOnChange: reloadOnChange ,
                        useCollectionPrefix: useCollectionPrefix ,
                        config => config.SetMinimumLevel(LogLevel.Trace).AddConsole()
                    );

        _hostWith = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                configureAppConfiguration(true , true)
            ).ConfigureServices(configureServices(true))
            .Build();

        _hostWithout = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                configureAppConfiguration(false , false)
            ).ConfigureServices(configureServices(false))
            .Build();
    }

    [TestMethod]
    public void TestConfigurationCounts() {
        var counts = _hostWith.Services.GetRequiredService<IConfiguration>()
            .GetSection(Initializer.DEFAULT_COLLECTION).GetChildren()
            .Count();
        Assert.AreEqual(2 , counts);
    }

    [TestMethod]
    public void TestCollectionNamePrefix() {

        List<(IHost host, Func<KeyValuePair<string , string> , bool> func)> setups = new() {
            (_hostWith, _ => _.Value is not null && _.Key.Contains($"{Initializer.DEFAULT_COLLECTION}")) ,
            (_hostWithout, _ => _.Value is not null && 
                Regex.IsMatch(_.Key , $"({nameof(HttpClientConfigs.GitHubApi)}|{nameof(HttpClientConfigs.TimeApi)})")
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

    [TestMethod]
    public void TestForRavenProvider() {

        var providers = (
            _hostWith?.Services
            ?.GetService<IConfiguration>() as IConfigurationRoot
        )?.Providers.ToList();

        Assert.IsNotNull(providers , $"Could not get {nameof(providers)} from DI");

        Assert.IsTrue(
            providers.Any(p =>
                PROVIDER_TYPE == p.GetType().FullName)
        );
    }

    [TestMethod]
    public void TestOptionsMonitor() {

        List<(IHost host, MethodInfo method)> setup = new() {
            (
                _hostWith,
                typeof(Assert).GetMethods()
                    .First(m => m.IsGenericMethod && m.Name == nameof(Assert.AreEqual))
            ) ,
            (
                _hostWithout,
                typeof(Assert).GetMethods()
                    .First(m => m.IsGenericMethod && m.Name == nameof(Assert.AreNotEqual))
            )
        };

        //. Test both reloading and non-reloading provider values
        setup.ForEach(i => {
            var genericMethod = i.method.MakeGenericMethod(typeof(TimeSpan));
            using var scope = i.host.Services.CreateScope();
            var httpConfigs = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<HttpClientConfigs>>();

            using var session = Initializer.DocumentStore.OpenSession();
            var r = session.Query<HttpClientConfigs>(collectionName: Initializer.DEFAULT_COLLECTION)
                .First();

            var oldValue = r.TimeApi.Timeout;

            r.TimeApi.Timeout = TimeSpan.FromSeconds(10);

            session.SaveChanges();

            //. give time for token reload
            Thread.Sleep(250);

            genericMethod
                ?.Invoke(null , new object[] { r.TimeApi.Timeout , httpConfigs.CurrentValue.TimeApi.Timeout });

            r.TimeApi.Timeout = oldValue;

            session.SaveChanges();
        });
    }

    public void Dispose() {
        _hostWith.Dispose();
        _hostWithout.Dispose();
    }
}

