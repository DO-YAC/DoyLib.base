using System;
using System.Globalization;
using System.IO;

namespace doylib.Ai;

public sealed class AiModelConfig
{
    private AiModelConfig(
        string modelPath,
        string? inputName,
        string? outputName,
        bool useGpu,
        int gpuDeviceId,
        bool allowCpuFallback,
        bool warmupOnStart)
    {
        ModelPath = modelPath;
        InputName = inputName;
        OutputName = outputName;
        UseGpu = useGpu;
        GpuDeviceId = gpuDeviceId;
        AllowCpuFallback = allowCpuFallback;
        WarmupOnStart = warmupOnStart;
    }

    public string ModelPath { get; }
    public string? InputName { get; }
    public string? OutputName { get; }
    public bool UseGpu { get; }
    public int GpuDeviceId { get; }
    public bool AllowCpuFallback { get; }
    public bool WarmupOnStart { get; }

    public static AiModelConfig FromEnvironment(string? assemblyDirectory = null)
    {
        assemblyDirectory ??= GetAssemblyDirectory();
        var defaultModelPath = Path.Combine(assemblyDirectory, "Ai", "models", "trading.onnx");

        var modelPath = GetEnvironmentVariableOrDefault("AI_MODEL_PATH", defaultModelPath) ?? defaultModelPath;
        var inputName = GetEnvironmentVariableOrDefault("AI_INPUT_NAME", null);
        var outputName = GetEnvironmentVariableOrDefault("AI_OUTPUT_NAME", null);

        var useGpu = ParseBooleanEnvironmentVariable("AI_USE_GPU", defaultValue: true);
        var allowCpuFallback = ParseBooleanEnvironmentVariable("AI_CPU_FALLBACK", defaultValue: true);
        var warmupOnStart = ParseBooleanEnvironmentVariable("AI_WARMUP_ON_START", defaultValue: true);
        var gpuDeviceId = ParseIntegerEnvironmentVariable("AI_GPU_DEVICE_ID", defaultValue: 0);

        return new AiModelConfig(
            modelPath,
            string.IsNullOrWhiteSpace(inputName) ? null : inputName,
            string.IsNullOrWhiteSpace(outputName) ? null : outputName,
            useGpu,
            gpuDeviceId,
            allowCpuFallback,
            warmupOnStart);
    }

    private static string GetAssemblyDirectory()
    {
        var location = typeof(AiModelConfig).Assembly.Location;
        return string.IsNullOrEmpty(location)
            ? AppContext.BaseDirectory
            : Path.GetDirectoryName(location) ?? AppContext.BaseDirectory;
    }

    private static string? GetEnvironmentVariableOrDefault(string name, string? defaultValue) =>
        Environment.GetEnvironmentVariable(name) ?? defaultValue;

    private static bool ParseBooleanEnvironmentVariable(string name, bool defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        if (bool.TryParse(raw, out var parsed))
        {
            return parsed;
        }

        if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var numeric))
        {
            return numeric != 0;
        }

        return defaultValue;
    }

    private static int ParseIntegerEnvironmentVariable(string name, int defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return defaultValue;
        }

        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }
}
