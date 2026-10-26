using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static NationalPark_API_1003.Models.Trail;

namespace NationalPark_API_1003.Models.DTOs
{
    public class TrailDto
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Distance { get; set; }
        [Required]
        public string Elevation { get; set; }
        
        public DifficultyType Difficulty { get; set; }
        public int NationalParkId { get; set; }
        
        public NationalPark NationalPark { get; set; }
    }
}
