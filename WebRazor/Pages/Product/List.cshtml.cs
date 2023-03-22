using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebRazor.Materials;
using WebRazor.Models;

namespace WebRazor.Pages.Product
{
    public class ListModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        [BindProperty] public List<Models.Product> Products { get; set; } = new List<Models.Product>();

        [BindProperty] public List<Category> Categories { get; set; }

        [FromQuery(Name = "page")] public int Page { get; set; } = 1;

        [FromQuery(Name = "order")] public String Order { get; set; } = "None";

        private int perPage = 4;

        public int Id { get; set; }

        public List<String> PagesLink { get; set; } = new List<string>();

        public ListModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IActionResult> OnGet(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            Categories = dbContext.Categories.ToList();

            Id = (int)id;

            int size = await dbContext.Products
                .Where(p => p.DeletedAt == null)
                .Where(p => p.CategoryId == id)
                .CountAsync();

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

            var list = dbContext.Products
                .Where(p => p.DeletedAt == null)
                .Where(p => p.CategoryId == id);

            switch (Order)
            {
                case "Asc":
                    list = list.OrderBy(p => p.UnitPrice);
                    break;
                case "Desc":
                    list = list.OrderByDescending(p => p.UnitPrice);
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
