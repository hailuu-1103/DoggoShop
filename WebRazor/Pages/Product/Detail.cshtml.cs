using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using DoggoShopClient.Models;
namespace WebRazor.Pages.Product
{
    public class DetailModel : PageModel
    {
        private HttpClient client;
        private string ProductApiUrl = "";
        public DetailModel()
        {
            this.client = new HttpClient();
        }

        [BindProperty]
        public DoggoShopClient.Models.Product Product { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }
            ProductApiUrl = "https://localhost:5000/api/Product/getActiveProductById/" + id;
            var response = await client.GetAsync(ProductApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Product =  JsonSerializer.Deserialize<DoggoShopClient.Models.Product>(data, options);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            if (id == null)
            {
                return NotFound();
            }

            ProductApiUrl = "https://localhost:5000/api/Product/getActiveProductById/" + id;
            var response = await client.GetAsync(ProductApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Product = JsonSerializer.Deserialize<DoggoShopClient.Models.Product>(data, options);

            if (Product == null)
            {
                return NotFound();
            }
            return Page();
        }

        
    }
}
