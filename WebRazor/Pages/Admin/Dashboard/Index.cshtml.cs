using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace WebRazor.Pages.Admin.Dashboard
{
    [Authorize(Roles = "Employee")]
    public class IndexModel : PageModel
    {
        private readonly PRN221DBContext dBContext;
        public IndexModel(PRN221DBContext dBContext)
        {
            this.dBContext = dBContext;
        }

        [FromQuery(Name = "txtDate")] public DateTime? Date { get; set; }

        public decimal TotalSales;
        public decimal WeeklySales;
        public decimal?[] StatisticOrders = {null, null, null, null, null, null, null, null, null, null, null, null};

        public int TotalCus;
        public int Guest;
        public float TotalCusMonth;
        public float GuestMonth;

        public async Task<IActionResult> OnGetAsync()
        {

            List<DoggoShopClient.Models.Order> Orders = await dBContext.Orders.Where(o => o.RequiredDate!= null).ToListAsync();
            TotalSales = (decimal)Orders.Sum(x => x.Freight);

            DateTime now = Date != null ? (DateTime) Date : DateTime.Now;
            ViewData["Date"] = now.Date.ToString("yyyy-MM-dd");
            DateTime sub = now.AddDays(-7);
            List<DoggoShopClient.Models.Order> OrdersWeek = Orders.Where(x => x.OrderDate >= sub
                            && x.OrderDate <= now).ToList();

            WeeklySales = (decimal)OrdersWeek.Sum(x => x.Freight);

            List<DoggoShopClient.Models.Order> OrderYear = Orders.Where(o => o.OrderDate.Value.Year == now.Year).ToList();

            for(int i = 0; i < now.Month; i++)
            {
                List<DoggoShopClient.Models.Order> OrdersMonth = OrderYear.Where(o => o.OrderDate.Value.Month == i + 1).ToList();
                StatisticOrders[i] = (decimal) OrdersMonth.Sum(x => x.Freight);
            }

            List<DoggoShopClient.Models.Customer> Customer = await dBContext.Customers.Include(x => x.Accounts).ToListAsync();
            TotalCus = Customer.Count();
            TotalCusMonth = Customer.Where(x => x.CreatedAt != null && x.CreatedAt.Value.Year == now.Year).ToList().Count() / now.Month;

            Guest = Customer.Where(x => x.Accounts.Count == 0).Count();
            GuestMonth = Customer.Where(x => x.Accounts.Count == 0).Where(x => x.CreatedAt != null && x.CreatedAt.Value.Year == now.Year)
                .Count() / now.Month;

            return Page();
        }
    }
}
