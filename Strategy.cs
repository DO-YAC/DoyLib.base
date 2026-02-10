using doylib.Enums;
using doylib.Logging;
using doylib.Models;
using doylib.Strategy;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace doylib
{
    public class strategy
    {
        public const string name = "RandomStrat";

        private static readonly ILogger sLogger = LoggerProvider.CreateLogger<strategy>();

        static strategy()
        {
            CombinedStrategyEngine.EnsureInitialised();
        }

        public static void EnsureAiReady()
        {
            CombinedStrategyEngine.Warmup();
        }

        public static void RegisterModule(IStrategyModule module)
        {
            CombinedStrategyEngine.RegisterModule(module);
        }

        public static IReadOnlyList<string> GetActiveModuleNames()
        {
            return CombinedStrategyEngine.GetActiveModuleNames();
        }

        public int Execute(JObject jLine)
        {
            sLogger.LogDebug("Execute called");

            if (jLine is null)
            {
                sLogger.LogDebug("jLine is null");
                throw new ArgumentNullException(nameof(jLine));
            }

            sLogger.LogDebug("jLine: {JLine}", jLine);

            var line = jLine.ToObject<Line>();

            if (line is null)
            {
                sLogger.LogDebug("line is null after conversion");
                throw new InvalidOperationException("Unable to convert input payload into a Line instance.");
            }

            sLogger.LogDebug("Calling CombinedStrategyEngine.Evaluate");
            var decision = CombinedStrategyEngine.Evaluate(line);
            sLogger.LogDebug("Got decision: {Decision}", decision);

            var result = decision switch
            {
                TradeAction.NONE => 0,
                TradeAction.BUY => 1,
                TradeAction.SELL => 2
            };

            sLogger.LogDebug("Returning: {Result}", result);
            return result;
        }
    }

}
