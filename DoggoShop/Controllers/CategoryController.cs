using DoggoShopAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoggoShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();
        [HttpGet]
        public IActionResult GetAllCategories()
        {
            var categories = context.Categories.ToList();
            return Ok(categories);
        }
        [HttpGet("getCategoryByName/{name}")]
        public IActionResult GetCategoryById(string name)
        {
            var category = context.Categories.FirstOrDefault(cat => cat.CategoryName.Equals(name));
            return Ok(category);
        }
    }
}
