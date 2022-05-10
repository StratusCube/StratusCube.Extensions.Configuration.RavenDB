using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Configuration;
using Raven.Client.Documents;
using Microsoft.Extensions.Logging;
using Raven.Embedded;
using Raven.Client.ServerWide;
using System.Collections.Generic;
using Raven.Client.Documents.Conventions;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Reflection;

namespace StratusCube.Tests.Extensions.Configuration.RavenDB {

    [TestClass]
    public class RavenConfigurationProviderTests {

        class FooOption {
            public string? Foo { get; set; }
        }

        class BarOption {
            public string? Bar { get; set; }
        }

        const string DEFAULT_COLLECTION = "@empty";
        //. Would use nameof or typeof but class is internal to the namespace
        const string PROVIDER_TYPE = "StratusCube.Extensions.Configuration.RavenConfigurationProvider";

        readonly IHost _hostWith;
        readonly IHost _hostWithout;
        readonly IDocumentStore _documentStore;

        public RavenConfigurationProviderTests() {

            EmbeddedServer.Instance.StartServer(new ServerOptions {
                AcceptEula = true ,
                CommandLineArgs = { "--RunInMemory=True" }
            });

            _documentStore = EmbeddedServer.Instance
                .GetDocumentStore(new DatabaseOptions(new DatabaseRecord(Guid.NewGuid().ToString()) {
                    Settings = {
                        ["RunInMemory"] = "True"
                    }
                }) { 
                    Conventions = new DocumentConventions {
                        FindCollectionNameForDynamic = type =>
                            DEFAULT_COLLECTION,
                        FindCollectionName = type =>
                            type == typeof(FooOption) || type == typeof(BarOption) ?
                                DEFAULT_COLLECTION : type.FullName
                        
                    }
                });

            using var session = _documentStore
                .OpenSession();

            session.Store(new FooOption { Foo = "Foo config" });
            session.Store(new BarOption { Bar = "Bar config" });
            session.SaveChanges();

            //. no real need to create extensions
            Action<HostBuilderContext, IServiceCollection> configureServices = (context, services) =>
                services.Configure<FooOption>(context.Configuration.GetSection(DEFAULT_COLLECTION))
                    .Configure<BarOption>(context.Configuration.GetSection(DEFAULT_COLLECTION));

            Action<IConfigurationBuilder> configureAppConfiguration(bool reloadOnChange, bool useCollectionPrefix) =>
                configuration =>
                    configuration.AddRavenDbCollection(
                            documentStore: _documentStore ,
                            collectionName: DEFAULT_COLLECTION ,
                            reloadOnChange: reloadOnChange ,
                            useCollectionPrefix: useCollectionPrefix ,
                            config => config.SetMinimumLevel(LogLevel.Trace).AddConsole()
                        );

            _hostWith = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(
                    configureAppConfiguration(true , true)
                ).ConfigureServices(configureServices)
                .Build();
            
            _hostWithout = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(
                    configureAppConfiguration(false , false)
                ).ConfigureServices(configureServices)
                .Build();

        }

        [TestMethod]
        public void TestConfigurationCounts() {
            var configs = _hostWith.Services.GetService<IConfiguration>();
            Assert.AreEqual(
                2,
                configs.AsEnumerable().Count(_ => _.Key.Contains($"{DEFAULT_COLLECTION}:"))
            );
        }

        [TestMethod]
        public void TestConfigurationValues() {
            var configs = _hostWith
                .Services.GetRequiredService<IConfiguration>();

            configs?.AsEnumerable().ToList()
                .ForEach(i =>
                    (i.Key switch {
                        $"{DEFAULT_COLLECTION}:Foo" => (Action)(() => Assert.AreEqual("Foo config" , i.Value)) ,
                        $"{DEFAULT_COLLECTION}:Bar" => () => Assert.AreEqual("Bar config" , i.Value) ,
                        _ => () => { }
                    })()
                );
        }

        [TestMethod]
        public void TestCollectionNamePrefix() {

            List<(IHost host, Func<KeyValuePair<string , string> , bool> func)> setups = new() {
                (_hostWith , _ => _.Key.Contains($"{DEFAULT_COLLECTION}:")),
                (_hostWithout , _ => Regex.IsMatch(_.Key , "(Foo|Bar)"))
            };

            var expectedCount = 2;

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

            if (providers is null)
                Assert.Fail($"Could not get {nameof(providers)} from DI");

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
                var genericMethod = i.method.MakeGenericMethod(typeof(string));
                using var scope = _hostWith.Services.CreateScope();
                var fooOption = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<FooOption>>();

                using var session = _documentStore.OpenSession();
                var r = session.Query<FooOption>(collectionName: DEFAULT_COLLECTION)
                    .First();

                var oldValue = r.Foo;

                r.Foo = "Updated foo";

                session.SaveChanges();

                //. give time for token reload
                Thread.Sleep(250);

                genericMethod
                    ?.Invoke(null, new object[] { r.Foo , fooOption.CurrentValue.Foo });

                r.Foo = oldValue;

                session.SaveChanges();

                Thread.Sleep(250);

                genericMethod?.Invoke(null , new object[] { oldValue , fooOption.CurrentValue.Foo });
            });

            
        }

        [TestCleanup]
        public void Cleanup() {
            EmbeddedServer.Instance.Dispose();
            _hostWith.Dispose();
            _hostWithout.Dispose();
        }
    }
}