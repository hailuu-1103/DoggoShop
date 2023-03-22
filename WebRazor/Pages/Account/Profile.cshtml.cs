using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Models;

namespace WebRazor.Pages.Account
{
    [Authorize(Roles = "Customer")]
    public class ProfileModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        [BindProperty]
        public Models.Account Auth { get; set; }

        public ProfileModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;

        }

        public async Task<IActionResult> OnGetAsync()
        {
            Auth = await dbContext.Accounts.Include(a => a.Customer)
                .FirstOrDefaultAsync(a => a.AccountId == Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value));

            if (Auth == null)
            {
                return NotFound();
            }

            return Page();
        }

    }
}
