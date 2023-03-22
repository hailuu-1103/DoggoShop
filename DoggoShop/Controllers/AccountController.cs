using DoggoShopAPI.DTO;
using DoggoShopAPI.Models;
using DoggoShopAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoggoShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();

        [HttpGet]
        public IActionResult GetAllAccounts()
        {
            var items = new List<Account>();
            foreach(var account in context.Accounts.Include(acc => acc.Customer).Include(acc => acc.Employee))
            {
                items.Add(account);
            }
            return Ok(items);
        }
        [HttpGet("id")]
        public IActionResult GetAccount(int id)
        {
            var account = context.Accounts.Include(acc => acc.Customer).Include(acc => acc.Employee).FirstOrDefault(acc => acc.AccountId == id);
            if (account == null)
            {
                return BadRequest("Not found account");
            }
            return Ok(account);
        }
        [HttpPost]
        public async Task<IActionResult> PostAccount(AccountDTO accDTO)
        {
            if (accDTO == null)
            {
                return BadRequest();
            }
            var cus = new Customer()
            {
                CustomerId = RandomUtils.RandomCustID(5),
                CompanyName = accDTO.CompanyName,
                ContactName = accDTO.ContactName,
                ContactTitle = accDTO.ContactTitle,
                Address = accDTO.Address,
                CreatedAt = DateTime.Now,
                Active = true,
            };
            var acc = new Account()
            {
                Password = HashPassword.Hash(accDTO.Password),
                Email = accDTO.Email,
                Role = accDTO.Role,
                CustomerId = cus.CustomerId
            };
            await context.Customers.AddAsync(cus);
            await context.Accounts.AddAsync(acc);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
