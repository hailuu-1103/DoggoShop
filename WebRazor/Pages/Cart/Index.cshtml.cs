using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Transactions;
using WebRazor.Materials;
using WebRazor.Models;
using Table = iText.Layout.Element.Table;
using iText.Layout.Borders;
using iText.Layout;
using iText.Kernel.Geom;
using System.Security.Claims;

namespace WebRazor.Pages.Cart
{
    public class IndexModel : PageModel
    {
        public Dictionary<Models.Product, int> Cart { get; set; } = new Dictionary<Models.Product, int>();

        [BindProperty]
        public Models.Customer? Customer { get; set; }

        public decimal Sum { get; set; } = 0;

        private readonly PRN221DBContext dbContext;

        public bool Disable = false;

        private Models.Order Order;

        public IndexModel(PRN221DBContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Load Info
        public async Task<bool> checkLogin()
        {
            try
            {
                Models.Account? auth = await dbContext.Accounts
                    .FirstOrDefaultAsync(a => a.AccountId == Int32.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value));

                Customer = await dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == auth.CustomerId);
                Customer.Accounts.Add(auth);

                Disable = true;
                return true;
            } catch (Exception e)
            {
                return false;
            }

        }

        private Dictionary<int, int> getCart()
        {
            String cart = HttpContext.Session.GetString("cart");


            Dictionary<int, int> list;

            if (cart != null)
            {
                list = JsonSerializer.Deserialize<Dictionary<int, int>>(cart);
            }
            else
            {
                list = new Dictionary<int, int>();
            }

            return list;
        }

        public async Task LoadCartAsync(Dictionary<int, int> list)
        {
            foreach (var item in list)
            {
                Models.Product product = await getProductAsync(item.Key);

                Cart.Add(product, item.Value);

                Sum += (decimal)product.UnitPrice * item.Value;
            }
        }

        public async Task LoadInfo()
        {
            await checkLogin();

            var listIdCart = getCart();

            await LoadCartAsync(listIdCart);
        }
        #endregion

        public async Task<IActionResult> OnGet()
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            await LoadInfo();

            return Page();
        }

        #region Order with cart
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task<Customer> AddCustomer()
        {
            Customer customer = new Customer();
            customer.CustomerId = RandomString(5);
            while (await dbContext.Customers.FirstOrDefaultAsync(c => c.CustomerId == customer.CustomerId) != null)
            {
                customer.CustomerId = RandomString(5);
            }
            
            customer.ContactName = Customer.ContactName;
            customer.Address = Customer.Address;
            customer.CompanyName = Customer.CompanyName;
            customer.ContactTitle = Customer.ContactTitle;
            customer.CreatedAt = DateTime.Now;
            await dbContext.Customers.AddAsync(customer);
            await dbContext.SaveChangesAsync();

            return await dbContext.Customers.OrderBy(o => o.CustomerId).LastOrDefaultAsync();
        }

        public async Task<Models.Order> AddOrder()
        {
            if (Customer.CustomerId == null)
            {
                Customer = await AddCustomer();
            }

            Models.Order order = new Models.Order();
            order.CustomerId = Customer.CustomerId;
            order.OrderDate = DateTime.Now;
            order.RequiredDate = DateTime.Now.AddDays(7);

            await dbContext.Orders.AddAsync(order);
            await dbContext.SaveChangesAsync();

            return await dbContext.Orders.OrderBy(o => o.OrderDate).LastOrDefaultAsync();
        }

        public async Task<decimal> AddOrderDetail(int key, int value, Models.Order order)
        {
            Models.Product product = await getProductAsync(key);

            if (product.UnitsInStock - (short)value < 0)
            {
                throw new Exception(product.ProductName + " not enough quantity. Units in stock:" + product.UnitsInStock);
            }
            product.UnitsInStock -= (short)value;
            product.UnitsOnOrder += (short)value;

            if (await dbContext.OrderDetails.Include(o => o.Order).Where(o => o.ProductId == product.ProductId && o.Order.CustomerId == Customer.CustomerId).CountAsync() > 0)
            {
                product.ReorderLevel += 1;
            }

            OrderDetail od = new OrderDetail();
            od.OrderId = order.OrderId;
            od.ProductId = product.ProductId;
            od.Quantity = (short)value;
            od.UnitPrice = (decimal)product.UnitPrice;
            od.Discount = 0;
            await dbContext.OrderDetails.AddAsync(od);
            await dbContext.SaveChangesAsync();

            return od.UnitPrice * od.Quantity;
        }
        #endregion

        public async Task<IActionResult> OnPost()
        {

            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadInfo();
                return Page();
            }
            var listIdCart = getCart();

            if (listIdCart.Count == 0)
            {
                await LoadInfo();
                return Page();
            }

            await checkLogin();

            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                decimal Freight = 0;
                try
                {
                    Models.Order order = await AddOrder();

                    foreach (var item in listIdCart)
                    {
                        Freight += await AddOrderDetail(item.Key, item.Value, order);
                    }

                    order.Freight = Freight;

                    await dbContext.SaveChangesAsync();

                    ViewData["success"] = "Order successfull";

                    HttpContext.Session.Remove("cart");
                    transaction.Complete();
                    Order = order;

                }
                catch (Exception e)
                {
                    transaction.Dispose();

                    ViewData["fail"] = e.Message;
                }
            }

            if (Customer.Accounts.Count > 0)
            {
                await SendMail();
            }

            await LoadInfo();

            return Page();
        }

        public async Task<Models.Product?> getProductAsync(int? id)
        {
            if (id == null)
            {
                return null;
            }

            Models.Product product = (await dbContext.Products.Where(p => p.DeletedAt == null).FirstOrDefaultAsync(p => p.ProductId == id));

            return product;
        }

        #region Action handler
        public async Task<IActionResult> OnGetAdd(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            Models.Product product = await getProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            if (product == null || product.UnitsInStock == 0)
            {
                TempData["fail"] = "Quantity = 0";
            }
            else

                try
                {
                    var listIdCart = getCart();

                    if ((listIdCart.Where(p => p.Key == id)).Count() == 0)
                    {
                        listIdCart.Add((int)id, 1);
                    }


                    HttpContext.Session.SetString("cart", JsonSerializer.Serialize(listIdCart));
                    TempData["success"] = "Add to cart successfull";

                    int size = listIdCart.Count;
                    HttpContext.Session.SetInt32("cartSize", size);
                }
                catch (Exception e)
                {
                    TempData["fail"] = e.Message;
                }

            return Redirect("/Product/Detail/" + id);
        }

        public async Task<IActionResult> OnGetDown(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            Models.Product product = await getProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            try
            {
                var listIdCart = getCart();

                if (listIdCart.ContainsKey((int)id))
                {
                    if (listIdCart[(int)id] == 1)
                    {
                        TempData["fail"] = "Quantity must > 1";
                        return Redirect("/Cart");
                    }
                    listIdCart[(int)id] -= 1;
                }


                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(listIdCart));
                TempData["success"] = "Quantity down successfull";
            }
            catch (Exception e)
            {
                TempData["fail"] = e.Message;
            }

            return Redirect("/Cart");
        }

        public async Task<IActionResult> OnGetUp(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            Models.Product product = await getProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }


            try
            {
                var listIdCart = getCart();

                if (listIdCart.ContainsKey((int)id))
                {
                    listIdCart[(int)id] += 1;
                }


                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(listIdCart));
                TempData["success"] = "Quanity up successfull";
            }
            catch (Exception e)
            {
                TempData["fail"] = e.Message;
            }

            return Redirect("/Cart");
        }

        public async Task<IActionResult> OnGetRemove(int? id)
        {
            var check = HttpContext.User.FindFirst(ClaimTypes.Role);

            if (check != null && check.Value.Equals("Employee"))
            {
                return NotFound();
            }

            Models.Product product = await getProductAsync(id);

            if (product == null)
            {
                return NotFound();
            }


            try
            {
                var listIdCart = getCart();

                if ((listIdCart.Where(p => p.Key == id)).Count() != 0)
                {
                    listIdCart.Remove((int)id);
                }


                HttpContext.Session.SetString("cart", JsonSerializer.Serialize(listIdCart));
                TempData["success"] = "Remove from cart successfull";

                int size = listIdCart.Count;
                HttpContext.Session.SetInt32("cartSize", size);
            }
            catch (Exception e)
            {
                TempData["fail"] = e.Message;
            }

            return Redirect("/Cart");
        }
        #endregion

        #region Send Mail
        public async Task SendMail()
        {

            MemoryStream ms = new MemoryStream();

            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, PageSize.A4, false);
            writer.SetCloseStream(false);

            Paragraph header = new Paragraph("Your order")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(20);
            document.Add(header);

            LineSeparator ls = new LineSeparator(new SolidLine());
            document.Add(ls);

            Paragraph subheader1 = new Paragraph("OrderID: #" + Order.OrderId)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(15);
            document.Add(subheader1);

            Paragraph subheader2 = new Paragraph("Order creation date: " + Order.OrderDate)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(15);
            document.Add(subheader2);

            document.Add(new Paragraph(""));
            document.Add(ls);
            document.Add(new Paragraph(""));

            document.Add(await GetPdfTable(Order.OrderDetails));

            float total = 0;
            foreach (var item in Order.OrderDetails)
            {
                float price = MathF.Round((float)item.UnitPrice * (float)item.Quantity * (float)(1 - item.Discount), 2, MidpointRounding.ToZero);
                total += price;
            }

            document.Add(new Paragraph(""));
            document.Add(ls);
            document.Add(new Paragraph(""));

            Paragraph totalTitle = new Paragraph("Total Price: $" + total.ToString())
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetFontSize(15);
            document.Add(totalTitle);

            int n = pdfDoc.GetNumberOfPages();
            for (int i = 1; i <= n; i++)
            {
                document.ShowTextAligned(new Paragraph(
                    String.Format("Page " + i + " of " + n)),
                    559, 806, i, TextAlignment.RIGHT,
                    VerticalAlignment.TOP, 0);
            }

            document.Close();

            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            Mail mail = new Mail();
            mail.SendEmailOrderAsync(Customer.Accounts.ToList()[0].Email, byteInfo);

        }

        private async Task<Table> GetPdfTable(ICollection<OrderDetail> ods)
        {
            // Table
            Table table = new Table(4, false).SetWidth(UnitValue.CreatePercentValue(100)); ;

            // Headings
            Cell cellProductId = new Cell(1, 1)
               .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Product ID"));

            Cell cellProductName = new Cell(1, 1)
               .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Product Name"));

            Cell cellQuantity = new Cell(1, 1)
               .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Quantity"));

            Cell cellUnitPrice = new Cell(1, 1)
               .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
               .SetTextAlignment(TextAlignment.CENTER)
               .SetBorder(Border.NO_BORDER)
               .Add(new Paragraph("Unit Price"));

            table.AddCell(cellProductId);
            table.AddCell(cellProductName);
            table.AddCell(cellQuantity);
            table.AddCell(cellUnitPrice);

            foreach (var item in ods)
            {
                Image image = new Image(ImageDataFactory.Create("wwwroot/img/2.jpg")).SetWidth(120).SetAutoScaleWidth(true);

                Cell cId = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph().Add(image));

                Cell cName = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph(item.Product.ProductName));

                Cell cQty = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph("Quantity: " + item.Quantity.ToString()));

                float price = MathF.Round((float)item.UnitPrice * (float)item.Quantity * (float)(1 - item.Discount), 2, MidpointRounding.ToZero);

                Cell cPrice = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetBorder(Border.NO_BORDER)
                    .Add(new Paragraph("$" + price.ToString()));

                table.AddCell(cId);
                table.AddCell(cName);
                table.AddCell(cQty);
                table.AddCell(cPrice);
            }

            return table;
        }
        #endregion
    }
}