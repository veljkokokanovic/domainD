using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Transactions;
using domainD.UnitTests.Entities;
using Fasterflect;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence.Sql;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.Serialization.Json;
using Xunit;

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public class DomainEventHandlerTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly IRepository<TestEntity> _repository;
        private readonly TransactionScope _transaction;

        public DomainEventHandlerTests()
        {
            _transaction = new TransactionScope();
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = @".",
                InitialCatalog = "EventStore",
                UserID = "sa",
                Password = "sql_express"
            };
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddNEventStore(wireup =>
                wireup.UsingSqlPersistence(
                        new NetStandardConnectionFactory(SqlClientFactory.Instance, connectionString.ConnectionString))
                    .EnlistInAmbientTransaction()
                    .WithDialect(new MsSqlDialect())
                    .UsingJsonSerialization()
            );
            serviceCollection.AddTransient(typeof(IDomainEventHandler<TestDone>), typeof(TestDoneEventHandler));

            _serviceProvider = serviceCollection.BuildServiceProvider();
            _repository = (IRepository<TestEntity>)_serviceProvider.GetService(typeof(IRepository<TestEntity>));
        }

        [Fact]
        public async Task Domain_event_handler_creates_new_aggregateroot()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test domain eh", 2), id);
            e1.Done();
            await _repository.SaveAsync(e1);
            var repository = (IRepository<NewAggregateRoot>)_serviceProvider.GetService(typeof(IRepository<NewAggregateRoot>));
            var n = await repository.GetAsync(TestDoneEventHandler.AggregateRootId);
            n.Should().NotBeNull();
            n.GetPropertyValue("FinallyDone").As<bool>().Should().BeTrue();
        }

        [Fact]
        public async Task Domain_event_handler_error_rollbacks_transaction()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test domain eh", 2), id);
            e1.Done(true);
            Action save = () => _repository.SaveAsync(e1).GetAwaiter().GetResult();
            save.Should().Throw<Exception>().WithMessage("error");
            (await _repository.GetAsync(id)).Should().BeNull();
            (await _repository.GetAsync(TestDoneEventHandler.AggregateRootId)).Should().BeNull();
        }

        public void Dispose()
        {
            _transaction?.Complete();
            _transaction?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}
