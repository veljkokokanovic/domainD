using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading.Tasks;
using domainD.UnitTests.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence.Sql;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.Serialization.Json;
using Xunit;

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public class RepositoryTests : IDisposable
    {
        private readonly ServiceProvider _serviceProvider;
        private IRepository<TestEntity> _repository;

        public RepositoryTests()
        {
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = @".",
                InitialCatalog = "EventStore",
                //IntegratedSecurity = true
                UserID = "sa",
               Password = "sql_express"
            };
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddNEventStore(wireup =>
                
                wireup.UsingSqlPersistence(
                        new NetStandardConnectionFactory(SqlClientFactory.Instance, connectionString.ConnectionString))
                    .WithDialect(new MsSqlDialect())
                    .UsingJsonSerialization()
            );
            _serviceProvider = serviceCollection.BuildServiceProvider();
            _repository = (IRepository<TestEntity>) _serviceProvider.GetService(typeof(IRepository<TestEntity>));
        }

        [Fact]
        public async Task Can_save_and_read_aggregate_root()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await _repository.SaveAsync(e1);
            var ent = await _repository.GetAsync(id);
            e1.Should().BeEquivalentTo(ent);
            ent.Version.Should().Be(1);
        }

        [Fact]
        public async Task Saving_aggregate_root_twice()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await _repository.SaveAsync(e1);
            await _repository.SaveAsync(e1);
        }

        [Fact]
        public async Task Saving_aggregate_root_twice_with_modifications()
        {
            var id = Guid.NewGuid();
            var e1 = AggregateRoot.Create<TestEntity>(new TestCreated("test two", 2), id);
            e1.Done();
            await _repository.SaveAsync(e1);
            var e2 = await _repository.GetAsync(id);
            e2.Property.SetName("1");
            await _repository.SaveAsync(e2);
            e2.Property.Name.Should().Be("1");
            e1.Version.Should().Be(1);
            e2.Version.Should().Be(3);
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }
    }
}
