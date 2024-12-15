namespace Heronest.Features.Api;

public static class QueryParameter
{
    public static T? TryGetValue<T>(HttpContext context, string key)
        where T : IParsable<T>
    {
        if (context.Request.Query.TryGetValue(key, out var value) 
            && T.TryParse(value.ToString(), null, out T? parsedValue))
        {
            return parsedValue;
        }
        return default;
    }

    // NOTE: A bit of duplicate here I guess
    public static T? TryGetValueFromStruct<T>(HttpContext context, string key)
        where T : struct, IParsable<T>
    {
        if (
            context.Request.Query.TryGetValue(key, out var value)
            && T.TryParse(value.ToString(), null, out T parsedValue)
        )
        {
            return parsedValue;
        }
        return null;
    }
}
