using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    //This is the contract that our repository will need to adhere to
    //These methods keep the persisted related code contained here
    public interface ICityInfoRepository
    {
        //Add method to Get the Cities
        Task<IEnumerable<City>> GetCitiesAsync();

        //Add the method to filter by city name, ordered by city name
        //Search is included as part of the filtering
        Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(
            string? name, string? searchQuery, int pageNumber, int pageSize);

        Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest);

        //Add method to determine if the City exists or not
        Task<bool> CityExistsAsync(int cityId);

        //Add method to get points of interest
        Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(int cityId);

        //Add method that returns a point of interest for a city
        Task<PointOfInterest?> GetPointOfInterestForCityAsync(int cityId,
            int pointOfInterestId);

        //Add method that accepts the pointOfInterest entity we want to add and the cityId,
        //because we need to add it for a specific city
        //This method adds the "" on the object context (ie, the in-memory representation of our objects) but not yet in the database.
        //Add is an in-memory operation, not an I/O operation. It is NOT an async
        Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest);

        //Add method that accepts the pointOfInterest asa a parameter to delete 
        //Delete is an in-memory operation, not an I/O operation. It is NOT an async
        void DeletePointOfInterest(PointOfInterest pointOfInterest);

        //Add method to persist everything, to add pointOfInterest to a City to the db
        Task<bool> SaveChangesAsync();
    }
}
