using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;

namespace mnPoCGeoIp.Extensions
{
    /// <summary>
    /// Extension methods for JsonNode to simplify value extraction
    /// </summary>
    public static class JsonNodeExtensions
    {
        // ---------------------------------------------------------------------------------------------------------------------
        public static bool TryGet<T>(this JsonNode? node, [MaybeNullWhen(false)] out T? value)
        {
            if (node is JsonValue v && v.TryGetValue<T>(out var tmp))
            {
                value = tmp;
                return true;
            }
            value = default;
            return false;
        }
        // ---------------------------------------------------------------------------------------------------------------------
        public static T GetOrThrow<T>(this JsonNode? node) where T : notnull
        {
            if (!TryGet<T>(node, out var res) || res == null)
                throw new NullReferenceException("Failed to get Json property");

            return res;
        }
        // ---------------------------------------------------------------------------------------------------------------------
    }
}
