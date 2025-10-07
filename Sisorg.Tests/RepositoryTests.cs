using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sisorg.Api.Repositories;
using Sisorg.Api.Domain.Entities;
using Sisorg.Tests.Helpers;

namespace Sisorg.Tests;

[TestClass]
public class RepositoryTests
{
    [TestMethod]
    public async Task Add_Get_Delete_Works_WithCascade()
    {
        var (ctx, conn) = TestDb.CreateSqliteInMemory();
        using var _ = conn;
        var repo = new RegistryRepository(ctx);

        var entity = new Registry
        {
            TimestampUtc = DateTime.UtcNow,
            Rows = new List<RegistryRow>
            {
                new RegistryRow { Name="AR", Value=1_500m, Color="FF0000" },
                new RegistryRow { Name="BR", Value=4_005m, Color="00FF00" }
            }
        };

        await repo.AddAsync(entity, CancellationToken.None);
        Assert.IsTrue(entity.Id > 0);

        var loaded = await repo.GetAsync(entity.Id, CancellationToken.None);
        Assert.IsNotNull(loaded);
        Assert.AreEqual(2, loaded!.Rows.Count);

        var deleted = await repo.DeleteAsync(entity.Id, CancellationToken.None);
        Assert.IsTrue(deleted);

        var again = await repo.GetAsync(entity.Id, CancellationToken.None);
        Assert.IsNull(again);
    }
}
