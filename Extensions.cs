using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StratusCube.Extensions.Configuration;

internal static class Extensions {

    public static IDictionary<string , string> ToDictionary(
        this JObject jobject ,
        string? keyPrefix = default
    ) {
        jobject.Remove("@metadata");
        jobject.Remove("Id");

        var prefix = !string.IsNullOrEmpty(keyPrefix)
            ? $"{keyPrefix}:" : string.Empty;

        return jobject.Descendants()
            .OfType<JValue>()
            .ToDictionary(
                jv => prefix + jv.Path.Replace('.' , ':') ,
                jv => $"{jv}"
            );
    }

    public static IDictionary<TKey , TValue> ToFlatDictionary<TKey, TValue>(
        this IEnumerable<IDictionary<TKey , TValue>> pairs
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
    ) => pairs.Aggregate((a , b) => a.Concat(b).ToDictionary(d => d.Key , d => d.Value));
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.


    public static JObject ToJObject(this object @object) =>
        JObject.Parse(JsonConvert.SerializeObject(
            @object
        ));
}

