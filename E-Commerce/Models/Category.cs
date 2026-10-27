using System.ComponentModel.DataAnnotations;

namespace E_Commerce.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}