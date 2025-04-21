using Gamestore.Data;
using Gamestore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GamestoreContext _context;

        public UsersController(GamestoreContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email || u.Username == userDto.Username))
            {
                return BadRequest("A user with the same email or username already exists.");
            }

            var user = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = userDto.Password,
                FullName = userDto.FullName,
                BirthDate = userDto.BirthDate,
                JoinedOn = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                ProfilePicture = "/user/user.jpg", // Ensure default profile picture is set
                IsClient = true,
                IsGameDeveloper = false,
                IsAdmin = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == loginRequest.Email && u.Password == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("Invalid email or password.");
            }

            user.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                JoinedOn = user.JoinedOn,
                LastLogin = user.LastLogin,
                IsClient = user.IsClient,
                IsGameDeveloper = user.IsGameDeveloper,
                IsAdmin = user.IsAdmin,
                ProfilePicture = user.ProfilePicture
            });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(user => new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    BirthDate = user.BirthDate,
                    JoinedOn = user.JoinedOn,
                    LastLogin = user.LastLogin,
                    IsClient = user.IsClient,
                    IsGameDeveloper = user.IsGameDeveloper,
                    IsAdmin = user.IsAdmin,
                    ProfilePicture = user.ProfilePicture
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetUser(Guid id)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FullName = u.FullName,
                    BirthDate = u.BirthDate,
                    JoinedOn = u.JoinedOn,
                    LastLogin = u.LastLogin,
                    IsClient = u.IsClient,
                    IsGameDeveloper = u.IsGameDeveloper,
                    IsAdmin = u.IsAdmin,
                    ProfilePicture = u.ProfilePicture
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound($"User with ID {id} not found.");
            }

            return Ok(user);
        }
    }
}
