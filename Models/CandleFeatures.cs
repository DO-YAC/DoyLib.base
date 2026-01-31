using System;

namespace doylib.Models
{
    // Represents the 16 features used for AI model input
    // Matches the Python training feature_cols exactly
    public class CandleFeatures
    {
        public double Ret1 { get; set; }

        public double HlRange { get; set; }
        public double Body { get; set; }
        public double WickUp { get; set; }
        public double WickDn { get; set; }

        public double CEma20 { get; set; }
        public double CEma60 { get; set; }

        public double Vol30 { get; set; }
        public double Rsi14 { get; set; }

        public double VZ60 { get; set; }
        public double NZ60 { get; set; }
        public double VwGap { get; set; }

        public double MSin { get; set; }
        public double MCos { get; set; }
        public double DowSin { get; set; }
        public double DowCos { get; set; }

        public float[] ToArray()
        {
            return new float[]
            {
                (float)Ret1,
                (float)HlRange,
                (float)Body,
                (float)WickUp,
                (float)WickDn,
                (float)CEma20,
                (float)CEma60,
                (float)Vol30,
                (float)Rsi14,
                (float)VZ60,
                (float)NZ60,
                (float)VwGap,
                (float)MSin,
                (float)MCos,
                (float)DowSin,
                (float)DowCos
            };
        }

        public override string ToString()
        {
            return $"Features[ret1:{Ret1:F4}, rsi14:{Rsi14:F4}, c_ema20:{CEma20:F4}, vol30:{Vol30:F4}]";
        }
    }
}
