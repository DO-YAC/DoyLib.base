using System;
using System.Collections.Generic;
using Microsoft.ML.OnnxRuntime;

namespace doylib.Ai.Interfaces;

public interface IAiSession : IDisposable
{
    string Name { get; }
    IReadOnlyDictionary<string, NodeMetadata> Inputs { get; }
    IReadOnlyDictionary<string, NodeMetadata> Outputs { get; }

    float[] Run(string inputName, ReadOnlySpan<float> data, ReadOnlySpan<long> shape);
}
