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
        [HttpGet("id/{id}")]
        public IActionResult GetAccountById(int id)
        {
            var account = context.Accounts.Include(acc => acc.Customer).Include(acc => acc.Employee).Include(account => account.Customer).ThenInclude(acc => acc.Orders).ThenInclude(o => o.OrderDetails).ThenInclude(od=>od.Product).FirstOrDefault(acc => acc.AccountId == id);
            if (account == null)
            {
                return BadRequest("Not found account");
            }
            return Ok(account);
        }

        [HttpGet("email/{email}")]
        public IActionResult GetAccountByEmail(string email)
        {
            var account = context.Accounts.Include(acc => acc.Customer).Include(acc => acc.Employee).FirstOrDefault(acc => acc.Email.Equals(email));
            if (account == null)
            {
                return BadRequest("Not found account");
            }
            return Ok(account);
        }
        [HttpGet("isAccountExists/{id}")]
        public IActionResult IsAccountExist(int id)
        {
            var result = context.Accounts.Any(e => e.AccountId == id);
            return Ok(result);
        }
        [HttpGet("isAccountWithEmailExists/{id}/{email}")]
        public IActionResult IsAccountWithEmailExist(int id, string email)
        {
            var result = context.Accounts.Any(e => e.Email == email && e.AccountId != id);
            return Ok(result);
        }
        [HttpGet("isEmailExist/{email}")]
        public IActionResult IsEmailExist(string email)
        {
            var result = context.Accounts.Any(e => e.Email.Equals(email));
            return Ok(result);
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

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, AccountDTO accDTO)
        {
            var accountExist = context.Accounts.Any(e => e.AccountId == id);
            if (accDTO == null)
            {
                return BadRequest();
            }
            var accountEmailExists = context.Accounts.Any(e => e.Email == accDTO.Email && e.AccountId != id);
            var account = context.Accounts.FirstOrDefault(acc => acc.AccountId == id);
            account.Email = accDTO.Email;
            account.Password = accDTO.Password;

            var customer = context.Accounts.Include(acc => acc.Customer).FirstOrDefault(acc => acc.AccountId == id).Customer;
            customer.CompanyName = accDTO.CompanyName;
            customer.ContactTitle = accDTO.ContactTitle;
            customer.ContactName = accDTO.ContactName;
            customer.Address = accDTO.Address;
            try
            {
                context.Entry<Account>(account).State = EntityState.Modified;
                context.Entry<Customer>(customer).State = EntityState.Modified;
                if (accountEmailExists)
                {
                    return BadRequest();
                }
                var countChange = await context.SaveChangesAsync();
                if(countChange == 0)
                {
                    return BadRequest();
                } 
            }
            catch (DbUpdateConcurrencyException) 
            {
                if (!accountExist)
                {
                    return NotFound();
                }
            }
            return Ok();
        }
    }
}
