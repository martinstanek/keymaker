using System;
using System.IO;

namespace Keymaker.Service.Support;

public static class Env
{
    public static string ReadString(string name, string defaultValue = "", string secretFile = "")
    {
        var val = GetEnvStr(name);

        if (!string.IsNullOrWhiteSpace(val))
        {
            return val;
        }

        if (string.IsNullOrWhiteSpace(secretFile))
        {
            return defaultValue;
        }

        var secretFilePath = GetEnvStr(secretFile);

        if (string.IsNullOrWhiteSpace(secretFilePath))
        {
            return defaultValue;
        }

        var secret = File.ReadAllText(secretFilePath).Trim();

        return string.IsNullOrWhiteSpace(secret) ? defaultValue : secret;
    }

    public static bool ReadBool(string name, bool defaultValue)
    {
        var val = GetEnvStr(name);

        return bool.TryParse(val, out var envBool)
            ? envBool
            : defaultValue;
    }

    public static int ReadInt(string name, int defaultValue)
    {
        var val = GetEnvStr(name);

        return int.TryParse(val, out var envInt)
            ? envInt
            : defaultValue;
    }

    public static Guid ReadGuid(string name)
    {
        var val = GetEnvStr(name);

        return Guid.TryParse(val, out var envGuid)
            ? envGuid
            : Guid.Empty;
    }

    public static T ReadEnum<T>(string name, T defaultValue) where T : Enum
    {
        var val = GetEnvStr(name);

        return Enum.TryParse(typeof(T), val, out var envEnum)
            ? (T) envEnum
            : defaultValue;
    }

    private static string GetEnvStr(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Environment.GetEnvironmentVariable(name) ?? string.Empty;
    }
}