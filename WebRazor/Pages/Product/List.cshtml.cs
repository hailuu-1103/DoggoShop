using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages.Product
{
    public class ListModel : PageModel
    {
        private HttpClient client;
        private string ProductApiUrl;
        private string CategoryApiUrl;
        [BindProperty] public List<DoggoShopClient.Models.Product> Products { get; set; } = new List<DoggoShopClient.Models.Product>();

        [BindProperty] public List<Category> Categories { get; set; }

        [FromQuery(Name = "page")] public int Page { get; set; } = 1;

        [FromQuery(Name = "order")] public String Order { get; set; } = "None";

        private int perPage = 4;

        public int Id { get; set; }

        public List<String> PagesLink { get; set; } = new List<string>();

        public ListModel()
        {
            this.client = new HttpClient();
        }

        public async Task<IActionResult> OnGet(int? id)
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

            Id = (int)id;

            ProductApiUrl = "https://localhost:5000/api/Product/getNumberOfProductsByCategory/" + id;
            var responseNewProducts = await client.GetAsync(ProductApiUrl);
            var dataNewProducts = await responseNewProducts.Content.ReadAsStringAsync();
            var size = JsonSerializer.Deserialize<int>(dataNewProducts, options);

            int total = CalcPagesCount(size);

            if (Page < 1 || Page > total)
            {
                return NotFound();
            }

            String orderUrl = "";

            if (Order != "None")
            {
                orderUrl = "order=" + Order;
            }

            PageLink page = new PageLink(perPage);
            PagesLink = page.getLink(Page, size, "/Product/List/" + id + "?" + orderUrl + "&");

            ProductApiUrl = "https://localhost:5000/api/Product/getActiveProductByCategory/" + id;
            var response = await client.GetAsync(ProductApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var list = JsonSerializer.Deserialize<List<DoggoShopClient.Models.Product>>(data, options);

            switch (Order)
            {
                case "Asc":
                    list = (List<DoggoShopClient.Models.Product>?)list.OrderBy(p => p.UnitPrice);
                    break;
                case "Desc":
                    list = (List<DoggoShopClient.Models.Product>?)list.OrderByDescending(p => p.UnitPrice);
                    break;
            }

            Products = list
                .Skip((Page - 1) * perPage)
                .Take(perPage)
                .ToList();

            return Page();
        }

        private int CalcPagesCount(int size)
        {
            int totalPage = size / perPage;

            if (size % perPage != 0) totalPage++;
            return totalPage;
        }
    }
}
