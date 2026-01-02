using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SuCaiFlow.EntityFramework.Data.Configurations;

public class DictionaryJsonConverter<TKey, TValue> : ValueConverter<Dictionary<TKey, TValue>, string>
    where TKey : notnull
{
    public DictionaryJsonConverter() : base(
        v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
        v => string.IsNullOrEmpty(v)
            ? new Dictionary<TKey, TValue>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<TKey, TValue>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<TKey, TValue>())
    {
    }
}
