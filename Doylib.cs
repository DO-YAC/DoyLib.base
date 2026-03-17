using doylib.Logging;
using doylib.Models;
using doylib.Engine;
using doylib.Engine.Modules;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;

namespace doylib
{
    public class Doylib
    {
        private readonly ILogger mLogger;
        private readonly DecisionEngine mDecisionEngine;

        public Doylib()
        {
            mLogger = LoggerProvider.CreateLogger<Doylib>();
            mDecisionEngine = new DecisionEngine();
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

            var line = jLine.ToObject<Line>();

            if (line is null)
            {
                mLogger.LogDebug("line is null after conversion");
                throw new InvalidOperationException("Unable to convert input payload into a Line instance.");
            }
            
            var decision = mDecisionEngine.Evaluate(line);
                
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
