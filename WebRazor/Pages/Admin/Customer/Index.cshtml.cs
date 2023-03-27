using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebRazor.Materials;

namespace WebRazor.Pages.Admin.Customer
{
    [Authorize(Roles = "Employee")]
    public class IndexModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        public IndexModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [FromQuery(Name = "page")] public int Page { get; set; } = 1;
        private int perPage = 10;
        public List<DoggoShopClient.Models.Customer> Customers { get; set; }
        [FromQuery(Name = "txtSearch")] public string Search { get; set; } = "";

        public List<String> PagesLink { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {

            await Load();

            return Page();
        }

        public async Task Load()
        {
            if (Search == null) Search = "";

            var query = dbContext.Customers
               .Where(p => p.ContactName.Contains(Search));


            Customers = await query
                .OrderByDescending(p => p.CustomerId)
                .Skip((Page - 1) * perPage).Take(perPage)
                .ToListAsync();

            PageLink page = new PageLink(perPage);
            String param = "txtSearch=" + Search;
            PagesLink = page.getLink(Page, await query.CountAsync(), "/Admin/Customer/Index?" + param + "&");
        }

        public async Task<IActionResult> OnGetActive(string? id)
        {
            DoggoShopClient.Models.Customer customer = await dbContext.Customers.FirstOrDefaultAsync(p => p.CustomerId == id);
            if (customer != null)
            {
                customer.Active = !customer.Active;
                await dbContext.SaveChangesAsync();
            }

            return Redirect("/Admin/Customer/Index");
        }
    }
}
