using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;
using WebRazor.Models;

namespace WebRazor.Pages
{
    public class IndexModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        public IndexModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public List<Category> Categories { get; set; }
        public List<Models.Product> BestSaleProducts { get; set; }
        public List<Models.Product> NewProducts { get; set; }
        public List<Models.Product> HotProducts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            Categories = dbContext.Categories.ToList();

            var products = dbContext.Products.Where(p => p.DeletedAt == null).ToList();

            var idsBestSale = dbContext.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.Product.DeletedAt == null)
                .GroupBy(d => d.ProductId)
                .Select(g => new { ProductId = g.Key, Sum = g.Sum(d => d.Quantity) })
                .OrderByDescending(o => o.Sum);

            BestSaleProducts = new List<Models.Product>();
            foreach (var id in idsBestSale.Take(4))
            {
                BestSaleProducts.Add(products.First(p => p.ProductId == id.ProductId));
            }

            NewProducts = dbContext.Products.Where(p => p.DeletedAt == null)
                .OrderByDescending(p => p.ProductId).Take(4).ToList();

            HotProducts = new List<Models.Product>();
            foreach (var id in idsBestSale.OrderByDescending(o => o.ProductId).Take(4))
            {
                HotProducts.Add(products.First(p => p.ProductId == id.ProductId));
            }
            return Page();

        }

    }
}
