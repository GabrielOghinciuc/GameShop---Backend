using Gamestore.Data;
using Gamestore.Models;
using Gamestore.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gamestore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly GamestoreContext _context;
        private readonly IFileStorage _fileStorage;
        private const string container = "users";

        public UsersController(GamestoreContext context, IFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserDto userDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool userExists = await _context.Users.AnyAsync(u =>
                u.Email == userDto.Email ||
                u.Username == userDto.Username);

            if (userExists)
            {
                return BadRequest("This email or username is already taken");
            }

            var newUser = new User
            {
                Username = userDto.Username,
                Email = userDto.Email,
                Password = userDto.Password,
                FullName = userDto.FullName,
                BirthDate = userDto.BirthDate,
                JoinedOn = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                ProfilePicture = "/user/user.jpg",
                IsClient = true,
                IsGameDeveloper = false,
                IsAdmin = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = newUser.Id }, newUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto loginRequest)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Email == loginRequest.Email &&
                    u.Password == loginRequest.Password);

            if (user == null)
            {
                return Unauthorized("Wrong email or password");
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
                ProfilePicture = user.ProfilePicture,
                BoughtGames = user.BoughtGames
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
                    ProfilePicture = user.ProfilePicture,
                    BoughtGames = user.BoughtGames
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

        [HttpPut("edit-profile/{id:guid}")]
        public async Task<IActionResult> EditProfile(Guid id, [FromForm] UserUpdateDto updates)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (!string.IsNullOrEmpty(updates.Username) && updates.Username != user.Username)
                {
                    bool usernameTaken = await _context.Users.AnyAsync(u =>
                        u.Id != id &&
                        u.Username == updates.Username);

                    if (usernameTaken)
                    {
                        return BadRequest("This username is already taken");
                    }
                    user.Username = updates.Username;
                }

                if (updates.ProfilePicture != null)
                {
                    user.ProfilePicture = await _fileStorage.Edit(
                        user.ProfilePicture,
                        container,
                        updates.ProfilePicture);
                }

                if (!string.IsNullOrEmpty(updates.Email) && updates.Email != user.Email) 
                    user.Email = updates.Email;
                if (!string.IsNullOrEmpty(updates.Password)) 
                    user.Password = updates.Password;
                if (!string.IsNullOrEmpty(updates.FullName)) user.FullName = updates.FullName;
                if (updates.BirthDate.HasValue) user.BirthDate = updates.BirthDate.Value;

                await _context.SaveChangesAsync();
                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating profile: {ex.Message}");
            }
        }

        [HttpPatch("{id:guid}/set-admin-status")]
        public async Task<IActionResult> SetAdminStatus(Guid id, [FromBody] bool isAdmin)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            user.IsAdmin = isAdmin;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                ProfilePicture = user.ProfilePicture,
                JoinedOn = user.JoinedOn,
                LastLogin = user.LastLogin,
                IsClient = user.IsClient,
                IsGameDeveloper = user.IsGameDeveloper,
                IsAdmin = user.IsAdmin
            });
        }

        [HttpPatch("{id:guid}/set-developer-status")]
        public async Task<IActionResult> SetDeveloperStatus(Guid id, [FromBody] bool isGameDeveloper)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            user.IsGameDeveloper = isGameDeveloper;
            await _context.SaveChangesAsync();

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                BirthDate = user.BirthDate,
                ProfilePicture = user.ProfilePicture,
                JoinedOn = user.JoinedOn,
                LastLogin = user.LastLogin,
                IsClient = user.IsClient,
                IsGameDeveloper = user.IsGameDeveloper,
                IsAdmin = user.IsAdmin
            });
        }

        [HttpPost("{userId}/add-games")]
        public async Task<IActionResult> AddGamesToUser(Guid userId, [FromBody] List<int> gameIds)
        {
            if (gameIds == null || !gameIds.Any())
                return BadRequest("No game found!");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return NotFound($"User with ID {userId} not found");

            if (user.BoughtGames == null)
                user.BoughtGames = new List<int>();

            var newGames = gameIds.Except(user.BoughtGames).ToList();
            if (!newGames.Any())
                return Ok("Already in inventory");

            user.BoughtGames.AddRange(newGames);
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Ok($"Have been added {newGames.Count} games.");
        }

        [HttpGet("{userId}/bought-games")]
        public async Task<IActionResult> GetUserBoughtGames(Guid userId)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                return Ok(new
                {
                    BoughtGames = user.BoughtGames
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{userId}/remove-game/{gameId}")]
        public async Task<IActionResult> RemoveGameFromUser(Guid userId, int gameId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Id == userId);

                if (user == null)
                {
                    return NotFound("User not found.");
                }

                var removedCount = user.BoughtGames.RemoveAll(id => id == gameId);

                if (removedCount == 0)
                {
                    return BadRequest("Game not found in user's library.");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok($"Removed {removedCount} instance(s) of game {gameId}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound($"User with ID {id} not found");
            }

            if (!string.IsNullOrEmpty(user.ProfilePicture) && user.ProfilePicture != "/user/user.jpg")
            {
                await _fileStorage.Delete(user.ProfilePicture, container);
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok($"User with ID {id} was deleted successfully");
        }
    }
}
