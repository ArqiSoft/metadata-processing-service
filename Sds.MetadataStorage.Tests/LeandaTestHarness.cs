using MassTransit;
using MassTransit.ExtensionsDependencyInjectionIntegration;
using MassTransit.RabbitMqTransport;
using MassTransit.Scoping;
using MassTransit.Testing.MessageObservers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Sds.MassTransit.RabbitMq;
using Sds.MetadataStorage.Domain.Events;
using Sds.Storage.Blob.Core;
using Sds.Storage.Blob.GridFs;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sds.MetadataStorage.Tests
{
    public class LeandaTestHarness : IDisposable
    {
        protected IServiceProvider _serviceProvider;

        public IBlobStorage BlobStorage { get { return _serviceProvider.GetService<IBlobStorage>(); } }

        public IMongoDatabase MongoDB { get { return _serviceProvider.GetService<IMongoDatabase>(); } }

        public IBusControl BusControl { get { return _serviceProvider.GetService<IBusControl>(); } }

        private List<ExceptionInfo> Faults = new List<ExceptionInfo>();

        public ReceivedMessageList Received { get; } = new ReceivedMessageList(TimeSpan.FromSeconds(10));

        public LeandaTestHarness()
        {
            var configuration = new ConfigurationBuilder()
                 .AddJsonFile("appsettings.json", true, true)
                 .AddEnvironmentVariables()
                 .Build();

            Log.Logger = new LoggerConfiguration()
                .CreateLogger();

            Log.Information("Staring Leanda tests");

            var services = new ServiceCollection();

            services.AddTransient<IBlobStorage, GridFsStorage>(x =>
            {
                var gridFsConnactionString = new MongoUrl(Environment.ExpandEnvironmentVariables(configuration["GridFs:ConnectionString"]));
                var client = new MongoClient(gridFsConnactionString);

                return new GridFsStorage(client.GetDatabase(gridFsConnactionString.DatabaseName));
            });

            services.AddTransient<IMongoDatabase>(service =>
            {
                var mongoConnectionString = Environment.ExpandEnvironmentVariables(configuration["MongoDb:ConnectionString"]);
                var mongoUrl = new MongoUrl(mongoConnectionString);

                return (new MongoClient(mongoUrl)).GetDatabase(mongoUrl.DatabaseName);
            });

            services.AddSingleton<IConsumerScopeProvider, DependencyInjectionConsumerScopeProvider>();

            services.AddSingleton(container => Bus.Factory.CreateUsingRabbitMq(x =>
            {
                IRabbitMqHost host = x.Host(new Uri(Environment.ExpandEnvironmentVariables(configuration["MassTransit:ConnectionString"])), h => { });

                x.RegisterConsumers(host, container, e =>
                {
                    e.UseInMemoryOutbox();
                });

                x.ReceiveEndpoint(host, "processing_fault_queue", e =>
                {
                    e.Handler<Fault>(async context =>
                    {
                        Faults.AddRange(context.Message.Exceptions.Where(ex => !ex.ExceptionType.Equals("System.InvalidOperationException")));

                        await Task.CompletedTask;
                    });
                });

                x.ReceiveEndpoint(host, "processing_update_queue", e =>
                {
                    e.Handler<MetadataGenerated>(context => { Received.Add(context); return Task.CompletedTask; });
                });
            }));

            _serviceProvider = services.BuildServiceProvider();

            var busControl = _serviceProvider.GetRequiredService<IBusControl>();

            busControl.Start();
        }

        public virtual void Dispose()
        {
            var busControl = _serviceProvider.GetRequiredService<IBusControl>();
            busControl.Stop();
        }
    }
}