using DoggoShopClient.DTO;
using DoggoShopClient.Models;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages
{
    public class IndexModel : PageModel
    {
        private HttpClient client;
        private string CategoryApiUrl;
        private string ProductApiUrl;
        private string OrderDetailApiUrl;
        public IndexModel()
        {
            client = new HttpClient();
        }

        public List<Category> Categories { get; set; }
        public List<DoggoShopClient.Models.Product> BestSaleProducts { get; set; }
        public List<DoggoShopClient.Models.Product> NewProducts { get; set; }
        public List<DoggoShopClient.Models.Product> HotProducts { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }
            CategoryApiUrl = "https://localhost:5000/api/category";
            var responseCategory = await client.GetAsync(CategoryApiUrl);
            var dataCat = await responseCategory.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Categories = JsonSerializer.Deserialize<List<Category>>(dataCat, options).ToList();
            ProductApiUrl = "https://localhost:5000/api/product";
            var responseProduct = await client.GetAsync(ProductApiUrl);
            var data = await responseProduct.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<List<DoggoShopClient.Models.Product>>(data, options).ToList();

            // BEST SALE
            OrderDetailApiUrl = "https://localhost:5000/api/orderdetail/getBestSaleProductId";
            var responseOd = await client.GetAsync(ProductApiUrl);
            var dataOd = await responseOd.Content.ReadAsStringAsync();
            var idsBestSale = JsonSerializer.Deserialize<List<BestSaleDTO>>(data, options).ToList();
            BestSaleProducts = new List<DoggoShopClient.Models.Product>();
            foreach (var id in idsBestSale.Take(4))
            {
                BestSaleProducts.Add(products.First(p => p.ProductId == id.ProductId));
            }

            // NEW Products
            ProductApiUrl = "https://localhost:5000/api/product/getNewActiveProducts/" + 4;
            var responseNewProducts = await client.GetAsync(ProductApiUrl);
            var dataNewProducts = await responseNewProducts.Content.ReadAsStringAsync();
            NewProducts = JsonSerializer.Deserialize<List<DoggoShopClient.Models.Product>>(dataNewProducts, options).ToList();

            // HOT Products
            HotProducts = new List<DoggoShopClient.Models.Product>();
            foreach (var id in idsBestSale.OrderByDescending(o => o.ProductId).Take(4))
            {
                HotProducts.Add(products.First(p => p.ProductId == id.ProductId));
            }
            return Page();

        }

    }
}
