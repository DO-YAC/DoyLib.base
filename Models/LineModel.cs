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

        public Line()
        {
            Date = string.Empty;
            Time = string.Empty;
        }
    }
}
