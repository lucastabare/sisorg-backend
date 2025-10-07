using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Sisorg.Api.Infrastructure;

namespace Sisorg.Tests.Helpers;

public static class TestDb
{
    public static (AppDbContext ctx, SqliteConnection conn) CreateSqliteInMemory()
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conn)
            .Options;

        var ctx = new AppDbContext(options);
        ctx.Database.EnsureCreated();
        return (ctx, conn);
    }
}
