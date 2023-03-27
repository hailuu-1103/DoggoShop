using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly HttpClient client;
        private string AccountApiUrl = "";

        public LoginModel()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        [BindProperty]
        public DoggoShopClient.Models.Account Account { get; set; }

        public void OnGet()
        {
        }

        private async Task<DoggoShopClient.Models.Account?> getAccount()
        {
            AccountApiUrl = "https://localhost:5000/api/account";
            var response = await client.GetAsync(AccountApiUrl);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var userList = JsonSerializer.Deserialize<List<DoggoShopClient.Models.Account>>(data, options).ToList();
            var acc = userList.SingleOrDefault(
                    a => a.Email.Equals(Account.Email)
                    && a.Password.Equals(HashPassword.Hash(Account.Password)));

            if (acc == null)
            {
                ViewData["msg"] = "Email/ Password is wrong";
                return null;
            }

            if (acc.Customer != null)
            {
                acc.Customer.Accounts = null;
            }

            if (acc.Employee != null)
            {
                acc.Employee.Accounts = null;
            }

            if ((acc.Employee != null && !(bool)acc.Employee.Active) || (acc.Customer != null && !(bool)acc.Customer.Active))
            {
                ViewData["msg"] = "Account was deactivated";
                return null;
            }

            return acc;
        }

        private string getRole(int Role)
        {
            switch (Role)
            {
                case 1:
                    return "Employee";
                case 2:
                    return "Customer";
                default:
                    return "Guess";
            }
        }

        private async Task SetCookie(DoggoShopClient.Models.Account acc)
        {
            // queried data that can be attached 
            // to the User's login identity
            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, acc.Email),
                new Claim(ClaimTypes.Role, getRole((int)acc.Role)),
                new Claim(ClaimTypes.NameIdentifier, acc.AccountId.ToString()),
                // ClaimTypes - Gender, Country, etc., many 
                // fields are available - even custom ones 
                // can be defined
            };

            // the value Authenticationscheme is "Cookies
            var claimsIdentity = new ClaimsIdentity(claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                /*IsPersistent = RememberMe*/
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60),
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
                );
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            DoggoShopClient.Models.Account acc = await getAccount();

            if (acc == null)
            {
                return Page();
            }

            await SetCookie(acc);

            if (acc.Role == 1)
            {
                return RedirectToPage("/Admin/Dashboard/Index");
            }
            return RedirectToPage("/index");
        }

        public async Task<IActionResult> OnGetLogout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToPage("/index");
        }
    }
}
