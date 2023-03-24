using DoggoShopAPI.Models;
using DoggoShopClient.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoggoShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderDetailController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();

        [HttpGet("getBestSaleProductId")]
        public IActionResult GetBestSaleProductId()
        {
            var result = new List<BestSaleDTO>();
            var items = context.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.Product.DeletedAt == null)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Sum = g.Sum(d => d.Quantity) })
                .OrderByDescending(o => o.Sum).ToList();
            foreach(var item in items)
            {
                result.Add(new BestSaleDTO { ProductId = item.ProductId,Sum = item.Sum });
            }
            return Ok(result);
        }
        [HttpGet("getOrderCount/{cusId}/{proId}")]
        public async Task<IActionResult> GetOrderCount(string cusId, int proId)
        {
            var result = await context.OrderDetails.Include(o => o.Order).Where(o => o.ProductId == proId && o.Order.CustomerId == cusId).CountAsync();
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> PostOrderDetailAsync(OrderDetail orderDetail)
        {
            context.OrderDetails.Add(orderDetail);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
