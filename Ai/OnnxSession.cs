using System;
using System.Collections.Generic;
using System.Linq;
using doylib.Ai.Interfaces;
using Microsoft.ML.OnnxRuntime;

namespace doylib.Ai;

internal sealed class OnnxSession : IAiSession
{
    private readonly InferenceSession mSession;

    public string Name { get; }
    public IReadOnlyDictionary<string, NodeMetadata> Inputs => mSession.InputMetadata;
    public IReadOnlyDictionary<string, NodeMetadata> Outputs => mSession.OutputMetadata;

    internal OnnxSession(string name, InferenceSession session)
    {
        Name = name;
        mSession = session;
    }

    public float[] Run(string inputName, ReadOnlySpan<float> data, ReadOnlySpan<long> shape)
    {
        var shapeArr = shape.ToArray();

        using var input = OrtValue.CreateTensorValueFromMemory(data.ToArray(), shapeArr);
        using var runOptions = new RunOptions();

        var outputName = mSession.OutputMetadata.Keys.First();

        using var outputs = mSession.Run(
            runOptions,
            new[] { inputName },
            new[] { input },
            new[] { outputName });

        return outputs[0].GetTensorDataAsSpan<float>().ToArray();
    }

    public void Dispose() => mSession.Dispose();
}
