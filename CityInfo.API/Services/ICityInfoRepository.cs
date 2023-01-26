using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    //This is the contract that our repository will need to adhere to
    public interface ICityInfoRepository
    {
        //Add method to Get the Cities
        Task<IEnumerable<City>> GetCitiesAsync();

        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);

        //Add method to determine if the City exists or not
        Task<bool> CityExistsAsync(int cityId);

        //Add method to get points of interest
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);

        //Add method that returns a point of interest for a city
        Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId,
            int pointOfInterestId);
    }
}
