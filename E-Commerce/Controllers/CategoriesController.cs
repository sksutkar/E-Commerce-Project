using E_Commerce.Data;
using E_Commerce.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ECommerceDBcontext _context;

        public CategoriesController(ECommerceDBcontext context)
        {
            _context = context;
        }

        // 1. Get All Categories
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            return Ok(categories);
        }

        // 2. Get Category By Id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found"
                });
            }

            return Ok(category);
        }

        // 3. Add Category
        [HttpPost]
        public async Task<IActionResult> AddCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Categories.Add(category);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetCategoryById),
                new { id = category.CategoryId },
                category
            );
        }

        // 4. Update Category
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            Category category)
        {
            if (id != category.CategoryId)
            {
                return BadRequest(new
                {
                    message = "Category ID mismatch"
                });
            }

            var existingCategory = await _context.Categories
                .FindAsync(id);

            if (existingCategory == null)
            {
                return NotFound(new
                {
                    message = "Category not found"
                });
            }

            existingCategory.CategoryName = category.CategoryName;
            existingCategory.Description = category.Description;
            existingCategory.IsActive = category.IsActive;

            await _context.SaveChangesAsync();

            return Ok(existingCategory);
        }

        // 5. Delete Category
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound(new
                {
                    message = "Category not found"
                });
            }

            _context.Categories.Remove(category);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Category deleted successfully"
            });
        }
    }
}