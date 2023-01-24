using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    public class City
    {
        //This property is a primary key
        [Key]
        //A new key will be generated when a city is added
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        //Make sure Name is not null
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        //Initialize to an empty list, to prevent null reference exceptions when trying to manipulate a list when
        //the points of interest haven't been loaded yet.
        public ICollection<PointOfInterest> PointsOfInterest { get; set; }
            = new List<PointOfInterest>();

        //Add a constructor that accepts a name parameter
        //Adding a constructor to a class ensures that the default paramterless constructor is no longer generated.
        //It explicitly states that we want the City class to always have a name.
        public City(string name)
        {
            Name = name;
        }
    }
}
