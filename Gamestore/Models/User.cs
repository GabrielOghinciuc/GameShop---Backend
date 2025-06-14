namespace Gamestore.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime JoinedOn { get; set; }
        public DateTime LastLogin { get; set; }
        public string ProfilePicture { get; set; } = "/user/user.jpg";
        public bool IsClient { get; set; }
        public bool IsGameDeveloper { get; set; }
        public bool IsAdmin { get; set; }

        public List<int> BoughtGames { get; set; } = new List<int>();
    }
}
