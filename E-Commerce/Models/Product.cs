using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace E_Commerce.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }
    }
}