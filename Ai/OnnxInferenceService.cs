using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using doylib.Ai.Interfaces;
using doylib.Logging;
using DoyVestment.Framework.Models;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;

namespace doylib.Ai;

internal sealed class OnnxInferenceService : IAiInferenceService
{
    private readonly ILogger mLogger;
    private readonly Dictionary<string, OnnxSession> mSessions = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> LoadedModels => mSessions.Keys;

    internal OnnxInferenceService(AiSettings settings)
    {
        mLogger = LoggerProvider.CreateLogger<OnnxInferenceService>();

        foreach (var model in settings.Models)
        {
            try
            {
                var session = BuildSession(settings, model);
                mSessions.Add(model.Name, new OnnxSession(model.Name, session));

                mLogger.LogInformation("Loaded ONNX model '{Name}' from '{Path}'", model.Name, model.Path);

                if (model.WarmupOnLoad)
                {
                    WarmupSession(mSessions[model.Name]);
                }
            }
            catch (Exception ex)
            {
                mLogger.LogError(ex, "Failed to load ONNX model '{Name}'", model.Name);
                throw;
            }
        }
    }

    public IAiSession GetSession(string name)
    {
        if (!mSessions.TryGetValue(name, out var session))
        {
            throw new KeyNotFoundException($"AI model '{name}' is not loaded. Available: {string.Join(", ", mSessions.Keys)}");
        }
        return session;
    }

    private InferenceSession BuildSession(AiSettings settings, AiModelSettings modelSettings)
    {
        var path = ResolvePath(modelSettings.Path);

        var options = new SessionOptions
        {
            GraphOptimizationLevel = ParseOptimizationLevel(settings.GraphOptimizationLevel)
        };

        if (settings.IntraOpThreads is int intra)
        {
            options.IntraOpNumThreads = intra;
        }
        if (settings.InterOpThreads is int inter)
        {
            options.InterOpNumThreads = inter;
        }

        if (settings.ExecutionProvider == AiExecutionProvider.CUDA)
        {
            try
            {
                using var cuda = new OrtCUDAProviderOptions();
                if (modelSettings.ProviderOverrides is { Count: > 0 })
                {
                    cuda.UpdateOptions(modelSettings.ProviderOverrides);
                }
                else
                {
                    cuda.UpdateOptions(new Dictionary<string, string>
                    {
                        ["device_id"] = settings.DeviceId.ToString()
                    });
                }
                options.AppendExecutionProvider_CUDA(cuda);
                mLogger.LogInformation("Using CUDA execution provider (device {DeviceId}) for model '{Name}'", settings.DeviceId, modelSettings.Name);
            }
            catch (Exception ex) when (settings.AllowCpuFallback)
            {
                mLogger.LogWarning(ex, "CUDA execution provider unavailable for model '{Name}', falling back to CPU", modelSettings.Name);
            }
        }

        return new InferenceSession(path, options);
    }

    private void WarmupSession(OnnxSession session)
    {
        try
        {
            var (name, meta) = session.Inputs.First();
            var shape = meta.Dimensions.Select(d => (long)(d <= 0 ? 1 : d)).ToArray();
            var length = shape.Aggregate(1L, (a, b) => a * b);
            
            session.Run(name, new float[length], shape);
            
            mLogger.LogDebug("Warmup inference completed for '{Name}'", session.Name);
        }
        catch (Exception ex)
        {
            mLogger.LogWarning(ex, "Warmup inference failed for '{Name}' (non-fatal)", session.Name);
        }
    }

    private static string ResolvePath(string path)
    {
        return Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
    }

    private static GraphOptimizationLevel ParseOptimizationLevel(string value)
    {
        return value.ToUpperInvariant() switch
        {
            "DISABLE" => GraphOptimizationLevel.ORT_DISABLE_ALL,
            "BASIC" => GraphOptimizationLevel.ORT_ENABLE_BASIC,
            "EXTENDED" => GraphOptimizationLevel.ORT_ENABLE_EXTENDED,
            _ => GraphOptimizationLevel.ORT_ENABLE_ALL
        };
    }
        

    public void Dispose()
    {
        foreach (var session in mSessions.Values)
        {
            session.Dispose();
        }
        mSessions.Clear();
    }
}
