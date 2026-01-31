using doylib.Enums;
using doylib.Models;
using doylib.Strategy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace doylib
{
    public class strategy
    {
        public const string name = "RandomStrat";

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
            Console.WriteLine("[STRATEGY DEBUG] Execute called");

            if (jLine is null)
            {
                Console.WriteLine("[STRATEGY DEBUG] jLine is null");
                throw new ArgumentNullException(nameof(jLine));
            }

            Console.WriteLine($"[STRATEGY DEBUG] jLine: {jLine}");

            var line = jLine.ToObject<Line>();

            if (line is null)
            {
                Console.WriteLine("[STRATEGY DEBUG] line is null after conversion");
                throw new InvalidOperationException("Unable to convert input payload into a Line instance.");
            }
            
            Console.WriteLine($"[STRATEGY DEBUG] Calling CombinedStrategyEngine.Evaluate");
            var decision = CombinedStrategyEngine.Evaluate(line);
            Console.WriteLine($"[STRATEGY DEBUG] Got decision: {decision}");

            var result = decision switch
            {
                TradeAction.NONE => 0,
                TradeAction.BUY => 1,
                TradeAction.SELL => 2
            };

            Console.WriteLine($"[STRATEGY DEBUG] Returning: {result}");
            return result;
        }
    }
    
}
