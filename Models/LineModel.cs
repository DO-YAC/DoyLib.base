using System.Globalization;

namespace doylib.Models
{
    public class Line
    {
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public long Volume { get; set; }
    }
}
