using AutoMapper;

namespace CityInfo.API.Profiles
{
    //Profiles should derive from the AutoMapper to the Profile class
    public class CityProfile : Profile
    {
        public CityProfile()
        {
            //create a map from the City entity (source type) to CityWithoutPointsOfInterestDto (destination type)
            //By default, if the property doesn't exist it will be ignored
            CreateMap<Entities.City, Models.CityWithoutPointsOfInterestDto>();
            //Create a map from City entity (source type) to CityDto (destination type)
            CreateMap<Entities.City, Models.CityDto>();

        }


    }
}
