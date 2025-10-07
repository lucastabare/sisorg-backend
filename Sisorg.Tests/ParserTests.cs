using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sisorg.Api.Services;

namespace Sisorg.Tests;

[TestClass]
public class ParserTests
{
    private static Stream MakeStream(string text) =>
        new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text));

    [TestMethod]
    public async Task ParseAsync_InvalidHex_Throws_WithLineNumber()
    {
        var parser = new FileParser();
        using var stream = MakeStream("Argentina#10#ZZZZZZ");

        var ex = await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => parser.ParseAsync(stream, CancellationToken.None));

        StringAssert.StartsWith(ex.Message, "Line 1: invalid color hex");
    }
}
