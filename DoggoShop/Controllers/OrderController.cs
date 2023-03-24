using DoggoShopAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoggoShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();
        [HttpPost]
        public async Task<IActionResult> PostOrderWithCusIdAsync(Order order)
        {
            await context.Orders.AddAsync(order);
            await context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("getLastOrder")]
        public async Task<IActionResult> GetLastOrderAsync()
        {
            var order = await context.Orders.OrderBy(o => o.OrderDate).LastOrDefaultAsync();
            return Ok(order);
        }
    }
}
