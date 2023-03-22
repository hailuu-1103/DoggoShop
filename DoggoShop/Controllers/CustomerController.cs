using DoggoShopAPI.DTO;
using DoggoShopAPI.Models;
using DoggoShopAPI.Utility;
using Microsoft.AspNetCore.Mvc;

namespace DoggoShopAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : Controller
    {
        private Prn221dbContext context = new Prn221dbContext();

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
        public IActionResult PostCustomer(CustomerDTO cusDTO)
        {
            /*if(cusDTO == null)
            {
                return BadRequest();
            }
            var customer = new Customer()
            {
                CustomerId = RandomUtils.RandomCustID(5),
                CompanyName = cusDTO.CompanyName,
                ContactName = cusDTO.ContactName,
                ContactTitle = cusDTO.ContactTitle,
                Address = cusDTO.Address,
                CreatedAt = cusDTO.CreatedAt,
                Active = cusDTO.Active,
            };
            context.Customers.Add(customer);
            context.SaveChanges();*/
            return Ok();
        }
    }
}
