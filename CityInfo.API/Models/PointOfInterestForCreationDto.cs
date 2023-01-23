using System.ComponentModel.DataAnnotations;

namespace CityInfo.API.Models
{
    //Use separate models(DTO: data-transfer-object)dto, programming for creating, updating and returing resources
    //good practice when you need to refactor code
    public class PointOfInterestForCreationDto
    {
        [Required(ErrorMessage = "You should provide a name value.")]
        [MaxLength(50)]
        public string Name { get; set; } = String.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
