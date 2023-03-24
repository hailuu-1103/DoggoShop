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
    }
}
