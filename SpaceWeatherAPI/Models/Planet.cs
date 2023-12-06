

namespace SpaceWeatherAPI.Models
{
    public class Planet : BaseModel
    {
        public List<Moon> Moons { get; set; } = new List<Moon>();

    }
}
