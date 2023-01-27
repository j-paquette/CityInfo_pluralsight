using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        //Inject the contract ICityInfoRepository and NOT the implementation
        //IMapper is the contract AutoMapper's mappers need to adhere to
        public CitiesController(ICityInfoRepository cityInforRepository, 
            IMapper mapper)
        {
            //Null check
            _cityInfoRepository = cityInforRepository ?? throw new ArgumentNullException(nameof(cityInforRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities()
        {
            var cityEntities = await _cityInfoRepository.GetCitiesAsync();


            //Return the results list
            //Map the cityEntities to CityWithoutPointsOfInterestDto
            //Pass the cityEntities we fetched from our repository as source parameter
            return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }

        [HttpGet("{id}")]
        //Return an IActionResult, instead of an ActionResult,
        //because the type value passed thru to the ActionResult will NOT ALWAYS by used by other parts of the code
        //such as the Swagger definition
        public async Task<IActionResult> GetCity(
            int id, bool includePointsOfInterest = false)
        {
            var city = await _cityInfoRepository.GetCityAsync(id, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }

            if (includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));

        }
    }
}
