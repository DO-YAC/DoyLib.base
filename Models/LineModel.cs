using System.Globalization;

namespace doylib.Models
{
    public class Line
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public int Volume { get; set; }

        public CandleFeatures? Features { get; set; }

        public Line()
        {
            Date = string.Empty;
            Time = string.Empty;
        }

        public Line(string[] values)
        {
            for (var i = 0; i <= 6; i++)
            {
                switch (i)
                {
                    case 0:
                        Date = values[i];
                        break;
                    case 1:
                        Time = values[i];
                        break;
                    case 2:
                        Open = double.Parse(values[i], CultureInfo.InvariantCulture);
                        break;
                    case 3:
                        High = double.Parse(values[i], CultureInfo.InvariantCulture);
                        break;
                    case 4:
                        Low = double.Parse(values[i], CultureInfo.InvariantCulture);
                        break;
                    case 5:
                        Close = double.Parse(values[i], CultureInfo.InvariantCulture);
                        break;
                    case 6:
                        Volume = int.Parse(values[i], CultureInfo.InvariantCulture);
                        break;
                }
            }
        }

        public float[] ToFeatureVector()
        {
            // If Features are provided, use them (16 features)
            if (Features != null)
            {
                return Features.ToArray();
            }

            return new[]
            {
                (float)Open,
                (float)High,
                (float)Low,
                (float)Close,
                Volume
            };
        }

    }
}
