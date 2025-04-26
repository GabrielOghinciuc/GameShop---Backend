using Microsoft.AspNetCore.Http;

namespace Gamestore.Models
{
    public class UserUpdateDto
    {
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public IFormFile? ProfilePicture { get; set; }
    }
}
