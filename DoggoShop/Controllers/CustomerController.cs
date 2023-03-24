using DoggoShopAPI.DTO;
using DoggoShopAPI.Models;
using DoggoShopAPI.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoggoShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();

        [HttpGet("getCustomerById/{id}")]
        public async Task<IActionResult> GetCustomerById(string id) 
        {
            var customer = await context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            return Ok(customer);
        }
        [HttpGet]
        public IActionResult GetAllCustomers() 
        {
            var items = new List<Customer>();
            foreach(var item in context.Customers)
            {
                items.Add(item);
            }
            return Ok(items);
        }
        [HttpPost]
        public IActionResult PostCustomer(Customer cus)
        {
            if (cus == null)
            {
                return BadRequest();
            }
            
            context.Customers.Add(cus);
            context.SaveChanges();
            return Ok();
        }
        [HttpGet("isCustomerIdExists/{id}")]
        public async Task<IActionResult> IsCustomerIdExists(string id)
        {
            var result = await context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id) == null;
            return Ok(result);
        }
        [HttpGet("getLastCustomer")]
        public async Task<IActionResult> GetLastCustomer()
        {
            var cus = await context.Customers.OrderBy(o => o.CustomerId).LastOrDefaultAsync();
            return Ok(cus);
        }
    }
}
