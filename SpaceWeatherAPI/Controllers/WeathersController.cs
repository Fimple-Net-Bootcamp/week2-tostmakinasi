using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using SpaceWeatherAPI.Context;
using SpaceWeatherAPI.CustomQueryParameters;
using SpaceWeatherAPI.CustomResponses;
using SpaceWeatherAPI.Models;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Numerics;

namespace SpaceWeatherAPI.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class WeathersController : ControllerBase
    {
        private readonly DataContext _context;
        public WeathersController(DataContext context)
        {
           _context = context;
        }


        /// <summary>
        /// Gets all planets with optional search, sorting, and paging parameters.
        /// </summary>
        /// <param name="parameters">Query parameters for filtering, sorting, and paging.</param>
        /// <returns>A paginated list of planets.</returns>
        //GET api/v1/Weathers/planets?SearchTerm=name&Sort=name_asc&PageNumber=1&PageSize=10
        [HttpGet("planets")]
        public IActionResult GetAllPlanets([FromQuery] QueryParameters parameters)
        {
            var planetsQuery = _context.Planets.AsQueryable();

            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                planetsQuery = planetsQuery.Where(x => x.Name.Contains(parameters.SearchTerm) ||
                x.WeatherInfo.Condition.ToString().Contains(parameters.SearchTerm));
            }


            if (parameters.GetOrder().ToLower() == "desc")
            {
                planetsQuery = (IQueryable<Planet>)planetsQuery.OrderByDescending(_context.GetSortProperty<Planet>(parameters.GetColumn()));
            }
            else
            {
                planetsQuery = (IQueryable<Planet>)planetsQuery.OrderBy(_context.GetSortProperty<Planet>(parameters.GetColumn()));

            }

            parameters.ValidationPageParams();

            var planets = planetsQuery.Skip((parameters.PageNumber - 1) * parameters.PageSize).Take(parameters.PageSize).ToList();

            if(planets == null || planets.Count == 0)
                return NotFound();

            var result = new PagedResponse<Planet>(planets, parameters.PageNumber, parameters.PageSize)
            {
                TotalRecords = _context.Planets.Count
            };
            result.TotalPages = (int)Math.Ceiling((double)result.TotalRecords / parameters.PageSize);

            return Ok(result);
        }

        /// <summary>
        /// Gets a specific planet by ID.
        /// </summary>
        /// <param name="id">The ID of the planet to retrieve.</param>
        /// <returns>The planet with the specified ID.</returns>
        //GET /api/v1/Weathers/planets/1
        [HttpGet("planets/{id}")]
        public IActionResult GetPlanetById(int id)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == id);

            if (planet == null)
                return NotFound();

            return Ok(planet);
        }

        /// <summary>
        /// Gets the moons of a specific planet with optional search, sorting, and paging parameters.
        /// </summary>
        /// <param name="planetId">The ID of the planet whose moons to retrieve.</param>
        /// <param name="parameters">Query parameters for filtering, sorting, and paging.</param>
        /// <returns>A paginated list of moons for the specified planet.</returns>
        //GET /api/v1/Weathers/planets/1/moons?SearchTerm=name&Sort=name_asc&PageNumber=1&PageSize=10
        [HttpGet("planets/{planetId}/moons")] 
        public IActionResult GetPlanetMoons(int planetId, [FromQuery] QueryParameters parameters)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == planetId);

            if (planet == null)
            {
                return NotFound("Planet not found.");
            }

            var moonsQuery = planet.Moons.AsQueryable();

            //Filtreleme işlemi
            if (!string.IsNullOrWhiteSpace(parameters.SearchTerm))
            {
                moonsQuery = moonsQuery.Where(x => x.Name.Contains(parameters.SearchTerm) ||
                x.WeatherInfo.Condition.ToString().Contains(parameters.SearchTerm));
            }

            //Sıralama İşlemi
            if (parameters.GetOrder().ToLower() == "desc")
            {
                moonsQuery = (IQueryable<Moon>)moonsQuery.OrderByDescending(_context.GetSortProperty<Moon>(parameters.GetColumn()));
            }
            else
            {
                moonsQuery = (IQueryable<Moon>)moonsQuery.OrderBy(_context.GetSortProperty<Moon>(parameters.GetColumn()));

            }

            //Page İşlemi
            parameters.ValidationPageParams();

            moonsQuery = moonsQuery.Skip((parameters.PageNumber - 1) * parameters.PageSize)
                                   .Take(parameters.PageSize);

            var moons = moonsQuery.ToList();

            if (moons.Count == 0)
            {
                return NotFound();
            }

            var result = new PagedResponse<Moon>(moons,parameters.PageNumber, parameters.PageSize)
            {
                TotalRecords = planet.Moons.Count,
                TotalPages = (int)Math.Ceiling((double)planet.Moons.Count / parameters.PageSize),
                Data = moons
            };

            return Ok(result);
        }

        /// <summary>
        /// Gets a specific moon by ID within a specific planet.
        /// </summary>
        /// <param name="planetId">The ID of the planet containing the moon.</param>
        /// <param name="id">The ID of the moon to retrieve.</param>
        /// <returns>The moon with the specified ID.</returns>
        //GET /api/v1/Weathers/planets/1/moons/1
        [HttpGet("planets/{planetId}/moons/{id}")]
        public IActionResult GetMoonById(int planetId, int id)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == planetId);

            if (planet == null)
                return NotFound("Planet not found.");

            var moon = planet.Moons.FirstOrDefault(x => x.Id == id);

            if (moon == null)
                return NotFound("Moon not found.");

            return Ok(moon);
        }

        /// <summary>
        /// Adds a new planet.
        /// </summary>
        /// <param name="planet">The planet object to add.</param>
        /// <returns>The added planet with its details.</returns>
        //POST api/v1/Weathers/planets
        [HttpPost("planets")]
        public IActionResult AddPlanet([FromBody] Planet planet)
        {
            if(!ModelState.IsValid ||planet == null)
            {
                return BadRequest();
            }

            var isRecordExists = _context.Planets.Any(x=> x.Name == planet.Name);

            if(isRecordExists)
                return BadRequest("Planet already exists.");

            Planet newPlanet = new()
            {
                Id = _context.GetNewPlanetId(),
                Name = planet.Name,
                WeatherInfo = planet.WeatherInfo,
                Moons = planet.Moons
            };

            int id = _context.GetNewMoonId(planet);
            newPlanet.Moons.ForEach(x =>
            {
                x.PlanetId = newPlanet.Id;
                x.Id = id++;
            });
            
            _context.Planets.Add(newPlanet);

            // Oluşturulan gezegenin bilgilerini içeren bir nesne döndür
            return CreatedAtAction(nameof(GetPlanetById), new { id = newPlanet.Id }, newPlanet);

        }

        /// <summary>
        /// Adds a new moon to a specific planet.
        /// </summary>
        /// <param name="id">The ID of the planet to which the moon will be added.</param>
        /// <param name="moon">The moon object to add.</param>
        /// <returns>The added moon with its details.</returns>
        //POST api/v1/Weathers/planets/1/moons
        [HttpPost("planets/{id}/moons")]
        public IActionResult AddMoonToPlanet(int id, [FromBody] Moon moon) 
        {

            var planet = _context.Planets.FirstOrDefault(x => x.Id == id);

            if (!ModelState.IsValid || planet == null)
            {
                return BadRequest("Planet not found.");
            }


            Moon newMoon = new()
            {
                Id = _context.GetNewMoonId(planet),
                Name = moon.Name,
                WeatherInfo = moon.WeatherInfo,
                PlanetId = id
            };

            planet.Moons.Add(newMoon);

            // Oluşturulan ayın bilgilerini içeren bir nesne döndür
            return CreatedAtAction(nameof(GetMoonById), new { planetId = moon.PlanetId, id = moon.Id }, newMoon);
        }

        /// <summary>
        /// Updates the specified properties of a planet.
        /// </summary>
        /// <param name="id">The ID of the planet to update.</param>
        /// <param name="updatedPlanet">The planet object with the properties to update.</param>
        /// <returns>The updated planet with its details.</returns>
        //PUT api/v1/Weathers/planets/1
        [HttpPut("planets/{id}")]
        public IActionResult UpdatePlanet(int id, [FromBody] Planet updatedPlanet)
        {
            var existingPlanet = _context.Planets.FirstOrDefault(x => x.Id == id);

            if (existingPlanet == null)
            {
                return NotFound("Planet not found.");
            }

            
            existingPlanet.Name = updatedPlanet.Name;
            existingPlanet.WeatherInfo = updatedPlanet.WeatherInfo;

            return Ok(existingPlanet);
        }

        /// <summary>
        /// Updates the specified properties of a moon within a specific planet.
        /// </summary>
        /// <param name="planetId">The ID of the planet containing the moon to update.</param>
        /// <param name="moonId">The ID of the moon to update.</param>
        /// <param name="updatedMoon">The moon object with the properties to update.</param>
        /// <returns>The updated moon with its details.</returns>
        //PUT api/v1/Weathers/planets/1/moons/1
        [HttpPut("planets/{planetId}/moons/{moonId}")]
        public IActionResult UpdateMoon(int planetId, int moonId, [FromBody] Moon updatedMoon)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == planetId);

            if (planet == null)
            {
                return NotFound("Planet not found.");
            }

            var existingMoon = planet.Moons.FirstOrDefault(x => x.Id == moonId);

            if (existingMoon == null)
            {
                return NotFound("Moon not found.");
            }

            
            existingMoon.Name = updatedMoon.Name;
            existingMoon.WeatherInfo = updatedMoon.WeatherInfo;

            return Ok(existingMoon);
        }

        /// <summary>
        /// Updates the specified properties of a planet.
        /// </summary>
        /// <param name="id">The ID of the planet to update.</param>
        /// <param name="patchDocument">The planet object with the properties to update.</param>
        /// <returns>The updated planet with its details.</returns>
        //PATCH api/v1/Weathers/planets/1
        [HttpPatch("planets/{id}")]
        public IActionResult PatchUpdatePlanet(int id, [FromBody] JsonPatchDocument<Planet> patchDocument)
        {
            var existingPlanet = _context.Planets.FirstOrDefault(x => x.Id == id);

            if (existingPlanet == null)
            {
                return NotFound("Planet not found.");
            }

            patchDocument.ApplyTo(existingPlanet, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(existingPlanet);
        }

        /// <summary>
        /// Partially updates the details of a moon on a specific planet using JSON Patch.
        /// </summary>
        /// <param name="planetId">The ID of the planet containing the moon to update.</param>
        /// <param name="moonId">The ID of the moon to update.</param>
        /// <param name="patchDocument">The JSON Patch document containing the changes to apply.</param>
        /// <returns>Ok response with the updated moon if the patch operation is successful.</returns>
        //PATCH api/v1/Weathers/planets/1/moons/"
        [HttpPatch("planets/{planetId}/moons/{moonId}")]
        public IActionResult PatchUpdateMoon(int planetId, int moonId, [FromBody] JsonPatchDocument<Moon> patchDocument)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == planetId);

            if (planet == null)
            {
                return NotFound("Planet not found.");
            }

            var existingMoon = planet.Moons.FirstOrDefault(x => x.Id == moonId);

            if (existingMoon == null)
            {
                return NotFound("Moon not found.");
            }

            patchDocument.ApplyTo(existingMoon, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(existingMoon);
        }

        /// <summary>
        /// Deletes a planet by ID.
        /// </summary>
        /// <param name="id">The ID of the planet to delete.</param>
        /// <returns>No content if the planet is successfully deleted, otherwise NotFound.</returns>
        //DELETE api/v1/Weathers/planets/1
        [HttpDelete("planets/{id}")]
        public IActionResult DeletePlanet(int id)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == id);

            if (planet == null)
            {
                return NotFound("Planet not found.");
            }

            _context.Planets.Remove(planet);

            return NoContent();
        }

        /// <summary>
        /// Deletes a moon from a specific planet.
        /// </summary>
        /// <param name="planetId">The ID of the planet containing the moon to delete.</param>
        /// <param name="moonId">The ID of the moon to delete.</param>
        /// <returns>No content if the deletion is successful.</returns>
        //DELETE api/v1/Weathers/planets/1/moons/1
        [HttpDelete("planets/{planetId}/moons/{moonId}")]
        public IActionResult DeleteMoon(int planetId, int moonId)
        {
            var planet = _context.Planets.FirstOrDefault(x => x.Id == planetId);

            if (planet == null)
            {
                return NotFound("Planet not found.");
            }

            var moon = planet.Moons.FirstOrDefault(x => x.Id == moonId);

            if (moon == null)
            {
                return NotFound("Moon not found.");
            }

            planet.Moons.Remove(moon);

            return NoContent();
        }
    }
}
