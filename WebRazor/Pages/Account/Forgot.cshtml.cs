using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebRazor.Materials;
using WebRazor.Models;

namespace WebRazor.Pages.Account
{
    public class ForgotModel : PageModel
    {
        private HttpClient client;
        private string AccountApiUrl = "";
        public ForgotModel()
        {
            this.client = new HttpClient();
        }

        public async Task OnGet()
        {
        }

         public async Task<IActionResult> OnPost(string? email)
         {
            if (email == null || email.Equals(""))
            {
                ViewData["error"] = "Email is required";
                return Page();
            }
            AccountApiUrl = "https://localhost:5000/api/Account/email/" + email;
            var response = await client.GetAsync(AccountApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            Models.Account account = JsonSerializer.Deserialize<Models.Account>(data, options);

            if (account == null)
            {
                ViewData["error"] = "Wrong email";
                return Page();
            }

            String password = Faker.Name.First() + Faker.RandomNumber.Next();
            Mail mail = new Mail();
            mail.SendEmailResetAsync(email, password);

            ViewData["success"] = "New password was send to your email";

            account.Password = HashPassword.Hash(password);
            return Page();
        }
    }
}
