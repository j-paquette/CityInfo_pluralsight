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

        //Implement method
        public async Task<bool> CityNameMatchesCityId(string? cityName, int cityId)
        {
            return await _context.Cities.AnyAsync(c => c.Id == cityId && c.Name == cityName);
        }

        //Add a method to implement filtering Cities by name
        //Search is included as part of the filtering
        //Can return metadata from this method, by converting method to a tuple.
        //Tuple: a language construct that allows us to return multiple values easily.
        public async Task<(IEnumerable<City>, PaginationMetadata)> GetCitiesAsync(
            string? name, string? searchQuery, int pageNumber, int pageSize)
        {
            //Collection to start from (filter by cities, search or both
            //This has to do with deferred execution
            var collection = _context.Cities as IQueryable<City>;

            //Implement the filtering first
            if (!string.IsNullOrWhiteSpace(name))
            {
                name = name.Trim();
                collection = collection.Where(c => c.Name == name);
            }

            //Implement the search
            //Returns any author for which the name/description is contained in the search query
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                searchQuery = searchQuery.Trim();
                collection = collection.Where(a => a.Name.Contains(searchQuery)
                || (a.Description != null && a.Description.Contains(searchQuery)));
                //do a null check on description to avoid null reference issues
            }

            //After the full query has been constructed, we execute a CountAsync on it
            //This is a db call
            var totaItemCount = await collection.CountAsync();

            //Then construct the PaginationMetadata
            var paginationMetadata = new PaginationMetadata(
                totaItemCount, pageSize, pageNumber);

            //Return the collection after ordering it and after calling ToListAsync on it
            //The built query is only sent to the db at the end, when the ToListAsync() statement is reached
            //Make sure you add paging functionality last because we want to page on the filtered, searched, ordered collection
            //Otherwise, the pagination will be done on ALL the data and provide the wrong results
            //.Skip the amount of authors, so if the user wants to skip to page 2, then items on page 1 will skipped
            //change it to a variable, to create the collection to return
            var collectionToReturn = await collection.OrderBy(c => c.Name)
                .Skip(pageSize * (pageNumber - 1))
                .ToListAsync();

            //Finally, both are returned as a tuple
            return (collectionToReturn, paginationMetadata);


            //The Where clause sends the request to the database, so the filter was applied at the database level.
            //Secure coding: this allows user to input by using a query string to manipulate our database queries.
            //Entity Framework protects agains SQL injection attacks, out-of-the-box
            //If we DON"T use an ORM like Entity Framework Core, then we need to check for this
            //Is this relevant/important with mongoDb, or CosmoDb??
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
    }

}
