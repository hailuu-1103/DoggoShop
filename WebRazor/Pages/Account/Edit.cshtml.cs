using DoggoShopAPI.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages.Account
{
    [Authorize(Roles = "Customer")]
    public class EditModel : PageModel
    {
        private HttpClient client;
        private string AccountApiUrl = "";
        [BindProperty]
        public DoggoShopClient.Models.Account? Auth { get; set; }

        public EditModel()
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

        private async Task<bool> AccountExists(int id)
        {
            var url = "https://localhost:5000/api/account/isAccountExists/" + id;
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<bool>(data, options);
            return result;
        }

        private async Task<bool> AccountEmailExists(int id, string? email)
        {
            var url = "https://localhost:5000/api/account/isAccountWithEmailExists/" + id + "/" + email;
            var response = await client.GetAsync(url);
            var data = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var result = JsonSerializer.Deserialize<bool>(data, options);
            return result;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool valid = true;
            var accId = Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
            foreach (var modelStateKey in ViewData.ModelState.Keys)
            {
                if (!modelStateKey.Equals("Auth.Password"))
                {
                    var value = ViewData.ModelState[modelStateKey];
                    valid = valid && !(value.ValidationState == ModelValidationState.Invalid);
                }
            }

            if (!valid)
            {
                return Page();
            }
            var accDTO = new AccountDTO()
            {
                Email = Auth.Email,
                Password = HashPassword.Hash(Auth.Password),
                CompanyName = Auth.Customer.CompanyName,
                ContactName = Auth.Customer.ContactName,
                ContactTitle = Auth.Customer.ContactTitle,
                Address = Auth.Customer.Address,
            };
            var isAccountWithEmailExists = await this.AccountEmailExists(Auth.AccountId, Auth.Email);
            var accJson = JsonSerializer.Serialize(accDTO);
            AccountApiUrl = "https://localhost:5000/api/Account/" + accId;
            var data = new StringContent(accJson, System.Text.Encoding.UTF8, "application/json");
            try
            {
                if (isAccountWithEmailExists)
                {
                    ViewData["fail"] = "Dublicate Email";
                    return Page();
                }
                var response = await client.PutAsync(AccountApiUrl, data);
                if (response.IsSuccessStatusCode)
                {
                    ViewData["success"] = "Update Successfull";
                }
                else
                {
                    ViewData["fail"] = "Failed to update account, reason " + response.ReasonPhrase;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                var isAccountExists = await this.AccountExists(Auth.AccountId);
                if (!isAccountExists)
                {
                    return NotFound();
                }
            }
            catch(DbUpdateException ex)
            {
                ViewData["fail"] = "Message: " + ex.Message;
                return NotFound();
            }
            return Page();

        }
}
}
