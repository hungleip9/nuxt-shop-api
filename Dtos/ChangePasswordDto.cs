namespace nuxt_shop.Dtos
{
    public class ChangePasswordDto
    {
        public string PhoneNumber { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
