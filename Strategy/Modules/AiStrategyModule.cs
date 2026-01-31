using System;
using System.Linq;
using doylib.Ai;
using doylib.Enums;
using doylib.Models;

namespace doylib.Strategy.Modules;

public sealed class AiStrategyModule : IStrategyModule, IStrategyWarmup
{
    public string Name => "OnnxAi";

    private readonly float mMinScore;
    private readonly float mMinMargin;
    private readonly bool mApplySoftmax;

    public AiStrategyModule()
    {
        mMinScore = ParseFloatEnv("AI_MIN_SCORE", 0.0f);
        mMinMargin = ParseFloatEnv("AI_MIN_MARGIN", 0.0f);
        mApplySoftmax = ParseBoolEnv("AI_SCORES_SOFTMAX", true);
    }

    public TradeAction Evaluate(Line line)
    {
        try
        {
            var scores = TradingAiEngine.Instance.GetTradeScores(line);

            if (scores is null || scores.Length < 3)
            {
                return TradeAction.NONE;
            }

            Console.WriteLine($"[AI DEBUG] Raw scores: [{string.Join(", ", scores)}]");

            if (mApplySoftmax)
            {
                scores = Softmax(scores);
                Console.WriteLine($"[AI DEBUG] After softmax: [{string.Join(", ", scores)}]");
            }

            var bestIdx = 0;
            var best = float.NegativeInfinity;
            var second = float.NegativeInfinity;

            for (var i = 0; i < scores.Length; i++)
            {
                var s = scores[i];
                if (s > best)
                {
                    second = best;
                    best = s;
                    bestIdx = i;
                }
                else if (s > second)
                {
                    second = s;
                }
            }

            Console.WriteLine($"[AI DEBUG] Best: {best} at index {bestIdx}, Second: {second}, MinScore: {mMinScore}, MinMargin: {mMinMargin}");

            if (best < mMinScore)
            {
                Console.WriteLine($"[AI DEBUG] Rejected: best score {best} < minScore {mMinScore}");
                return TradeAction.NONE;
            }

            if ((best - second) < mMinMargin)
            {
                Console.WriteLine($"[AI DEBUG] Rejected: margin {best - second} < minMargin {mMinMargin}");
                return TradeAction.NONE;
            }

            var action = bestIdx switch
            {
                0 => TradeAction.NONE,
                1 => TradeAction.SELL,
                2 => TradeAction.BUY
            };

            Console.WriteLine($"[AI DEBUG] Final decision: {action}");
            return action;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[AI DEBUG] Exception in Evaluate: {ex.Message}");
            return TradeAction.NONE;
        }
    }

    public void Warmup()
    {
        try
        {
            TradingAiEngine.Instance.EnsureInitialized();
            Console.WriteLine("[AI] AiStrategyModule warmup successful");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] AiStrategyModule warmup failed: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static float ParseFloatEnv(string name, float @default)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw)) return @default;
        return float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var v)
            ? v
            : @default;
    }

    private static bool ParseBoolEnv(string name, bool @default)
    {
        var raw = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(raw)) return @default;
        if (bool.TryParse(raw, out var b)) return b;
        if (int.TryParse(raw, out var i)) return i != 0;
        return @default;
    }

    private static float[] Softmax(float[] scores)
    {
        var max = scores.Max();
        var exps = new double[scores.Length];
        double sum = 0.0;
        for (var i = 0; i < scores.Length; i++)
        {
            var e = Math.Exp(scores[i] - max);
            exps[i] = e;
            sum += e;
        }

        var probs = new float[scores.Length];
        if (sum <= 0)
        {
            return probs;
        }
        for (var i = 0; i < scores.Length; i++)
        {
            probs[i] = (float)(exps[i] / sum);
        }
        return probs;
    }
}
