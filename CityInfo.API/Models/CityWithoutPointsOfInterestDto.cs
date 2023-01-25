namespace CityInfo.API.Models
{
    //Created this class to match CityInfoRepository.GetCityAsync, where the pointsOfInterest is either included/not
    public class CityWithoutPointsOfInterestDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
