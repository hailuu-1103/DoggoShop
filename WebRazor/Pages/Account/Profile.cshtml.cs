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
        private HttpClient client;
        private string AccountApiUrl = "";

        [BindProperty]
        public Models.Account Auth { get; set; }

        public ProfileModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
            this.client = new HttpClient();
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var accId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            AccountApiUrl = "https://localhost:5000/api/Account/id/" + accId;
            var response = await client.GetAsync(AccountApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Auth = JsonSerializer.Deserialize<Models.Account>(data, options);
          
            if (Auth == null)
            {
                return NotFound();
            }

            return Page();
        }

    }
}
