using DoggoShopAPI.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using WebRazor.Materials;
using WebRazor.Models;

namespace WebRazor.Pages.Account
{
    public class SignUpModel : PageModel
    {
        private readonly PRN221DBContext dbContext;
        private HttpClient client;

        private string AccountApiUrl = "";

        public SignUpModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
            this.client = new HttpClient();
            this.AccountApiUrl = "https://localhost:5000/api/account";
        }

        [BindProperty]
        public Customer Customer { get; set; }

        [BindProperty]
        public Models.Account Account { get; set; }

        [BindProperty, Required(ErrorMessage = "Re-password is required")]
        public string RePassword { get; set; }

        public void OnGet()
        {

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

            var acc = await dbContext.Accounts.SingleOrDefaultAsync(a => a.Email.Equals(Account.Email));

            if ( acc != null)
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
