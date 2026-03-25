using System;
using doylib.Engine;
using doylib.Engine.Modules;
using doylib.Logging;
using DoyVestment.Framework.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace doylib
{
    public class Doylib
    {
        private readonly ILogger mLogger;
        private readonly DecisionEngine mDecisionEngine;
        private readonly CandleWindowService mCandleWindowService;

        public Doylib(DoylibSettings settings)
        {
            mLogger = LoggerProvider.CreateLogger<Doylib>();
            mDecisionEngine = new DecisionEngine(settings);
            mDecisionEngine.Register(new ExampleModule());
            mCandleWindowService = new CandleWindowService(LoggerProvider.CreateLogger<CandleWindowService>());
            mCandleWindowService.Initialize(settings.MaxCandleWindowSize);
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

            var line = jLine.ToObject<Candle>();

            if (line is null)
            {
                mLogger.LogDebug("line is null after conversion");
                throw new InvalidOperationException("Unable to convert input payload into a Line instance.");
            }
            
            var decision = mDecisionEngine.Evaluate(line, mCandleWindowService);
                
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

        public void AddCandle(Candle candle)
        {
            mCandleWindowService.AddCandle(candle);
        }

        public int CandleCount => mCandleWindowService.Count;
    }

}
