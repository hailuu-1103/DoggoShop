using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebRazor.Pages.Admin.Product
{
    [Authorize(Roles = "Employee")]
    public class CreateModel : PageModel
    {
        [BindProperty] public DoggoShopClient.Models.Product Product { get; set; }
        public List<DoggoShopClient.Models.Category> Categories;
        private readonly PRN221DBContext dbContext;
        private readonly IHubContext<HubServer> hubContext;

        public CreateModel(PRN221DBContext dbContext, IHubContext<HubServer> hubContext)
        {
            this.dbContext = dbContext;
            this.hubContext = hubContext;
        }

        public async Task LoadProduct()
        {
            Categories = await dbContext.Categories.ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync()
        {

            await LoadProduct();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await LoadProduct();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            await dbContext.Products.AddAsync(Product);
            await dbContext.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("Reload");

            return Redirect("/Admin/Product/Index");
        }
    }
}
