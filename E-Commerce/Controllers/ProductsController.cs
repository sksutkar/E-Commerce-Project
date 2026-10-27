using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ECommerceDBcontext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(
            ECommerceDBcontext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // 1. Get All Products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .AsNoTracking()
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    p.CategoryId,
                    CategoryName = p.Category != null
                        ? p.Category.CategoryName
                        : null
                })
                .ToListAsync();

            return Ok(products);
        }

        // 2. Get Product By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .Where(p => p.ProductId == id)
                .Select(p => new
                {
                    p.ProductId,
                    p.ProductName,
                    p.Description,
                    p.Price,
                    p.StockQuantity,
                    p.ImageUrl,
                    p.CategoryId,
                    CategoryName = p.Category != null
                        ? p.Category.CategoryName
                        : null
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            return Ok(product);
        }

        // 3. Add Product With Image
        [HttpPost]
        public async Task<IActionResult> AddProduct(
            [FromForm] Product product,
            IFormFile? image)
        {
            if (string.IsNullOrWhiteSpace(product.ProductName))
            {
                return BadRequest("Product name is required.");
            }

            if (product.Price < 0 || product.StockQuantity < 0)
            {
                return BadRequest(
                    "Price and stock quantity cannot be negative.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == product.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Invalid CategoryId.");
            }

            if (image != null)
            {
                var imageResult = await SaveImage(image);

                if (!imageResult.Success)
                {
                    return BadRequest(imageResult.Error);
                }

                product.ImageUrl = imageResult.ImageUrl;
            }

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProductById),
                new { id = product.ProductId },
                product
            );
        }

        // 4. Update Product
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            [FromForm] Product updatedProduct,
            IFormFile? image)
        {
            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            if (string.IsNullOrWhiteSpace(updatedProduct.ProductName))
            {
                return BadRequest("Product name is required.");
            }

            if (updatedProduct.Price < 0 ||
                updatedProduct.StockQuantity < 0)
            {
                return BadRequest(
                    "Price and stock quantity cannot be negative.");
            }

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.CategoryId == updatedProduct.CategoryId);

            if (!categoryExists)
            {
                return BadRequest("Invalid CategoryId.");
            }

            if (image != null)
            {
                var imageResult = await SaveImage(image);

                if (!imageResult.Success)
                {
                    return BadRequest(imageResult.Error);
                }

                // Delete old image after successfully saving the new one.
                DeleteImage(existingProduct.ImageUrl);

                existingProduct.ImageUrl = imageResult.ImageUrl;
            }

            existingProduct.ProductName = updatedProduct.ProductName;
            existingProduct.Description = updatedProduct.Description;
            existingProduct.Price = updatedProduct.Price;
            existingProduct.StockQuantity = updatedProduct.StockQuantity;
            existingProduct.CategoryId = updatedProduct.CategoryId;

            await _context.SaveChangesAsync();

            return Ok(existingProduct);
        }

        // 5. Delete Product
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new
                {
                    message = "Product not found"
                });
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            DeleteImage(product.ImageUrl);

            return Ok(new
            {
                message = "Product deleted successfully"
            });
        }

        // Save image to wwwroot/images/products
        private async Task<(bool Success, string? ImageUrl, string? Error)>
            SaveImage(IFormFile image)
        {
            if (image.Length == 0)
            {
                return (false, null, "Image file is empty.");
            }

            if (image.Length > 5 * 1024 * 1024)
            {
                return (false, null, "Image size must not exceed 5 MB.");
            }

            var allowedExtensions = new[]
            {
                ".jpg", ".jpeg", ".png", ".webp"
            };

            var extension = Path.GetExtension(image.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return (false, null,
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            if (string.IsNullOrWhiteSpace(_environment.WebRootPath))
            {
                return (false, null, "Web root path is not configured.");
            }

            var folderPath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products");

            Directory.CreateDirectory(folderPath);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            try
            {
                using var stream = new FileStream(
                    filePath,
                    FileMode.CreateNew);

                await image.CopyToAsync(stream);
            }
            catch (IOException)
            {
                return (false, null, "Unable to save image.");
            }

            return (
                true,
                $"/images/products/{fileName}",
                null
            );
        }

        // Delete image from folder
        private void DeleteImage(string? imageUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) ||
                string.IsNullOrWhiteSpace(_environment.WebRootPath))
            {
                return;
            }

            var fileName = Path.GetFileName(imageUrl);

            var filePath = Path.Combine(
                _environment.WebRootPath,
                "images",
                "products",
                fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}