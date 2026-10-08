using System.Text;
using Application.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using OnlineShop.Domain.Entities;
using Services.Services.Uploader;
using Xunit;

namespace Application.Tests;

public class ProductModel3DTests : IDisposable
{
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), "model3d-tests-" + Guid.NewGuid().ToString("N"));
    private readonly ModelFileStorage _storage;

    public ProductModel3DTests()
    {
        Directory.CreateDirectory(_webRoot);
        _storage = new ModelFileStorage(new FakeEnv(_webRoot));
    }

    public void Dispose()
    {
        try { Directory.Delete(_webRoot, recursive: true); } catch { /* temp cleanup */ }
    }

    private static byte[] Glb(uint version = 2, int extra = 16)
    {
        var bytes = new byte[12 + extra];
        Encoding.ASCII.GetBytes("glTF").CopyTo(bytes, 0);
        BitConverter.GetBytes(version).CopyTo(bytes, 4);
        BitConverter.GetBytes((uint)bytes.Length).CopyTo(bytes, 8);
        return bytes;
    }

    private static IFormFile File(string name, byte[] content) =>
        new FormFile(new MemoryStream(content), 0, content.Length, "file", name);

    [Fact]
    public async Task Valid_glb_is_stored_unchanged_under_the_requested_folder()
    {
        var content = Glb();
        var result = await _storage.SaveModelAsync(File("vase.GLB", content), UploadPaths.ProductModels(7));

        Assert.True(result.Ok);
        Assert.StartsWith("uploads/products/7/models/", result.Path);
        Assert.EndsWith(".glb", result.Path);
        Assert.Equal(content.Length, result.SizeBytes);
        Assert.Equal(content, await System.IO.File.ReadAllBytesAsync(Path.Combine(_webRoot, result.Path!.Replace('/', Path.DirectorySeparatorChar))));
    }

    [Fact]
    public async Task Glb_with_wrong_signature_or_version_is_rejected()
    {
        var notGlb = Encoding.ASCII.GetBytes("this is definitely not a glb file");
        Assert.False((await _storage.SaveModelAsync(File("a.glb", notGlb), "uploads/x")).Ok);
        Assert.False((await _storage.SaveModelAsync(File("b.glb", Glb(version: 1)), "uploads/x")).Ok);
    }

    [Theory]
    [InlineData("model.obj")]
    [InlineData("model.fbx")]
    [InlineData("model.exe")]
    public async Task Unsupported_model_extensions_are_rejected(string name)
    {
        var result = await _storage.SaveModelAsync(File(name, Glb()), "uploads/x");
        Assert.False(result.Ok);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task Gltf_must_be_self_contained()
    {
        var embedded = Encoding.UTF8.GetBytes("{\"asset\":{\"version\":\"2.0\"},\"buffers\":[{\"uri\":\"data:application/octet-stream;base64,AAAA\",\"byteLength\":3}]}");
        var external = Encoding.UTF8.GetBytes("{\"asset\":{\"version\":\"2.0\"},\"buffers\":[{\"uri\":\"scene.bin\",\"byteLength\":3}]}");

        Assert.True((await _storage.SaveModelAsync(File("ok.gltf", embedded), "uploads/x")).Ok);
        Assert.False((await _storage.SaveModelAsync(File("bad.gltf", external), "uploads/x")).Ok);
        Assert.False((await _storage.SaveModelAsync(File("junk.gltf", Encoding.UTF8.GetBytes("not json")), "uploads/x")).Ok);
    }

    [Fact]
    public async Task Usdz_requires_zip_signature()
    {
        var zip = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0, 0, 0, 0 };
        Assert.True((await _storage.SaveUsdzAsync(File("a.usdz", zip), "uploads/x")).Ok);
        Assert.False((await _storage.SaveUsdzAsync(File("b.usdz", new byte[] { 1, 2, 3, 4, 5 }), "uploads/x")).Ok);
        Assert.False((await _storage.SaveUsdzAsync(File("c.glb", zip), "uploads/x")).Ok);
    }

    [Theory]
    [InlineData("shot.jpg", "image")]
    [InlineData("shot.HEIC", "image")]
    [InlineData("walk.mp4", "video")]
    [InlineData("walk.mov", "video")]
    public async Task Scan_captures_accept_phone_photo_and_video_formats(string name, string kind)
    {
        var result = await _storage.SaveScanSourceAsync(File(name, new byte[] { 1, 2, 3 }), UploadPaths.ProductScans(3));
        Assert.True(result.Ok);
        Assert.Equal(kind, _storage.ScanKind(result.Path!));
    }

    [Fact]
    public async Task Scan_captures_reject_other_types()
    {
        Assert.False((await _storage.SaveScanSourceAsync(File("x.zip", new byte[] { 1 }), "uploads/x")).Ok);
        Assert.False((await _storage.SaveScanSourceAsync(File("x.jpg", Array.Empty<byte>()), "uploads/x")).Ok);
    }

    [Fact]
    public void Domain_entity_tracks_model_and_optional_usdz()
    {
        var model = ProductModel3D.Create(5, "uploads/products/5/models/a.glb", 1234, currentUserId: 1);
        Assert.Equal(5, model.ProductId);
        Assert.Null(model.UsdzUrl);

        model.SetUsdz("uploads/products/5/models/a.usdz", 99, 1);
        Assert.Equal(99, model.UsdzSizeBytes);

        model.SetUsdz(null, 99, 1);
        Assert.Null(model.UsdzUrl);
        Assert.Null(model.UsdzSizeBytes);

        Assert.Throws<ArgumentException>(() => ProductModel3D.Create(5, " ", 1, 1));
    }

    private sealed class FakeEnv(string webRoot) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = webRoot;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = webRoot;
        public string EnvironmentName { get; set; } = "Development";
    }
}
