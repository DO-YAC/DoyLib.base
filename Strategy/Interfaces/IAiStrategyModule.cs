using doylib.Ai.Interfaces;

namespace doylib.Strategy.Interfaces;

public interface IAiStrategyModule : IStrategyModule
{
    void AttachAi(IAiInferenceService ai);
}
