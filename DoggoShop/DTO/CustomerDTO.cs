namespace DoggoShopAPI.DTO
{
    public class CustomerDTO
    {
        public string CompanyName { get; set; } = null!;

        public string? ContactName { get; set; }

        public string? ContactTitle { get; set; }

        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }

        public bool Active { get; set; }
    }
}
