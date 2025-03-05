namespace MauiBackend.Models
{
    public class Asset
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public string Period { get; set; } = "1H";
    }
}
