namespace nuxt_shop.Models
{
    public class UserInfo
    {
        public string UserName { get; set; } = null!;

        public string? FullName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? Address { get; set; }

        public int Id { get; set; }

        public string? Status { get; set; }

        public string? Photo { get; set; }

        public bool IsLocked { get; set; }

        public string? IdNumber { get; set; }

        public DateOnly? DateOfBirth { get; set; }
    }
}
