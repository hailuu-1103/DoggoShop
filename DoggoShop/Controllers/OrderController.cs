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
        [HttpPut("updateFreight/{id}")]
        public async Task<IActionResult> UpdateOrderFreightAsync(int id, Order order)
        {
            var orderToUpdate = context.Orders.FirstOrDefault(o => o.OrderId== id);
            if(orderToUpdate==null) {
                return BadRequest();
            }
            try
            {
                var result = context.Orders.Where(o => o.OrderId == id).AsNoTracking().ExecuteUpdate(od => od.SetProperty(o => o.Freight, o => order.Freight));
                if(result > 0 )
                {
                    return Ok();
                }else
                {
                    return BadRequest();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
    }
}
