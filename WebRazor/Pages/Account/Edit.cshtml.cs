using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader.Log;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WebRazor.Models;

namespace WebRazor.Pages.Account
{
    [Authorize(Roles = "Customer")]
    public class EditModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        [BindProperty]
        public Models.Account? Auth { get; set; }

        public EditModel(PRN221DBContext dbContext)
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
            
            dbContext.Attach(Auth).State = EntityState.Modified;
            dbContext.Entry(Auth).Property(x => x.Password).IsModified = false;
            dbContext.Entry(Auth).Property(x => x.Role).IsModified = false;
            dbContext.Entry(Auth).Property(x => x.Email).IsModified = true;
            dbContext.Entry(Auth.Customer).Property(x => x.CompanyName).IsModified = true;
            dbContext.Entry(Auth.Customer).Property(x => x.ContactTitle).IsModified = true;
            dbContext.Entry(Auth.Customer).Property(x => x.ContactName).IsModified = true;
            dbContext.Entry(Auth.Customer).Property(x => x.Address).IsModified = true;

            try
            {
                if (AccountEmailExists(Auth.AccountId, Auth.Email))
                {
                    ViewData["fail"] = "Dublicat Email";
                    return Page();
                }
                await dbContext.SaveChangesAsync();
                ViewData["success"] = "Update Successfull";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountExists(Auth.AccountId))
                {
                    return NotFound();
                }
            }
            return Page();

        }
}
}
