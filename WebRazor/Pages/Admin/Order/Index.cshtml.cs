using ClosedXML.Excel;
using DoggoShopClient.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;
using System.Security.Claims;
using WebRazor.Materials;

namespace WebRazor.Pages.Admin.Order
{
    [Authorize(Roles = "Employee")]
    public class IndexModel : PageModel
    {
        private readonly PRN221DBContext dbContext;
        [FromQuery(Name = "page")] public int Page { get; set; } = 1;
        [BindProperty] public List<DoggoShopClient.Models.Order> Orders { get; set; }
        public List<String> PagesLink { get; set; } = new List<string>();

        [FromQuery(Name = "txtStartOrderDate")] public DateTime StartDate { get; set; }
        [FromQuery(Name = "txtEndOrderDate")] public DateTime EndDate { get; set; }
        private int perPage = 10;

        public IndexModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        private SqlDateTime? validateDateTime(DateTime time)
        {
            DateTime Min = (DateTime)SqlDateTime.MinValue;
            DateTime Max = (DateTime)SqlDateTime.MaxValue;
            

            if (time >= Min && time <= Max)
            {
                return SqlDateTime.Parse(time.Date.ToString("yyyy-MM-dd HH:mm:ss"));
            }

            return null;
        }

        public async Task LoadData(bool paging = true)
        {
            var queryRaw = dbContext.Orders
                .Where(q => true);

            SqlDateTime? sqlStartDate = validateDateTime(StartDate);
            SqlDateTime? sqlEndDate = validateDateTime(EndDate);


            if (sqlStartDate != null)
            {
                queryRaw = queryRaw.Where(q => (DateTime)q.OrderDate >= StartDate);
            }

            if (sqlEndDate != null)
            {
                queryRaw = queryRaw.Where(q => (DateTime)q.OrderDate <= EndDate);
            }

            var query = queryRaw;
                query = query.Include(q => q.Customer).Include(q => q.Employee)
                    .OrderByDescending(o => o.OrderDate);

            if (paging)
                query = query.Skip((Page - 1) * perPage).Take(perPage);

            Orders = await query.ToListAsync();

            ViewData["StartDate"] = sqlStartDate != null ? StartDate.Date.ToString("yyyy-MM-dd") : "";
            ViewData["EndDate"] = sqlEndDate != null ? EndDate.Date.ToString("yyyy-MM-dd") : "";

            PageLink page = new PageLink(perPage);
            String param = (!(ViewData["StartDate"].Equals("") && ViewData["EndDate"].Equals("")) 
                ? "txtStartOrderDate=" + ViewData["StartDate"] 
                + "&txtEndOrderDate=" + ViewData["EndDate"] : "") + "&";
            PagesLink = page.getLink(Page, await queryRaw.CountAsync(), "/Admin/Order/Index?" + param);
        }


        public async Task<IActionResult> OnGetAsync()
        {

            await LoadData();

            return Page();
        }

        public async Task<IActionResult> OnGetCancel(int? id, string target)
        {

            DoggoShopClient.Models.Order order = await dbContext.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

            await LoadData();

            if (order == null)
            {
                return Redirect(target);
            }
            order.RequiredDate = null;
            await dbContext.SaveChangesAsync();
            return Redirect(target);
        }

        string format(DateTime? date)
        {
            if (date == null)
                return "";
            return ((DateTime)date).Date.ToString("dd-MM-yyyy");
        }

        string getStatus(DateTime? requiredDate, DateTime? shippedDate)
        {
            if (shippedDate != null)
            {
                return "Completed";
            }

            if (requiredDate != null)
            {
                return "Pending";
            }

            return "Order canceled";
        }

        public async Task<IActionResult> OnGetExport()
        {

            await LoadData(false);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Sheet1");
                ws.Range("A1:H1").Merge();
                string title = "Report Order";
                SqlDateTime? sqlStartDate = validateDateTime(StartDate);
                SqlDateTime? sqlEndDate = validateDateTime(EndDate);
                if (sqlStartDate != null)
                {
                    title += " From " + StartDate.ToString("dd/MM/yyyy");
                }
                if (sqlEndDate != null)
                {
                    title += " To " + EndDate.ToString("dd/MM/yyyy");
                }
                ws.Cell(1, 1).Value = title;
                ws.Cell(1, 1).Style.Font.Bold = true;
                ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                ws.Cell(1, 1).Style.Font.FontSize = 30;

                //Header
                ws.Cell(4, 1).Value = "OrderID";
                ws.Cell(4, 2).Value = "OrderDate";
                ws.Cell(4, 3).Value = "RequiredDate";
                ws.Cell(4, 4).Value = "ShippedDate";
                ws.Cell(4, 5).Value = "Employee";
                ws.Cell(4, 6).Value = "Customer";
                ws.Cell(4, 7).Value = "Freight($)";
                ws.Cell(4, 8).Value = "Status";

                ws.Range("A4:H4").Style.Fill.BackgroundColor = XLColor.Alizarin;

                ws.Columns().AdjustToContents();

                int i = 5;
                foreach (DoggoShopClient.Models.Order order in Orders)
                {
                    ws.Cell(i, 1).Value = order.OrderId.ToString();
                    ws.Cell(i, 2).Value = format(order.OrderDate);
                    ws.Cell(i, 3).Value = format(order.RequiredDate);
                    ws.Cell(i, 4).Value = format(order.ShippedDate);
                    ws.Cell(i, 5).Value = order.Employee != null ? (order.Employee.FirstName + " " + order.Employee.LastName) : "";
                    ws.Cell(i, 6).Value = order.Customer != null ? order.Customer.ContactName : "";
                    ws.Cell(i, 7).Value = ((decimal)order.Freight).ToString("G29");
                    ws.Cell(i, 8).Value = getStatus(order.RequiredDate, order.ShippedDate);
                    i++;
                }
                i--;

                ws.Cells("A4:H" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cells("A4:H" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cells("A4:H" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cells("A4:H" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument-speadsheetml.sheet",
                        "Order.xlsx"
                        );
                }
            }
        }
    }
}
