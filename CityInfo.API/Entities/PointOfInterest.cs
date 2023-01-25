using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CityInfo.API.Entities
{
    //This is the dependent class (dependent on City)
    //Best practice: apply attributes to db properties to enforce data integrity
    public class PointOfInterest
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
        public string Description { get; set; }

        //State what the foreign key will be
        //Convention where the navigation property is discovered on a type
        //Relationship will be created, and will always target the primary key of the principal entity (CityId) 
        [ForeignKey("CityId")]
        public City? City { get; set; }

        //Explicitly define the foreign key (good practice)
        public int CityId { get; set; }

        //Add a constructor that accepts a name parameter
        //Adding a constructor to a class ensures that the default paramterless constructor is no longer generated.
        //It explicitly states that we want the City class to always have a name.
        public PointOfInterest(string name)
        {
            Name = name;
        }
    }
}
