using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Sds.MetadataStorage.Domain.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Sds.MetadataStorage.Tests
{
    public class FileEventHandlersTestsFixture
    {
        public FileEventHandlersTestsFixture(LeandaTestHarness harness)
        {
        }
    }

    [Collection("Leanda Test Harness")]
    public class FileEventHandlersTests : LeandaTest, IClassFixture<FileEventHandlersTestsFixture>
    {
        private readonly IMongoCollection<BsonDocument> records;
        private readonly IMongoCollection<BsonDocument> files;
        private readonly IMongoCollection<BsonDocument> metadata;

        private readonly Guid FileId;

        public FileEventHandlersTests(LeandaTestHarness harness, ITestOutputHelper output, FileEventHandlersTestsFixture initFixture) : base(harness, output)
        {
            records = harness.MongoDB.GetCollection<BsonDocument>("Records");
            files = harness.MongoDB.GetCollection<BsonDocument>("Files");
            metadata = harness.MongoDB.GetCollection<BsonDocument>("Metadata");

            FileId = Guid.NewGuid();

            files.InsertOne(new
            {
                _id = FileId,
                Properties = new { Fields = new[] { "F1" } }
            }.ToBsonDocument());
        }

        [Fact]
        public async Task BooleanTests()
        {
            string[] booleans = new[] { "true", "false", "1", "0", "y", "n", "yes", "no", "YES", "True", "N" };
           
            var rnd = new Random();
            var records = new List<BsonDocument>();
            for (int i = 0; i < 11_000; i++)
            {
                records.Add(new
                {
                    FileId,
                    Properties = new
                    {
                        Fields = new[]
                        {
                            new
                            {
                                Name="F1",
                                Value = booleans[rnd.Next(booleans.Length-1)].ToString()
                            }
                        }
                    }
                }.ToBsonDocument());
            }
            await this.records.InsertManyAsync(records);

            await Harness.BusControl.Publish<GenerateMetadata>(new { FileId });

            Harness.WaitWhileMetadataGenerated(FileId);

            var generatedDoc = await metadata.Find(new BsonDocument("_id", FileId)).FirstOrDefaultAsync();
            var expectedDoc = new
            {
                _id = FileId,
                Fields = new[] { new { Name = "F1", DataType = "boolean" }.ToBsonDocument() }
            }.ToBsonDocument();

            (generatedDoc == expectedDoc).Should().BeTrue();
        }

        [Fact]
        public async Task IntegerTests()
        {
            var rnd = new Random();
            var records = new List<BsonDocument>();
            var ints = new List<int>();
            for (int i = 0; i < 11_000; i++)
            {
                int value = rnd.Next(7000);
                ints.Add(value);
                records.Add(new
                {
                    FileId,
                    Properties = new
                    {
                        Fields = new[] { new { Name = "F1", Value = value.ToString() } }
                    }
                }.ToBsonDocument());
            }
            await this.records.InsertManyAsync(records);

            await Harness.BusControl.Publish<GenerateMetadata>(new { FileId });

            Harness.WaitWhileMetadataGenerated(FileId);

            var generatedDoc = await metadata.Find(new BsonDocument("_id", FileId)).FirstOrDefaultAsync();
            var expectedDoc = new
            {
                _id = FileId,
                Fields = new[]
                {
                    new
                    {
                        Name = "F1",
                        DataType = "integer",
                        MinValue = ints.Min(),
                        MaxValue = ints.Max()
                    }.ToBsonDocument()
                }
            }.ToBsonDocument();
            (generatedDoc == expectedDoc).Should().BeTrue();
        }

        [Fact]
        public async Task DecimalTests()
        {
            var rnd = new Random();
            var records = new List<BsonDocument>();
            var decimals = new List<decimal>();
            for (int i = 0; i < 11_000; i++)
            {
                decimal value = ((decimal)rnd.Next(30_000)) / 3;
                decimals.Add(value);
                records.Add(new
                {
                    FileId,
                    Properties = new
                    {
                        Fields = new[] { new { Name = "F1", Value = value.ToString() } }
                    }
                }.ToBsonDocument());
            }
            await this.records.InsertManyAsync(records);

            await Harness.BusControl.Publish<GenerateMetadata>(new { FileId });

            Harness.WaitWhileMetadataGenerated(FileId);

            var generatedDoc = await metadata.Find(new BsonDocument("_id", FileId)).FirstOrDefaultAsync();
            var expectedDoc = new
            {
                _id = FileId,
                Fields = new[]
                {
                    new
                    {
                        Name = "F1",
                        DataType = "decimal",
                        MinValue = (object)decimals.Min(),
                        MaxValue = (object)decimals.Max()
                    }.ToBsonDocument()
                }
            }.ToBsonDocument();

            (generatedDoc == expectedDoc).Should().BeTrue();
        }

        [Fact]
        public async Task StringsTests()
        {
            var rnd = new Random();
            var records = new List<BsonDocument>();

            for (int i = 0; i < 1_000; i++)
                records.Add(new BsonDocument("FileId", FileId).Add("Properties", new BsonDocument("Fields", new BsonArray(new[] { new { Name = "F1", Value = i.ToString() }.ToBsonDocument() }))));

            for (int i = 0; i < 1_000; i++)
            {
                decimal value = ((decimal)rnd.Next(777)) / 3;
                records.Add(new BsonDocument("FileId", FileId).Add("Properties", new BsonDocument("Fields", new BsonArray(new[] { new { Name = "F1", Value = value.ToString(NumberFormatInfo.InvariantInfo) }.ToBsonDocument() }))));
            }

            records.Add(new BsonDocument("FileId", FileId).Add("Properties", new BsonDocument("Fields", new BsonArray(new[] { new { Name = "F1", Value = "string value" }.ToBsonDocument() }))));

            await this.records.InsertManyAsync(records);

            await Harness.BusControl.Publish<GenerateMetadata>(new { FileId });

            Harness.WaitWhileMetadataGenerated(FileId);

            var doc = await metadata.Find(new BsonDocument("_id", FileId)).FirstOrDefaultAsync();

            var doc1 = new BsonDocument("_id", FileId).Add("Fields", new BsonArray(new[] { new { Name = "F1", DataType = "string" }.ToBsonDocument() }));

            (doc == doc1).Should().BeTrue();
        }

        [Fact]
        public async Task EmptyFieldsTest()
        {
            await Harness.BusControl.Publish<GenerateMetadata>(new { FileId });

            Harness.WaitWhileMetadataGenerated(FileId);

            var generatedDoc = await metadata.Find(new BsonDocument("_id", FileId)).FirstOrDefaultAsync();
            var expectedDoc = new
            {
                _id = FileId,
                Fields = new[] { new { Name = "F1", DataType = "string" }.ToBsonDocument() }
            }.ToBsonDocument();

            (generatedDoc == expectedDoc).Should().BeTrue();
        }
    }
}