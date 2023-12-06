namespace SpaceWeatherAPI.Models
{
    public abstract class BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public WeatherInfo WeatherInfo { get; set; }
    }
}
