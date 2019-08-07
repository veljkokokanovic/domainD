﻿using domainD.UnitTests.Entities;
using Microsoft.Extensions.DependencyInjection;
using NEventStore;
using NEventStore.Persistence.Sql;
using NEventStore.Persistence.Sql.SqlDialects;
using NEventStore.Serialization.Json;
using System;
using System.Data.SqlClient;
using System.Transactions;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace domainD.Repository.NEventStore.IntegrationTests
{
    public abstract class TestBase : IDisposable
    {
        private ServiceProvider _serviceProvider;
        private readonly TransactionScope _transaction;
        protected readonly ServiceCollection ServiceCollection;

        protected TestBase()
        {
            ServiceCollection = new ServiceCollection();
            _transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var connectionString = new SqlConnectionStringBuilder
            {
                DataSource = @".",
                InitialCatalog = "EventStore",
                UserID = "sa",
                Password = "sql_express"
            };
            ServiceCollection.AddNEventStore(wireup =>
                wireup.UsingSqlPersistence(new NetStandardConnectionFactory(SqlClientFactory.Instance, connectionString.ConnectionString))
                    .EnlistInAmbientTransaction()
                    .WithDialect(new MsSqlDialect())
                    .UsingJsonSerialization()
            );
        }

        protected ServiceProvider Services => _serviceProvider = _serviceProvider ?? ServiceCollection.BuildServiceProvider();

        protected IRepository<TestEntity> Repository => Services.GetRequiredService<IRepository<TestEntity>>();

        public virtual void Dispose()
        {
            _transaction?.Dispose();
            _serviceProvider?.Dispose();
        }
    }
}
