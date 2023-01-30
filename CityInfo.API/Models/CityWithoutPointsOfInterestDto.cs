namespace CityInfo.API.Models
{
    /// <summary>
    /// A DTO for a city without points of interest
    /// </summary>
    //Created this class to match CityInfoRepository.GetCityAsync, where the pointsOfInterest is either included/not
    public class CityWithoutPointsOfInterestDto
    {
        /// <summary>
        /// The id of the city
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the city
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the city
        /// </summary>
        public string? Description { get; set; }
    }
}
