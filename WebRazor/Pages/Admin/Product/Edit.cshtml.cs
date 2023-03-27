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
    public class EditModel : PageModel
    {
        [BindProperty] public DoggoShopClient.Models.Product Product { get; set; }
        public List<DoggoShopClient.Models.Category> Categories;
        private readonly PRN221DBContext dbContext;
        private readonly IHubContext<HubServer> hubContext;

        public EditModel(PRN221DBContext dbContext, IHubContext<HubServer> hubContext)
        {
            this.dbContext = dbContext;
            this.hubContext = hubContext;
        }

        public async Task LoadProduct(int? id)
        {
            Categories = await dbContext.Categories.ToListAsync();
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {

            Product = await dbContext.Products.Where(p => p.DeletedAt == null).FirstOrDefaultAsync(p => p.ProductId == (int)id);

            await LoadProduct(id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {

            await LoadProduct(id);

            if (!ModelState.IsValid)
            {
                return Page();
            }

            DoggoShopClient.Models.Product product = await dbContext.Products.Where(p => p.DeletedAt == null).FirstOrDefaultAsync(p => p.ProductId == (int) id);
            product.ProductName = Product.ProductName;
            product.CategoryId = Product.CategoryId;
            product.UnitPrice = Product.UnitPrice;
            product.QuantityPerUnit = Product.QuantityPerUnit;
            product.UnitsInStock = product.UnitsInStock;

            ViewData["success"] = "Update successfully";
            await dbContext.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("Reload");

            return Page();
        }

        public async Task<IActionResult> OnGetDelete(int? id)
        {

            Product = await dbContext.Products.Where(p => p.DeletedAt == null).FirstOrDefaultAsync(p => p.ProductId == (int)id);

            Product.DeletedAt = DateTime.Now;

            await dbContext.SaveChangesAsync();
            await hubContext.Clients.All.SendAsync("Reload");

            return Redirect("/Admin/Product/Index");
        }
    }
}
