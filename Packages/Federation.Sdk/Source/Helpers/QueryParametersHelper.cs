namespace HttpsRichardy.Federation.Sdk.Helpers;

public static class QueryParametersParser
{
    public static string ToQueryString<TParameters>(TParameters instance)
    {
        if (instance is null) return string.Empty;

        var properties = typeof(TParameters).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var stringBuilder = new StringBuilder();

        bool first = true;
        foreach (var property in properties)
        {
            var value = property.GetValue(instance);
            if (value is null) continue;

            string name = ToCamelCase(property.Name);
            string stringValue = value switch
            {
                bool builder => builder.ToString().ToLowerInvariant(),
                _ => value?.ToString() ?? string.Empty
            };

            stringValue = Uri.EscapeDataString(stringValue);

            if (!first)
            {
                stringBuilder.Append('&');
            }
            else
            {
                first = false;
            }

            stringBuilder.Append($"{name}={stringValue}");
        }

        return stringBuilder.ToString();
    }

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrEmpty(value) || char.IsLower(value[0]))
            return value;

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
