using CityInfo.API.DbContexts;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private CityInfoContext _context;

        //Inject CityInfoContext through constructor injection
        public CityInfoRepository(CityInfoContext context)
        {
            //Add a null check
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }
        public async Task<IEnumerable<City>> GetCitiesAsync()
        {
            return await _context.Cities.OrderBy(c => c.Name).ToListAsync();
        }

        //Let the consumer choose to include the pointsOfInterest when getting a City, by using a boolean 
        public async Task<City?> GetCityAsync(int cityId, bool includePointsOfInterest)
        {
            //check if includePointsOfInterest is true
            if (includePointsOfInterest)
            {
                return await _context.Cities.Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId).FirstOrDefaultAsync();
                //Only return the city that matches the cityId
            }

            return await _context.Cities
                .Where(c => c.Id == cityId).FirstOrDefaultAsync();
        }

        public async Task<bool> CityExistsAsync(int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId);
        }

        public async Task<PointOfInterest?> GetPointOfInterestForCityAsync(
            int cityId, 
            int pointOfInterestId)
        {
            return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<PointOfInterest>> GetPointsOfInterestForCityAsync(
            int cityId)
        {
            return await _context.PointsOfInterest
                .Where(p => p.CityId == cityId).ToListAsync();
        }

        //Implement this repository contract, from the method in ICityInfoRepository
        public async Task AddPointOfInterestForCityAsync(int cityId, PointOfInterest pointOfInterest)
        {
            var city = await GetCityAsync(cityId, false);
            if (city != null)
            {
                //Then add the pointOfInterest to its collection of PointsOfInterest
                //This will ensure the foreign key is set to the cityId when persisting
                //This is NOT async because it's NOT an I/O call
                //this method does NOT go to the db
                //To persist everything, we need to call SaveChangesAsync() on the context
                city.PointsOfInterest.Add(pointOfInterest);
            }
        }

        //Implement this repository contract, from the method in ICityInfoRepository
        //Call Remove on the pointsOfInterest DbSet, passing thru the pointOfInterest we want to remove
        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.PointsOfInterest.Remove(pointOfInterest);
        }

        //Implement this repository contract, from the method in ICityInfoRepository
        public async Task<bool> SaveChangesAsync()
        { 
            //Demo pruposes only: This method should be true when 0 or more entities have been saved
            //Production: This method should be true when 1 or more entities have been saved successfully
            return (await _context.SaveChangesAsync() >= 0);
        }

        public void DeletePointOfInterest(int cityId, PointOfInterest pointOfInterest)
        {
            throw new NotImplementedException();
        }
    }
}
