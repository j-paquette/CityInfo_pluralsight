using AutoMapper;

namespace CityInfo.API.Profiles
{
    public class PointOfInterestProfile : Profile
    {
        public PointOfInterestProfile()
        {
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestDto>();
            CreateMap<Models.PointOfInterestForCreationDto, Entities.PointOfInterest>();
            //Creates a map from the PointOfInterestForUpdateDto to a PointOfInterest entity
            CreateMap<Models.PointOfInterestForUpdateDto, Entities.PointOfInterest>();
            //Creates a map from the PointOfInterest entity to the PointOfInterestForUpdateDto
            CreateMap<Entities.PointOfInterest, Models.PointOfInterestForUpdateDto>();
        }
    }
}
