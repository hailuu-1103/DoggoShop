using DocumentFormat.OpenXml.Spreadsheet;
using DoggoShopAPI.DTO;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Models;

namespace WebRazor.Pages.Account
{
    [Authorize(Roles = "Customer")]
    public class EditModel : PageModel
    {
        private readonly PRN221DBContext dbContext;
        private HttpClient client;
        private string AccountApiUrl = "";
        [BindProperty]
        public Models.Account? Auth { get; set; }

        public EditModel(PRN221DBContext dbContext)
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

        private bool AccountExists(int id)
        {
            return dbContext.Accounts.Any(e => e.AccountId == id);
        }

        private bool AccountEmailExists(int id, string? email)
        {
            return dbContext.Accounts.Any(e => e.Email == email && e.AccountId != id);
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
                Password = Auth.Password,
                CompanyName = Auth.Customer.CompanyName,
                ContactName = Auth.Customer.ContactName,
                ContactTitle = Auth.Customer.ContactTitle,
                Address = Auth.Customer.Address,
            };
            var accJson = JsonSerializer.Serialize(accDTO);
            AccountApiUrl = "https://localhost:5000/api/Account/" + accId;
            var data = new StringContent(accJson, System.Text.Encoding.UTF8, "application/json");
            try
            {
                if (AccountEmailExists(Auth.AccountId, Auth.Email))
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
                if (!AccountExists(Auth.AccountId))
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
