using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sisorg.Api.Middleware;
using System.Text;
using System.Text.Json;

namespace Sisorg.Tests;

[TestClass]
public class MiddlewareTests
{
    private static async Task<(int Status, string Json)> InvokeAsync(RequestDelegate next)
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        var mw = new ErrorHandlingMiddleware(next);
        await mw.Invoke(ctx);
        ctx.Response.Body.Position = 0;
        var json = await new StreamReader(ctx.Response.Body, Encoding.UTF8).ReadToEndAsync();
        Console.WriteLine(json);
        return (ctx.Response.StatusCode, json);
    }

    private static bool TryGetInt(JsonElement obj, string name, out int value)
    {
        value = 0;
        if (!obj.TryGetProperty(name, out var el)) return false;
        if (el.ValueKind == JsonValueKind.Number) return el.TryGetInt32(out value);
        if (el.ValueKind == JsonValueKind.String && int.TryParse(el.GetString(), out value)) return true;
        return false;
    }

    private static bool TryGetString(JsonElement obj, string name, out string value)
    {
        value = "";
        if (!obj.TryGetProperty(name, out var el)) return false;
        if (el.ValueKind == JsonValueKind.String) { value = el.GetString() ?? ""; return true; }
        return false;
    }

    private static bool TryReadStatusOrCode(JsonElement root, out int status, out string code)
    {
        status = 0; code = "";
        if (TryGetInt(root, "Status", out status) || TryGetInt(root, "status", out status)) return true;
        if (TryGetInt(root, "StatusCode", out status) || TryGetInt(root, "statusCode", out status)) return true;
        if (TryGetInt(root, "Code", out status) || TryGetInt(root, "code", out status)) return true;
        if (TryGetString(root, "Code", out code) || TryGetString(root, "code", out code)) return true;
        if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
        {
            if (TryGetInt(err, "Status", out status) || TryGetInt(err, "status", out status)) return true;
            if (TryGetInt(err, "StatusCode", out status) || TryGetInt(err, "statusCode", out status)) return true;
            if (TryGetInt(err, "Code", out status) || TryGetInt(err, "code", out status)) return true;
            if (TryGetString(err, "Code", out code) || TryGetString(err, "code", out code)) return true;
        }
        return false;
    }

    private static string ReadAnyString(JsonElement root, params string[] names)
    {
        foreach (var n in names)
        {
            if (TryGetString(root, n, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
            var camel = char.ToLowerInvariant(n[0]) + n[1..];
            if (TryGetString(root, camel, out v) && !string.IsNullOrWhiteSpace(v)) return v;
        }
        if (root.TryGetProperty("error", out var err) && err.ValueKind == JsonValueKind.Object)
        {
            foreach (var n in names)
            {
                if (TryGetString(err, n, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
                var camel = char.ToLowerInvariant(n[0]) + n[1..];
                if (TryGetString(err, camel, out v) && !string.IsNullOrWhiteSpace(v)) return v;
            }
        }
        return "";
    }

    [TestMethod]
    public async Task InvalidOperationException_Returns_400_With_Json()
    {
        var (status, json) = await InvokeAsync(_ => throw new InvalidOperationException("bad"));
        Assert.AreEqual(400, status);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var ok = TryReadStatusOrCode(root, out var statusInJson, out var codeText);
        Assert.IsTrue(ok);
        if (statusInJson != 0) Assert.AreEqual(400, statusInJson);
        else Assert.AreEqual("VALIDATION_ERROR", codeText, true);
        var msg = ReadAnyString(root, "Message", "Detail", "Title");
        var cid = ReadAnyString(root, "CorrelationId", "TraceId", "RequestId");
        Assert.IsFalse(string.IsNullOrWhiteSpace(msg));
        Assert.IsFalse(string.IsNullOrWhiteSpace(cid));
    }

    [TestMethod]
    public async Task UnexpectedException_Returns_500_With_Json()
    {
        var (status, json) = await InvokeAsync(_ => throw new Exception("x"));
        Assert.AreEqual(500, status);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var ok = TryReadStatusOrCode(root, out var statusInJson, out var codeText);
        Assert.IsTrue(ok);
        if (statusInJson != 0) Assert.AreEqual(500, statusInJson);
        else Assert.AreEqual("INTERNAL_ERROR", codeText, true);
        var msg = ReadAnyString(root, "Message", "Detail", "Title");
        var cid = ReadAnyString(root, "CorrelationId", "TraceId", "RequestId");
        Assert.IsFalse(string.IsNullOrWhiteSpace(msg));
        Assert.IsFalse(string.IsNullOrWhiteSpace(cid));
    }
}
