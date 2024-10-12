using System.ComponentModel.DataAnnotations;

namespace eComStore.Model
{
    public class ConfigurationType
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? Name { get; set; }     
    }
}
