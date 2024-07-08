using System.ComponentModel.DataAnnotations;

namespace eComStore.Model
{
    public class Company
    {
        [Key]
        public int Id { get; set; }
        [Required]  
        public string Name { get; set; }    
        public string? StreetAdress { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? PhoneNumber { get; set; }

    }
}
