using DoggoShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace DoggoShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();

        [HttpGet("getActiveProductById/{id}")]
        public IActionResult GetActiveProductById(int id)
        {
            var product = context.Products.Include(product => product.Category).Where(p => p.DeletedAt == null).FirstOrDefault(m => m.ProductId == id);
            return Ok(product);
        }
        [HttpGet]
        public IActionResult GetAllActiveProducts()
        {
            var product = context.Products.Where(p => p.DeletedAt == null).ToList();
            return Ok(product);
        }
        [HttpGet("getNewActiveProducts/{number}")]
        public IActionResult GetNewActiveProducts(int number)
        {
            var product = context.Products.Where(p => p.DeletedAt == null)
                .OrderByDescending(p => p.ProductId).Take(number).ToList();
            return Ok(product);
        }
        [HttpGet("getNumberOfProductsByCategory/{id}")]
        public async Task<IActionResult> GetNumberOfProductsByCategory(int id)
        {
            var result = await context.Products
                .Where(p => p.DeletedAt == null)
                .Where(p => p.CategoryId == id)
                .CountAsync();
            return Ok(result);
        }
        [HttpGet("getActiveProductByCategory/{id}")]
        public async Task<IActionResult> GetActiveProductByCategory(int id)
        {
            var result = context.Products
                .Where(p => p.DeletedAt == null)
                .Where(p => p.CategoryId == id)
                .ToList();
            return Ok(result);
        }
    }
}
