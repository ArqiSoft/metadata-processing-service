using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.Domain;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.MetadataStorage.Tests
{
    public class PropertiesAddedTestsFixture
    {
        public PropertiesAddedTestsFixture(LeandaTestHarness harness)
        {
        }
    }

    [Collection("Leanda Test Harness")]
    public class PropertiesAddedTests : LeandaTest, IClassFixture<PropertiesAddedTestsFixture>
    {
        private readonly IMongoCollection<BsonDocument> metadata;

        public PropertiesAddedTests(LeandaTestHarness harness, ITestOutputHelper output, PropertiesAddedTestsFixture initFixture) : base(harness, output)
        {
            metadata = harness.MongoDB.GetCollection<BsonDocument>("Metadata");
        }

        [Fact]
        public async Task Metadatata_AddRecordFields_GenerateAppropriateMetadata()
        {
            var id = Guid.NewGuid();

            await Harness.PublishPropertiesAdded(id, Guid.NewGuid(), new Property[] { new Property("String Property", "string value"), new Property("Int property", 7), new Property("Boolean property", true) });

            Harness.WaitWhileMetadataGenerated(id);

            var doc = await metadata.Find(new BsonDocument("InfoBoxType", "chemical-properties")).FirstOrDefaultAsync();

            doc.Should().NotBeNull();
        }
    }
}
