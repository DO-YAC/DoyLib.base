using System;
using System.Globalization;

namespace doylib.Strategy;

internal static class StrategyEnvironment
{
    public static bool EnableAi => GetBoolean("ENABLE_AI", defaultValue: false);
    public static bool EnableRandomStrategy => GetBoolean("ENABLE_STRATEGY", defaultValue: true);
    public static bool RequireConsensus => GetBoolean("REQUIRE_CONSENSUS", defaultValue: true);

    private static bool GetBoolean(string name, bool defaultValue)
    {
        var value = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(value))
        {
            return defaultValue;
        }

        if (bool.TryParse(value, out var parsed))
        {
            return parsed;
        }

        if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
        {
            return numeric != 0;
        }

        return defaultValue;
    }
}
