namespace SaveNature.Models
{
    public class EditProfileViewModel
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string? AboutMe { get; set; }
    }
}