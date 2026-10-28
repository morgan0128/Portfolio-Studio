using System;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WebApplication6.Backend.Data;
using WebApplication6.Backend.Models;
using Xunit;

namespace WebApplication6.Tests.Tests.Integration.Repositories;

public class AlbumRepositoryTransactionalModifyDataTests(TransactionalTestDatabaseFixture dbFixture) : IClassFixture<TransactionalTestDatabaseFixture>
{
    public TransactionalTestDatabaseFixture Fixture { get; } = dbFixture;

    /* DeleteAlbumByIdAsync */
    // [Fact]
    // public async Task DeleteAlbumByIdAsync_AlbumExists_AlbumIsDeleted()
    // {
    //     
    // }

}
