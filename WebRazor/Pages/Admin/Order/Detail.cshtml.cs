using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace WebRazor.Pages.Admin.Order
{
    [Authorize(Roles = "Employee")]
    public class DetailModel : PageModel
    {
        public DoggoShopClient.Models.Order Order { get; set;}

        private readonly PRN221DBContext dbContext;

        public int ID;

        public DetailModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task<IActionResult> OnGet(int? id)
        {

            ID = (int)id;

            Order = await dbContext.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            foreach (var item in Order.OrderDetails)
            {
                item.Product = await dbContext.Products.Where(p => p.DeletedAt == null).FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
            }

            return Page();
        }
    }
}
