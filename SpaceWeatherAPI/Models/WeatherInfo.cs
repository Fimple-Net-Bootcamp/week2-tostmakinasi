using System.Text.Json.Serialization;

namespace SpaceWeatherAPI.Models
{
    public class WeatherInfo
    {
        public double Temperature { get; set; }
        public WeatherCondition Condition { get; set; }
    }

    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum WeatherCondition
    {
        Clear = 1,
        Stormy = 2,
        Rainy = 4,
        Snowy = 8,
        Dusty = 16,
        Cold = 32,
        Blizzard = Cold | Snowy | Stormy,
        Sandstorm = Dusty | Stormy,
        RainWithSnow = Rainy | Snowy,
    }
}
