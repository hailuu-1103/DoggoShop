using ClosedXML.Excel;
using DoggoShopClient.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Security.Claims;
using System.Text.Json;
using WebRazor.Materials;

namespace WebRazor.Pages.Admin.Product
{
    [Authorize(Roles = "Employee")]
    public class IndexModel : PageModel
    {
        private HttpClient client = new HttpClient();
        private string CategoryApiUrl;
        [FromQuery(Name = "page")] public int Page { get; set; } = 1;
        [FromQuery(Name = "txtSearch")] public string Search { get; set; } = "";
        [FromQuery(Name = "categoryId")] public int CatId { get; set; } = 0;
        public List<String> PagesLink { get; set; } = new List<string>();

        [AllowedExtensions(new string[] { ".xls", ".xlsx" })]
        [BindProperty]
        [Required(ErrorMessage = "File is required")]
        public IFormFile FileUpload { get; set; }

        public List<DoggoShopClient.Models.Product> Products;
        public List<Category> Categories;
        private readonly PRN221DBContext dbContext;
        private int perPage = 10;
        private IWebHostEnvironment _hostingEnvironment;
        private readonly IHubContext<HubServer> hubContext;

        public IndexModel(PRN221DBContext dbContext, IWebHostEnvironment environment, IHubContext<HubServer> hubContext)
        {
            this.dbContext = dbContext;
            _hostingEnvironment = environment;
            this.hubContext = hubContext;
        }

        public async Task Load(bool paging = true)
        {
            if (Search == null) Search = "";

            Categories = await dbContext.Categories.ToListAsync();
            var queryRaw = dbContext.Products
                .Where(p => p.DeletedAt == null)
               .Where(p => p.ProductName.Contains(Search));

            if (CatId != 0)
            {
                queryRaw = queryRaw.Where(p => p.CategoryId == CatId);
            }

            var query = queryRaw;

            query = query.Include(p => p.Category)
                .OrderByDescending(p => p.ProductId);

            if (paging)
            {
                query = query.Skip((Page - 1) * perPage).Take(perPage);
            }

            Products = await query.ToListAsync();

            PageLink page = new PageLink(perPage);
            String param = "categoryId=" + CatId + "&txtSearch=" + Search;
            PagesLink = page.getLink(Page, await queryRaw.CountAsync(), "/Admin/Product/Index?" + param + "&");
        }

        public async Task<IActionResult> OnGetAsync()
        {

            await Load();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {

            await ReadFile();

            await Load();

            return Page();
        }

        public async Task<bool> ReadFile()
        {
            
            DataSet ds = new DataSet();
            IExcelDataReader reader = null;
            Stream FileStream = null;

            ViewData["Fail"] = "Have some error with data. Need include ProductName, CategoryId, QuantityPerUnit,"
+ " UnitPrice, UnitsInStock, Discontinued";
            ViewData["Success"] = "";

            try
            {
                FileStream = FileUpload.OpenReadStream();
                if (FileStream != null)
                {
                    if (FileUpload.FileName.EndsWith(".xls"))
                        reader = ExcelReaderFactory.CreateBinaryReader(FileStream);
                    else if (FileUpload.FileName.EndsWith(".xlsx"))
                        reader = ExcelReaderFactory.CreateOpenXmlReader(FileStream);

                    ds = reader.AsDataSet();
                    reader.Close();
                }
                CategoryApiUrl = "";
                if (ds != null && ds.Tables.Count > 0)
                {
                    DataTable dtRecords = ds.Tables[0];
                    for (int i = 1; i < dtRecords.Rows.Count; i++)
                    {
                        var productId = dtRecords.Rows[i][0];
                        DoggoShopClient.Models.Product product = new DoggoShopClient.Models.Product();
                        product.ProductName = Convert.ToString(dtRecords.Rows[i][1]);
                        CategoryApiUrl = "https://localhost:5000/api/Category/getCategoryByName/" + dtRecords.Rows[i][5];
                        var response = await client.GetAsync(CategoryApiUrl);
                        var data = await response.Content.ReadAsStringAsync();
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        };
                        var category = JsonSerializer.Deserialize<Category>(data, options);
                        product.CategoryId = dtRecords.Rows[i][5].Equals("NULL") ? null : category.CategoryId;
                        product.QuantityPerUnit = dtRecords.Rows[i][3].Equals("NULL") ? null : Convert.ToString(dtRecords.Rows[i][3]);
                        product.UnitPrice = dtRecords.Rows[i][2].Equals("NULL") ? null : Convert.ToDecimal(dtRecords.Rows[i][2]);
                        product.UnitsInStock = dtRecords.Rows[i][4].Equals("NULL") ? null : Convert.ToInt16(dtRecords.Rows[i][4]);
                        product.Discontinued = Convert.ToBoolean(dtRecords.Rows[i][6]);

                        if(dbContext.Products.FirstOrDefault(p => !p.ProductId.Equals(productId)) == null)
                        {
                            await dbContext.Products.AddAsync(product);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                    await hubContext.Clients.All.SendAsync("Reload");
                    ViewData["Fail"] = "";
                    ViewData["Success"] = "Upload Success";
                }
            }
            catch (Exception e)
            {

            }
            return true;
        }

        public async Task<IActionResult> OnGetExport()
        {

            await Load(false);

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Sheet1");
                //Header
                ws.Cell(1, 1).Value = "ProductID";
                ws.Cell(1, 2).Value = "ProductName";
                ws.Cell(1, 3).Value = "UnitPrice";
                ws.Cell(1, 4).Value = "Unit";
                ws.Cell(1, 5).Value = "UnitInStock";
                ws.Cell(1, 6).Value = "Category";
                ws.Cell(1, 7).Value = "Discontinued";

                ws.Range("A1:G1").Style.Fill.BackgroundColor = XLColor.Alizarin;

                int i = 2;
                foreach (DoggoShopClient.Models.Product product in Products)
                {
                    ws.Cell(i, 1).Value = product.ProductId.ToString();
                    ws.Cell(i, 2).Value = product.ProductName;
                    ws.Cell(i, 3).Value = ((decimal)product.UnitPrice).ToString("G29");
                    ws.Cell(i, 4).Value = product.QuantityPerUnit;
                    ws.Cell(i, 5).Value = product.UnitsInStock;
                    ws.Cell(i, 6).Value = product.Category.CategoryName;
                    ws.Cell(i, 7).Value = product.Discontinued;
                    i++;
                }
                i--;

                ws.Cells("A1:G" + i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                ws.Cells("A1:G" + i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                ws.Cells("A1:G" + i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                ws.Cells("A1:G" + i).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument-speadsheetml.sheet",
                        "Product.xlsx"
                        );
                }
            }
        }
    }
}
