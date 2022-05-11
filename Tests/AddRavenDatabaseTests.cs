using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Initializer = Tests.RavenTestInitializer;

namespace Tests {
    [TestClass]
    public class AddRavenDatabaseTests : RavenConfigurationTests {
        public AddRavenDatabaseTests()
            : base(
                "StratusCube.Extensions.Configuration.RavenDatabaseConfigurationProvider" ,
                () => configuration => {
                    configuration.AddRavenDb(
                        Initializer.DocumentStore ,
                        true ,
                        builder => builder
                            .SetMinimumLevel(LogLevel.Debug)
                            .AddConsole()
                    );
                } ,
                () => configuration => {
                    configuration.AddRavenDb(
                        Initializer.DocumentStore ,
                        false ,
                        builder => builder.SetMinimumLevel(LogLevel.Debug)
                    );
                }
            ) { }
    }
}
