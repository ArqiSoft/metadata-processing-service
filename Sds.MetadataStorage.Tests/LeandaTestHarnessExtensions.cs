using MassTransit;
using Sds.Domain;
using Sds.MetadataStorage.Domain.Events;
using Sds.Osdr.RecordsFile.Domain.Events.Records;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sds.MetadataStorage.Tests
{
    public static class LeandaTestHarnessExtensions
    {
        public static async Task<Guid> UploadResource(this LeandaTestHarness harness, string bucket, string fileName)
        {
            return await UploadFile(harness, bucket, Path.Combine(Directory.GetCurrentDirectory(), "Resources", fileName));
        }

        public static async Task<Guid> UploadFile(this LeandaTestHarness harness, string bucket, string path)
        {
            var source = new FileStream(path, FileMode.Open, FileAccess.Read);
            return await harness.BlobStorage.AddFileAsync(Path.GetFileName(path), source, "application/octet-stream", bucket);
        }

        public static async Task PublishPropertiesAdded(this LeandaTestHarness harness, Guid id, Guid userId, IEnumerable<Property> properties)
        {
            await harness.BusControl.Publish<PropertiesAdded>(new
            {
                Id = id,
                UserId = userId,
                Properties = properties
            });
        }

        //public static void WaitWhileProcessingFinished(this LeandaTestHarness harness, Guid correlationId)
        //{
        //    if (!harness.Received.Select<CorrelatedBy<Guid>>(m => m.Context.Message.CorrelationId == correlationId).Any())
        //    {
        //        throw new TimeoutException();
        //    }
        //}

        //public static MetadataGenerated GetMetadataGeneratedEvent(this LeandaTestHarness harness, Guid id)
        //{
        //    return harness.Received
        //        .ToList()
        //        .Where(m => m.Context.GetType().IsGenericType && m.Context.GetType().GetGenericArguments()[0] == typeof(MetadataGenerated))
        //        .Select(m => (m.Context as ConsumeContext<MetadataGenerated>).Message)
        //        .Where(m => m.Id == id).ToList().SingleOrDefault();
        //}

        public static Guid WaitWhileMetadataGenerated(this LeandaTestHarness harness, Guid id)
        {
            if (!harness.Received.Select<MetadataGenerated>(m => m.Context.Message.Id == id).Any())
            {
                throw new TimeoutException();
            }

            return id;
        }
    }
}
