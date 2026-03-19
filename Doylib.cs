using doylib.Logging;
using doylib.Engine;
using doylib.Engine.Modules;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using DoyVestment.Framework.Models;

namespace doylib
{
    public class Doylib
    {
        private readonly ILogger mLogger;
        private readonly DecisionEngine mDecisionEngine;

        public Doylib(DoylibSettings settings)
        {
            mLogger = LoggerProvider.CreateLogger<Doylib>();
            mDecisionEngine = new DecisionEngine(settings);
            mDecisionEngine.Register(new ExampleModule());
        }

        public int Execute(JObject jLine)
        {
            if (mLogger.IsEnabled(LogLevel.Debug))
            {
                mLogger.LogDebug("Execute called");    
            }
            

            if (jLine is null)
            {
                mLogger.LogDebug("jLine is null");
                throw new ArgumentNullException(nameof(jLine));
            }

            if (mLogger.IsEnabled(LogLevel.Debug))
            {
                mLogger.LogDebug("jLine: {JLine}", jLine);    
            }

            var candle = jLine.ToObject<Candle>();

            if (candle is null)
            {
                mLogger.LogDebug("line is null after conversion");
                throw new InvalidOperationException("Unable to convert input payload into a Line instance.");
            }
            
            var decision = mDecisionEngine.Evaluate(candle);
                
            if (mLogger.IsEnabled(LogLevel.Debug))
            {
                mLogger.LogDebug("Got decision: {Decision}", decision);
            }
            
            return (int)decision;
        }
        
        public void Warmup()
        {
            mDecisionEngine.Warmup();
        }
        
        public string[] GetActiveModules()
        {
            return mDecisionEngine.GetActiveModules();
        }
    }

}
