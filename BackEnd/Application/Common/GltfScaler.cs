using System.Buffers.Binary;
using System.Text;
using System.Text.Json.Nodes;

namespace Application.Common
{
    /// <summary>
    /// Uniformly scales a glTF 2.0 asset about the origin by editing only the node transforms
    /// of the scene roots (no vertex data is touched). Used to make a scanned / AI-generated
    /// model match the real product size, which is what AR "view on table" shows.
    /// </summary>
    public static class GltfScaler
    {
        private const uint JsonChunk = 0x4E4F534A; // "JSON"

        public const double MinFactor = 0.001;
        public const double MaxFactor = 1000;

        /// <summary>Scales a binary GLB. Returns null and an error message when it cannot.</summary>
        public static byte[]? ScaleGlb(byte[] glb, double factor, out string? error)
        {
            error = CheckFactor(factor);
            if (error != null) return null;

            if (glb.Length < 20 || glb[0] != 0x67 || glb[1] != 0x6C || glb[2] != 0x54 || glb[3] != 0x46)
            {
                error = "فایل GLB معتبر نیست.";
                return null;
            }

            var jsonLength = (int)BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(12, 4));
            var jsonType = BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(16, 4));
            if (jsonType != JsonChunk || 20 + jsonLength > glb.Length)
            {
                error = "ساختار GLB معتبر نیست.";
                return null;
            }

            var json = JsonNode.Parse(glb.AsSpan(20, jsonLength).ToArray()) as JsonObject;
            if (json == null || !ScaleRoots(json, factor, out error))
                return null;

            var newJson = Encoding.UTF8.GetBytes(json.ToJsonString());
            var padded = (newJson.Length + 3) & ~3;

            var rest = glb.AsSpan(20 + jsonLength); // remaining chunks (BIN), already aligned
            var total = 12 + 8 + padded + rest.Length;

            var output = new byte[total];
            glb.AsSpan(0, 12).CopyTo(output);
            BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(8, 4), (uint)total);
            BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(12, 4), (uint)padded);
            BinaryPrimitives.WriteUInt32LittleEndian(output.AsSpan(16, 4), JsonChunk);
            newJson.CopyTo(output.AsSpan(20));
            for (var i = newJson.Length; i < padded; i++) output[20 + i] = 0x20; // pad with spaces
            rest.CopyTo(output.AsSpan(20 + padded));
            return output;
        }

        /// <summary>Scales a self-contained .gltf (pure JSON).</summary>
        public static byte[]? ScaleGltf(byte[] gltf, double factor, out string? error)
        {
            error = CheckFactor(factor);
            if (error != null) return null;

            var json = JsonNode.Parse(gltf) as JsonObject;
            if (json == null || !ScaleRoots(json, factor, out error))
                return null;
            return Encoding.UTF8.GetBytes(json.ToJsonString());
        }

        private static string? CheckFactor(double factor) =>
            double.IsFinite(factor) && factor >= MinFactor && factor <= MaxFactor
                ? null
                : "ضریب مقیاس نامعتبر است.";

        private static bool ScaleRoots(JsonObject json, double k, out string? error)
        {
            error = null;
            var nodes = json["nodes"] as JsonArray;
            var scenes = json["scenes"] as JsonArray;
            if (nodes == null || scenes == null || scenes.Count == 0)
            {
                error = "مدل فاقد صحنه یا node است.";
                return false;
            }

            var sceneIndex = json["scene"]?.GetValue<int>() ?? 0;
            if (sceneIndex < 0 || sceneIndex >= scenes.Count ||
                scenes[sceneIndex]?["nodes"] is not JsonArray roots || roots.Count == 0)
            {
                error = "صحنه‌ی مدل خالی است.";
                return false;
            }

            foreach (var rootRef in roots)
            {
                var index = rootRef?.GetValue<int>() ?? -1;
                if (index < 0 || index >= nodes.Count || nodes[index] is not JsonObject node)
                {
                    error = "node ریشه نامعتبر است.";
                    return false;
                }

                if (node["matrix"] is JsonArray matrix && matrix.Count == 16)
                {
                    // Column-major 4x4: uniform scale multiplies the 3x3 block and the translation.
                    for (var col = 0; col < 4; col++)
                        for (var row = 0; row < 3; row++)
                        {
                            var i = col * 4 + row;
                            matrix[i] = matrix[i]!.GetValue<double>() * k;
                        }
                }
                else
                {
                    var scale = ReadVec3(node["scale"], 1);
                    node["scale"] = new JsonArray(scale[0] * k, scale[1] * k, scale[2] * k);
                    if (node["translation"] != null)
                    {
                        var t = ReadVec3(node["translation"], 0);
                        node["translation"] = new JsonArray(t[0] * k, t[1] * k, t[2] * k);
                    }
                }
            }
            return true;
        }

        private static double[] ReadVec3(JsonNode? node, double fallback) =>
            node is JsonArray { Count: 3 } a
                ? new[] { a[0]!.GetValue<double>(), a[1]!.GetValue<double>(), a[2]!.GetValue<double>() }
                : new[] { fallback, fallback, fallback };
    }
}
