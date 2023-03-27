using DocumentFormat.OpenXml.Bibliography;
using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages.Order
{
    public class IndexModel : PageModel
    {
        private HttpClient client;
        private string AccountApiUrl;
        private string OrderApiUrl;
        private readonly PRN221DBContext dbContext;

        public IndexModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
            client = new HttpClient();
        }

        [BindProperty]
        public DoggoShopClient.Models.Account Auth { get; set; }
        public List<DoggoShopClient.Models.Order> Orders { get; set; }

        private int perPage = 5;

        [FromQuery(Name = "page")] public int Page { get; set; } = 1;

        public List<String> PagesLink { get; set; } = new List<string>();

        public async Task getData()
        {
            var accId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            AccountApiUrl = "https://localhost:5000/api/Account/id/" + accId;
            var response = await client.GetAsync(AccountApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Auth = JsonSerializer.Deserialize<DoggoShopClient.Models.Account>(data, options);
        }

        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> OnGetAsync()
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            await getData();

            Orders = Auth.Customer.Orders
                .OrderByDescending(o => o.OrderDate)
                .Skip((Page - 1) * perPage).Take(perPage).ToList();

            PageLink page = new PageLink(perPage);
            PagesLink = page.getLink(Page, Auth.Customer.Orders.Count(), "/Order/Index?" );

            return Page();
        }

        private Dictionary<int, int> getCart()
        {
            var cart = HttpContext.Session.GetString("cart");

            Dictionary<int, int> list;

            if (cart != null)
            {
                list = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
            }
            else
            {
                list = new Dictionary<int, int>();
            }

            return list;
        }

        public async Task<IActionResult> OnGetAdd(int? id)
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

            DoggoShopClient.Models.Product product = (await dbContext.Products.Where(p => p.DeletedAt == null).FirstOrDefaultAsync(p => p.ProductId == id));

            if (product == null || product.UnitsInStock == 0)
            {
                TempData["fail"] = "Quantity = 0";
                return Redirect("/Product/Detail/" + id);
            } else
            {
                Dictionary<int, int> list = getCart();

                if ((list.Where(p => p.Key == id)).Count() == 0)
                {
                    list.Add((int)id, 1);
                }


                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(list));
                return Redirect("/Cart/Index");
            }    


        }


    }
}
