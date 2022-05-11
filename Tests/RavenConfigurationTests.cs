using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Initializer = Tests.RavenTestInitializer;

namespace Tests;
public abstract class RavenConfigurationTests : IDisposable {
    internal readonly IHost _hostWith;
    internal readonly IHost _hostWithout;
    internal readonly string _providerTypeFullName;
    
    internal IEnumerable<IHost> Hosts => new[] {
        _hostWith, _hostWithout
    };

    public delegate Action<HostBuilderContext , IServiceCollection> ConfigureServices();
    public delegate Action<IConfigurationBuilder> ConfigureAppConfiguration();

    Func<Action<HostBuilderContext , IServiceCollection>> DefaultConfigureServices =
        () => (context , services) => {
            services.Configure<HttpClientConfigs>(context.Configuration);
        };

    public RavenConfigurationTests(
        string providerTypeFullName ,
        ConfigureAppConfiguration withConfigureApp, 
        ConfigureAppConfiguration withoutConfigureAppConfiguration ,
        Func<Action<HostBuilderContext , IServiceCollection>>? withConfigureServices = null ,
        Func<Action<HostBuilderContext , IServiceCollection>>? withoutConfigureServices = null
    ) {
        withConfigureServices ??= DefaultConfigureServices;
        withoutConfigureServices ??= DefaultConfigureServices;
        _providerTypeFullName = providerTypeFullName;

        _hostWith = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                withConfigureApp.Invoke()
            ).ConfigureServices(withConfigureServices())
            .Build();

        _hostWithout = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                withoutConfigureAppConfiguration()
            ).ConfigureServices(withoutConfigureServices())
            .Build();
    }

    [TestMethod]
    public virtual void TestConfigurationCounts() {
        foreach(var host in Hosts) {
            var expected = 4;
            var counts = host.Services.GetRequiredService<IConfiguration>()
                .AsEnumerable()
                .Where(s => s.Value is not null 
                    && (Regex.IsMatch(s.Key , Initializer.KEY_REGEX) || 
                    s.Key.Contains($"{Initializer.DEFAULT_COLLECTION}:"))
                );
            Assert.AreEqual(expected , counts.Count());
        }
    }

    [TestMethod]
    public void TestForRavenProvider() {

        foreach(var host in Hosts) {
            var providers = (
                host?.Services
                ?.GetService<IConfiguration>() as IConfigurationRoot
            )?.Providers.ToList();

            Assert.IsNotNull(providers , $"Could not get {nameof(providers)} from DI");

            Assert.IsTrue(
                providers.Any(p =>
                    _providerTypeFullName == p.GetType().FullName)
            );
        }
    }

    [TestMethod]
    public virtual void TestOptionsMonitor() {

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
