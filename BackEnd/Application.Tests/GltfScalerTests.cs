using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Nodes;
using Application.Common;
using Xunit;

namespace Application.Tests;

public class GltfScalerTests
{
    private static byte[] BuildGlb(string json, byte[]? bin = null)
    {
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        var jsonPadded = (jsonBytes.Length + 3) & ~3;
        bin ??= new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var total = 12 + 8 + jsonPadded + 8 + bin.Length;

        var glb = new byte[total];
        Encoding.ASCII.GetBytes("glTF").CopyTo(glb, 0);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(4), 2);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(8), (uint)total);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(12), (uint)jsonPadded);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(16), 0x4E4F534A);
        jsonBytes.CopyTo(glb, 20);
        for (var i = jsonBytes.Length; i < jsonPadded; i++) glb[20 + i] = 0x20;
        var o = 20 + jsonPadded;
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(o), (uint)bin.Length);
        BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(o + 4), 0x004E4942);
        bin.CopyTo(glb, o + 8);
        return glb;
    }

    private static JsonObject ReadJson(byte[] glb)
    {
        var len = (int)BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(12));
        return (JsonObject)JsonNode.Parse(glb.AsSpan(20, len).ToArray())!;
    }

    private const string TrsScene =
        "{\"asset\":{\"version\":\"2.0\"},\"scene\":0,\"scenes\":[{\"nodes\":[0]}]," +
        "\"nodes\":[{\"mesh\":0,\"translation\":[1,2,3],\"scale\":[1,2,1]}],\"meshes\":[{}]}";

    [Fact]
    public void Scales_root_node_scale_and_translation_and_keeps_binary_chunk()
    {
        var bin = new byte[] { 9, 8, 7, 6, 5, 4, 3, 2 };
        var scaled = GltfScaler.ScaleGlb(BuildGlb(TrsScene, bin), 0.5, out var error)!;

        Assert.Null(error);
        var node = ReadJson(scaled)["nodes"]![0]!;
        Assert.Equal(new[] { 0.5, 1.0, 0.5 }, node["scale"]!.AsArray().Select(x => x!.GetValue<double>()));
        Assert.Equal(new[] { 0.5, 1.0, 1.5 }, node["translation"]!.AsArray().Select(x => x!.GetValue<double>()));

        // Header total length matches, JSON chunk is 4-byte aligned, BIN chunk is byte-identical.
        Assert.Equal((uint)scaled.Length, BinaryPrimitives.ReadUInt32LittleEndian(scaled.AsSpan(8)));
        var jsonLen = (int)BinaryPrimitives.ReadUInt32LittleEndian(scaled.AsSpan(12));
        Assert.Equal(0, jsonLen % 4);
        Assert.Equal(bin, scaled.AsSpan(20 + jsonLen + 8).ToArray());
    }

    [Fact]
    public void Node_without_scale_gets_uniform_scale()
    {
        var json = "{\"asset\":{\"version\":\"2.0\"},\"scenes\":[{\"nodes\":[0]}],\"nodes\":[{\"mesh\":0}]}";
        var scaled = GltfScaler.ScaleGlb(BuildGlb(json), 10, out _)!;
        var scale = ReadJson(scaled)["nodes"]![0]!["scale"]!.AsArray().Select(x => x!.GetValue<double>());
        Assert.Equal(new[] { 10.0, 10.0, 10.0 }, scale);
    }

    [Fact]
    public void Matrix_roots_scale_the_3x3_block_and_translation_but_not_the_last_row()
    {
        var json = "{\"asset\":{\"version\":\"2.0\"},\"scenes\":[{\"nodes\":[0]}],\"nodes\":[{\"matrix\":[1,0,0,0, 0,1,0,0, 0,0,1,0, 4,5,6,1]}]}";
        var scaled = GltfScaler.ScaleGlb(BuildGlb(json), 2, out _)!;
        var m = ReadJson(scaled)["nodes"]![0]!["matrix"]!.AsArray().Select(x => x!.GetValue<double>()).ToArray();
        Assert.Equal(new[] { 2.0, 0, 0, 0, 0, 2, 0, 0, 0, 0, 2, 0, 8, 10, 12, 1 }, m);
    }

    [Fact]
    public void Self_contained_gltf_is_scaled_too()
    {
        var scaled = GltfScaler.ScaleGltf(Encoding.UTF8.GetBytes(TrsScene), 2, out var error)!;
        Assert.Null(error);
        var node = (JsonObject)JsonNode.Parse(scaled)!["nodes"]![0]!;
        Assert.Equal(2.0, node["scale"]![0]!.GetValue<double>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(0.0001)]
    [InlineData(5000)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Absurd_factors_are_rejected(double factor)
    {
        Assert.Null(GltfScaler.ScaleGlb(BuildGlb(TrsScene), factor, out var error));
        Assert.NotNull(error);
    }

    [Fact]
    public void Invalid_inputs_are_rejected_without_throwing()
    {
        Assert.Null(GltfScaler.ScaleGlb(Encoding.ASCII.GetBytes("not a glb at all, really not"), 2, out var e1));
        Assert.NotNull(e1);

        var noScenes = "{\"asset\":{\"version\":\"2.0\"},\"nodes\":[{}]}";
        Assert.Null(GltfScaler.ScaleGlb(BuildGlb(noScenes), 2, out var e2));
        Assert.NotNull(e2);

        var badRoot = "{\"asset\":{\"version\":\"2.0\"},\"scenes\":[{\"nodes\":[5]}],\"nodes\":[{}]}";
        Assert.Null(GltfScaler.ScaleGlb(BuildGlb(badRoot), 2, out var e3));
        Assert.NotNull(e3);
    }
}
