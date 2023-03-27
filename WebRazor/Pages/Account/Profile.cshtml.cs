using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;

namespace WebRazor.Pages.Account
{
    [Authorize(Roles = "Customer")]
    public class ProfileModel : PageModel
    {
        private HttpClient client;
        private string AccountApiUrl = "";

        [BindProperty]
        public DoggoShopClient.Models.Account Auth { get; set; }

        public ProfileModel()
        {
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
            Auth = JsonSerializer.Deserialize<DoggoShopClient.Models.Account>(data, options);
          
            if (Auth == null)
            {
                return NotFound();
            }

            return Page();
        }

    }
}
