using System.Globalization;
using System.Linq;
using Amazon.DynamoDBv2.Model;

namespace AI4NGClassifierLambda.Utils
{
    public static class DynamoAttributeValueExtensions
    {
        public static string GetString(this Dictionary<string, AttributeValue> map, string key, string defaultValue = "")
        {
            return map.TryGetValue(key, out var av) && av.S != null ? av.S : defaultValue;
        }

        public static int GetInt(this Dictionary<string, AttributeValue> map, string key, int defaultValue = 0)
        {
            if (map.TryGetValue(key, out var av) && av.N != null &&
                int.TryParse(av.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            return defaultValue;
        }

        public static long GetLong(this Dictionary<string, AttributeValue> map, string key, long defaultValue = 0)
        {
            if (map.TryGetValue(key, out var av) && av.N != null &&
                long.TryParse(av.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            return defaultValue;
        }

        public static double GetDouble(this Dictionary<string, AttributeValue> map, string key, double defaultValue = 0.0)
        {
            if (map.TryGetValue(key, out var av) && av.N != null &&
                double.TryParse(av.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            return defaultValue;
        }

        public static float GetFloat(this Dictionary<string, AttributeValue> map, string key, float defaultValue = 0f)
        {
            if (map.TryGetValue(key, out var av) && av.N != null &&
                float.TryParse(av.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                return v;
            return defaultValue;
        }

        public static int[] GetIntArray(this Dictionary<string, AttributeValue> map, string key)
        {
            if (map.TryGetValue(key, out var av) && av.L != null)
            {
                var list = new List<int>();
                foreach (var item in av.L)
                {
                    if (item.N != null && int.TryParse(item.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        list.Add(v);
                }
                return list.ToArray();
            }
            return Array.Empty<int>();
        }

        public static double[] GetDoubleArray(this Dictionary<string, AttributeValue> map, string key)
        {
            if (map.TryGetValue(key, out var av) && av.L != null)
            {
                var list = new List<double>();
                foreach (var item in av.L)
                {
                    if (item.N != null && double.TryParse(item.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        list.Add(v);
                }
                return list.ToArray();
            }
            return Array.Empty<double>();
        }

        public static float[] GetFloatArray(this Dictionary<string, AttributeValue> map, string key)
        {
            if (map.TryGetValue(key, out var av) && av.L != null)
            {
                var list = new List<float>();
                foreach (var item in av.L)
                {
                    if (item.N != null && float.TryParse(item.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var v))
                        list.Add(v);
                }
                return list.ToArray();
            }
            return Array.Empty<float>();
        }

        public static object? ToPlainObject(this AttributeValue attributeValue)
        {
            if (attributeValue == null)
                return null;
            if (attributeValue.NULL)
                return null;
            if (attributeValue.S != null)
                return attributeValue.S;
            if (attributeValue.N != null)
            {
                if (long.TryParse(attributeValue.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var l))
                    return l;
                if (double.TryParse(attributeValue.N, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                    return d;
                return attributeValue.N;
            }
            if (attributeValue.BOOL)
                return attributeValue.BOOL;
            if (attributeValue.L?.Count > 0)
                return attributeValue.L.Select(v => v.ToPlainObject()).ToArray();
            if (attributeValue.M?.Count > 0)
                return attributeValue.M.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToPlainObject());
            if (attributeValue.SS?.Count > 0)
                return attributeValue.SS.ToArray();
            if (attributeValue.NS?.Count > 0)
                return attributeValue.NS.Select(s =>
                {
                    if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                        return (object)d;
                    return (object)s;
                }).ToArray();
            return null;
        }
    }
}
