using MassTransit;
using Sds.Storage.Blob.Core;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Sds.MetadataStorage.Tests
{
    [CollectionDefinition("Leanda Test Harness")]
    public class OsdrTestCollection : ICollectionFixture<LeandaTestHarness>
    {
    }

    public abstract class LeandaTest
    {
        public LeandaTestHarness Harness { get; }

        protected IBus Bus => Harness.BusControl;
        protected IBlobStorage BlobStorage => Harness.BlobStorage;

        public LeandaTest(LeandaTestHarness fixture, ITestOutputHelper output = null)
        {
            Harness = fixture;

            if (output != null)
            {
                Log.Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo
                    .TestOutput(output, LogEventLevel.Verbose)
                    .CreateLogger()
                    .ForContext<LeandaTest>();
            }
        }
    }
}
