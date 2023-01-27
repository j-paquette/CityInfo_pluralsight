﻿using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsofinterest")]
    //The ApiController Attribute, anotations are automatically checked during model binding ModelState dictionary,
    //and ensures when invalid ModelState returns a 400 Bad Request, along with validation errors returned in the response body 
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {
        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger, 
            IMailService mailService,
            ICityInfoRepository cityInfoRepository,
            IMapper mapper)
        {
            //Add a null check
            _logger = logger ?? 
                throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? 
                throw new ArgumentNullException(nameof(mailService));
            _cityInfoRepository = cityInfoRepository ?? 
                throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ?? 
                throw new ArgumentNullException(nameof(mapper));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            //Check whether the city exists or not
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                _logger.LogInformation(
                    $"City with id {cityId} wasn't found when accessing points of interest.");
                return NotFound();
            }

            var pointsOfInterestForCity = await _cityInfoRepository
                .GetPointsOfInterestForCityAsync(cityId);

            return Ok(_mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity));


        }

        [HttpGet("{pointofinterestid}", Name = "GetPointOfInterest")]
        public async Task<ActionResult<PointOfInterestDto>> GetPointOfInterest(
            int cityId, int pointOfInterestId)
        {
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterest = await _cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PointOfInterestDto>(pointOfInterest));
        }

        [HttpPost]
        public async Task<ActionResult<PointOfInterestDto>> CreatePointOfInterest(
            int cityId,
            PointOfInterestForCreationDto pointOfInterest)
        {

            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            //The primary key column Id is auto-generated
            //Map the incoming PointOfInterestForCreationDto to a pointOfInterest entity
            //Calls the AddPointOfInterestForCityAsync, SaveChangesAsync methods
            //First: add the point of interest for a city
            var finalPointOfInterest = _mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            await _cityInfoRepository.AddPointOfInterestForCityAsync(
                cityId, finalPointOfInterest);

            //Then: save the changes
            //As soon as this is called, the entity will have its Id filled out, autogenerated at db level
            await _cityInfoRepository.SaveChangesAsync();

            //Then: map the entity back to a Dto and return it
            var createdPointOfInterestToReturn =
                _mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);


            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId = cityId,
                    pointOfInterestId = createdPointOfInterestToReturn.Id
                },
                //included in the response body
                createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        //This update method is a Task<ActionResult> NOT a task of type ActionResult<T> because it returns no content eventually 
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointOfInterestId,
            PointOfInterestForUpdateDto pointOfInterest)
        {
            //First: check whether the city exists or not
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            //Next: check if the resource we want already exists
            // find a point of interest by calling into GetPointOfInterestForCityAsync on our repository
            var pointOfInterestEntity = await _cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            //Then: use another mapper.Map method
            //If we pass in the source object (ie, pointOfInterest that's been passed in by the Request body) as the first parameter.
            //then pass in the destination object (ie, the Entity) as the second parameter,
            //AutoMapper will override the values in the destination with the values from the source object.
            //The pointOfInterest entity is up-to-date, and is tracked by the DbContext
            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            //Then save the changes, so they are persisted to the db
            await _cityInfoRepository.SaveChangesAsync();

             return NoContent();
        }

        //Use the PointOfInterestUpdateForDto because it already has all the parammeters we need without changing the ID.
        //Make sure to check that the ID matches to the Id in PointOfInterestDto before applying the change to that record.
        [HttpPatch("{pointofinterestid}")]
        //You need to find the entity, then map it to the Dto before applying the patchDocument
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(
            int cityId, int pointOfInterestId,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            //First: check whether the city exists or not
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            //Next: get the PointOfInterest entity
            var pointOfInterestEntity = await _cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            //Then: map the entity to a PointOfInterestForUpdateDto becauee that's what the patchDocument works on
            //Pass thru the entity and map it to a PointOfInterestForUpdateDto
            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(
                pointOfInterestEntity);

            //Apply the patch document 
            //passing ModelState to the ApplyTo method will catch any errors of that type, and make this ModelState invalid
            patchDocument.ApplyTo(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Need to check that the ModelState is valid AFTER applying the patchDocument
            //TryValidateModel triggers validation of our model and any errors will end up in the ModelState
            if (!TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            //Then: map the changes back into the entity
            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            //Then: save changes on our repository
            await _cityInfoRepository.SaveChangesAsync(); 

            return NoContent();
        }

        [HttpDelete("{pointOfInterestId}")]
        public async Task<ActionResult> DeletePointOfInterest(
            int cityId, int pointOfInterestId)
        {
            //First: check whether the city exists or not
            if (!await _cityInfoRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            //Next: get the PointOfInterest entity we want to delete
            var pointOfInterestEntity = await _cityInfoRepository
                .GetPointOfInterestForCityAsync(cityId, pointOfInterestId);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);
            await _cityInfoRepository.SaveChangesAsync();

            //Get an email notification when a point of interest is deleted
            _mailService.Send(
                "Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointOfInterestEntity.Id} was deleted.");

            return NoContent();
        }

    }
}
