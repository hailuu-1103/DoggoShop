using DoggoShopAPI.DTO;
using DoggoShopClient.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private HttpClient client;

        private string AccountApiUrl = "";

        public SignUpModel()
        {
            this.client = new HttpClient();
            this.AccountApiUrl = "https://localhost:5000/api/account";
        }

        [BindProperty]
        public Customer Customer { get; set; }

        [BindProperty]
        public DoggoShopClient.Models.Account Account { get; set; }

        [BindProperty, Required(ErrorMessage = "Re-password is required")]
        public string RePassword { get; set; }

        public void OnGet()
        {

        }
        private async Task<bool> IsEmailExists(string? email)
        {
            var url = "https://localhost:5000/api/account/isEmailExist/" + email;
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = System.Text.Json.JsonSerializer.Deserialize<bool>(data, options);
            return result;
        }
        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (RePassword != Account.Password)
            {
                ViewData["msg-repassword"] = "Re-password not match";
                return Page();
            }

            var acc = await this.IsEmailExists(Account.Email);

            if (acc != null)
            {
                ViewData["msg"] = "This email is exist";
                return Page();
            }
            var accDTO = new AccountDTO()
            {
                Email= Account.Email,
                Password = Account.Password,
                Role = 2,
                CompanyName = Customer.CompanyName,
                ContactName = Customer.ContactName,
                ContactTitle = Customer.ContactTitle,
                Address = Customer.Address,
                CreatedAt = DateTime.Now,
                Active = true,
            };
            var accJson = System.Text.Json.JsonSerializer.Serialize(accDTO);
            var content = new StringContent(accJson, Encoding.UTF8, "application/json");
            var accClientRespone = await client.PostAsync(AccountApiUrl, content);
            if(accClientRespone.IsSuccessStatusCode)
            {
                return RedirectToPage("/index");
            }
            else
            {
                return RedirectToPage("/register");
            }
        }
    }
}
