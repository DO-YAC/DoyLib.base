using System;
using System.Collections.Generic;

namespace doylib.Ai.Interfaces;

public interface IAiInferenceService : IDisposable
{
    IReadOnlyCollection<string> LoadedModels { get; }

    IAiSession GetSession(string name);
}
