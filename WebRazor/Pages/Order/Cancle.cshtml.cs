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
    [Authorize(Roles = "Customer")]
    public class CancleModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        public CancleModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [BindProperty]
        public DoggoShopClient.Models.Account Auth { get; set; }
        public List<DoggoShopClient.Models.Order> Orders { get; set; }

        private int perPage = 5;

        [FromQuery(Name = "page")] public int Page { get; set; } = 1;

        public List<String> PagesLink { get; set; } = new List<string>();

        public async Task getData()
        {
            Auth = await dbContext.Accounts.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value));

            var cus = await dbContext.Customers.ToListAsync();
            var ord = await dbContext.Orders.ToListAsync();
            var ordDe = await dbContext.OrderDetails.ToListAsync();
            var pro = await dbContext.Products.Where(p => p.DeletedAt == null).ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await getData();

            Orders = Auth.Customer.Orders.Where(o => o.RequiredDate == null)
                .OrderByDescending(o => o.OrderDate)
                .Skip((Page - 1) * perPage).Take(perPage).ToList();

            PageLink page = new PageLink(perPage);
            PagesLink = page.getLink(Page, Auth.Customer.Orders.Where(o => o.RequiredDate == null).ToList().Count(), "/Order/Index?");

            return Page();
        }

    }
}
