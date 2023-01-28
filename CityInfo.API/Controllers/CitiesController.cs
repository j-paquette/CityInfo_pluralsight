using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        //Add const to limit the maximum pageSize
        const int maxCitiesPageSize = 20;

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
        //If your variable does NOT have the same name as the key in the query string,
        //you can pass thru the key name from the query string by using the name property on the FromQuery attribute
        //ie, ...[FromQuery(Name = "filteronname")] string? name)
        //Search is included as part of the filtering
        //pageNumber, pageSize should have a default in case the user doesn't specify
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(
            [FromQuery] string? name, string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            //Check that pageSize doesn't go over maxCitiesPageSize
            if (pageSize > maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }

            //Call the overload method that accepts the name
            //Put the citEntitites into 2 different variables, to easily access both the collectionToReturn, paginationMetadata
            var (cityEntities, paginationMetadata) = await _cityInfoRepository
                .GetCitiesAsync(name, searchQuery, pageNumber, pageSize);

            //Add the PaginationMetadata as a header to our response
            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetadata));


            

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
