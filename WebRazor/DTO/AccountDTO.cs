namespace DoggoShopAPI.DTO
{
    public class AccountDTO
    {
        public string? Email { get; set; }

        public string? Password { get; set; }
        public int? Role { get; set; }
        public string CompanyName { get; set; } = null!;

        public string? ContactName { get; set; }

        public string? ContactTitle { get; set; }

        public string? Address { get; set; }
        public DateTime? CreatedAt { get; set; }

        public bool Active { get; set; }
    }
}
