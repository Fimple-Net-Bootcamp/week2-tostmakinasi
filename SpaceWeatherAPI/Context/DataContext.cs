using SpaceWeatherAPI.CustomQueryParameters;
using SpaceWeatherAPI.CustomResponses;
using SpaceWeatherAPI.Models;
using System.Linq.Expressions;
using System.Numerics;

namespace SpaceWeatherAPI.Context
{
    public class DataContext
    {
        public List<Planet> Planets;

        public DataContext()
        {
            Planets = new List<Planet>();
            SeedPlanets();
        }
        /// <summary>
        /// Gets a new unique identifier for a planet based on the existing planets' identifiers.
        /// </summary>
        /// <returns>A new unique identifier for a planet.</returns>
        public int GetNewPlanetId()
        {
            return Planets.OrderBy(x => x.Id).Last().Id + 1;
        }

        /// <summary>
        /// Gets a new unique identifier for a moon based on the existing moons' identifiers within a planet.
        /// </summary>
        /// <param name="planet">The planet to which the moon belongs.</param>
        /// <returns>A new unique identifier for a moon.</returns>
        public int GetNewMoonId(Planet planet)
        {
            if (planet.Moons == null || planet.Moons.Count == 0)
                return 1;

            return planet.Moons.OrderBy(x => x.Id).Last().Id + 1;
        }

        public IQueryable<Planet> GetPlanetsAsIQueryable()
        {
            return Planets.AsQueryable();
        }

        private void SeedPlanets()
        {
            Random random = new();

            for (int i = 1; i <= 100; i++)
            {
                Planet planet = new()
                {
                    Id = i,
                    Name = $"Gezegen {i}",
                    WeatherInfo = new WeatherInfo
                    {
                        Temperature = random.Next(-150, 150),
                        Condition = GetRandomCondition()
                    },
                    Moons = SeedMoons(i)
                };

                Planets.Add(planet);

            }


        }

        private List<Moon> SeedMoons(int planetId)
        {
            Random random = new();
            int moonCount = random.Next(1, 4); 

            List<Moon> moons = new();

            for (int i = 1; i <= moonCount; i++)
            {
                Moon moon = new()
                {
                    Id = i,
                    PlanetId = planetId,
                    Name = $"Ay {i}",
                    WeatherInfo = new WeatherInfo
                    {
                        Temperature = random.Next(-150, 150),
                        Condition = GetRandomCondition()
                    }
                };

                moons.Add(moon);
            }

            return moons;
        }

        private WeatherCondition GetRandomCondition()
        {
            Array values = Enum.GetValues(typeof(WeatherCondition));
            Random random = new();
            WeatherCondition randomCondition = (WeatherCondition)values.GetValue(random.Next(values.Length));
            return randomCondition;
        }
    }
}
