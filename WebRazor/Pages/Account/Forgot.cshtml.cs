using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebRazor.Materials;
using WebRazor.Models;

namespace WebRazor.Pages.Account
{
    public class ForgotModel : PageModel
    {
        private readonly PRN221DBContext dbContext;

        public ForgotModel(PRN221DBContext dBContext)
        {
            this.dbContext = dBContext;
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

            var a = await dbContext.Accounts.ToListAsync();

            Models.Account account = await dbContext.Accounts.FirstOrDefaultAsync(a => a.Email != null && a.Email.Equals(email));

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
            await dbContext.SaveChangesAsync();
            return Page();
        }
    }
}
