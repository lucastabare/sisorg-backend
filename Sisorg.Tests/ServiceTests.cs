using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sisorg.Api.Repositories;
using Sisorg.Api.Services;
using Sisorg.Tests.Helpers;
using System.Text;

namespace Sisorg.Tests;

[TestClass]
public class ServiceTests
{
    private static IFormFile MakeFormFile(string text, string fileName = "data.txt")
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };
    }

    [TestMethod]
    public async Task UploadAsync_Persists_And_Returns_ViewModel()
    {
        var (ctx, conn) = TestDb.CreateSqliteInMemory();
        using var _ = conn;
        var repo = new RegistryRepository(ctx);
        var service = new RegistryService(repo);

        var file = MakeFormFile("Argentina#1500#FF0000\nBrazil#4005#00FF00");

        var vm = await service.UploadAsync(file, CancellationToken.None);

        Assert.IsTrue(vm.ID > 0);
        Assert.AreEqual(2, vm.Count);
        Assert.AreEqual(2, vm.Rows.Count);
        Assert.AreEqual("Argentina", vm.Rows[0].Name);

        var fromDb = await service.GetAsync(vm.ID, CancellationToken.None);
        Assert.IsNotNull(fromDb);
        Assert.AreEqual(2, fromDb!.Rows.Count);
    }

    [TestMethod]
    public async Task UploadAsync_InvalidColor_Throws()
    {
        var (ctx, conn) = TestDb.CreateSqliteInMemory();
        using var _ = conn;
        var repo = new RegistryRepository(ctx);
        var service = new RegistryService(repo);

        var bad = MakeFormFile("Argentina#1500#GG0000");

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
        {
             await service.UploadAsync(bad, CancellationToken.None);
        });
    }

    [TestMethod]
    public async Task GetAsync_NotFound_ReturnsNull()
    {
        var (ctx, conn) = TestDb.CreateSqliteInMemory();
        using var _ = conn;
        var repo = new RegistryRepository(ctx);
        var service = new RegistryService(repo);

        var result = await service.GetAsync(999999, CancellationToken.None);
        Assert.IsNull(result);
    }
}
