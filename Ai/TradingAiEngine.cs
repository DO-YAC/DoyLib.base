using System;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using doylib.Models;

namespace doylib.Ai;

public sealed class TradingAiEngine : IDisposable
{
    private static readonly Lazy<TradingAiEngine> LazyInstance = new(() => new TradingAiEngine());
    private readonly object sLockObject = new();

    private InferenceSession? mSession;
    private AiModelConfig? mConfig;
    private bool mDisposed;

    private TradingAiEngine()
    {
    }

    public static TradingAiEngine Instance => LazyInstance.Value;

    public bool IsReady => mSession is not null;

    public AiModelConfig CurrentConfig
    {
        get
        {
            EnsureInitialized();
            return mConfig!;
        }
    }

    public void EnsureInitialized(AiModelConfig? config = null)
    {
        if (mSession is not null)
        {
            return;
        }

        lock (sLockObject)
        {
            if (mSession is not null)
            {
                return;
            }

            mConfig = config ?? AiModelConfig.FromEnvironment();

            if (!File.Exists(mConfig.ModelPath))
            {
                throw new FileNotFoundException($"AI model not found at '{mConfig.ModelPath}'.", mConfig.ModelPath);
            }

            var options = CreateSessionOptions(mConfig);
            mSession = new InferenceSession(mConfig.ModelPath, options);

            if (mConfig.WarmupOnStart)
            {
                TryWarmUp();
            }
        }
    }

    public int GetTradeDecision(Line line)
    {
        var values = GetTradeScores(line);

        var bestIndex = 0;
        var bestScore = float.MinValue;

        for (var i = 0; i < values.Length; i++)
        {
            var score = values[i];
            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    public float[] GetTradeScores(Line line)
    {
        if (line is null)
        {
            throw new ArgumentNullException(nameof(line));
        }

        EnsureInitialized();

        var features = BuildFeatureVector(line);
        var tensor = new DenseTensor<float>(features, new[] { 1, 1, features.Length });
        var inputs = new[] { NamedOnnxValue.CreateFromTensor("input", tensor) };

        using var results = mSession.Run(inputs);
        using var output = results.First();
        var outputTensor = output.AsTensor<float>();
        return outputTensor.ToArray();
    }
    
    public void Dispose()
    {
        if (mDisposed)
        {
            return;
        }

        lock (sLockObject)
        {
            if (mDisposed)
            {
                return;
            }

            mSession?.Dispose();
            mSession = null;
            mDisposed = true;
        }
    }

    private static SessionOptions CreateSessionOptions(AiModelConfig config)
    {
        var options = new SessionOptions
        {
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL
        };

        if (config.UseGpu)
        {
            try
            {
                options.AppendExecutionProvider_CUDA(config.GpuDeviceId);
            }
            catch (DllNotFoundException) when (config.AllowCpuFallback)
            {
            }
            catch (OnnxRuntimeException) when (config.AllowCpuFallback)
            {
            }
        }

        return options;
    }

    private static float[] BuildFeatureVector(Line line) => line.ToFeatureVector();

    private void TryWarmUp()
    {
        if (mSession is null)
        {
            return;
        }

        var inputName = mSession.InputMetadata.Keys.First();
        var metadata = mSession.InputMetadata[inputName];

        var warmupVectorLength = metadata.Dimensions.LastOrDefault(d => d > 0);
        if (warmupVectorLength <= 0)
        {
            warmupVectorLength = 5;
        }

        var zeros = new float[warmupVectorLength];
        var tensor = new DenseTensor<float>(zeros, new[] { 1, zeros.Length });
        var inputs = new[] { NamedOnnxValue.CreateFromTensor(inputName, tensor) };

        try
        {
            using var results = mSession.Run(inputs);
            using var _ = results.First();
        }
        catch
        {
            // Happens nothing to keep the system alive
        }
    }
}
