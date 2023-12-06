﻿using SpaceWeatherAPI.CustomQueryParameters;
using SpaceWeatherAPI.CustomResponses;
using SpaceWeatherAPI.Models;
using System.Linq.Expressions;
using System.Numerics;

namespace SpaceWeatherAPI.Context
{
    public class DataContext
    {
        public readonly List<Planet> Planets = new();

        public DataContext()
        {
            SeedPlanets();
        }

        /// <summary>
        /// Gets the sorting property based on the specified sorting column for a generic type.
        /// </summary>
        /// <typeparam name="T">Type of the object to sort.</typeparam>
        /// <param name="sortingColumn">Column to use for sorting.</param>
        /// <returns>Expression defining the property to be used for sorting.</returns>
        public Expression<Func<T, object>> GetSortProperty<T>(string? sortingColumn) where T : BaseModel
        {
            return sortingColumn?.ToLower() switch
            {
                "name" => model => model.Name,
                "temperature" => model => ((BaseModel)model).WeatherInfo.Temperature,
                _ => model => model.Id
            };
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
                        Temperature = random.Next(-150, 150), // -50 ile 50 arasında rastgele sıcaklık
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
            int moonCount = random.Next(1, 4); // 1 ile 3 arasında rastgele uydu sayısı

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
                        Temperature = random.Next(-150, 150), // -50 ile 50 arasında rastgele sıcaklık
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