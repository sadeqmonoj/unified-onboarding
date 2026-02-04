namespace Platform.Infrastructure.Utilities;

public static class TimeSpanParser
{
    public static TimeSpan ParseFlexible(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Invalid TimeSpan value");
        }
        value = value.Trim().ToLower();
        if (value.EndsWith("ms"))
        {
            return TimeSpan.FromMilliseconds(int.Parse(value.Replace("ms", "")));
        }
        if (value.EndsWith("s"))
        {
            return TimeSpan.FromSeconds(int.Parse(value.Replace("s", "")));
        }
        if (value.EndsWith("m"))
        {
            return TimeSpan.FromMinutes(int.Parse(value.Replace("m", "")));
        }
        if (value.EndsWith("h"))
        {
            return TimeSpan.FromHours(int.Parse(value.Replace("h", "")));
        }
        if (value.EndsWith("d"))
        {
            return TimeSpan.FromDays(int.Parse(value.Replace("d", "")));
        }
        // fallback for full TimeSpan format: "00:01:00"
        return TimeSpan.Parse(value);
    }
}
