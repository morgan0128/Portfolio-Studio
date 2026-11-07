using Xunit;

namespace WebApplication6.Tests.Tests;

[CollectionDefinition("TransactionalTests", DisableParallelization = true)]
public class TransactionalTestsCollection : ICollectionFixture<TransactionalTestDatabaseFixture>
{
}