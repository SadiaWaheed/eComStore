using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace eComStore.Model
{
    public class Configurations
    {
        public int Id { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }

        [Display(Name = "Image")]
        public string? ImagePath { get; set; }

        [Required]
        [Display(Name = "Configuration Type")]
        public int ConfigurationTypeId { get; set; }
        [ValidateNever]
        public ConfigurationType? ConfigurationType { get; set; }
    }
}
