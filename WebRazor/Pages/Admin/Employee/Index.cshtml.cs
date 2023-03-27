using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebRazor.Materials;

namespace WebRazor.Pages.Admin.Employee
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
        public List<DoggoShopClient.Models.Employee> Employees  { get; set; }
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

            var query = dbContext.Employees;


            Employees = await query
                .OrderByDescending(p => p.EmployeeId)
                .Skip((Page - 1) * perPage).Take(perPage)
                .ToListAsync();

            PageLink page = new PageLink(perPage);
            PagesLink = page.getLink(Page, await query.CountAsync(), "/Admin/Employee/Index?");
        }

        public async Task<IActionResult> OnGetActive(int? id)
        {

            DoggoShopClient.Models.Employee employee = await dbContext.Employees.FirstOrDefaultAsync(p => p.EmployeeId == id);
            if (employee != null)
            {
                employee.Active = !employee.Active;
                await dbContext.SaveChangesAsync();
            }

            return Redirect("/Admin/Employee/Index");
        }
    }
}
