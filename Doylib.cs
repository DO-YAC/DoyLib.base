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
    public class Doylib
    {
        private static readonly ILogger sLogger = LoggerProvider.CreateLogger<Doylib>();

        static Doylib()
        {
        }

        public int Execute(JObject jLine)
        {
            if (sLogger.IsEnabled(LogLevel.Debug))
            {
                sLogger.LogDebug("Execute called");    
            }
            

            if (jLine is null)
            {
                sLogger.LogDebug("jLine is null");
                throw new ArgumentNullException(nameof(jLine));
            }

            if (sLogger.IsEnabled(LogLevel.Debug))
            {
                sLogger.LogDebug("jLine: {JLine}", jLine);    
            }

            var line = jLine.ToObject<Line>();

            if (line is null)
            {
                sLogger.LogDebug("line is null after conversion");
                throw new InvalidOperationException("Unable to convert input payload into a Line instance.");
            }
            
            var decision = StrategyEngine.Evaluate(line);
                
            if (sLogger.IsEnabled(LogLevel.Debug))
            {
                sLogger.LogDebug("Got decision: {Decision}", decision);
            }
            
            return (int)decision;
        }
    }

}
